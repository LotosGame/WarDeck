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

    private void Start()
    {
        if (tilemap == null && GridManager.Instance != null)
        {
            tilemap = GridManager.Instance.Tilemap;
        }
    }

    private void Update()
    {
        // Во время хода противника управление заблокировано
        if (GameManager.Instance != null && !GameManager.Instance.IsPlayerTurn)
        {
            if (selectedUnit != null)
            {
                selectedUnit = null;
                if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
            }
            return;
        }

        // Сброс выбора по ПКМ
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (selectedUnit != null)
            {
                Debug.Log($"Снят выбор с [{selectedUnit.unitName}]");
                selectedUnit = null;
                if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
            }
        }

        // Клик ЛКМ
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (tilemap == null && GridManager.Instance != null)
        {
            tilemap = GridManager.Instance.Tilemap;
        }

        if (tilemap == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        Vector3Int targetCellPos = tilemap.WorldToCell(mouseWorldPos2D);

        // Проверяем клик по юниту через OverlapPoint или через сетку
        Unit clickedUnit = null;
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos2D, unitLayer);
        if (hit != null)
        {
            clickedUnit = hit.GetComponent<Unit>();
        }
        if (clickedUnit == null && GridManager.Instance != null)
        {
            clickedUnit = GridManager.Instance.GetUnitAt(targetCellPos);
        }

        // СЛУЧАЙ 1: Юнит еще не выбран
        if (selectedUnit == null)
        {
            if (clickedUnit != null)
            {
                if (!clickedUnit.isPlayerUnit)
                {
                    Debug.LogWarning($"[{clickedUnit.unitName}] — это вражеский красный юнит! Вы можете управлять только своими синими войсками.");
                    return;
                }

                if (!clickedUnit.hasMoved)
                {
                    selectedUnit = clickedUnit;
                    selectedUnit.gridPosition = tilemap.WorldToCell(selectedUnit.transform.position);
                    Debug.Log($"<color=cyan>[Выбор]</color> Выбран игровой юнит: {selectedUnit.unitName}");
                    if (GridManager.Instance != null) GridManager.Instance.ShowMoveRange(selectedUnit);
                }
                else
                {
                    Debug.Log($"[{clickedUnit.unitName}] уже походил в этом раунде.");
                }
            }
            return;
        }

        // СЛУЧАЙ 2: Юнит уже выбран
        // 2.1 Переключение на другого своего юнита ИЛИ исцеление Монахом
        if (clickedUnit != null && clickedUnit.isPlayerUnit)
        {
            if (clickedUnit == selectedUnit)
            {
                // Кликнули по тому же юниту — отменяем выбор
                selectedUnit = null;
                if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
                return;
            }

            // Способность Монаха: исцеление раненого союзника
            if (selectedUnit.unitType == UnitType.Monk && clickedUnit.currentHealth < clickedUnit.maxHealth)
            {
                int healDist = Mathf.Abs(clickedUnit.gridPosition.x - selectedUnit.gridPosition.x) +
                               Mathf.Abs(clickedUnit.gridPosition.y - selectedUnit.gridPosition.y);
                if (healDist <= selectedUnit.attackRange)
                {
                    selectedUnit.HealTarget(clickedUnit, 4);
                    selectedUnit = null;
                    if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
                    return;
                }
            }

            if (!clickedUnit.hasMoved)
            {
                selectedUnit = clickedUnit;
                selectedUnit.gridPosition = tilemap.WorldToCell(selectedUnit.transform.position);
                Debug.Log($"<color=cyan>[Выбор]</color> Переключен на: {selectedUnit.unitName}");
                if (GridManager.Instance != null) GridManager.Instance.ShowMoveRange(selectedUnit);
                return;
            }
        }

        // 2.2 Атака вражеского юнита
        if (clickedUnit != null && !clickedUnit.isPlayerUnit)
        {
            int attackDist = Mathf.Abs(clickedUnit.gridPosition.x - selectedUnit.gridPosition.x) +
                             Mathf.Abs(clickedUnit.gridPosition.y - selectedUnit.gridPosition.y);

            if (attackDist <= selectedUnit.attackRange)
            {
                selectedUnit.Attack(clickedUnit);
                selectedUnit = null;
                if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
                return;
            }
            else
            {
                Debug.LogWarning($"Враг слишком далеко для атаки! Дистанция: {attackDist}, радиус атаки: {selectedUnit.attackRange}");
                return;
            }
        }

        // 2.3 Атака вражеской столицы
        Capital enemyCap = GridManager.Instance != null ? GridManager.Instance.GetEnemyCapital() : null;
        if (enemyCap != null && targetCellPos == enemyCap.gridPosition)
        {
            int capDist = Mathf.Abs(targetCellPos.x - selectedUnit.gridPosition.x) +
                          Mathf.Abs(targetCellPos.y - selectedUnit.gridPosition.y);

            if (capDist <= selectedUnit.attackRange)
            {
                selectedUnit.Attack(enemyCap);
                selectedUnit = null;
                if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
                return;
            }
            else
            {
                Debug.LogWarning($"Вражеская столица слишком далеко для атаки! Дистанция: {capDist}");
                return;
            }
        }

        // 2.4 Перемещение на свободную клетку
        if (tilemap.HasTile(targetCellPos))
        {
            if (selectedUnit.moveDistance <= 0)
            {
                Debug.LogWarning($"[{selectedUnit.unitName}] является стационарным сооружением и не может двигаться!");
                return;
            }

            if (GridManager.Instance != null && GridManager.Instance.IsCellOccupied(targetCellPos))
            {
                Debug.LogWarning($"Клетка {targetCellPos} уже занята!");
                return;
            }

            int distance = Mathf.Abs(targetCellPos.x - selectedUnit.gridPosition.x) +
                           Mathf.Abs(targetCellPos.y - selectedUnit.gridPosition.y);

            if (distance <= selectedUnit.moveDistance)
            {
                Vector3 targetWorldPos = tilemap.GetCellCenterWorld(targetCellPos);
                targetWorldPos.z = 0;
                selectedUnit.MoveTo(targetWorldPos, targetCellPos);
                selectedUnit = null;
                if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
            }
            else
            {
                Debug.LogWarning($"Слишком далеко! Дистанция: {distance}, Максимум: {selectedUnit.moveDistance}");
            }
        }
        else
        {
            // Клик за пределами поля — снимаем выбор
            selectedUnit = null;
            if (GridManager.Instance != null) GridManager.Instance.HideMoveRange();
        }
    }
}