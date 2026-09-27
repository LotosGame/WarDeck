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
        // 1-й ход: мана 1/5
        currentMana = Mathf.Min(turnCount, maxMana);
        UpdateManaUI();

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(EndTurn);
        }
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
    }

    private void UpdateManaUI()
    {
        if (manaText != null)
        {
            manaText.text = $"Мана: {currentMana}/{maxMana}";
        }
    }
}