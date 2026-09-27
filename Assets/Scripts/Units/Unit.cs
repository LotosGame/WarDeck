using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Settings")]
    public string unitName = "Warrior";
    public int moveDistance = 2; // Дальность хода в клетках
    public float moveSpeed = 5f;

    [Header("Team & Control")]
    public bool isPlayerUnit = true; // Синий юнит = true (игровой), Красный = false (неигровой)
    public int teamId = 1;           // 1 = Player, 2 = Enemy

    [HideInInspector] public Vector3Int gridPosition; // Позиция на Tilemap
    [HideInInspector] public bool hasMoved = false;   // Ходил ли в этом ходу

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Красный воин (WarriorPlayer1 в сцене или со спрайтом red) — вражеский неигровой
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
    }

    private void Start()
    {
        if (GridManager.Instance != null && gridPosition == Vector3Int.zero)
        {
            gridPosition = GridManager.Instance.WorldToCell(transform.position);
        }
    }

    public void MoveTo(Vector3 targetWorldPosition, Vector3Int targetGridPosition)
    {
        gridPosition = targetGridPosition;
        hasMoved = true;

        // Визуально затемняем юнита, если он уже сходил
        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;

        StartCoroutine(AnimateMove(targetWorldPosition));
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

    // Сброс хода при нажатии кнопки «Конец хода»
    public void ResetTurn()
    {
        hasMoved = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }
}