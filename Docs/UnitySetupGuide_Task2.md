# 🛠️ Unity Setup Guide — Day 2 (Tasks 2.1 → 2.3)

Hướng dẫn thiết lập UI, Game Over, CTA trong Unity Editor.

---

## 📁 Scripts mới

```
Assets/Scripts/
├── UI/
│   └── UIManager.cs          ← Quản lý tất cả UI panels
└── CTA/
    └── CTAManager.cs         ← Màn hình CTA + nút download
```

---

## 1. Tạo Canvas (UI Root)

### Hierarchy
1. **Right-click** trong Hierarchy → **UI → Canvas**
2. Đặt tên `UICanvas`

### Canvas Component
| Field | Giá trị |
|-------|---------|
| `Render Mode` | **Screen Space - Overlay** |
| `Sort Order` | `10` (nằm trên game objects) |

### Canvas Scaler Component (tự động tạo cùng Canvas)
| Field | Giá trị |
|-------|---------|
| `UI Scale Mode` | **Scale With Screen Size** |
| `Reference Resolution` | `1080 x 1920` |
| `Match Width Or Height` | `0.5` |

---

## 2. Intro Panel (con của UICanvas)

### Hierarchy
```
UICanvas
└── IntroPanel              [Panel]
    ├── IntroHeadline        [Image — Tagline_1.png]
    └── HandPointing         [Image — Hand_Pointing.png]
```

### Tạo:
1. Right-click `UICanvas` → **UI → Panel** → đặt tên `IntroPanel`
2. Xóa Image component (hoặc set alpha = 0 cho transparent background)
3. Con của IntroPanel:
   - **IntroHeadline**: UI → Image → kéo `Tagline_1.png` vào `Source Image`
     - Anchor: **Center** | Pivot: (0.5, 0.5)
     - Pos Y: `200` (trên center)
     - Check `Preserve Aspect`
   - **HandPointing**: UI → Image → kéo `Hand_Pointing.png` vào `Source Image`
     - Anchor: **Bottom-Center** | Pos Y: `400`
     - Check `Preserve Aspect`
     - Width/Height: khoảng `200x200`

---

## 3. Ingame Panel (con của UICanvas)

### Hierarchy
```
UICanvas
└── IngamePanel             [Panel — transparent]
    ├── ScoreText            [TextMeshPro — UGUI]
    └── ComboText            [TextMeshPro — UGUI]
```

