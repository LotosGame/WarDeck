using UnityEngine;

public class Capital : MonoBehaviour
{
    [Header("Capital Info")]
    public string capitalName = "Capital";
    public bool isPlayerCapital = false; // Синяя столица = true, Красная = false
    public int teamId = 1;               // 1 = Player (Blue), 2 = Enemy (Red)

    [Header("Stats")]
    public int maxHealth = 20;
    public int currentHealth = 20;

    [HideInInspector] public Vector3Int gridPosition;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // CapitalPlayer2 (синяя столица) принадлежит игроку
        if (gameObject.name.Contains("Player2") || (sr != null && sr.color.b > sr.color.r))
        {
            isPlayerCapital = true;
            teamId = 1;
            capitalName = "Player Capital (Blue)";
        }
        else
        {
            isPlayerCapital = false;
            teamId = 2;
            capitalName = "Enemy Capital (Red)";
        }
    }

    private void Start()
    {
        if (GridManager.Instance != null)
        {
            gridPosition = GridManager.Instance.WorldToCell(transform.position);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"[{capitalName}] получил {amount} урона! Осталось HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"[{capitalName}] уничтожена!");
        }
    }
}
