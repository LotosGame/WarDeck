using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance { get; private set; }

    [Header("Enemy AI Settings")]
    [SerializeField] private GameObject enemyUnitPrefab;
    [SerializeField] private Sprite enemyUnitSprite;
    [SerializeField] private float stepDelay = 0.5f;
    [SerializeField] [Tooltip("Максимум вражеских юнитов на поле одновременно")]
    private int maxEnemyUnits = 6;

    private int enemyMana = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CacheEnemySprite();
    }

    public void CacheEnemySprite()
    {
        if (enemyUnitSprite != null) return;

        // Ищем существующего красного воина на сцене (WarriorPlayer1)
        GameObject redObj = GameObject.Find("WarriorPlayer1");
        if (redObj != null)
        {
            SpriteRenderer sr = redObj.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                enemyUnitSprite = sr.sprite;
                return;
            }
        }

        // Ищем среди других юнитов
        foreach (var u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (u != null && !u.isPlayerUnit)
            {
                SpriteRenderer sr = u.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    enemyUnitSprite = sr.sprite;
                    return;
                }
            }
        }
    }

    public void ExecuteTurn(Action onFinished)
    {
        StartCoroutine(TurnRoutine(onFinished));
    }

    private IEnumerator TurnRoutine(Action onFinished)
    {
        yield return new WaitForSeconds(stepDelay);

        // 1. Начисляем ману врагу (по номеру раунда, как у игрока)
        int turn = GameManager.Instance != null ? GameManager.Instance.TurnCount : 1;
        int maxMana = GameManager.Instance != null ? GameManager.Instance.MaxMana : 5;
        enemyMana = Mathf.Min(turn, maxMana);

        Debug.Log($"<color=red>[EnemyAI]</color> Начало хода ИИ! Мана врага: {enemyMana}/{maxMana}");

        // 2. Фаза розыгрыша / призыва войск
        yield return StartCoroutine(SpawnPhaseRoutine());

        yield return new WaitForSeconds(stepDelay);

        // 3. Фаза передвижения и атак
        yield return StartCoroutine(MoveAndAttackRoutine());

        yield return new WaitForSeconds(stepDelay);

        Debug.Log($"<color=red>[EnemyAI]</color> Ход ИИ завершен. Передача хода игроку.");
        onFinished?.Invoke();
    }

    private IEnumerator SpawnPhaseRoutine()
    {
        if (GridManager.Instance == null) yield break;

        Capital enemyCapital = GridManager.Instance.GetEnemyCapital();
        if (enemyCapital == null) yield break;

        Capital playerCapital = GridManager.Instance.GetPlayerCapital();
        Vector3Int targetPos = playerCapital != null ? playerCapital.gridPosition : Vector3Int.zero;

        int warriorCost = 1;

        while (enemyMana >= warriorCost)
        {
            // Проверка лимита юнитов на поле
            int currentEnemyCount = CountEnemyUnits();
            if (currentEnemyCount >= maxEnemyUnits)
            {
                Debug.Log($"<color=red>[EnemyAI]</color> Достигнут лимит юнитов ({currentEnemyCount}/{maxEnemyUnits}). Спавн остановлен.");
                break;
            }

            List<Vector3Int> validSpawnTiles = GetAvailableSpawnTiles();
            if (validSpawnTiles.Count == 0)
                break; // Нет свободных клеток

            // Сортируем клетки: выбираем те, что ближе к базе игрока (наступление вперед)
            validSpawnTiles.Sort((a, b) =>
            {
                int distA = Mathf.Abs(a.x - targetPos.x) + Mathf.Abs(a.y - targetPos.y);
                int distB = Mathf.Abs(b.x - targetPos.x) + Mathf.Abs(b.y - targetPos.y);
                return distA.CompareTo(distB);
            });

            Vector3Int chosenTile = validSpawnTiles[0];
            SpawnEnemyWarrior(chosenTile);
            enemyMana -= warriorCost;

            yield return new WaitForSeconds(0.6f);
        }
    }

    private int CountEnemyUnits()
    {
        int count = 0;
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit != null && !unit.isPlayerUnit)
                count++;
        }
        return count;
    }

    private List<Vector3Int> GetAvailableSpawnTiles()
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        if (GridManager.Instance == null) return tiles;

        Capital cap = GridManager.Instance.GetEnemyCapital();
        if (cap == null) return tiles;

        Vector3Int capPos = cap.gridPosition;
        int radius = GridManager.Instance.PlayerSpawnRadius;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int cell = capPos + new Vector3Int(x, y, 0);
                if (GridManager.Instance.IsInEnemySpawnZone(cell) &&
                    GridManager.Instance.HasTile(cell) &&
                    !GridManager.Instance.IsCellOccupied(cell))
                {
                    tiles.Add(cell);
                }
            }
        }
        return tiles;
    }

    private void SpawnEnemyWarrior(Vector3Int cellPos)
    {
        if (GridManager.Instance == null) return;

        GameObject prefab = enemyUnitPrefab;
        if (prefab == null && CardManager.Instance != null)
        {
            prefab = CardManager.Instance.GetDefaultUnitPrefab();
        }

        if (prefab == null)
        {
            // Попробуем взять шаблон из существующего на сцене воина
            GameObject sceneWarrior = GameObject.Find("WarriorPlayer1");
            if (sceneWarrior != null)
                prefab = sceneWarrior;
        }

        if (prefab == null)
        {
            Debug.LogError("[EnemyAI] Не удалось найти префаб для спавна вражеского юнита!");
            return;
        }

        CacheEnemySprite();

        Vector3 worldPos = GridManager.Instance.GetCellCenterWorld(cellPos);
        worldPos.z = 0;

        GameObject spawned = Instantiate(prefab, worldPos, Quaternion.identity);
        spawned.name = $"EnemyWarrior_{cellPos.x}_{cellPos.y}";
        spawned.transform.localScale = new Vector3(0.3f, 0.25f, 1f);

        Unit unit = spawned.GetComponent<Unit>();
        if (unit == null)
            unit = spawned.AddComponent<Unit>();

        unit.isPlayerUnit = false;
        unit.teamId = 2;
        unit.unitName = "Enemy Warrior (Red)";
        unit.gridPosition = cellPos;
        unit.hasMoved = false;

        int unitLayer = LayerMask.NameToLayer("Units");
        if (unitLayer != -1)
        {
            spawned.layer = unitLayer;
        }

        SpriteRenderer sr = spawned.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (enemyUnitSprite != null)
            {
                sr.sprite = enemyUnitSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.color = new Color(1f, 0.35f, 0.35f, 1f);
            }
            sr.sortingLayerName = "Units";
            sr.sortingOrder = 5;
        }

        Collider2D col = spawned.GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D boxCol = spawned.AddComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                boxCol.size = new Vector2(0.8f, 0.8f);
            }
        }

        Debug.Log($"<color=red>[EnemyAI]</color> Призван {unit.unitName} на клетку {cellPos}!");
    }

    private IEnumerator MoveAndAttackRoutine()
    {
        if (GridManager.Instance == null) yield break;

        // Собираем всех активных вражеских юнитов
        List<Unit> enemyUnits = new List<Unit>();
        foreach (var u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (u != null && !u.isPlayerUnit)
            {
                enemyUnits.Add(u);
            }
        }

        Capital playerCapital = GridManager.Instance.GetPlayerCapital();

        foreach (var unit in enemyUnits)
        {
            if (unit == null || unit.hasMoved) continue;

            // 1. Проверяем, есть ли рядом цель для атаки ДО перемещения
            TargetCandidate adjacentTarget = FindAdjacentTarget(unit);
            if (adjacentTarget != null)
            {
                AttackTarget(unit, adjacentTarget);
                yield return new WaitForSeconds(0.6f);
                continue;
            }

            // 2. Если цель рядом отсутствует, определяем стратегическую цель (база или юнит игрока)
            Vector3Int targetPos = GetStrategicTargetPos(unit, playerCapital);

            // 3. Ищем лучшую клетку для продвижения к цели
            Vector3Int? nextTile = FindBestMoveTile(unit, targetPos);
            if (nextTile.HasValue && nextTile.Value != unit.gridPosition)
            {
                Vector3Int targetCell = nextTile.Value;
                Vector3 targetWorld = GridManager.Instance.GetCellCenterWorld(targetCell);
                targetWorld.z = 0;

                unit.MoveTo(targetWorld, targetCell);
                yield return new WaitForSeconds(0.6f);

                // 4. После перемещения проверяем, можем ли атаковать цель
                TargetCandidate postMoveTarget = FindAdjacentTarget(unit);
                if (postMoveTarget != null)
                {
                    AttackTarget(unit, postMoveTarget);
                    yield return new WaitForSeconds(0.6f);
                }
            }
            else
            {
                // Не может сделать ход
                unit.hasMoved = true;
                unit.SetDimmed(true);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private class TargetCandidate
    {
        public Unit unit;
        public Capital capital;
    }

    private TargetCandidate FindAdjacentTarget(Unit attacker)
    {
        if (attacker == null || GridManager.Instance == null) return null;

        Vector3Int pos = attacker.gridPosition;
        Vector3Int[] orthogonalDirections = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        // 1. Приоритет: юнит игрока
        foreach (var dir in orthogonalDirections)
        {
            Vector3Int checkPos = pos + dir;
            Unit u = GridManager.Instance.GetUnitAt(checkPos);
            if (u != null && u.isPlayerUnit)
            {
                return new TargetCandidate { unit = u };
            }
        }

        // 2. Приоритет: база игрока
        foreach (var dir in orthogonalDirections)
        {
            Vector3Int checkPos = pos + dir;
            Capital c = GridManager.Instance.GetCapitalAt(checkPos);
            if (c != null && c.isPlayerCapital)
            {
                return new TargetCandidate { capital = c };
            }
        }

        return null;
    }

    private void AttackTarget(Unit attacker, TargetCandidate target)
    {
        if (target.unit != null)
        {
            attacker.Attack(target.unit);
        }
        else if (target.capital != null)
        {
            attacker.Attack(target.capital);
        }
    }

    private Vector3Int GetStrategicTargetPos(Unit unit, Capital playerCapital)
    {
        Vector3Int capitalPos = playerCapital != null ? playerCapital.gridPosition : Vector3Int.zero;

        // Ищем ближайшего юнита игрока
        Unit closestPlayerUnit = null;
        int minUnitDistance = int.MaxValue;

        foreach (var u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (u != null && u.isPlayerUnit)
            {
                int dist = Mathf.Abs(u.gridPosition.x - unit.gridPosition.x) +
                           Mathf.Abs(u.gridPosition.y - unit.gridPosition.y);
                if (dist < minUnitDistance)
                {
                    minUnitDistance = dist;
                    closestPlayerUnit = u;
                }
            }
        }

        // Если есть юнит игрока в радиусе 4 клеток, направляемся к нему для перехвата
        if (closestPlayerUnit != null && minUnitDistance <= 4)
        {
            return closestPlayerUnit.gridPosition;
        }

        // Иначе двигаемся к столице игрока
        return capitalPos;
    }

    private Vector3Int? FindBestMoveTile(Unit unit, Vector3Int targetPos)
    {
        if (GridManager.Instance == null) return null;

        Vector3Int currentPos = unit.gridPosition;
        int currentDistance = Mathf.Abs(currentPos.x - targetPos.x) + Mathf.Abs(currentPos.y - targetPos.y);

        Vector3Int bestTile = currentPos;
        int bestDistance = currentDistance;

        int moveRange = unit.moveDistance;

        for (int dx = -moveRange; dx <= moveRange; dx++)
        {
            for (int dy = -moveRange; dy <= moveRange; dy++)
            {
                int stepDist = Mathf.Abs(dx) + Mathf.Abs(dy);
                if (stepDist == 0 || stepDist > moveRange) continue;

                Vector3Int candidate = currentPos + new Vector3Int(dx, dy, 0);

                if (!GridManager.Instance.HasTile(candidate)) continue;
                if (GridManager.Instance.IsCellOccupied(candidate)) continue;

                // Для шага на 2 клетки проверяем, что хотя бы один промежуточный шаг проходим
                if (stepDist == 2)
                {
                    bool hasPassableMid = false;
                    if (dx != 0 && dy != 0)
                    {
                        Vector3Int mid1 = currentPos + new Vector3Int(dx, 0, 0);
                        Vector3Int mid2 = currentPos + new Vector3Int(0, dy, 0);
                        if ((GridManager.Instance.HasTile(mid1) && !GridManager.Instance.IsCellOccupied(mid1)) ||
                            (GridManager.Instance.HasTile(mid2) && !GridManager.Instance.IsCellOccupied(mid2)))
                        {
                            hasPassableMid = true;
                        }
                    }
                    else if (dx != 0)
                    {
                        Vector3Int mid = currentPos + new Vector3Int(dx / 2, 0, 0);
                        if (GridManager.Instance.HasTile(mid) && !GridManager.Instance.IsCellOccupied(mid))
                            hasPassableMid = true;
                    }
                    else if (dy != 0)
                    {
                        Vector3Int mid = currentPos + new Vector3Int(0, dy / 2, 0);
                        if (GridManager.Instance.HasTile(mid) && !GridManager.Instance.IsCellOccupied(mid))
                            hasPassableMid = true;
                    }

                    if (!hasPassableMid) continue;
                }

                int distToTarget = Mathf.Abs(candidate.x - targetPos.x) + Mathf.Abs(candidate.y - targetPos.y);
                if (distToTarget < bestDistance)
                {
                    bestDistance = distToTarget;
                    bestTile = candidate;
                }
            }
        }

        if (bestTile != currentPos)
        {
            return bestTile;
        }

        return null;
    }
}
