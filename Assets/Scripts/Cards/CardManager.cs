using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardPrefab;

    [Header("Deck System")]
    [Tooltip("8 базовых видов карт (Воины, Лучники, Конница, Стрелы, Катапульта, Монах, Копейщики, Башня)")]
    [SerializeField] private List<CardData> availableCardTypes = new List<CardData>();

    [SerializeField] private int deckSize = 20;
    [SerializeField] private int startingHandSize = 2;

    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    public GameObject CardPrefab => cardPrefab;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;

    public GameObject GetDefaultUnitPrefab()
    {
        EnsureAvailableCardTypes();
        foreach (var c in availableCardTypes)
        {
            if (c != null && c.cardName == "Воины" && c.unitPrefab != null)
                return c.unitPrefab;
        }
        foreach (var c in availableCardTypes)
        {
            if (c != null && c.unitPrefab != null)
                return c.unitPrefab;
        }
        GameObject loaded = Resources.Load<GameObject>("Units/WarriorPrefab");
        if (loaded != null) return loaded;

        return null;
    }

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

        // Если в инспекторе не были назначены виды карт, автоматически находим/создаем 8 типов
        EnsureAvailableCardTypes();
    }

    private void Start()
    {
        // 1. Создаем колоду из 20 карт со случайным количеством каждого из 8 видов и перемешиваем
        BuildDeck();

        // 2. Раздаем стартовую руку из 2 карт (первый ход — ход осмотра)
        DealStartingHand(startingHandSize);

        // Первый ход — ход осмотра: добор 1 из 2 карт не предлагается на 1-м ходу.
        // Он будет предложен на 2-й ход и далее каждые 3 хода (Ход 2, Ход 5, Ход 8...).
    }

    private void EnsureAvailableCardTypes()
    {
        if (availableCardTypes == null)
            availableCardTypes = new List<CardData>();

        // 1. Ищем существующие CardData в проекте/памяти
        CardData[] loaded = Resources.FindObjectsOfTypeAll<CardData>();
        foreach (var card in loaded)
        {
            if (card != null && !availableCardTypes.Contains(card))
            {
                availableCardTypes.Add(card);
            }
        }

        // 2. Если каких-то из 8 обязательных видов нет, создаем их процедурно
        string[] names = { "Воины", "Лучники", "Конница", "Стрелы", "Катапульта", "Монах", "Копейщики", "Башня" };
        CardType[] types = { CardType.Unit, CardType.Unit, CardType.Unit, CardType.Spell, CardType.Unit, CardType.Unit, CardType.Unit, CardType.Building };
        int[] costs = { 1, 2, 3, 1, 4, 2, 2, 3 };
        Color[] colors = {
            new Color(0.22f, 0.42f, 0.78f, 1f), // Воины (Синий)
            new Color(0.25f, 0.65f, 0.35f, 1f), // Лучники (Зеленый)
            new Color(0.85f, 0.55f, 0.15f, 1f), // Конница (Оранжевый)
            new Color(0.85f, 0.25f, 0.25f, 1f), // Стрелы (Красный)
            new Color(0.45f, 0.4f, 0.35f, 1f),  // Катапульта (Темно-серый)
            new Color(0.15f, 0.7f, 0.7f, 1f),   // Монах (Бирюзовый)
            new Color(0.35f, 0.35f, 0.7f, 1f),  // Копейщики (Индиго)
            new Color(0.5f, 0.35f, 0.5f, 1f)    // Башня (Фиолетовый)
        };

        // Ищем базовый префаб юнита
        GameObject fallbackUnitPrefab = null;
        foreach (var c in availableCardTypes)
        {
            if (c != null && c.unitPrefab != null)
            {
                fallbackUnitPrefab = c.unitPrefab;
                break;
            }
        }

        for (int i = 0; i < names.Length; i++)
        {
            bool exists = false;
            foreach (var c in availableCardTypes)
            {
                if (c != null && c.cardName == names[i])
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                CardData newCard = ScriptableObject.CreateInstance<CardData>();
                newCard.name = $"Card_{names[i]}";
                newCard.cardName = names[i];
                newCard.cardType = types[i];
                newCard.cost = costs[i];
                newCard.backgroundColor = colors[i];
                newCard.unitPrefab = fallbackUnitPrefab;
                availableCardTypes.Add(newCard);
            }
        }
    }

    public void BuildDeck()
    {
        EnsureAvailableCardTypes();

        drawPile.Clear();
        discardPile.Clear();

        if (availableCardTypes.Count == 0)
        {
            Debug.LogError("[CardManager] Нет доступных видов карт для создания колоды!");
            return;
        }

        // Заполняем колоду ровно 20 картами, выбирая случайно из 8 доступных видов
        for (int i = 0; i < deckSize; i++)
        {
            int randomIndex = Random.Range(0, availableCardTypes.Count);
            drawPile.Add(availableCardTypes[randomIndex]);
        }

        // Перемешивание колоды (Fisher-Yates Shuffle)
        for (int i = 0; i < drawPile.Count; i++)
        {
            int rnd = Random.Range(i, drawPile.Count);
            CardData temp = drawPile[i];
            drawPile[i] = drawPile[rnd];
            drawPile[rnd] = temp;
        }

        Debug.Log($"<color=yellow>[CardManager]</color> Создана и перемешана колода из {drawPile.Count} карт (8 видов со случайным распределением).");
    }

    public void DealStartingHand(int count)
    {
        if (handPanel == null) return;

        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < count; i++)
        {
            DrawCardToHand();
        }

        Debug.Log($"<color=yellow>[CardManager]</color> Игроку выдана стартовая рука из {count} карт.");
    }

    public CardData DrawCardToHand()
    {
        if (drawPile.Count == 0)
        {
            // Если колода закончилась — замешиваем сброс
            if (discardPile.Count > 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();

                for (int i = 0; i < drawPile.Count; i++)
                {
                    int rnd = Random.Range(i, drawPile.Count);
                    CardData temp = drawPile[i];
                    drawPile[i] = drawPile[rnd];
                    drawPile[rnd] = temp;
                }
                Debug.Log("[CardManager] Сброс замешан обратно в колоду.");
            }
            else if (availableCardTypes.Count > 0)
            {
                drawPile.Add(availableCardTypes[Random.Range(0, availableCardTypes.Count)]);
            }
        }

        if (drawPile.Count == 0) return null;

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);

        AddCardToHand(drawnCard);
        return drawnCard;
    }

    public void AddCardToHand(CardData cardData)
    {
        if (cardData == null || handPanel == null) return;

        CreateCardUI(cardData);

        if (CardFanLayout.Instance != null)
        {
            CardFanLayout.Instance.UpdateFanLayout();
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
            cardUI.isDraggable = true;
        }
    }

    public void TriggerCardChoice()
    {
        // Берем 2 карты из колоды (или из доступного пула)
        CardData card1 = PullCardForChoice();
        CardData card2 = PullCardForChoice();

        if (card1 == null || card2 == null)
        {
            Debug.LogWarning("[CardManager] Недостаточно карт для предложения выбора.");
            return;
        }

        Debug.Log($"<color=cyan>[CardManager]</color> Предложен выбор из двух карт: [{card1.cardName}] и [{card2.cardName}].");

        if (CardChoiceUI.Instance == null)
        {
            GameObject choiceObj = new GameObject("CardChoiceSystem");
            choiceObj.AddComponent<CardChoiceUI>();
        }

        if (CardChoiceUI.Instance != null)
        {
            CardChoiceUI.Instance.ShowChoice(card1, card2, (chosen, rejected) =>
            {
                AddCardToHand(chosen);
                discardPile.Add(rejected);
                Debug.Log($"<color=green>[CardManager]</color> Игрок выбрал карту: '{chosen.cardName}'. Невыбранная '{rejected.cardName}' отправлена в сброс.");
            });
        }
        else
        {
            // Запасной вариант если UI еще не готов: добавляем первую в руку
            AddCardToHand(card1);
            discardPile.Add(card2);
        }
    }

    private CardData PullCardForChoice()
    {
        if (drawPile.Count > 0)
        {
            CardData c = drawPile[0];
            drawPile.RemoveAt(0);
            return c;
        }

        if (discardPile.Count > 0)
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            CardData c = drawPile[0];
            drawPile.RemoveAt(0);
            return c;
        }

        if (availableCardTypes.Count > 0)
        {
            return availableCardTypes[Random.Range(0, availableCardTypes.Count)];
        }

        return null;
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
        bool hasMana = GameManager.Instance != null && GameManager.Instance.CanAfford(cardUI.CardData.cost);

        bool isSpell = cardUI.CardData.cardType == CardType.Spell || cardUI.CardData.cardName.Contains("Стрелы");

        bool isValid;
        if (isSpell)
        {
            // Заклинание «Стрелы» можно применить по любой существующей клетке на поле
            isValid = hasTile && hasMana;
        }
        else
        {
            // Юниты и постройки ставятся только в зоне базы на свободную клетку
            bool inSpawnZone = GridManager.Instance.IsInPlayerSpawnZone(cellPos);
            bool isOccupied = GridManager.Instance.IsCellOccupied(cellPos);
            isValid = hasTile && inSpawnZone && !isOccupied && hasMana;
        }

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

        // 2. Проверяем наличие тайла на сетке
        if (!GridManager.Instance.HasTile(cellPos))
        {
            Debug.LogWarning($"[CardManager] Клетка {cellPos} находится за пределами игрового поля.");
            return false;
        }

        Vector3 targetWorldPos = GridManager.Instance.GetCellCenterWorld(cellPos);
        targetWorldPos.z = 0;

        // 3. ЗАКЛИНАНИЕ «СТРЕЛЫ» — не требует спавн-зоны, наносит урон и проигрывает эффект
        bool isArrowSpell = cardData.cardType == CardType.Spell || cardData.cardName.Contains("Стрелы");
        if (isArrowSpell)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SpendMana(cardData.cost);
            }

            // Запускаем визуальный эффект падающих стрел и вспышки попадания
            ArrowStormEffect.Spawn(targetWorldPos, () =>
            {
                // Наносим 4 урона всем врагам на клетке
                Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
                foreach (var u in units)
                {
                    if (u != null && !u.isPlayerUnit && u.gridPosition == cellPos)
                    {
                        u.TakeDamage(4);
                    }
                }

                // Урон вражеской столице, если попали по ней
                Capital enemyCap = GridManager.Instance.GetEnemyCapital();
                if (enemyCap != null && (enemyCap.gridPosition == cellPos || GridManager.Instance.WorldToCell(enemyCap.transform.position) == cellPos))
                {
                    enemyCap.TakeDamage(4);
                }
            });

            Debug.Log($"<color=orange>[Заклинание]</color> 'Стрелы' нанесли урон по клетке {cellPos}!");

            discardPile.Add(cardData);
            Destroy(cardUI.gameObject);

            if (CardFanLayout.Instance != null)
                CardFanLayout.Instance.UpdateFanLayout();

            return true;
        }

        // 4. ДЛЯ ЮНИТОВ И ПОСТРОЕК: проверяем зону спавна и занятость
        if (!GridManager.Instance.IsInPlayerSpawnZone(cellPos))
        {
            Debug.LogWarning($"[CardManager] Размещать отряды и постройки можно только возле своей столицы!");
            return false;
        }

        if (GridManager.Instance.IsCellOccupied(cellPos))
        {
            Debug.LogWarning($"[CardManager] Клетка {cellPos} уже занята!");
            return false;
        }

        if (cardData.unitPrefab == null)
        {
            Debug.LogError($"[CardManager] У карты '{cardData.cardName}' не назначен unitPrefab!");
            return false;
        }

        // 5. Списываем ману
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendMana(cardData.cost);
        }

        // 6. Спавним объект с учетом смещения ног на изометрическом тайле
        Vector3 spawnWorldPos = targetWorldPos + (GridManager.Instance != null ? GridManager.Instance.UnitVisualOffset : Vector3.zero);
        GameObject spawnedUnitObj = Instantiate(cardData.unitPrefab, spawnWorldPos, Quaternion.identity);
        spawnedUnitObj.name = $"{cardData.cardName}_{cellPos.x}_{cellPos.y}";

        Unit unit = spawnedUnitObj.GetComponent<Unit>();
        if (unit == null)
            unit = spawnedUnitObj.AddComponent<Unit>();

        unit.isPlayerUnit = true;
        unit.teamId = 1;
        unit.gridPosition = cellPos;

        int unitLayer = LayerMask.NameToLayer("Units");
        if (unitLayer != -1)
        {
            spawnedUnitObj.layer = unitLayer;
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

        // 7. Конфигурируем характеристики и визуал юнита в зависимости от карты
        Sprite customSprite = cardData.unitSprite;
        string cName = cardData.cardName;
        if (cName.Contains("Лучник"))
        {
            unit.SetupUnit(UnitType.Archer, "Лучники", 7, 2, 2, 2, new Color(0.65f, 1f, 0.75f, 1f), customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.3f, 0.25f, 1f);
        }
        else if (cName.Contains("Конниц"))
        {
            unit.SetupUnit(UnitType.Cavalry, "Конница", 12, 4, 1, 3, new Color(1f, 0.88f, 0.55f, 1f), customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.33f, 0.28f, 1f);
        }
        else if (cName.Contains("Копейщ"))
        {
            unit.SetupUnit(UnitType.Spearmen, "Копейщики", 12, 3, 1, 2, new Color(0.75f, 0.8f, 1f, 1f), customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.3f, 0.25f, 1f);
        }
        else if (cName.Contains("Катапульт"))
        {
            unit.SetupUnit(UnitType.Catapult, "Катапульта", 8, 6, 3, 1, new Color(0.85f, 0.7f, 0.55f, 1f), customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.36f, 0.3f, 1f);
        }
        else if (cName.Contains("Монах"))
        {
            unit.SetupUnit(UnitType.Monk, "Монах", 8, 1, 1, 2, new Color(0.6f, 0.95f, 1f, 1f), customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.28f, 0.24f, 1f);
        }
        else if (cName.Contains("Башн"))
        {
            // Башня: стационарное оборонительное сооружение (moveDistance = 0!)
            unit.SetupUnit(UnitType.Tower, "Башня", 15, 3, 2, 0, new Color(0.85f, 0.85f, 0.95f, 1f), customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.35f, 0.38f, 1f);
        }
        else
        {
            // По умолчанию — Воины
            unit.SetupUnit(UnitType.Warrior, "Воины", 10, 3, 1, 2, Color.white, customSprite);
            spawnedUnitObj.transform.localScale = new Vector3(0.3f, 0.25f, 1f);
        }

        Debug.Log($"<color=cyan>[CardManager]</color> Карта '{cardData.cardName}' сыграна! Размещен {unit.unitName} (HP: {unit.maxHealth}, Атака: {unit.attackPower}, Ход: {unit.moveDistance}) на клетку {cellPos}.");

        // 8. Отправляем карту в сброс и удаляем из руки
        discardPile.Add(cardData);
        Destroy(cardUI.gameObject);

        if (CardFanLayout.Instance != null)
        {
            CardFanLayout.Instance.UpdateFanLayout();
        }

        return true;
    }
}