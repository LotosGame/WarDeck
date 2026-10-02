using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileData : MonoBehaviour
{
    [Header("Координаты сетки")]
    public int x;
    public int y;

    [Header("Состояние ячейки")]
    public bool isWalkable = true;
    public bool isOccupied = false;

    [Header("Визуал")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color defaultColor = Color.green;
    [SerializeField] private Color highlightColor = Color.yellow; // Поставим желтый для контраста

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int posX, int posY)
    {
        x = posX;
        y = posY;
        gameObject.name = $"Tile_{x}_{y}";

        // Принудительно красим ячейку в стандартный цвет при спавне
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }

    private void OnMouseEnter()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = highlightColor;
    }

    private void OnMouseExit()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }
}