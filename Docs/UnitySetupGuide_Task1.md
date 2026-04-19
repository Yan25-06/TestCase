# 🛠️ Unity Setup Guide — Tasks 1.0 → 1.4

Hướng dẫn thiết lập trong Unity Editor để các script hoạt động.

---

## 📁 Cấu trúc file đã tạo

```
Assets/Scripts/
├── Core/
│   ├── GameState.cs          ← Enums (GameState, TileType, TileState, HitResult)
│   ├── GameManager.cs        ← Singleton quản lý game
│   └── OrientationManager.cs ← Detect orientation, swap BG
├── Config/
│   └── GameConfig.cs         ← ScriptableObject (mở rộng từ asset có sẵn)
├── Tiles/
│   ├── TileController.cs     ← Controller cho tất cả Tile types
│   └── StartPanelController.cs ← Logic riêng Start Tile
└── Spawner/
    ├── TileSpawner.cs         ← Spawn theo beatmap
    └── TilePool.cs            ← Object pooling
```

---

## 1. GameManager (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `GameManager`
2. Đặt ở root của Scene

### Components
| Component | Ghi chú |
|-----------|---------|
| `GameManager.cs` | Kéo thả script vào |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Game Config` | Kéo file `Assets/GameConfig.asset` vào |

---

## 2. OrientationManager (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `OrientationManager`
2. Đặt ở root của Scene

### Components
| Component | Ghi chú |
|-----------|---------|
| `OrientationManager.cs` | Kéo thả script vào |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Main Camera` | Kéo Main Camera vào (hoặc để trống → auto-find) |
| `Game Config` | Kéo `Assets/GameConfig.asset` vào |
| `Bg Ingame Portrait` | Xem bước "Setup Backgrounds" bên dưới |
| `Bg Ingame Landscape` | Xem bước "Setup Backgrounds" bên dưới |
| `Bg CTA Portrait` | Xem bước "Setup Backgrounds" bên dưới |
| `Bg CTA Landscape` | Xem bước "Setup Backgrounds" bên dưới |

### Setup Backgrounds
1. Tạo **4 Empty GameObjects** là con của `OrientationManager` (hoặc riêng):
   - `BG_Ingame_Portrait` → Add `SpriteRenderer` → assign `Squid_Game_2_9x16.png`
   - `BG_Ingame_Landscape` → Add `SpriteRenderer` → assign `Squid_Game_16x9_2.png`
   - `BG_CTA_Portrait` → Add `SpriteRenderer` → assign `BGEC_Squid_Game_9x16.png`
   - `BG_CTA_Landscape` → Add `SpriteRenderer` → assign `BGEC_Squid_Game_16x9.png`
2. Set Sorting Layer / Order đủ thấp để nằm sau game objects (VD: Sorting Order = -10)
3. Kéo 4 SpriteRenderer đó vào Inspector của `OrientationManager`

---

## 3. TilePool (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `TilePool`
2. Đặt ở root của Scene (các pooled tiles sẽ là con của nó)

### Components
| Component | Ghi chú |
|-----------|---------|
| `TilePool.cs` | Kéo thả script vào |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Short Tile Prefab` | Kéo `Assets/My Prefabs/Short Tile.prefab` vào |
| `Long Tile Prefab` | Kéo `Assets/My Prefabs/Long Tile.prefab` vào |
| `Start Tile Prefab` | Kéo `Assets/My Prefabs/Start Tile.prefab` vào |
| `Initial Pool Size` | `5` (mặc định, đủ cho 15-30s gameplay) |

---

## 4. TileSpawner (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `TileSpawner`
2. Đặt ở root của Scene

### Components
| Component | Ghi chú |
|-----------|---------|
| `TileSpawner.cs` | Kéo thả script vào |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Game Config` | Kéo `Assets/GameConfig.asset` vào |

---

## 5. Cập nhật Prefabs

### Short Tile & Long Tile Prefabs
Mở từng prefab, thêm assignment cho field mới:

| Field | Giá trị |
|-------|---------|
| `Body Renderer` | Kéo child **"Body"** (SpriteRenderer) vào field `Body Renderer` |
| `Body Original Color` | Đặt màu hồng: `(255, 128, 153, 255)` hoặc hex `#FF8099` |
| `Body Hold Color` | Đặt màu vàng: `(255, 217, 0, 255)` hoặc hex `#FFD900` |

> ⚠️ **Không cần sửa** `Head Renderer` và `Dead Head Sprite` — đã assign sẵn trong prefab.

### Start Tile Prefab
- `Body Renderer` → **để trống** (Start Tile không có Body child)
- Đảm bảo `StartPanelController` đã tự lấy `TileController` qua `GetComponent`

---

## 6. GameConfig.asset — Thêm Beatmap

1. Click vào `Assets/GameConfig.asset` trong Project window
2. Trong Inspector, sẽ thấy các section mới:
   - **SCORING**: Đặt `Good Hit Score = 3`, `Perfect Hit Score = 6`
   - **ORIENTATION**: Đặt `Portrait Camera Size = 8`, `Landscape Camera Size = 5`, `Landscape Lane Scale = 1.5`
   - **BEATMAP**: Click `+` để thêm entries. **Tạm thời để trống** — sẽ điền sau khi nghe nhạc ở Task 1.8

> ⚠️ Các field gốc (`laneCount`, `baseScrollSpeed`, `spawnInterval`, `spawnY`, `missY`, `hitWindowTop`, `hitWindowBottom`) **giữ nguyên giá trị cũ**.

---

