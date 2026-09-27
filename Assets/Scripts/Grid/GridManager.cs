using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponentInChildren<Tilemap>();
    }

    // Метод получения центра клетки Tilemap в мировых координатах
    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        if (tilemap != null)
        {
            Vector3Int cellPosition = new Vector3Int(gridPos.x, gridPos.y, 0);
            return tilemap.GetCellCenterWorld(cellPosition);
        }

        return transform.position;
    }
}