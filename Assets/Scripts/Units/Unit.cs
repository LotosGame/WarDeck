using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UnitType
{
    Warrior,
    Archer,
    Cavalry,
    Spearmen,
    Catapult,
    Monk,
    Tower
}

public class Unit : MonoBehaviour
{
    [Header("Unit Profile")]
    public string unitName = "Warrior";
    public UnitType unitType = UnitType.Warrior;

    [Header("Combat Stats")]
    public int maxHealth = 10;
    public int currentHealth = 10;
    public int attackPower = 3;
    public int attackRange = 1;

    [Header("Movement")]
    public int moveDistance = 2; // 0 = стационарный (Башня)
    public float moveSpeed = 5f;

    [Header("Team & Control")]
    public bool isPlayerUnit = true; // true = игрок (синий), false = противник (красный)
    public int teamId = 1;           // 1 = Player, 2 = Enemy

    [HideInInspector] public Vector3Int gridPosition;
    [HideInInspector] public bool hasMoved = false;
    [HideInInspector] public bool hasAttacked = false;

    [Header("Visual Alignment")]
    [Tooltip("Смещение по Y для точной посадки ног юнита в центр изометрической клетки")]
    public float visualYOffset = 0.18f;

    private SpriteRenderer spriteRenderer;

    // UI элементы полоски здоровья
    private Canvas healthBarCanvas;
    private Image healthBarFill;
    private Image healthBarBg;
    private TextMeshProUGUI unitBadgeText;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Определение стороны по имени или цвету спрайта
        if (gameObject.name.Contains("Player1") || (spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name.Contains("red")))
        {
            isPlayerUnit = false;
            teamId = 2;
            unitName = "Enemy Warrior (Red)";
        }
        else
        {
            isPlayerUnit = true;
            teamId = 1;
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (GridManager.Instance != null)
        {
            if (gridPosition == Vector3Int.zero)
            {
                gridPosition = GridManager.Instance.WorldToCell(transform.position - new Vector3(0f, visualYOffset, 0f));
            }

            Vector3 center = GridManager.Instance.GetCellCenterWorld(gridPosition);
            transform.position = new Vector3(center.x, center.y + visualYOffset, 0f);
        }

        CreateHealthBar();
        UpdateHealthBar();
    }

    /// <summary>
    /// Создает World Space Canvas над головой юнита с полоской HP и иконкой/значком класса
    /// </summary>
    private void CreateHealthBar()
    {
        // Проверяем, нет ли уже созданного канваса
        if (healthBarCanvas != null) return;

        GameObject canvasObj = new GameObject("HealthBar_Canvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, 0.85f, 0);

        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.sortingLayerName = "Units";
        healthBarCanvas.sortingOrder = 25;

        RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(80, 20);

        // Компенсируем немасштабность родителя, чтобы полоска и текст не сплющивались
        float pX = Mathf.Abs(transform.localScale.x) > 0.001f ? Mathf.Abs(transform.localScale.x) : 0.3f;
        float pY = Mathf.Abs(transform.localScale.y) > 0.001f ? Mathf.Abs(transform.localScale.y) : 0.25f;
        canvasRt.localScale = new Vector3(0.006f / pX, 0.006f / pY, 1f);

        // 1. Фон полоски HP
        GameObject bgObj = new GameObject("HP_Bg");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 0f);
        bgRt.anchorMax = new Vector2(0.5f, 0f);
        bgRt.pivot = new Vector2(0.5f, 0f);
        bgRt.sizeDelta = new Vector2(70, 7);

        healthBarBg = bgObj.AddComponent<Image>();
        healthBarBg.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);

        // 2. Заливка полоски HP
        GameObject fillObj = new GameObject("HP_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(1, 1);
        fillRt.offsetMax = new Vector2(-1, -1);

        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = 0;
        healthBarFill.color = isPlayerUnit ? new Color(0.2f, 0.85f, 0.3f, 1f) : new Color(0.9f, 0.25f, 0.25f, 1f);

        // 3. Название класса над полоской HP
        GameObject badgeObj = new GameObject("UnitBadge");
        badgeObj.transform.SetParent(canvasObj.transform, false);
        RectTransform badgeRt = badgeObj.AddComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0.5f, 1f);
        badgeRt.anchorMax = new Vector2(0.5f, 1f);
        badgeRt.pivot = new Vector2(0.5f, 0f);
        badgeRt.sizeDelta = new Vector2(100, 20);
        badgeRt.anchoredPosition = new Vector2(0, 1);

