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

        for (int i = 0; i < count; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            // Индекс карты относительно центра (от -centerIndex до +centerIndex)
            float indexFromCenter = i - centerIndex;

            // Нормализованное смещение от -1 до 1 (для расчёта поворота и высоты)
            float normalizedOffset = (count > 1) ? indexFromCenter / centerIndex : 0f;

            // 1. Позиция X (симметрично от центра 0)
            float xPos = indexFromCenter * cardSpacing;

            // 2. Позиция Y (параболическая дуга)
            float yPos = -Mathf.Pow(normalizedOffset, 2) * curveHeight;

            // 3. Поворот Z (наклон)
            float rotationZ = -normalizedOffset * maxRotation;

            // Применяем локальную позицию относительно центра HandPanel
            child.anchoredPosition = new Vector2(xPos, yPos);
            child.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }
    }
}