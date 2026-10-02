using UnityEngine;

[ExecuteAlways]
public class CardFanLayout : MonoBehaviour
{
    [Header("Fan Settings")]
    [SerializeField] private float curveHeight = 30f;   // Подъем центральных карт
    [SerializeField] private float maxRotation = 12f;   // Поворот крайних карт
    [SerializeField] private float cardSpacing = 60f;   // Шаг/расстояние между картами

    public static CardFanLayout Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateFanLayout();
    }

    public void UpdateFanLayout()
    {
        int count = transform.childCount;
        if (count == 0) return;

        // Находим реальный центр руки
        float centerIndex = (count - 1) / 2f;

        // Если карт 3 или меньше — они просто становятся в ровный горизонтальный ряд
        bool isStraightRow = count <= 3;
        float rowSpacing = Mathf.Max(cardSpacing, 110f);

        for (int i = 0; i < count; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            // Индекс карты относительно центра (от -centerIndex до +centerIndex)
            float indexFromCenter = i - centerIndex;

            if (isStraightRow)
            {
                // Ровный ряд: поворот 0, высота Y 0, комфортное расстояние
                float xPos = indexFromCenter * rowSpacing;
                child.anchoredPosition = new Vector2(xPos, 0f);
                child.localRotation = Quaternion.identity;
            }
            else
            {
                // Веер при > 3 картах: параболическая дуга и наклон
                float normalizedOffset = (count > 1) ? indexFromCenter / centerIndex : 0f;
                float xPos = indexFromCenter * cardSpacing;
                float yPos = -Mathf.Pow(normalizedOffset, 2) * curveHeight;
                float rotationZ = -normalizedOffset * maxRotation;

                child.anchoredPosition = new Vector2(xPos, yPos);
                child.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            }
        }
    }
}