### Tạo:
1. Right-click `UICanvas` → **UI → Panel** → đặt tên `IngamePanel`
2. Set alpha = 0 (transparent)
3. Con của IngamePanel:
   - **ScoreText**: UI → TextMeshPro - Text (UI) → đặt tên `ScoreText`
     - Anchor: **Top-Center** | Pos Y: `-100`
     - Font Size: `72` | Alignment: Center
     - Text mặc định: `0`
     - Color: **White**
   - **ComboText**: UI → TextMeshPro - Text (UI) → đặt tên `ComboText`
     - Anchor: **Top-Center** | Pos Y: `-200`
     - Font Size: `48` | Alignment: Center
     - Text mặc định: `x2`
     - Color: **Yellow (#FFD900)**
     - Set `Active = false` mặc định (chỉ hiện khi combo > 1)

---

## 4. Game Over Panel (con của UICanvas)

### Hierarchy
```
UICanvas
└── GameOverPanel           [Panel — semi-transparent]
    ├── GameOverText         [TextMeshPro — "GAME OVER"]
    └── GameOverScoreText    [TextMeshPro — "Score: 0"]
```

### Tạo:
1. Right-click `UICanvas` → **UI → Panel** → đặt tên `GameOverPanel`
2. Image color: `(0, 0, 0, 0.5)` — overlay đen mờ
3. Set `Active = false` mặc định
4. Con:
   - **GameOverText**: TextMeshPro → `"GAME OVER"`
     - Anchor: Center | Font Size: `80` | Color: Red | Bold
   - **GameOverScoreText**: TextMeshPro → `"Score: 0"`
     - Anchor: Center | Pos Y: `-100` | Font Size: `48` | Color: White

---

## 5. CTA Panel (con của UICanvas)

### Hierarchy
```
UICanvas
└── CTAPanel                [Panel]
    ├── ECHeadline           [Image — EC_Headline.png]
    ├── GameInfo             [Image — Game_Info.png]
    └── CTAButton            [Button — EC_button.png]
```

### Tạo:
1. Right-click `UICanvas` → **UI → Panel** → đặt tên `CTAPanel`
2. Set `Active = false` mặc định
3. Background: transparent hoặc tùy ý (BG EC đã có qua OrientationManager)
4. Con:
   - **ECHeadline**: UI → Image → kéo `EC_Headline.png`
     - Anchor: Top-Center | Pos Y: `-200` | Preserve Aspect
   - **GameInfo**: UI → Image → kéo `Game_Info.png`
     - Anchor: Center | Pos Y: `-100` | Preserve Aspect
   - **CTAButton**: UI → Button → đặt tên `CTAButton`
     - Kéo `EC_button.png` vào Image `Source Image` của Button
     - Anchor: Bottom-Center | Pos Y: `300`
     - Xóa child Text nếu có (nút đã có text trên sprite)
     - `Preserve Aspect`

---

## 6. UIManager (gắn vào UICanvas)

### Components
| Component | Ghi chú |
|-----------|---------|
| `UIManager.cs` | Kéo thả script vào UICanvas |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `Intro Panel` | Kéo `IntroPanel` vào |
| `Ingame Panel` | Kéo `IngamePanel` vào |
| `Game Over Panel` | Kéo `GameOverPanel` vào |
| `CTA Panel` | Kéo `CTAPanel` vào |
| `Score Text` | Kéo `ScoreText` (TextMeshProUGUI) vào |
| `Combo Text` | Kéo `ComboText` (TextMeshProUGUI) vào |
| `Game Over Text` | Kéo `GameOverText` vào |
| `Game Over Score Text` | Kéo `GameOverScoreText` vào |
| `Game Over To CTA Delay` | `2` (giây) |
| `Intro Headline` | Kéo `IntroHeadline` (Image) vào |
| `Hand Pointing` | Kéo `HandPointing` (Image) vào |

---

## 7. CTAManager (gắn vào CTAPanel)

### Components
| Component | Ghi chú |
|-----------|---------|
| `CTAManager.cs` | Kéo thả script vào CTAPanel |

### Inspector Assignments
| Field | Giá trị |
|-------|---------|
| `CTA Button` | Kéo `CTAButton` vào |
| `EC Headline` | Kéo `ECHeadline` (Image) vào |
| `Game Info` | Kéo `GameInfo` (Image) vào |
| `Final Score Text` | Kéo TextMeshPro hiển thị score (nếu có) |
| `Store URL` | URL store app (VD: `https://play.google.com/store/apps/details?id=...`) |
| `Fade In Duration` | `0.5` |
| `Button Pulse Scale` | `1.1` |
| `Button Pulse Duration` | `0.8` |

---

## 8. Hierarchy tổng thể (Final)

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
├── TileSpawner          [TileSpawner.cs]
├── InputHandler         [InputHandler.cs]
├── ScoreManager         [ScoreManager.cs]
├── AudioManager         [AudioManager.cs]
└── UICanvas             [Canvas, CanvasScaler, UIManager.cs]
    ├── IntroPanel
    │   ├── IntroHeadline    [Image — Tagline_1.png]
    │   └── HandPointing     [Image — Hand_Pointing.png]
    ├── IngamePanel
    │   ├── ScoreText        [TextMeshProUGUI]
    │   └── ComboText        [TextMeshProUGUI]
    ├── GameOverPanel        [Active=false]
    │   ├── GameOverText     [TextMeshProUGUI]
    │   └── GameOverScoreText [TextMeshProUGUI]
    └── CTAPanel             [Active=false, CTAManager.cs]
        ├── ECHeadline       [Image — EC_Headline.png]
        ├── GameInfo         [Image — Game_Info.png]
        └── CTAButton        [Button — EC_button.png]
```

---

## ✅ Kiểm tra sau khi setup

1. Nhấn **Play** → Intro hiện (headline fade in)
2. Sau 0.5s → WaitingToStart → Ingame panel hiện, score = 0, hand pointing bob animation
3. Tap Start Tile → Playing → hand biến mất, tiles rơi
4. Hold tile → đổi màu vàng → head vào zone → đổi mặt XX
5. Nhả tay → "Good!" hoặc "PERFECT!" + score cập nhật + combo
6. Miss tile → Game Over panel overlay đen → "GAME OVER" scale in → 2s → CTA
7. CTA screen → headline + game info + nút pulse → nhấn nút → mở link store
