# 🔍 PLAYABLE AD CODE AUDIT — "Less is More"

**Project:** Squid Game Rhythm — Unity WebGL Playable Ad
**Files Reviewed:** 13 scripts (~2,300 LOC)
**Audit Date:** 2026-04-20
**Target:** 60FPS mobile browser, < 5MB build, 15-30s play session

---

## 📊 TỔNG QUAN KIẾN TRÚC

```
Scripts/ (13 files)
├── Audio/       AudioManager.cs (253 LOC)
├── CTA/         CTAManager.cs (145 LOC)
├── Config/      GameConfig.cs (244 LOC)
├── Core/        GameManager.cs (199), GameState.cs (35), OrientationManager.cs (268)
├── Gameplay/    [TRỐNG]
├── Input/       InputHandler.cs (338 LOC)
├── Scoring/     ScoreManager.cs (175 LOC)
├── Spawner/     TilePool.cs (208), TileSpawner.cs (257)
├── Tiles/       TileController.cs (346), StartPanelController.cs (74)
└── UI/          UIManager.cs (194 LOC)
```

**Flow:** `Intro` → tap Start Tile → `Playing` (beatmap spawn) → miss tile → `GameOver` → 1.5s delay → `CTA`

---

## ✂️ DANH SÁCH ĐỀ XUẤT CẮT GIẢM

---

### ✂️ 1. `AudioManager.Update()` — Empty Update Loop

