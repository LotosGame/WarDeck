using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Mana Settings")]
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private Button endTurnButton;

    [SerializeField] private int maxMana = 5;

    private int currentMana;
    private int turnCount = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        TMP_FontAsset cyrFont = CyrillicFontRuntimeFallback.GetCyrillicFont();
        if (manaText != null && cyrFont != null)
        {
            manaText.font = cyrFont;
        }

        if (endTurnButton != null)
        {
            TMP_Text btnText = endTurnButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null && cyrFont != null)
            {
                btnText.font = cyrFont;
            }
            endTurnButton.onClick.AddListener(EndTurn);
        }

        // 1-й ход: мана 1/5
        currentMana = Mathf.Min(turnCount, maxMana);
        UpdateManaUI();
    }

    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public int TurnCount => turnCount;

    public bool CanAfford(int amount) => currentMana >= amount;

    public bool SpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateManaUI();
            return true;
        }
        return false;
    }

    public void EndTurn()
    {
        turnCount++;

        // Каждый ход мана восполняется согласно номеру хода: 1/5, 2/5, 3/5, 4/5, 5/5
        // и не растет выше 5
        currentMana = Mathf.Min(turnCount, maxMana);
        UpdateManaUI();

        Debug.Log($"--- Ход {turnCount} начат! Мана: {currentMana}/{maxMana} ---");

        // Сбрасываем ход только для юнитов игрока
        foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit != null && unit.isPlayerUnit)
            {
                unit.ResetTurn();
            }
        }

        // Первый ход — ход осмотра.
        // На 2-й ход и далее каждые 3 хода (Ход 2, Ход 5, Ход 8, Ход 11...) предлагается добор 1 из 2 карт
        if (turnCount >= 2 && (turnCount - 2) % 3 == 0)
        {
            if (CardManager.Instance != null)
            {
                CardManager.Instance.TriggerCardChoice();
            }
        }
    }

    private void UpdateManaUI()
    {
        if (manaText != null)
        {
            manaText.text = $"Мана: {currentMana}/{maxMana}";
        }
    }
}