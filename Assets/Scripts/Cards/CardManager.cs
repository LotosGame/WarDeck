using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardPrefab;

    [Header("Deck Configuration")]
    [SerializeField] private List<CardData> deck = new List<CardData>();

    private List<CardData> currentHand = new List<CardData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Для теста при старте сцены спавним начальные карты
        SpawnInitialHand();
    }

    public void SpawnInitialHand()
    {
        if (handPanel == null) return;

        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (CardData cardData in deck)
        {
            CreateCardUI(cardData);
        }
    }

    public void CreateCardUI(CardData cardData)
    {
        if (cardPrefab == null || handPanel == null) return;

        GameObject cardObj = Instantiate(cardPrefab, handPanel);
        CardUI cardUI = cardObj.GetComponent<CardUI>();

        if (cardUI != null)
        {
            cardUI.Setup(cardData);
        }
    }

    public void OnCardSelected(CardUI cardUI)
    {
        Debug.Log($"Карта выбрана: {cardUI.CardData?.cardName}");
    }

    public void OnCardHoverTile(CardUI cardUI, Vector2 screenPosition)
    {
        if (GridManager.Instance == null || cardUI == null || cardUI.CardData == null)
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
        worldPoint.z = 0;

        Vector3Int cellPos = GridManager.Instance.WorldToCell(worldPoint);

        bool hasTile = GridManager.Instance.HasTile(cellPos);
        bool inSpawnZone = GridManager.Instance.IsInPlayerSpawnZone(cellPos);
        bool isOccupied = GridManager.Instance.IsCellOccupied(cellPos);
        bool hasMana = GameManager.Instance != null && GameManager.Instance.CanAfford(cardUI.CardData.cost);

        bool isValid = hasTile && inSpawnZone && !isOccupied && hasMana;

        if (hasTile)
        {
            GridManager.Instance.ShowHighlight(cellPos, isValid);
        }
        else
        {
            GridManager.Instance.HideHighlight();
        }
    }

    public bool TryPlayCard(CardUI cardUI, Vector2 screenPosition)
    {
        if (cardUI == null || cardUI.CardData == null)
            return false;

        CardData cardData = cardUI.CardData;

        // 1. Проверяем, хватает ли маны
        if (GameManager.Instance != null && !GameManager.Instance.CanAfford(cardData.cost))
        {
            Debug.LogWarning($"[CardManager] Недостаточно маны для розыгрыша '{cardData.cardName}'! Нужно: {cardData.cost}, доступно: {GameManager.Instance.CurrentMana}");
            return false;
        }

        // 2. Получаем мировую точку клика/дропа
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
        worldPoint.z = 0;

        if (GridManager.Instance == null)
        {
            Debug.LogError("[CardManager] GridManager не найден на сцене!");
            return false;
        }

        Vector3Int cellPos = GridManager.Instance.WorldToCell(worldPoint);

        // 3. Проверяем наличие тайла на сетке
        if (!GridManager.Instance.HasTile(cellPos))
        {
            Debug.LogWarning($"[CardManager] Клетка {cellPos} находится за пределами игрового поля.");
            return false;
        }

        // 4. Проверяем зону призыва (только возле синей столицы игрока)
        if (!GridManager.Instance.IsInPlayerSpawnZone(cellPos))
        {
            Debug.LogWarning($"[CardManager] Войска можно призывать только возле своей столицы (синий квадрат)!");
            return false;
        }

        // 5. Проверяем, не занята ли клетка другим юнитом или столицей
        if (GridManager.Instance.IsCellOccupied(cellPos))
        {
            Debug.LogWarning($"[CardManager] Клетка {cellPos} уже занята!");
            return false;
        }

        // 6. Проверяем префаб юнита
        if (cardData.unitPrefab == null)
        {
            Debug.LogError($"[CardManager] У карты '{cardData.cardName}' не назначен unitPrefab!");
            return false;
        }

        // 7. Списываем ману
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendMana(cardData.cost);
        }

        // 8. Спавним синего воина в центре клетки
        Vector3 spawnWorldPos = GridManager.Instance.GetCellCenterWorld(cellPos);
        spawnWorldPos.z = 0;

        GameObject spawnedUnitObj = Instantiate(cardData.unitPrefab, spawnWorldPos, Quaternion.identity);
        spawnedUnitObj.name = $"{cardData.cardName}_{cellPos.x}_{cellPos.y}";

        // Устанавливаем масштаб как у синего воина на сцене (0.3, 0.25, 1)
        spawnedUnitObj.transform.localScale = new Vector3(0.3f, 0.25f, 1f);

        // Убедимся, что на юните есть компонент Unit и коллайдер для выбора
        Unit unit = spawnedUnitObj.GetComponent<Unit>();
        if (unit == null)
            unit = spawnedUnitObj.AddComponent<Unit>();

        unit.isPlayerUnit = true; // Призванный воин — игровой синий
        unit.teamId = 1;
        unit.gridPosition = cellPos;

        // Назначаем слой Units, чтобы UnitSelectionController мог сразу выбирать нового юнита
        int unitLayer = LayerMask.NameToLayer("Units");
        if (unitLayer != -1)
        {
            spawnedUnitObj.layer = unitLayer;
        }

        SpriteRenderer sr = spawnedUnitObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white; // Чистый цвет без затемнения/красного оттенка
            sr.sortingLayerName = "Units";
            sr.sortingOrder = 5;
        }

        Collider2D col = spawnedUnitObj.GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D boxCol = spawnedUnitObj.AddComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                boxCol.size = new Vector2(0.8f, 0.8f);
            }
        }

        Debug.Log($"<color=cyan>[CardManager]</color> Карта '{cardData.cardName}' сыграна! Синий воин призван возле столицы на клетку {cellPos}. Потрачено маны: {cardData.cost}");

        // 9. Удаляем карту из руки
        Destroy(cardUI.gameObject);

        // 10. Обновляем веерную раскладку оставшихся карт
        if (CardFanLayout.Instance != null)
        {
            CardFanLayout.Instance.UpdateFanLayout();
        }

        return true;
    }
}