> [AudioManager.cs:101-115](file:///d:/UPLIVE/TestCase/Assets/Scripts/Audio/AudioManager.cs#L101-L115)

* 🚩 **Bloat Reason:** `Update()` chạy mỗi frame nhưng body rỗng — chỉ có comment block, KHÔNG thực thi logic nào. Mỗi MonoBehaviour có `Update()` tốn overhead gọi hàm từ native C++ → managed C# mỗi frame.
* 💡 **Lightweight Alternative:** **XÓA HOÀN TOÀN** hàm `Update()`. Logic "nhạc kết thúc" đã được `TileSpawner.LateUpdate()` xử lý rồi.

---

### ✂️ 2. `TileController.GetBottomY()` — `GetComponent<BoxCollider2D>()` mỗi frame

> [TileController.cs:315-324](file:///d:/UPLIVE/TestCase/Assets/Scripts/Tiles/TileController.cs#L315-L324)

* 🚩 **Bloat Reason:** `GetComponent<BoxCollider2D>()` được gọi **mỗi frame** trong `Update()` (qua `GetBottomY()`). `GetComponent` là operation đắt, đặc biệt khi có nhiều tile active cùng lúc. Với 90 beat notes, đây là bottleneck nghiêm trọng.
* 💡 **Lightweight Alternative:** Cache `BoxCollider2D` reference trong `Init()` hoặc `Awake()`:
  ```csharp
  private BoxCollider2D _cachedCollider;
  private void Awake() { _cachedCollider = GetComponent<BoxCollider2D>(); }
  private float GetBottomY() => _cachedCollider != null ? _cachedCollider.bounds.min.y : transform.position.y;
  ```

---

### ✂️ 3. `InputHandler.TryTapStartTile()` — Physics2D.Raycast cho Start Tile

> [InputHandler.cs:247-262](file:///d:/UPLIVE/TestCase/Assets/Scripts/Input/InputHandler.cs#L247-L262)

* 🚩 **Bloat Reason:** Sử dụng **Physics2D.Raycast** chỉ để detect tap vào Start Tile — một object duy nhất, tĩnh, vị trí cố định. Physics engine là hệ thống nặng, buộc Unity include Physics2D module trong build WebGL → tăng build size.
* 💡 **Lightweight Alternative:** Dùng simple bounds check (so sánh world position với rect của Start Tile):
  ```csharp
  private void TryTapStartTile(Vector2 screenPos) {
      Vector3 wp = mainCamera.ScreenToWorldPoint(screenPos);
      // StartPanelController đã biết vị trí → check distance hoặc bounds
      if (StartPanelController.Instance != null)
          StartPanelController.Instance.TryTapAt(wp);
  }
  ```
  Hoặc dùng `StartPanelController.OnMouseDown()` (đã có sẵn backup!) và bỏ hoàn toàn `TryTapStartTile`.

---

### ✂️ 4. `ScoreManager.ShakeCamera()` — Camera Shake cho Playable Ad

> [ScoreManager.cs:159-173](file:///d:/UPLIVE/TestCase/Assets/Scripts/Scoring/ScoreManager.cs#L159-L173)

* 🚩 **Bloat Reason:** Camera shake gây **disorientation** trên mobile browser, đặc biệt khi chơi trong iframe quảng cáo nhỏ. `DOShakePosition` vibrato=10 tạo 10 micro-movements mỗi lần Perfect → gây jank trên thiết bị yếu. Ngoài ra, shake camera ảnh hưởng OrientationManager vì nó modify `transform.position` trực tiếp.
* 💡 **Lightweight Alternative:** **CẮT BỎ HOÀN TOÀN** camera shake. Thay bằng `DOPunchScale` trên score text (đã có sẵn ở combo) — chi phí gần bằng 0.

---

### ✂️ 5. `ScoreManager.SpawnFloatingText()` — Instantiate/Destroy runtime

> [ScoreManager.cs:99-150](file:///d:/UPLIVE/TestCase/Assets/Scripts/Scoring/ScoreManager.cs#L99-L150)

* 🚩 **Bloat Reason:** Mỗi lần hit tile → `Instantiate()` một GameObject mới → chạy DOTween sequence → `Destroy()`. Với 90 notes, tạo ra **90 lần GC allocation**. Trên WebGL (single-threaded, no incremental GC), mỗi Destroy trigger GC spike → frame drop.
* 💡 **Lightweight Alternative:** Dùng **pool 2-3 TextMeshPro objects**, recycle thay vì Instantiate/Destroy:
  ```csharp
  // Pre-create 3 floating text objects, cycle qua chúng
  private TextMeshPro[] _textPool = new TextMeshPro[3];
  private int _nextText = 0;
  ```

---

### ✂️ 6. `GameState.WaitingToStart` — State thừa không bao giờ dùng

> [GameState.cs:7](file:///d:/UPLIVE/TestCase/Assets/Scripts/Core/GameState.cs#L7), logic check rải khắp InputHandler, UIManager

* 🚩 **Bloat Reason:** `StartPanelController.OnTap()` nhảy thẳng `Intro → Playing`, **bỏ qua WaitingToStart**. Nhưng `InputHandler.Update()` vẫn check `state != GameState.WaitingToStart` mỗi frame. `UIManager` vẫn có case `WaitingToStart`. `GameManager.SetState(WaitingToStart)` reset score/combo — nhưng không ai gọi nó. Dead code = confusion + maintenance cost.
* 💡 **Lightweight Alternative:** **XÓA** enum value `WaitingToStart` và tất cả logic liên quan. Flow chỉ cần: `Intro → Playing → GameOver → CTA`.

---

### ✂️ 8. `TileController.Update()` — Cập nhật speed mỗi frame

> [TileController.cs:124](file:///d:/UPLIVE/TestCase/Assets/Scripts/Tiles/TileController.cs#L124)

* 🚩 **Bloat Reason:** `_scrollSpeed = GameManager.Instance.CurrentScrollSpeed;` gọi mỗi frame, mà `CurrentScrollSpeed` lại gọi `gameConfig.GetCurrentSpeed(_score)` = phép nhân + cộng mỗi frame × mỗi active tile. Với `speedIncreasePerScore = 0.01f`, sự thay đổi tốc độ gần như không đáng kể.
* 💡 **Lightweight Alternative:** Cập nhật speed **1 lần khi score thay đổi** (subscribe `OnScoreChanged`), hoặc đơn giản hơn — **giữ tốc độ cố định** cho playable ad 15s. Người chơi không kịp cảm nhận tốc độ tăng dần trong 15 giây.

---

### ✂️ 9. `OrientationManager.Update()` — Poll resolution mỗi frame

> [OrientationManager.cs:81-97](file:///d:/UPLIVE/TestCase/Assets/Scripts/Core/OrientationManager.cs#L81-L97)

* 🚩 **Bloat Reason:** Check `Screen.width/height` mỗi frame. Trong playable ad (iframe cố định), orientation **KHÔNG BAO GIỜ thay đổi** sau khi load. Đây là overhead không cần thiết chạy vĩnh viễn.
* 💡 **Lightweight Alternative:** Detect orientation **1 lần trong Awake()** rồi **xóa Update()**. Nếu vẫn muốn support rotate, dùng `Screen.orientation` event hoặc check mỗi 1-2 giây bằng `InvokeRepeating` thay vì mỗi frame.

---

### ✂️ 10. `TilePool._activeTiles` — Linear search O(n) mỗi tap

> [TilePool.cs:172-206](file:///d:/UPLIVE/TestCase/Assets/Scripts/Spawner/TilePool.cs#L172-L206)

* 🚩 **Bloat Reason:** `GetTileInHitZone()` và `GetLowestScrollingTile()` duyệt **toàn bộ** `_activeTiles` list mỗi lần player tap. Khi nhiều tile active (5-10+), hai hàm này chạy tuần tự = 2× traversal. `_activeTiles.Remove()` cũng là O(n).
* 💡 **Lightweight Alternative:** Dùng `Dictionary<int, List<TileController>>` theo lane index, hoặc đơn giản hơn — với pool size nhỏ (max ~10 tiles), giữ nguyên nhưng **gộp 2 hàm thành 1 pass duy nhất**:
  ```csharp
  public TileController GetBestTileForLane(int lane) {
      // Single pass: tìm InHitZone hoặc lowest Scrolling
  }
  ```

---

### ✂️ 11. `SFXType` enum quá nhiều — 5 loại SFX cho 15s ad

> [AudioManager.cs:6-13](file:///d:/UPLIVE/TestCase/Assets/Scripts/Audio/AudioManager.cs#L6-L13)

Loại bỏ tất cả sfx audio, chỉ giữ lại nhạc nền

---

### ✂️ 12. `Debug.Log` rải khắp nơi — 20+ log statements

* 🚩 **Bloat Reason:** `Debug.Log` trong WebGL build **KHÔNG bị strip** trừ khi dùng `Conditional` attribute hoặc custom preprocessor. Mỗi string interpolation (`$"..."`) tạo GC allocation. 20+ log gọi mỗi frame/event = GC pressure liên tục.
* 💡 **Lightweight Alternative:** Wrap trong `#if UNITY_EDITOR`:
  ```csharp
  #if UNITY_EDITOR
  Debug.Log($"[TileSpawner] Spawned {beat.type}");
  #endif
  ```
  Hoặc dùng `[System.Diagnostics.Conditional("UNITY_EDITOR")]` trên helper method.

---

### ✂️ 13. `UIManager.Invoke(nameof(TransitionToCTA))` — String-based delay

> [UIManager.cs:87](file:///d:/UPLIVE/TestCase/Assets/Scripts/UI/UIManager.cs#L87)

* 🚩 **Bloat Reason:** `Invoke(string)` dùng reflection — chậm và không type-safe. Nếu rename method sẽ silent fail.
* 💡 **Lightweight Alternative:** Dùng DOTween delay (đã có trong project):
  ```csharp
  DOVirtual.DelayedCall(gameOverToCTADelay, TransitionToCTA);
  ```

---

### ✂️ 14. `GameConfig.speedIncreasePerScore` — Dynamic difficulty trong 15s ad

> [GameConfig.cs:65-66](file:///d:/UPLIVE/TestCase/Assets/Scripts/Config/GameConfig.cs#L65-L66)

* 🚩 **Bloat Reason:** Hệ thống tăng tốc dần (progressive difficulty) thiết kế cho game dài hạn, **vô nghĩa trong 15-30s**. Gây thêm phép tính mỗi frame trên mỗi tile và khiến gameplay khó predict/tune.
* 💡 **Lightweight Alternative:** **XÓA** `speedIncreasePerScore`. Giữ `baseScrollSpeed` cố định. Đơn giản hóa `GetCurrentSpeed()` → return `baseScrollSpeed` trực tiếp.

---

### ✂️ 15. `GameConfig.GetLaneX()` và `fallbackLaneXPositions` — Duplicate logic

> [GameConfig.cs:225-234](file:///d:/UPLIVE/TestCase/Assets/Scripts/Config/GameConfig.cs#L225-L234)

* 🚩 **Bloat Reason:** `OrientationManager.GetLaneX()` tính dynamic lane positions từ camera. `GameConfig.GetLaneX()` là fallback system riêng với `fallbackLaneXPositions[]` + `landscapeLaneScale`. Hai hệ thống tính lane X song song → confusing, dễ bug.
* 💡 **Lightweight Alternative:** Chọn **MỘT** hệ thống duy nhất. `OrientationManager.GetLaneX()` đã handle tất cả — xóa fallback logic trong GameConfig.
---

## 🟢 NHỮNG GÌ ĐÃ LÀM TỐT (KEEP)

| Yếu tố | Lý do giữ |
|---------|-----------|
| **Object Pooling** (`TilePool`) | ✅ Tránh Instantiate/Destroy runtime — chuẩn WebGL |
| **DOTween** thay vì Animator | ✅ Nhẹ hơn Animator controller nhiều lần |
| **Event-driven** (`OnGameStateChanged`) | ✅ Loose coupling, không polling state |
| **Singleton pattern** đơn giản | ✅ Phù hợp cho game 15s, không cần DI framework |
| **Beatmap hardcode** (concept) | ✅ Zero-dependency, no file parsing |
| **Forgiving input** (tap nhầm = bỏ qua) | ✅ Đúng cho casual playable ad |

---

## 📋 ƯU TIÊN HÀNH ĐỘNG (Priority Matrix)

| Priority | Item | Impact | Effort |
|----------|------|--------|--------|
| 🔴 P0 | Cache `GetComponent` trong TileController | FPS +++ | 5 phút |
| 🔴 P0 | Wrap `Debug.Log` trong `#if UNITY_EDITOR` | Build size + GC | 15 phút |
| 🟡 P1 | Xóa `AudioManager.Update()` rỗng | CPU mỗi frame | 1 phút |
| 🟡 P1 | Xóa camera shake | Stability | 2 phút |
| 🟡 P1 | Xóa `WaitingToStart` state | Code clarity | 15 phút |
| 🟡 P1 | Pool floating text thay vì Instantiate | GC pressure | 20 phút |
| 🟡 P1 | Xóa `speedIncreasePerScore` | Simplicity | 5 phút |
| 🟢 P2 | Bỏ Physics2D.Raycast cho Start Tile | Build size | 10 phút |
| 🟢 P2 | Bỏ SFX audio | Build size | 10 phút |
| 🟢 P2 | Giảm OrientationManager Update frequency | CPU | 5 phút |
| 🟢 P2 | Gộp tile search thành single pass | CPU | 10 phút |
| 🟢 P2 | Thay `Invoke` bằng DOTween delay | Code quality | 2 phút |

---

## 📐 ESTIMATED SAVINGS

| Metric | Before | After (est.) |
|--------|--------|-------------|
| **Per-frame `Update()` calls** | 6 MonoBehaviours | 3 (xóa Audio, giảm Orientation) |
| **GC Allocations per hit** | 2+ (Debug.Log + Instantiate) | ~0 (pooled + stripped) |
| **Audio files** | 5 SFX + 1 BGM | 2 SFX + 1 BGM |
| **Beatmap entries** | 90 | ~35 |
| **Gameplay duration** | ~26s | ~15s → CTA nhanh hơn |
| **Dead code removed** | 0 | ~150 LOC |
