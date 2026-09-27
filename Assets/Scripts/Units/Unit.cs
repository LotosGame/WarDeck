using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Settings")]
    public string unitName = "Warrior";
    public int moveDistance = 2; // Дальность хода в клетках
    public float moveSpeed = 5f;

    [HideInInspector] public Vector3Int gridPosition; // Позиция на Tilemap
    [HideInInspector] public bool hasMoved = false;   // Ходил ли в этом ходу

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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