using UnityEngine;
using System;

// ============================================================
// GameConfig.cs — ScriptableObject chứa toàn bộ config
// Đã có sẵn GameConfig.asset trong project, file này mở rộng
// thêm BeatmapEntry[] cho hệ thống beatmap thủ công.
// ============================================================

/// <summary>
/// Một entry trong beatmap: thời điểm, loại tile, và lane
/// </summary>
[Serializable]
public struct BeatmapEntry
{
    [Tooltip("Thời điểm spawn (giây tính từ lúc nhạc bắt đầu)")]
    public float time;

    [Tooltip("Loại tile: Short hoặc Long")]
    public TileType type;

    [Tooltip("Lane index (0 → laneCount-1)")]
    public int lane;

    public BeatmapEntry(float time, TileType type, int lane)
    {
        this.time = time;
        this.type = type;
        this.lane = lane;
    }
}

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    private void OnEnable()
    {
#if UNITY_EDITOR
        // Auto-load if empty
        if (beatmap == null || beatmap.Length == 0)
        {
            LoadMingleGameBeatmap();
        }
#endif
    }

    [Header("=== LANE SYSTEM ===")]
    [Tooltip("Số lane (mặc định 4)")]
    public int laneCount = 4;

    [Tooltip("Thời gian delay nhạc so với tile (giây). Nhạc sẽ bắt đầu phát sau leadTime giây, tile đến đúng nhịp)")]
    public float musicStartDelay = 2f;

    [Tooltip("Padding ngang giữa các lane")]
    public float horizontalPadding = 0f;

    [Tooltip("Vị trí X của từng lane (fallback nếu không tính dynamic)")]
    public float[] fallbackLaneXPositions = { -1.5f, -0.5f, 0.5f, 1.5f };

    [Header("=== SCROLLING ===")]
    [Tooltip("Tốc độ cuộn cơ bản (units/giây)")]
    public float baseScrollSpeed = 8f;

    [Tooltip("Tốc độ tăng thêm mỗi điểm")]
    public float speedIncreasePerScore = 0.01f;

    [Header("=== SPAWN ===")]
    [Tooltip("Khoảng cách spawn mặc định (giây) - dùng khi không có beatmap")]
    public float spawnInterval = 1f;

    [Tooltip("Vị trí Y spawn tile")]
    public float spawnY = 8f;

    [Tooltip("Vị trí Y mà tile bị coi là miss")]
    public float missY = -10f;

    [Header("=== HIT ZONE ===")]
    [Tooltip("Cạnh trên vùng Hit Zone (Y)")]
    public float hitWindowTop = -3.1f;

    [Tooltip("Cạnh dưới vùng Hit Zone (Y)")]
    public float hitWindowBottom = -4.1f;

    [Header("=== SCORING ===")]
    [Tooltip("Điểm cho Good Hit")]
    public int goodHitScore = 3;

    [Tooltip("Điểm cho Perfect Hit")]
    public int perfectHitScore = 6;

    [Header("=== ORIENTATION ===")]
    [Tooltip("Camera orthographic size khi Portrait")]
    public float portraitCameraSize = 8f;

    [Tooltip("Camera orthographic size khi Landscape")]
    public float landscapeCameraSize = 5f;

    [Tooltip("Scale factor cho lane X positions khi Landscape")]
    public float landscapeLaneScale = 1.5f;

    [Header("=== BEATMAP ===")]
    [Tooltip("Danh sách beat thủ công — nghe nhạc, ghi timestamp")]
    public BeatmapEntry[] beatmap;

    // ---- Beatmap Presets ----

    /// <summary>
    /// Auto-generated beatmap from MingleGame_Creative_playable.mid
    /// Tempo: 105 BPM | Ticks per beat: 96
    /// Track: 'main2' | Total notes: 90
    /// Note mapping: 96→lane0, 97→lane1, 98→lane2, 99→lane3
    /// Long threshold: duration >= 96 ticks (1 beat)
    /// Right-click the GameConfig component in Inspector and select
    /// "Load MingleGame Beatmap" to populate the beatmap field.
    /// </summary>
    [ContextMenu("Load MingleGame Beatmap")]
    public void LoadMingleGameBeatmap()
    {
        beatmap = new BeatmapEntry[]
        {
            new BeatmapEntry(0.000f, TileType.Short, 3),
            new BeatmapEntry(0.190f, TileType.Short, 2),
            new BeatmapEntry(0.381f, TileType.Short, 1),
            new BeatmapEntry(0.571f, TileType.Short, 0),
            new BeatmapEntry(0.952f, TileType.Short, 3),
            new BeatmapEntry(1.143f, TileType.Short, 0),
            new BeatmapEntry(2.286f, TileType.Short, 0),
            new BeatmapEntry(2.476f, TileType.Short, 1),
            new BeatmapEntry(2.667f, TileType.Short, 2),
            new BeatmapEntry(2.857f, TileType.Short, 3),
            new BeatmapEntry(3.238f, TileType.Short, 0),
            new BeatmapEntry(3.429f, TileType.Short, 3),
            new BeatmapEntry(4.571f, TileType.Short, 0),
            new BeatmapEntry(4.952f, TileType.Short, 2),
            new BeatmapEntry(5.143f, TileType.Short, 0),
            new BeatmapEntry(5.476f, TileType.Short, 3),
            new BeatmapEntry(5.714f, TileType.Short, 1),
            new BeatmapEntry(6.095f, TileType.Short, 2),
            new BeatmapEntry(6.286f, TileType.Short, 0),
            new BeatmapEntry(6.667f, TileType.Short, 3),
            new BeatmapEntry(6.857f, TileType.Short, 1),
            new BeatmapEntry(7.429f, TileType.Short, 3),
            new BeatmapEntry(7.429f, TileType.Short, 1),
            new BeatmapEntry(8.000f, TileType.Long,  2),
            new BeatmapEntry(8.000f, TileType.Long,  0),
            new BeatmapEntry(9.143f, TileType.Short, 3),
            new BeatmapEntry(9.333f, TileType.Short, 2),
            new BeatmapEntry(9.524f, TileType.Short, 1),
            new BeatmapEntry(9.714f, TileType.Short, 0),
            new BeatmapEntry(10.095f, TileType.Short, 3),
            new BeatmapEntry(10.286f, TileType.Short, 0),
            new BeatmapEntry(10.857f, TileType.Short, 3),
            new BeatmapEntry(10.857f, TileType.Short, 0),
            new BeatmapEntry(11.429f, TileType.Short, 0),
            new BeatmapEntry(11.619f, TileType.Short, 1),
            new BeatmapEntry(11.810f, TileType.Short, 2),
            new BeatmapEntry(12.000f, TileType.Short, 3),
            new BeatmapEntry(12.381f, TileType.Short, 0),
            new BeatmapEntry(12.571f, TileType.Short, 3),
            new BeatmapEntry(13.143f, TileType.Short, 3),
            new BeatmapEntry(13.143f, TileType.Short, 0),
            new BeatmapEntry(13.714f, TileType.Short, 3),
            new BeatmapEntry(14.095f, TileType.Short, 2),
            new BeatmapEntry(14.095f, TileType.Short, 0),
            new BeatmapEntry(14.286f, TileType.Short, 3),
            new BeatmapEntry(14.667f, TileType.Short, 2),
            new BeatmapEntry(14.667f, TileType.Short, 0),
            new BeatmapEntry(14.857f, TileType.Short, 1),
            new BeatmapEntry(15.238f, TileType.Short, 2),
            new BeatmapEntry(15.429f, TileType.Short, 1),
            new BeatmapEntry(15.810f, TileType.Short, 3),
            new BeatmapEntry(16.000f, TileType.Short, 0),
            new BeatmapEntry(16.381f, TileType.Short, 2),
            new BeatmapEntry(16.571f, TileType.Short, 1),
            new BeatmapEntry(16.952f, TileType.Short, 3),
            new BeatmapEntry(17.143f, TileType.Short, 0),
            new BeatmapEntry(18.286f, TileType.Short, 0),
            new BeatmapEntry(18.476f, TileType.Short, 1),
            new BeatmapEntry(18.667f, TileType.Short, 2),
            new BeatmapEntry(18.857f, TileType.Short, 3),
            new BeatmapEntry(19.238f, TileType.Short, 0),
            new BeatmapEntry(19.429f, TileType.Short, 3),
            new BeatmapEntry(20.000f, TileType.Short, 3),
            new BeatmapEntry(20.000f, TileType.Short, 0),
            new BeatmapEntry(20.571f, TileType.Short, 3),
            new BeatmapEntry(20.762f, TileType.Short, 2),
            new BeatmapEntry(20.952f, TileType.Short, 1),
            new BeatmapEntry(21.143f, TileType.Short, 0),
            new BeatmapEntry(21.524f, TileType.Short, 3),
            new BeatmapEntry(21.714f, TileType.Short, 0),
            new BeatmapEntry(22.286f, TileType.Short, 3),
            new BeatmapEntry(22.286f, TileType.Short, 0),
            new BeatmapEntry(22.857f, TileType.Short, 1),
            new BeatmapEntry(23.238f, TileType.Short, 2),
            new BeatmapEntry(23.238f, TileType.Short, 0),
            new BeatmapEntry(23.429f, TileType.Short, 3),
            new BeatmapEntry(23.810f, TileType.Short, 3),
            new BeatmapEntry(23.810f, TileType.Short, 1),
            new BeatmapEntry(24.000f, TileType.Short, 0),
            new BeatmapEntry(24.381f, TileType.Short, 2),
            new BeatmapEntry(24.381f, TileType.Short, 0),
            new BeatmapEntry(24.571f, TileType.Short, 3),
            new BeatmapEntry(24.952f, TileType.Short, 2),
            new BeatmapEntry(24.952f, TileType.Short, 0),
            new BeatmapEntry(25.143f, TileType.Short, 1),
            new BeatmapEntry(25.143f, TileType.Short, 3),
            new BeatmapEntry(25.714f, TileType.Short, 2),
            new BeatmapEntry(25.714f, TileType.Short, 0),
            new BeatmapEntry(26.286f, TileType.Short, 3),
            new BeatmapEntry(26.286f, TileType.Short, 1),
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log($"[GameConfig] Loaded MingleGame beatmap: {beatmap.Length} entries.");
#endif
    }

    // ---- Helper Methods ----

    /// <summary>
    /// Lấy vị trí X của lane, có tính scale theo orientation
    /// </summary>
    public float GetLaneX(int laneIndex, bool isLandscape)
    {
        if (laneIndex < 0 || laneIndex >= fallbackLaneXPositions.Length)
            return 0f;

        float x = fallbackLaneXPositions[laneIndex];
        if (isLandscape)
            x *= landscapeLaneScale;
        return x;
    }

    /// <summary>
    /// Tốc độ cuộn hiện tại dựa trên điểm số
    /// </summary>
    public float GetCurrentSpeed(int currentScore)
    {
        return baseScrollSpeed + (currentScore * speedIncreasePerScore);
    }
}
