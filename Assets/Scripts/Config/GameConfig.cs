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
    [Header("=== LANE SYSTEM ===")]
    [Tooltip("Số lane (mặc định 4)")]
    public int laneCount = 4;

    [Tooltip("Padding ngang giữa các lane")]
    public float horizontalPadding = 0f;

    [Tooltip("Vị trí X của từng lane (fallback nếu không tính dynamic)")]
    public float[] fallbackLaneXPositions = { -1.5f, -0.5f, 0.5f, 1.5f };

    [Header("=== SCROLLING ===")]
    [Tooltip("Tốc độ cuộn cơ bản (units/giây)")]
    public float baseScrollSpeed = 4f;

    [Tooltip("Tốc độ tăng thêm mỗi điểm")]
    public float speedIncreasePerScore = 0.03f;

    [Header("=== SPAWN ===")]
    [Tooltip("Khoảng cách spawn mặc định (giây) - dùng khi không có beatmap")]
    public float spawnInterval = 1f;

    [Tooltip("Vị trí Y spawn tile")]
    public float spawnY = 13f;

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
