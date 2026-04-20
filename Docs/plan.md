# 📋 KẾ HOẠCH PHÁT TRIỂN 2 NGÀY
**Dự án:** Playable Ad - Rhythm Game (Theme: Squid Game)  
**Nền tảng:** Unity WebGL  
**Ngày bắt đầu:** 19/04/2026  
**Ngày hoàn thành dự kiến:** 20/04/2026  

---

## 📊 ĐÁNH GIÁ HIỆN TRẠNG DỰ ÁN

### ✅ Đã có sẵn
| Hạng mục | Chi tiết |
|----------|---------|
| Unity Project | Đã setup, URP, TextMesh Pro |
| Prefabs | `Start Tile`, `Short Tile`, `Long Tile` — đã tạo sẵn |
| Sprites | `Start_Tile_Squid_Game.png`, `Short_Tile_Squid_Game.png`, `Long_Tile_Squid_Game.png`, `Squid_Game_Upper.png/2.png` |
| Audio | `MingleGame_Creative_playable.mid` — nhạc nền (chuyển sang `.mp3`/`.ogg` để Unity dùng được) |
| GameConfig | ScriptableObject với `laneCount=4`, `baseScrollSpeed=4`, `spawnInterval=1`, `hitWindowTop/Bottom`, v.v. |
| Plugin | DOTween (Demigiant) — animation tweening |
| Background Sprites | **Đã có cặp Portrait + Landscape:** BG Ingame (`9x16` / `16x9`), BG EC (`9x16` / `16x9`). UI sprites (Headline, EC button, Hand, Game Info) dùng chung |

### ❌ Chưa có (Cần xây dựng)
- Folder `Assets/Scripts` — **trống hoàn toàn**, chưa có bất kỳ script nào
- Chưa có logic gameplay, UI, âm thanh, hay scene flow

---

## 🗓️ NGÀY 1 — NỀN TẢNG & CORE GAMEPLAY (19/04/2026)

### ⏰ Buổi sáng (8h - 12h) — Kiến trúc & Cơ chế Tile

#### Task 1.0: Responsive / Orientation System (~1h) ⚡ MỚI
Game cần chạy trên cả **mobile (portrait)** và **desktop (landscape)**. Xử lý bằng:

- `OrientationManager.cs` (Singleton, chạy sớm nhất):
  - Mỗi frame detect `Screen.width` vs `Screen.height` → xác định `Portrait` hoặc `Landscape`
  - Phát event `OnOrientationChanged(Orientation)` khi có thay đổi
  - **Background swap**: Giữ 2 `SpriteRenderer` per-screen (portrait + landscape), enable/disable theo orientation
    - Ingame: `Squid_Game_2_9x16.png` ↔ `Squid_Game_16x9_2.png`
    - CTA: `BGEC_Squid_Game_9x16.png` ↔ `BGEC_Squid_Game_16x9.png`
  - **Camera**: Orthographic, tự điều chỉnh `orthographicSize` theo aspect ratio
    ```csharp
    // Đảm bảo game area luôn fit trong viewport
    float targetAspect = isPortrait ? 9f/16f : 16f/9f;
    cam.orthographicSize = isPortrait ? 8f : 5f;
    ```
  - **Lane scaling**: Scale `fallbackLaneXPositions` theo tỉ lệ viewport width
    ```csharp
    float scaleFactor = isPortrait ? 1f : 1.5f; // lanes rộng hơn trên landscape
    ```
- **UI Canvas**: `Canvas Scaler` mode `Scale With Screen Size`
  - Reference Resolution: `1080x1920` (portrait) — auto scale cho landscape
  - Match Width Or Height: `0.5` → cân bằng cả 2 chiều
  - UI elements dùng **Anchor Points** đúng chuẩn → tự reflow

