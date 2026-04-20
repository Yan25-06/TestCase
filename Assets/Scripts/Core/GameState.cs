// ============================================================
// GameState.cs — Enum cho trạng thái game
// ============================================================
public enum GameState
{
    Intro,    // Màn hình intro / splash
    Playing,  // Gameplay đang diễn ra
    GameOver, // Người chơi miss tile
    CTA       // Màn hình Call-To-Action
}

// ============================================================
// TileType — Loại tile
// ============================================================
public enum TileType
{
    Start = 0,
    Short = 1,
    Long  = 2
}

// ============================================================
// TileState — Trạng thái của một tile cụ thể
// ============================================================
public enum TileState
{
    Idle,       // Chưa active (trong pool)
    Scrolling,  // Đang trượt xuống
    InHitZone,  // Đã vào vùng Hit Zone
    Holding,    // Đang được giữ
    Completed,  // Đã xử lý xong (Good/Perfect)
    Missed      // Bị miss → Game Over
}
