using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UnitSelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private LayerMask unitLayer;

    private Unit selectedUnit;

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleClick();
        }
    }

    private void HandleClick()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // 1. Выбор юнита
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos2D, unitLayer);

        if (hit != null)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null && !unit.hasMoved)
            {
                selectedUnit = unit;
                // Инициализируем стартовую позицию на сетке
                selectedUnit.gridPosition = tilemap.WorldToCell(selectedUnit.transform.position);
                Debug.Log($"Выбран юнит: {selectedUnit.unitName}");
                return;
            }
        }

        // 2. Перемещение юнита с проверкой дистанции
        if (selectedUnit != null)
        {
            Vector3Int targetCellPos = tilemap.WorldToCell(mouseWorldPos2D);

            if (tilemap.HasTile(targetCellPos))
            {
                // Считаем расстояние по сетке (Манхэттенская дистанция для изометрии)
                int distance = Mathf.Abs(targetCellPos.x - selectedUnit.gridPosition.x) +
                               Mathf.Abs(targetCellPos.y - selectedUnit.gridPosition.y);

                if (distance <= selectedUnit.moveDistance)
                {
                    Vector3 targetWorldPos = tilemap.GetCellCenterWorld(targetCellPos);
                    selectedUnit.MoveTo(targetWorldPos, targetCellPos);
                    selectedUnit = null;
                }
                else
                {
                    Debug.LogWarning($"Слишком далеко! Дистанция: {distance}, Максимум: {selectedUnit.moveDistance}");
                }
            }
        }
    }
}