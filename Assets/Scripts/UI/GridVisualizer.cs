using UnityEngine;
using System.Collections.Generic;

// ============================================================
// GridVisualizer.cs — Tự động vẽ các đường lưới (vertical lines)
// chia cách các lane và đường ngang (horizontal line) cho Hit Zone.
// Tự động căn chỉnh scale theo OrientationManager.
// ============================================================
public class GridVisualizer : MonoBehaviour
{
    [Header("=== Settings ===")]
    [Tooltip("Màu của đường lưới (có thể chỉnh alpha để mờ đi)")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.5f);

    [Tooltip("Độ dày của đường lưới")]
    [SerializeField] private float lineWidth = 0.05f;

    [Tooltip("Vật liệu cho LineRenderer (nên dùng Sprite Default hoặc Unlit)")]
    [SerializeField] private Material lineMaterial;

    [Tooltip("Sorting Layer cho đường lưới")]
    [SerializeField] private string sortingLayerName = "Tiles";
    [SerializeField] private int sortingOrder = -1; // Nằm dưới tile

    [Tooltip("Hiển thị đường ngang cho Hit Zone?")]
    [SerializeField] private bool showHitZoneLine = true;

    // ---- Runtime ----
    private List<LineRenderer> _verticalLines = new List<LineRenderer>();
    private LineRenderer _horizontalLine;

    // ============================================================
    // LIFECYCLE
    // ============================================================
    private void Start()
    {
        // Chờ các hệ thống khác khởi tạo xong
        Invoke(nameof(UpdateGrid), 0.1f);
    }

    private void OnEnable()
    {
        OrientationManager.OnOrientationChanged += HandleOrientationChanged;
    }

    private void OnDisable()
    {
        OrientationManager.OnOrientationChanged -= HandleOrientationChanged;
    }

    private void HandleOrientationChanged(bool isPortrait)
    {
        UpdateGrid();
    }

    // ============================================================
    // GRID GENERATION & UPDATE
    // ============================================================
    public void UpdateGrid()
    {
        if (OrientationManager.Instance == null || GameManager.Instance == null) return;
        
        GameConfig config = GameManager.Instance.Config;
        if (config == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        int laneCount = config.laneCount;
        float laneWidth = OrientationManager.Instance.GetLaneWidth();
        float camHeight = cam.orthographicSize * 2f;

        // Cần (laneCount + 1) đường dọc để bọc kín các lane
        int requiredLines = laneCount + 1;

        // Tạo thêm LineRenderer nếu thiếu
        while (_verticalLines.Count < requiredLines)
        {
            _verticalLines.Add(CreateLine("VerticalLine_" + _verticalLines.Count));
        }

        // Vẽ các đường dọc
        for (int i = 0; i < requiredLines; i++)
        {
            LineRenderer lr = _verticalLines[i];
            lr.gameObject.SetActive(true);

            // X của đường dọc:
            // Nếu i < laneCount: Lấy cạnh TRÁI của lane thứ i
            // Nếu i == laneCount: Lấy cạnh PHẢI của lane cuối cùng
            float xPos = 0f;
            if (i < laneCount)
            {
                xPos = OrientationManager.Instance.GetLaneX(i) - (laneWidth / 2f);
            }
            else
            {
                xPos = OrientationManager.Instance.GetLaneX(laneCount - 1) + (laneWidth / 2f);
            }

            // Đặt điểm đầu và cuối của đường
            lr.SetPosition(0, new Vector3(xPos, cam.transform.position.y + camHeight / 2f, 0f));
            lr.SetPosition(1, new Vector3(xPos, cam.transform.position.y - camHeight / 2f, 0f));
        }

        // Vẽ đường ngang Hit Zone
        if (showHitZoneLine)
        {
            if (_horizontalLine == null)
            {
                _horizontalLine = CreateLine("HorizontalLine_HitZone");
            }
            _horizontalLine.gameObject.SetActive(true);

            float leftEdge = OrientationManager.Instance.GetLaneX(0) - (laneWidth / 2f);
            float rightEdge = OrientationManager.Instance.GetLaneX(laneCount - 1) + (laneWidth / 2f);
            float yPos = config.hitWindowTop; // Cạnh trên của Hit Zone

            _horizontalLine.SetPosition(0, new Vector3(leftEdge, yPos, 0f));
            _horizontalLine.SetPosition(1, new Vector3(rightEdge, yPos, 0f));
        }
        else if (_horizontalLine != null)
        {
            _horizontalLine.gameObject.SetActive(false);
        }
    }

    private LineRenderer CreateLine(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = gridColor;
        lr.endColor = gridColor;

        if (lineMaterial != null)
            lr.material = lineMaterial;

        lr.sortingLayerName = sortingLayerName;
        lr.sortingOrder = sortingOrder;

        return lr;
    }
}
