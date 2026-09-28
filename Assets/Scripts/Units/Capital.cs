using System.Collections;
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

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // CapitalPlayer2 (синяя столица) принадлежит игроку
        if (gameObject.name.Contains("Player2") || (spriteRenderer != null && spriteRenderer.color.b > spriteRenderer.color.r))
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

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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
        StartCoroutine(DamageFlash());
        Debug.Log($"<color={(isPlayerCapital ? "blue" : "red")}>[{capitalName}]</color> получил {amount} урона! Осталось HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"<color=yellow>[{capitalName}] уничтожена!</color>");
            if (isPlayerCapital)
            {
                Debug.Log("<color=red>=== ПОРАЖЕНИЕ! Ваша столица была разрушена! ===</color>");
            }
            else
            {
                Debug.Log("<color=green>=== ПОБЕДА! Вражеская столица разрушена! ===</color>");
            }
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            if (this != null && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}
