using UnityEngine;
using UnityEngine.InputSystem; // Добавляем пространство имен

public class CardPlayer : MonoBehaviour
{
    [SerializeField] private CardData activeCard;

    private void Update()
    {
        // Проверка клика ЛКМ через новый Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                TileData tile = hit.collider.GetComponent<TileData>();

                if (tile != null && !tile.isOccupied && activeCard != null && activeCard.unitPrefab != null)
                {
                    Instantiate(activeCard.unitPrefab, tile.transform.position, Quaternion.identity);
                    tile.isOccupied = true;
                    Debug.Log($"Юнит {activeCard.cardName} призван на клетку [{tile.x}, {tile.y}]");
                }
            }
        }
    }
}