# Long Tile Hold Fill Effect — Unity 2D Implementation Prompt

---

## Prompt (paste vào Cursor / GitHub Copilot / Claude)

```
You are a senior Unity 2D engineer. Implement a hold-fill visual effect on a Long Tile prefab. Make only the changes described below. Do not refactor, rename, or add features beyond what is asked.

## Context (carry forward)
- Engine: Unity 2D, URP
- Tile prefab hierarchy:
  Long Tile (root)
  ├── Body   (SpriteRenderer — white/dark body sprite, pivot center)
  └── Head   (SpriteRenderer — circular head sprite)
- Project uses DOTween (available)
- Input system: [INSERT YOUR INPUT SYSTEM — e.g. IPointerDownHandler / custom TouchManager]
- Sorting Layer for tiles: "Tiles"

## Task
Add a bottom-to-top yellow fill animation on Body that plays while the player holds the tile, and resets if released early.

## Implementation steps — follow in order, confirm each with ✅ before continuing

### Step 1 — Sprite Setup (do this manually, confirm when done)
- Open Body's sprite in Sprite Editor
- Set pivot to Bottom (X = 0.5, Y = 0)
- Apply and close

### Step 2 — Hierarchy changes inside Long Tile prefab
Add these two GameObjects as children of Body:

FillMask (child of Body)
  - Add component: SpriteMask
  - Sprite: same sprite as Body
  - localPosition: (0, 0, 0)
  - localScale: (1, 0, 1)   ← starts hidden

FillSprite (child of FillMask)
  - Add component: SpriteRenderer
  - Sprite: same sprite as Body
  - Color: #FFD700 (yellow)
  - Mask Interaction: Visible Inside Mask
  - Sorting Layer: Tiles
  - Order in Layer: Body's order + 1

### Step 3 — Create TileHoldEffect.cs
Attach to Long Tile root. Implement:

public class TileHoldEffect : MonoBehaviour
{
    [SerializeField] Transform fillMask;      // assign FillMask in Inspector
    [SerializeField] float holdDuration = 1f; // seconds to fill completely

    float _progress;
    bool _holding;

    void Update()
    {
        if (!_holding) return;
        _progress += Time.deltaTime / holdDuration;
        _progress = Mathf.Clamp01(_progress);
        fillMask.localScale = new Vector3(1f, _progress, 1f);
        if (_progress >= 1f) OnHoldComplete();
    }

    public void StartHold()
    {
        _holding = true;
        _progress = 0f;
    }

    public void StopHold()
    {
        _holding = false;
        _progress = 0f;
        fillMask.localScale = new Vector3(1f, 0f, 1f);
    }

    void OnHoldComplete()
    {
        _holding = false;
        // TODO: call your tile-clear logic here
        Debug.Log("[LongTile] Hold complete");
    }
}

### Step 4 — Wire input
In the existing script that handles touch/click on Long Tile, call:
- holdEffect.StartHold()  when finger/mouse presses down on this tile
- holdEffect.StopHold()   when finger/mouse releases

If no such script exists, create TileInput.cs on Long Tile root implementing
IPointerDownHandler and IPointerUpHandler, require PhysicsRaycaster on Camera.

## Constraints
- MUST NOT touch Head GameObject or its SpriteRenderer
- MUST NOT change Body's SpriteRenderer color or sprite reference
- MUST NOT use Coroutines — use Update loop as shown
- MUST assign fillMask in the Inspector, not via GetComponentInChildren at runtime
- Output ONLY the two C# files (TileHoldEffect.cs, TileInput.cs if needed) and a
  numbered checklist of manual Editor steps. No explanations beyond that.
```

---

## Manual Editor Checklist (làm trước khi paste prompt)

- [ ] Mở **Body** sprite → Sprite Editor → đặt pivot `X=0.5, Y=0` → Apply
- [ ] Tạo `FillMask` (Empty GameObject, child of Body) → gắn `SpriteMask`
- [ ] Tạo `FillSprite` (child of FillMask) → gắn `SpriteRenderer`, color `#FFD700`, Mask Interaction = Visible Inside Mask
- [ ] Gắn `TileHoldEffect.cs` vào **Long Tile** root
- [ ] Kéo `FillMask` vào field `fillMask` trong Inspector
- [ ] Điền `holdDuration` theo nhịp bài hát (thường = độ dài tile tính bằng giây)

---

## Điền vào trước khi dùng

| Placeholder | Giá trị cần điền |
|---|---|
| `[INSERT YOUR INPUT SYSTEM]` | `IPointerDownHandler` / `TouchManager` / tên class hiện tại |
| `holdDuration` | Thời gian giữ (giây) — khớp với BPM bài |
| `OnHoldComplete()` | Tên hàm clear tile trong game logic của bạn |
