using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Tilemap tilemap;

    [Header("Spawn Settings")]
    [Tooltip("Радиус клеток вокруг синей столицы, где разрешен призыв юнитов")]
    [SerializeField] private int playerSpawnRadius = 2;

    private GameObject highlightObj;
    private SpriteRenderer highlightRenderer;

    private Capital playerCapital;
    private Capital enemyCapital;

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

        if (tilemap == null)
            tilemap = GetComponentInChildren<Tilemap>();
    }

    private void Start()
    {
        EnsureCapitalsRegistered();
    }

    public Tilemap Tilemap => tilemap;
    public int PlayerSpawnRadius => playerSpawnRadius;

    public void EnsureCapitalsRegistered()
    {
        if (playerCapital == null)
        {
            GameObject p2 = GameObject.Find("CapitalPlayer2");
            if (p2 != null)
            {
                playerCapital = p2.GetComponent<Capital>() ?? p2.AddComponent<Capital>();
                playerCapital.isPlayerCapital = true;
                playerCapital.teamId = 1;
                playerCapital.gridPosition = WorldToCell(p2.transform.position);
            }
        }

        if (enemyCapital == null)
        {
            GameObject p1 = GameObject.Find("CapitalPlayer1");
            if (p1 != null)
            {
                enemyCapital = p1.GetComponent<Capital>() ?? p1.AddComponent<Capital>();
                enemyCapital.isPlayerCapital = false;
                enemyCapital.teamId = 2;
                enemyCapital.gridPosition = WorldToCell(p1.transform.position);
            }
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        if (tilemap != null)
            return tilemap.WorldToCell(worldPos);

        return Vector3Int.zero;
    }

    public Vector3 GetCellCenterWorld(Vector3Int cellPos)
    {
        if (tilemap != null)
            return tilemap.GetCellCenterWorld(cellPos);

        return transform.position;
    }

    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        return GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
    }

    public bool HasTile(Vector3Int cellPos)
    {
        return tilemap != null && tilemap.HasTile(cellPos);
    }

    public Capital GetPlayerCapital()
    {
        EnsureCapitalsRegistered();
        return playerCapital;
    }

    public bool IsInPlayerSpawnZone(Vector3Int cellPos)
    {
        EnsureCapitalsRegistered();

        if (playerCapital == null)
            return true; // Если база не обнаружена, не блокируем спавн

        Vector3Int capCell = playerCapital.gridPosition;

        // Нельзя спавнить прямо поверх самого квадрата столицы
        if (cellPos == capCell)
            return false;

        // Расстояние по сетке (Манхэттенская дистанция для изометрической сетки)
        int distance = Mathf.Abs(cellPos.x - capCell.x) + Mathf.Abs(cellPos.y - capCell.y);

        return distance <= playerSpawnRadius;
    }

    public bool IsCellOccupied(Vector3Int cellPos)
    {
        EnsureCapitalsRegistered();

        // Проверка наличия юнитов на клетке
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var unit in units)
        {
            if (unit != null && unit.gridPosition == cellPos)
                return true;
        }

        // Проверка наличия столицы на клетке
        if (playerCapital != null && playerCapital.gridPosition == cellPos)
            return true;

        if (enemyCapital != null && enemyCapital.gridPosition == cellPos)
            return true;

        Capital[] capitals = FindObjectsByType<Capital>(FindObjectsSortMode.None);
        foreach (var cap in capitals)
        {
            if (cap != null && cap.gridPosition == cellPos)
                return true;
        }

        return false;
    }

    public void ShowHighlight(Vector3Int cellPos, bool isValid)
    {
        if (!HasTile(cellPos))
        {
            HideHighlight();
            return;
        }

        if (highlightObj == null)
            CreateHighlightObject();

        highlightObj.transform.position = GetCellCenterWorld(cellPos);
        if (highlightRenderer != null)
        {
            highlightRenderer.color = isValid
                ? new Color(0.2f, 1f, 0.2f, 0.6f)
                : new Color(1f, 0.2f, 0.2f, 0.6f);
        }
        highlightObj.SetActive(true);
    }

    public void HideHighlight()
    {
        if (highlightObj != null)
        {
            highlightObj.SetActive(false);
        }
    }

    private void CreateHighlightObject()
    {
        if (highlightObj != null) return;

        highlightObj = new GameObject("TileHighlightMarker");
        highlightObj.transform.SetParent(transform);
        highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
        highlightRenderer.sortingOrder = 2; // Поверх тайлов, но под юнитами

        if (tilemap != null)
        {
            // Берем спрайт первого попавшегося тайла для идеального соответствия изометрической форме
            BoundsInt bounds = tilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    Sprite s = tilemap.GetSprite(pos);
                    if (s != null)
                    {
                        highlightRenderer.sprite = s;
                        break;
                    }
                }
            }
        }

        highlightObj.SetActive(false);
    }
}