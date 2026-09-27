using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;

    public CardData CardData { get; private set; }

    public void Setup(CardData data)
    {
        CardData = data;
        if (data == null) return;

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.cost.ToString();

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
                backgroundImage.color = Color.white;
            }
        }
        else
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
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

    public void OnCardClick()
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardSelected(this);
        }
    }
}
