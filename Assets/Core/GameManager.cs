using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Mana Settings")]
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private Button endTurnButton;

    [SerializeField] private int maxMana = 10;
    [SerializeField] private int manaPerTurn = 1;

    private int currentMaxMana = 1;
    private int currentMana;
    private int turnCount = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentMana = currentMaxMana;
        UpdateManaUI();

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(EndTurn);
        }
    }

    public int CurrentMana => currentMana;
    public int CurrentMaxMana => currentMaxMana;
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

        // Увеличиваем лимит маны каждый ход вплоть до maxMana
        if (currentMaxMana < maxMana)
        {
            currentMaxMana += manaPerTurn;
        }

        currentMana = currentMaxMana;
        UpdateManaUI();

        Debug.Log($"--- Ход {turnCount} начат! Мана восполнена: {currentMana}/{currentMaxMana} ---");

        // В Unity 6 вызов FindObjectsByType<Unit>() работает без параметров
        foreach (var unit in FindObjectsByType<Unit>())
        {
            unit.ResetTurn();
        }

        // Здесь в будущем вызываем логику хода ИИ / второго игрока или добор карт
    }

    private void UpdateManaUI()
    {
        if (manaText != null)
        {
            manaText.text = $"Мана: {currentMana}/{currentMaxMana}";
        }
    }
}