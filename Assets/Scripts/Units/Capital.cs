using UnityEngine;

public class Capital : MonoBehaviour
{
    [Header("Capital Info")]
    public string capitalName = "Capital";
    public int teamId = 1; // 1 = Player, 2 = Enemy

    [Header("Stats")]
    public int maxHealth = 20;
    public int currentHealth = 20;

    [HideInInspector] public Vector3Int gridPosition;

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