#### Task 1.1: Thiết lập cấu trúc thư mục Scripts (~30 phút)
```
Assets/Scripts/
├── Core/           → GameManager, GameState (enum), OrientationManager
├── Config/         → GameConfig (ScriptableObject - đã có)
├── Tiles/          → BaseTile, StartTile, ShortTile, LongTile
├── Spawner/        → TileSpawner, TilePool
├── Input/          → InputHandler
├── Scoring/        → ScoreManager
├── Audio/          → AudioManager
├── UI/             → UIManager, ScoreUI, FloatingText
└── CTA/            → CTAManager
```

#### Task 1.2: GameManager & Game State Machine (~1h)
- Tạo `GameManager.cs` (Singleton) quản lý trạng thái game
- Định nghĩa các trạng thái:
  ```
  Intro → WaitingToStart → Playing → GameOver → CTA
  ```
- Xử lý chuyển đổi trạng thái, event-driven

#### Task 1.3: Hệ thống Tile cơ bản (~1.5h)
- `BaseTile.cs`: Abstract class chung cho tất cả Tile
  - Thuộc tính: `lane`, `speed`, `tileState` (Idle/Active/Holding/Completed/Missed)
  - Logic di chuyển từ trên xuống (scroll)
  - Phát hiện khi Tile rời khỏi màn hình → trigger Game Over
- `StartTile.cs`: Kế thừa BaseTile
  - Hiển thị chữ "START", chờ tap để bắt đầu game
- `ShortTile.cs` & `LongTile.cs`: Kế thừa BaseTile
  - Khác biệt về kích thước & thời gian hold

#### Task 1.4: Lane System & Spawner (~1h)
- `TileSpawner.cs`: 
  - Đọc vị trí lane từ `GameConfig.fallbackLaneXPositions`
  - Spawn Tile **theo `beatmap[]`** — duyệt qua mảng, spawn Tile tại đúng timestamp `beatmap[i]`
  - Hỗ trợ `TileType` per-beat (Short/Long) có thể config trong `BeatmapEntry`
- Object Pooling cơ bản cho hiệu suất WebGL

---

### ⏰ Buổi chiều (13h30 - 18h) — Input, Scoring & Audio

#### Task 1.5: Input Handler — Hold to Eliminate (~1.5h)
- `InputHandler.cs`:
  - Nhận diện Touch/Mouse trên từng lane
  - Phát hiện Tile đang trong Hit Zone (`hitWindowTop` → `hitWindowBottom`)
  - Xử lý 3 trạng thái: **Touch Down** → **Holding** → **Release**
  - Hỗ trợ cả Touch (mobile) và Mouse (desktop/editor)

#### Task 1.6: Cơ chế Hold & Visual Feedback (~1h)
- Khi hold:
  - Phần Body chuyển dần từ **Hồng → Vàng** (dùng DOTween `DOColor`)
  - Tracking thời gian hold
- Khi release:
  - Kiểm tra vị trí Head đã vào Hit Zone chưa
  - **Good Hit**: nhả sớm (chỉ Body trong zone) → +3 điểm
  - **Perfect Hit**: Head chạm zone → đổi mặt Tam Giác → XX → +6 điểm

#### Task 1.7: Scoring System (~30 phút)
- `ScoreManager.cs`:
  - Theo dõi điểm, combo
  - Event `OnScoreChanged` để UI cập nhật
  - Floating text "Good!" / "PERFECT!" (dùng DOTween scale + fade)