        unitBadgeText = badgeObj.AddComponent<TextMeshProUGUI>();
        unitBadgeText.font = CyrillicFontRuntimeFallback.GetCyrillicFont();
        unitBadgeText.fontSize = 13;
        unitBadgeText.fontStyle = FontStyles.Bold;
        unitBadgeText.alignment = TextAlignmentOptions.Center;
        unitBadgeText.color = isPlayerUnit ? new Color(0.9f, 0.95f, 1f, 1f) : new Color(1f, 0.6f, 0.6f, 1f);
        unitBadgeText.text = GetUnitBadgeString();
    }

    public string GetUnitBadgeString()
    {
        if (!isPlayerUnit) return "Враг";

        switch (unitType)
        {
            case UnitType.Archer: return "Лучник";
            case UnitType.Cavalry: return "Конница";
            case UnitType.Spearmen: return "Копейщик";
            case UnitType.Catapult: return "Катапульта";
            case UnitType.Monk: return "Монах";
            case UnitType.Tower: return "Башня";
            default: return "Воин";
        }
    }

    public void SetupUnit(UnitType type, string name, int hp, int power, int range, int moveDist, Color tintColor, Sprite customSprite = null)
    {
        unitType = type;
        unitName = name;
        maxHealth = hp;
        currentHealth = hp;
        attackPower = power;
        attackRange = range;
        moveDistance = moveDist;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            if (customSprite != null)
            {
                spriteRenderer.sprite = customSprite;
                spriteRenderer.color = Color.white; // Чистый цвет для отображения кастомного спрайта
            }
            else
            {
                spriteRenderer.color = tintColor;
            }
        }

        if (unitBadgeText != null)
        {
            unitBadgeText.text = GetUnitBadgeString();
        }

        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();

        Debug.Log($"<color=red>[Урон]</color> {unitName} получил {damage} урона. Здоровье: {currentHealth}/{maxHealth}");

        // Вспышка красным цветом при получении урона
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            Color orig = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.12f);
            spriteRenderer.color = orig;
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
        Debug.Log($"<color=green>[Лечение]</color> {unitName} исцелен на {amount}. Здоровье: {currentHealth}/{maxHealth}");
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float fillPct = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            healthBarFill.fillAmount = fillPct;

            // Цвет HP меняется от зеленого к желтому и красному при уменьшении
            if (isPlayerUnit)
            {
                if (fillPct > 0.5f)
                    healthBarFill.color = Color.Lerp(Color.yellow, new Color(0.2f, 0.85f, 0.3f, 1f), (fillPct - 0.5f) * 2f);
                else
                    healthBarFill.color = Color.Lerp(Color.red, Color.yellow, fillPct * 2f);
            }
        }
    }

    public void Attack(Unit target)
    {
        if (target == null) return;
        hasMoved = true;
        hasAttacked = true;
        SetDimmed(true);
        Debug.Log($"<color=orange>[Атака]</color> {unitName} атакует {target.unitName} на {attackPower} урона!");
        target.TakeDamage(attackPower);
    }

    public void Attack(Capital capital)
    {
        if (capital == null) return;
        hasMoved = true;
        hasAttacked = true;
        SetDimmed(true);
        Debug.Log($"<color=orange>[Атака]</color> {unitName} атакует {capital.capitalName} на {attackPower} урона!");
        capital.TakeDamage(attackPower);
    }

    public void HealTarget(Unit target, int amount)
    {
        if (target == null) return;
        hasMoved = true;
        hasAttacked = true;
        SetDimmed(true);
        Debug.Log($"<color=green>[Исцеление]</color> {unitName} исцеляет {target.unitName} на {amount} HP!");
        target.Heal(amount);
    }

    public void SetDimmed(bool dimmed)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = dimmed ? new Color(0.55f, 0.55f, 0.55f, 1f) : Color.white;
        }
    }

    public void MoveTo(Vector3 targetWorldPosition, Vector3Int targetGridPosition)
    {
        if (moveDistance <= 0)
        {
            Debug.LogWarning($"[{unitName}] не может перемещаться!");
            return;
        }

        gridPosition = targetGridPosition;
        hasMoved = true;
        SetDimmed(true);

        Vector3 targetPos = targetWorldPosition;
        if (GridManager.Instance != null)
        {
            Vector3 center = GridManager.Instance.GetCellCenterWorld(targetGridPosition);
            targetPos = new Vector3(center.x, center.y + visualYOffset, 0f);
        }
        else
        {
            targetPos.y += visualYOffset;
        }

        StartCoroutine(AnimateMove(targetPos));
    }

    private IEnumerator AnimateMove(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }

    public void ResetTurn()
    {
        hasMoved = false;
        hasAttacked = false;
        SetDimmed(false);
    }

    public void Die()
    {
        Debug.Log($"<color=red>[Смерть]</color> {unitName} погиб!");
        Destroy(gameObject);
    }
}