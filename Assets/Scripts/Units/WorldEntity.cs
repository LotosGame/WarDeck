using UnityEngine;

public class WorldEntity : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void SetGridPosition(Vector2Int gridPos, Vector3 worldPos)
    {
        GridPosition = gridPos;
        transform.position = worldPos;
    }
}