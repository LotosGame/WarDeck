using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardChoiceUI : MonoBehaviour
{
    public static CardChoiceUI Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject choicePanel;

    private Action<CardData, CardData> onChoiceCallback;
    private GameObject cardContainer;
    private TMP_Text titleText;
    private TMP_Text subtitleText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        EnsureUIExists();
    }

    private void EnsureUIExists()
    {
        if (choicePanel != null) return;

        // Ищем в Canvas существующую панель или создаем динамически
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null) return;

        // 1. Создаем корневую панель с затемнением
        choicePanel = new GameObject("CardChoiceModal");
        choicePanel.transform.SetParent(canvas.transform, false);

        RectTransform rt = choicePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.SetAsLastSibling();

        Image bgImage = choicePanel.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.82f);
        bgImage.raycastTarget = true; // Блокирует клики по игровому полю

        TMP_FontAsset defaultFont = CyrillicFontRuntimeFallback.GetCyrillicFont();
        if (defaultFont == null)
        {
            TextMeshProUGUI existingText = canvas.GetComponentInChildren<TextMeshProUGUI>();
            if (existingText != null) defaultFont = existingText.font;
        }

        // 2. Заголовок
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(choicePanel.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.8f);
        titleRt.anchorMax = new Vector2(0.5f, 0.8f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(600, 60);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) titleText.font = defaultFont;
        titleText.text = "ВЫБЕРИТЕ КАРТУ В РУКУ";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.85f, 0.3f, 1f); // Золотистый

        // 3. Подзаголовок
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(choicePanel.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.74f);
        subRt.anchorMax = new Vector2(0.5f, 0.74f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.sizeDelta = new Vector2(600, 40);

        subtitleText = subObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) subtitleText.font = defaultFont;
        subtitleText.text = "Нажмите на одну из двух карт, чтобы добавить её в руку";
        subtitleText.fontSize = 18;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        // 4. Контейнер для двух карт
        cardContainer = new GameObject("CardContainer");
        cardContainer.transform.SetParent(choicePanel.transform, false);
        RectTransform containerRt = cardContainer.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.45f);
        containerRt.anchorMax = new Vector2(0.5f, 0.45f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.sizeDelta = new Vector2(700, 360);

        HorizontalLayoutGroup hlg = cardContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 140;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        choicePanel.SetActive(false);
    }

    public void ShowChoice(CardData card1, CardData card2, Action<CardData, CardData> onChosen)
    {
        EnsureUIExists();

        if (choicePanel == null || cardContainer == null)
        {
            Debug.LogError("[CardChoiceUI] Не удалось инициализировать UI выбора карт.");
            onChosen?.Invoke(card1, card2);
            return;
        }

        onChoiceCallback = onChosen;

        // Очищаем старые карты в контейнере
        foreach (Transform child in cardContainer.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject prefab = CardManager.Instance != null ? CardManager.Instance.CardPrefab : null;
        if (prefab == null)
        {
            Debug.LogError("[CardChoiceUI] cardPrefab не найден в CardManager!");
            onChosen?.Invoke(card1, card2);
            return;
        }

        // Создаем 2 карты на выбор
        CreateChoiceCard(card1, card2, prefab);
        CreateChoiceCard(card2, card1, prefab);

        choicePanel.SetActive(true);
        choicePanel.transform.SetAsLastSibling();
    }

    private void CreateChoiceCard(CardData targetCard, CardData otherCard, GameObject prefab)
    {
        GameObject cardObj = Instantiate(prefab, cardContainer.transform);
        cardObj.transform.localScale = Vector3.one * 1.6f; // Увеличенный размер для презентации

        CardUI cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.SetBaseScale(Vector3.one * 1.6f);
            cardUI.Setup(targetCard);
            cardUI.isDraggable = false; // Отключаем перетаскивание на поле
            cardUI.onCardClicked = (clickedUI) =>
            {
                OnCardPicked(targetCard, otherCard);
            };
        }

        // Также добавляем кнопку «ВЗЯТЬ» под картой
        GameObject btnObj = new GameObject("PickButton");
        btnObj.transform.SetParent(cardObj.transform, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, -0.15f);
        btnRt.anchorMax = new Vector2(0.5f, -0.15f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(110, 34);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Зеленая кнопка

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => OnCardPicked(targetCard, otherCard));

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = btnTextObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        if (titleText != null && titleText.font != null) btnText.font = titleText.font;
        else btnText.font = CyrillicFontRuntimeFallback.GetCyrillicFont();
        btnText.text = "ВЗЯТЬ";
        btnText.fontSize = 14;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
    }

    private void OnCardPicked(CardData picked, CardData discarded)
    {
        choicePanel.SetActive(false);
        onChoiceCallback?.Invoke(picked, discarded);
        onChoiceCallback = null;
    }

    public void Hide()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);
    }
}
