using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;

    public CardData CardData { get; private set; }
    public bool isDraggable = true;
    public System.Action<CardUI> onCardClicked;

    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;
    private bool isDragging = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Setup(CardData data)
    {
        CardData = data;
        if (data == null) return;

        TMP_FontAsset cyrFont = CyrillicFontRuntimeFallback.GetCyrillicFont();
        if (nameText != null)
        {
            if (cyrFont != null) nameText.font = cyrFont;
            nameText.text = data.cardName;
        }

        if (costText != null)
        {
            if (cyrFont != null) costText.font = cyrFont;
            costText.text = data.cost.ToString();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = data.backgroundColor;
        }

        ApplyCardArt(data);
    }

    private void ApplyCardArt(CardData data)
    {
        if (iconImage == null) return;

        StretchToFill(iconImage.rectTransform);
        iconImage.type = Image.Type.Simple;
        iconImage.preserveAspect = false;
        iconImage.raycastTarget = false;

        if (data.cardArt != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = data.cardArt;
            iconImage.color = Color.white;
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.clear;
            }
            if (nameText != null) nameText.gameObject.SetActive(false);
            if (costText != null) costText.gameObject.SetActive(false);
        }
        else
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            if (backgroundImage != null)
            {
                backgroundImage.color = data.backgroundColor;
            }
            if (nameText != null) nameText.gameObject.SetActive(true);
            if (costText != null) costText.gameObject.SetActive(true);
        }
    }

    private static void StretchToFill(RectTransform rect)
    {
        if (rect == null) return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private Vector3 baseScale = Vector3.one;
    private bool isBaseScaleSet = false;

    public void SetBaseScale(Vector3 scale)
    {
        baseScale = scale;
        isBaseScaleSet = true;
        transform.localScale = scale;
    }

    private void EnsureBaseScale()
    {
        if (!isBaseScaleSet && transform.localScale != Vector3.zero)
        {
            baseScale = transform.localScale;
            isBaseScaleSet = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        EnsureBaseScale();
        transform.localScale = baseScale * 1.15f; // Увеличиваем размер при наведении
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        EnsureBaseScale();
        transform.localScale = baseScale; // Возвращаем исходный размер
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        EnsureBaseScale();
        isDragging = true;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalLocalScale = baseScale;
        originalLocalRotation = transform.localRotation;

        // Выносим на верхний уровень Canvas, чтобы карта была поверх всех элементов UI
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
        }
        transform.SetAsLastSibling();

        // Отключаем блокировку лучей, чтобы события доходили до поля под картой
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        // Выравниваем наклон и слегка уменьшаем для лучшего обзора поля боя
        transform.localRotation = Quaternion.identity;
        transform.localScale = baseScale * 0.9f;

        // Если это юнит или постройка, визуально подсвечиваем зону призыва
        if (CardData != null && CardData.cardType != CardType.Spell && !CardData.cardName.Contains("Стрелы"))
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ShowPlayerSpawnZone();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // Перемещаем карту за курсором/пальцем
        transform.position = eventData.position;

        // Обновляем подсветку клетки поля
        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardHoverTile(this, eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        isDragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        // Скрываем маркер подсветки и зону спавна
        if (GridManager.Instance != null)
        {
            GridManager.Instance.HideHighlight();
            GridManager.Instance.HidePlayerSpawnZone();
        }

        // Пробуем разыграть карту на поле
        bool played = false;
        if (CardManager.Instance != null)
        {
            played = CardManager.Instance.TryPlayCard(this, eventData.position);
        }

        // Если не удалось разыграть — возвращаем в руку
        if (!played)
        {
            ReturnToHand();
        }
    }

    private void ReturnToHand()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
        EnsureBaseScale();
        transform.localScale = baseScale;
        transform.localRotation = originalLocalRotation;

        if (CardFanLayout.Instance != null)
        {
            CardFanLayout.Instance.UpdateFanLayout();
        }
    }

    public void OnCardClick()
    {
        if (onCardClicked != null)
        {
            onCardClicked.Invoke(this);
            return;
        }

        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardSelected(this);
        }
    }
}