#### Task 1.8: Audio Manager & Beatmap (~1h)
- **Chuẩn bị beatmap thủ công** (~20 phút):
  - Nghe bài nhạc, bấm timestamp từng beat bằng công cụ đơn giản (VD: [onlinesequencer.net](https://onlinesequencer.net) hoặc chỉ cần tab theo nhịp)
  - Ghi ra mảng trực tiếp vào `GameConfig`:
    ```csharp
    // Trong GameConfig.cs (ScriptableObject)
    public BeatmapEntry[] beatmap = {
        new BeatmapEntry(1.5f, TileType.Short, 0),
        new BeatmapEntry(2.1f, TileType.Short, 2),
        new BeatmapEntry(2.8f, TileType.Long,  1),
        new BeatmapEntry(3.5f, TileType.Short, 3),
        // ...
    };
    // BeatmapEntry { float time; TileType type; int lane; }
    ```
  - Không cần thư viện, không cần parse nhị phân — **zero risk**
- `AudioManager.cs`:
  - Phát `AudioClip` (mp3/ogg) khi nhấn Start Tile bằng `AudioSource.Play()`
  - Dùng `AudioSettings.dspTime` hoặc `audioSource.time` để đồng bộ spawn chính xác
  - Xử lý SFX cho tap, good hit, perfect hit

---

## 🗓️ NGÀY 2 — UI, POLISH & BUILD (20/04/2026)

### ⏰ Buổi sáng (8h - 12h) — UI & Scene Flow

#### Task 2.1: Intro Screen (~1h)
- Setup scene layout **responsive** (portrait 9:16 + landscape 16:9)
- Background Intro: đặt cả 2 sprite (9x16 + 16x9), `OrientationManager` tự swap
- Headline Intro + Hand Pointing: anchor center, scale theo Canvas Scaler
- Animation vào scene (DOTween: fade in, slide)
- Nút "START" hoặc auto-transition sau 2-3s

#### Task 2.2: Ingame UI (~1.5h)
- `UIManager.cs` quản lý tất cả UI panels
- **Canvas Scaler**: `Scale With Screen Size`, ref `1080x1920`, match `0.5`
- **Score Display**: Hiển thị điểm ở top (TextMesh Pro), anchor Top-Center
- **Hit Zone Indicator**: Vùng bấm visual ở dưới màn hình, anchor Bottom-Stretch
  - Highlight khi có Tile tiến vào zone
- **Floating Score Text**: "+3 Good!" / "+6 PERFECT!"
- **Combo Counter** (tùy chọn): Hiển thị streak

#### Task 2.3: Game Over & CTA Screen (~1.5h)
- `CTAManager.cs`:
  - **Trường hợp Game Over**: Hiển thị text "Game Over", delay 1s → chuyển CTA
  - **Trường hợp hoàn thành bài**: Hiển thị final score → chuyển CTA
- **CTA Screen**:
  - Background EC: 2 sprite (9x16 + 16x9), `OrientationManager` swap
  - Headline EC + Nút Download/CTA: anchor center, auto scale
  - Game Info panel
  - `Application.OpenURL()` khi nhấn nút

---

### ⏰ Buổi chiều (13h30 - 18h) — Polish, Test & Build

#### Task 2.4: Visual Polish (~1.5h)
- **Tile Elimination Effect**: 
  - Perfect Hit: Head swap Tam Giác → XX (đổi sprite) + particle burst
  - Good Hit: Tile fade out nhẹ nhàng
- **Screen Shake** nhẹ khi Perfect (DOTween `DOShakePosition`)
- **Color Transition** mượt mà Hồng → Vàng trên body tile

#### Task 2.5: Gameplay Tuning & Balancing (~1h)
- Điều chỉnh `GameConfig`:
  - `baseScrollSpeed`: test tốc độ vừa phải cho casual player
  - `spawnInterval`: đảm bảo rhythm feel tự nhiên
  - `hitWindowTop/Bottom`: đủ rộng để forgiving
  - `speedIncreasePerScore`: tăng tốc vừa phải, không quá khó
- Test flow: Start → chơi 15-30s → hết bài → CTA
- Đảm bảo **không Game Over khi tap nhầm** (theo GDD)
- Đảm bảo **Game Over khi miss tile** (tile trôi qua)

#### Task 2.6: WebGL Build & Optimization (~1h)
- Build Settings → WebGL
- Nén Brotli/Gzip
- Tối ưu texture (atlas sprites)
- Strip Engine Code
- Test file size < 5MB (target Playable Ad)
- Test trên trình duyệt: Chrome, Safari Mobile

#### Task 2.7: Testing cuối cùng (~30 phút)
- [ ] Start Tile tap → nhạc chạy, tile bắt đầu rơi
- [ ] Short Tile: hold ngắn → Good/Perfect hoạt động
- [ ] Long Tile: hold dài → Good/Perfect hoạt động
- [ ] Tap nhầm vào khoảng trống → KHÔNG bị phạt
- [ ] Miss tile → Game Over ngay lập tức
- [ ] Score hiển thị đúng, floating text hoạt động
- [ ] Game Over → CTA screen hiển thị
- [ ] CTA button → mở link store
- [ ] Toàn bộ flow chạy trong 15-30 giây
- [ ] WebGL build chạy mượt trên mobile browser

---

## 📁 DANH SÁCH FILE CẦN TẠO

| # | File | Mô tả | Ngày |
|---|------|--------|------|
| 1 | `Scripts/Core/OrientationManager.cs` | Detect portrait/landscape, swap BG, scale camera + lanes | D1 |
| 2 | `Scripts/Core/GameManager.cs` | Singleton, quản lý state machine | D1 |
| 3 | `Scripts/Core/GameState.cs` | Enum các trạng thái game | D1 |
| 4 | `Scripts/Tiles/BaseTile.cs` | Abstract class cho Tile | D1 |
| 5 | `Scripts/Tiles/StartTile.cs` | Logic Start Tile | D1 |
| 6 | `Scripts/Tiles/ShortTile.cs` | Logic Short Tile | D1 |
| 7 | `Scripts/Tiles/LongTile.cs` | Logic Long Tile | D1 |
| 8 | `Scripts/Spawner/TileSpawner.cs` | Spawn & pool Tile | D1 |
| 9 | `Scripts/Spawner/TilePool.cs` | Object pooling | D1 |
| 10 | `Scripts/Input/InputHandler.cs` | Touch/Mouse input | D1 |
| 11 | `Scripts/Scoring/ScoreManager.cs` | Điểm, combo, events | D1 |
| 12 | `Scripts/Audio/AudioManager.cs` | Nhạc, SFX, sync theo `beatmap[]` | D1 |
| 13 | `Scripts/UI/UIManager.cs` | Quản lý UI panels + Canvas Scaler | D2 |
| 14 | `Scripts/UI/ScoreUI.cs` | Hiển thị điểm | D2 |
| 15 | `Scripts/UI/FloatingText.cs` | Text "Good!"/"PERFECT!" | D2 |
| 16 | `Scripts/CTA/CTAManager.cs` | Màn hình CTA, deep link | D2 |

---

## ⚠️ RỦI RO & GIẢI PHÁP

| Rủi ro | Xác suất | Giải pháp |
|--------|----------|-----------|
| ~~MIDI parsing phức tạp~~ | ~~Cao~~ | **✅ ĐÃ LOẠI BỎ** — Dùng `float[] beatmap` thủ công trong GameConfig |
| Beatmap ghi sai timestamp | Thấp | Playtest nhanh trong Editor, chỉnh số là xong, không cần rebuild |
| WebGL build quá nặng (>5MB) | Trung bình | Giảm chất lượng texture, strip unused packages, nén audio (dùng `.ogg`) |
| Touch input không chính xác trên mobile | Trung bình | Mở rộng hit zone, test sớm trên device thật |
| Thiếu thời gian polish visual | Trung bình | Ưu tiên core gameplay hoạt động đúng, polish là nice-to-have |
| DOTween conflict với URP | Thấp | DOTween đã có sẵn trong project, ít khả năng xung đột |

---

## 🎯 TIÊU CHÍ HOÀN THÀNH (Definition of Done)

1. ✅ Game chạy được trên WebGL, mở bằng trình duyệt mobile
2. ✅ Flow hoàn chỉnh: Intro → Start Tile → Gameplay → CTA
3. ✅ 3 loại Tile hoạt động đúng (Start, Short, Long)
4. ✅ Cơ chế "Hold to Eliminate" với Good/Perfect feedback
5. ✅ Tap nhầm không bị phạt, miss tile = Game Over
6. ✅ Scoring system với floating text
7. ✅ Nhạc nền chạy đúng thời điểm
8. ✅ CTA screen với nút download hoạt động
9. ✅ Thời lượng chơi 15-30 giây
10. ✅ File build < 5MB