## 7. Camera Setup

1. Chọn **Main Camera** trong Scene
2. Đảm bảo:
   - `Projection` = **Orthographic**
   - `Size` = **8** (sẽ được OrientationManager điều chỉnh runtime)
   - `Clear Flags` = **Solid Color** hoặc **Skybox**

---

## 8. InputHandler (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `InputHandler`
2. Đặt ở root của Scene

### Components
| Component | Ghi chú |
|-----------|---------|
| `InputHandler.cs` | Kéo thả script vào |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Game Config` | Kéo `Assets/GameConfig.asset` vào |
| `Main Camera` | Kéo Main Camera vào (hoặc để trống → auto-find) |

---

## 9. ScoreManager (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `ScoreManager`
2. Đặt ở root của Scene

### Components
| Component | Ghi chú |
|-----------|---------|
| `ScoreManager.cs` | Kéo thả script vào |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Main Camera` | Kéo Main Camera vào (để shake khi Perfect) |
| `Floating Text Prefab` | **Tùy chọn** — để trống cũng được (auto-tạo runtime). Hoặc tạo prefab TextMeshPro nếu muốn customize font |
| `Text Offset Y` | `1.5` (mặc định) |
| `Text Duration` | `0.8` (mặc định) |
| `Text Rise Distance` | `2` (mặc định) |
| `Good Color` | Xanh lá `#33D933` |
| `Perfect Color` | Vàng gold `#FFD900` |
| `Shake Strength` | `0.3` |
| `Shake Duration` | `0.2` |

> 💡 **Floating Text Prefab** có thể để trống lúc đầu — ScoreManager sẽ tự tạo TextMeshPro object runtime để test. Khi polish ở Day 2, tạo prefab riêng với font/style đẹp hơn.

---

## 10. AudioManager (Empty GameObject)

### Hierarchy
1. **Tạo Empty GameObject** → đặt tên `AudioManager`
2. Đặt ở root của Scene

### Components
| Component | Ghi chú |
|-----------|---------|
| `AudioManager.cs` | Kéo thả script vào |
| `AudioSource` (x2) | Script sẽ **tự tạo** nếu để trống. Hoặc add thủ công 2 AudioSource |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Music Source` | Để trống (auto-create) hoặc kéo AudioSource component vào |
| `Music Clip` | **Kéo file nhạc** (.mp3 hoặc .ogg) vào. ⚠️ File MIDI hiện tại (`MingleGame_Creative_playable.mid`) cần **convert sang mp3/ogg** trước |
| `Sfx Source` | Để trống (auto-create) hoặc kéo AudioSource thứ 2 vào |
| `Sfx Tap` | Clip SFX khi tap (tùy chọn) |
| `Sfx Good Hit` | Clip SFX khi Good Hit (tùy chọn) |
| `Sfx Perfect Hit` | Clip SFX khi Perfect Hit (tùy chọn) |
| `Sfx Game Over` | Clip SFX khi Game Over (tùy chọn) |
| `Music Volume` | `0.7` |
| `Sfx Volume` | `1.0` |

> ⚠️ **Quan trọng:** File `MingleGame_Creative_playable.mid` là MIDI, Unity không phát trực tiếp được. Cần convert sang `.mp3` hoặc `.ogg` rồi import vào Unity.

---

## 11. Hierarchy tổng thể (sau khi setup)

```
Scene: SampleScene
├── Main Camera
├── GameManager          [GameManager.cs]
├── OrientationManager   [OrientationManager.cs]
│   ├── BG_Ingame_Portrait    [SpriteRenderer]
│   ├── BG_Ingame_Landscape   [SpriteRenderer]
│   ├── BG_CTA_Portrait       [SpriteRenderer]
│   └── BG_CTA_Landscape      [SpriteRenderer]
├── TilePool             [TilePool.cs]
│   └── (pooled tiles sẽ spawn ở đây)
├── TileSpawner          [TileSpawner.cs]
├── InputHandler         [InputHandler.cs]
├── ScoreManager         [ScoreManager.cs]
├── AudioManager         [AudioManager.cs, AudioSource x2]
└── (UI Canvas — sẽ thêm ở Day 2)
```

---

## ⚠️ Tags & Layers
Hiện tại **KHÔNG cần** tạo Tag hay Layer đặc biệt nào. Tất cả logic dựa trên component references, không dùng `FindWithTag` hay Layer masks.

---

## ✅ Kiểm tra sau khi setup

1. Mở Unity → đợi compile (không có lỗi đỏ trong Console)
2. Nhấn **Play** → Console hiện:
   - `[GameManager] State: Intro → Intro`
   - `[TileSpawner] Lead time = X.XXs ...`
   - `[AudioManager] Playing music: ...` (khi chuyển sang Playing)
3. Test flow cơ bản:
   - Trong Inspector của GameManager, thay đổi state sang `WaitingToStart`
   - Start Tile xuất hiện → tap vào → nhạc chạy, tiles bắt đầu rơi
   - Hold tile khi vào Hit Zone → nhả ra → thấy "Good!" hoặc "PERFECT!"
   - Miss tile → Game Over

## 🎵 Chuẩn bị file nhạc

1. Convert `MingleGame_Creative_playable.mid` → `.mp3` hoặc `.ogg`
   - Dùng tool online hoặc DAW (Audacity, FL Studio...)
   - Export ra file ~15-30 giây
2. Import file vào `Assets/_Playable/Sprites/MP3/` (hoặc tạo folder `Audio`)
3. Kéo vào `Music Clip` field của AudioManager
