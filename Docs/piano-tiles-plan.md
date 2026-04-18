# Piano Tiles 4 Lane - Kế hoạch Triển khai Playable Ads (Unity WebGL)

## 1. Objective theo Brief
Triển khai playable game tương tự Sample Playable trên Unity WebGL, bảo đảm nhẹ, tải nhanh, chạy mượt trên nhiều kích thước màn hình, đáp ứng yêu cầu đóng gói cho IronSource, AppLovin, Mintegral và có thể tích hợp Luna.

## 2. Success Criteria
1. Gameplay tương đương bản mẫu về core loop và cảm giác chơi.
2. Một phiên bản Unity source duy nhất, xuất được 3 bản build preview cho 3 network.
3. Khả năng responsive tốt trên tỉ lệ màn hình dọc phổ biến của mobile ads.
4. Asset nén hợp lý, tổng dung lượng build tối ưu cho playable ads.
5. Có thêm 3 cải tiến sáng tạo về tương tác, animation hoặc VFX.

## 3. Scope kỹ thuật bắt buộc
1. Unity WebGL tối ưu tải nhanh và runtime nhẹ.
2. Hệ Tile Spawner và Object Pooling làm xương sống.
3. Hệ input phản hồi nhanh theo lane.
4. Cấu hình responsive theo safe area và nhiều aspect ratio.
5. Chuẩn bị packaging tương thích Luna (nếu áp dụng trong pipeline).

## 4. Kiến trúc triển khai

### 4.1 Gameplay Core
- GameManager: state, score, tốc độ, flow start/end.
- TileSpawner: spawn theo nhịp, kiểm soát lane pattern.
- PoolManager: mượn và trả tile từ pool.
- LaneInputController: map input sang 4 lane.
- LaneHitResolver: quyết định hit hoặc miss.
- TileController: vòng đời tile, di chuyển, trạng thái.

### 4.2 Ads Runtime Layer
- BuildProfileManager: chứa config theo network.
- PlayableBootstrap: entry point riêng cho WebGL ad.
- CTAController: logic hiển thị CTA đúng thời điểm.
- AudioGate: bật tắt âm thanh theo policy network.

### 4.3 Responsive Layer
- AdaptiveCanvasController: scale UI theo width/height.
- CameraBoundsController: giữ vùng gameplay nhất quán.
- SafeAreaFitter: tránh cắt UI quan trọng ở màn hình hẹp.

### 4.4 Cấu trúc file cập nhật (đề xuất áp dụng)
```text
Assets/
 └── Scripts/
	 ├── Core/
	 │   └── Game/
	 │       ├── GameManager.cs
	 │       ├── GameState.cs
	 │       └── GameConfig.cs
	 ├── Gameplay/
	 │   ├── Pooling/
	 │   │   ├── IPoolable.cs
	 │   │   ├── PoolConfig.cs
	 │   │   ├── ObjectPool.cs
	 │   │   └── PoolManager.cs
	 │   ├── Spawning/
	 │   │   ├── TileSpawner.cs
	 │   │   ├── SpawnPatternProvider.cs
	 │   │   └── SpawnSequence.cs
	 │   ├── Tiles/
	 │   │   ├── TileController.cs
	 │   │   ├── TileType.cs
	 │   │   └── TileMovement.cs
	 │   └── Lanes/
	 │       ├── LaneInputController.cs
	 │       └── LaneHitResolver.cs
	 ├── Runtime/
	 │   ├── PlayableBootstrap.cs
	 │   ├── BuildProfileManager.cs
	 │   ├── CTAController.cs
	 │   └── AudioGate.cs
	 └── UI/
		 ├── HUDController.cs
		 ├── StartPanelController.cs
		 └── GameOverPanelController.cs
```

Ghi chú migration:
- Chuẩn hóa dùng 1 tên thư mục duy nhất là `Gameplay` (không dùng song song `GamePlay`).
- File `TileController` hiện có cần chuyển dần về `Assets/Scripts/Gameplay/Tiles/` để tránh phân tán logic.

## 5. Kế hoạch theo giai đoạn

### Giai đoạn 1 - Core & Data Config
Mục tiêu:
- Chốt khung state machine và luồng game chuẩn cho playable ads.
- Tập trung toàn bộ tham số gameplay vào ScriptableObject để tinh chỉnh nhanh.

Đầu ra:
- Gameplay loop cơ bản chạy được.
- Thông số điều chỉnh qua ScriptableObject.

Phạm vi công việc chi tiết:
1. Hoàn thiện `GameState` với các trạng thái tối thiểu: Idle, Playing, Paused, GameOver.
2. Hoàn thiện `GameManager`:
	- State transition an toàn.
	- Score + difficulty scaling theo score.
	- Event phát cho UI và gameplay systems.
3. Hoàn thiện `GameConfig`:
	- Lane X positions (4 lane).
	- Spawn interval, spawn Y, miss Y.
	- Base speed và speed increase.
	- Hit window top/bottom.
4. Chuẩn hóa naming và cấu trúc thư mục để chuẩn bị mở rộng giai đoạn 3-6.

Definition of Done (Giai đoạn 1):
1. Có thể Start -> Playing -> GameOver -> Reset ổn định.
2. Sửa tham số trong `GameConfig` ảnh hưởng trực tiếp gameplay mà không cần sửa code.
3. Không có compile error trong các file thuộc Core/Game.

### Giai đoạn 2 - Object Pooling
Mục tiêu:
- Hoàn thiện hệ pooling dùng lại được cho tile và VFX ngắn hạn.
- Loại bỏ hoàn toàn vòng lặp Instantiate/Destroy trong gameplay runtime.

Đầu ra:
- Không Instantiate hoặc Destroy liên tục trong gameplay loop.

Phạm vi công việc chi tiết:
1. Định nghĩa `IPoolable` với callback vòng đời khi Spawn/Despawn.
2. Hoàn thiện `ObjectPool`:
	- Queue inactive + tập active.
	- Hỗ trợ prewarm.
	- Hỗ trợ giới hạn max size và tùy chọn expand.
3. Hoàn thiện `PoolManager`:
	- Quản lý nhiều pool qua `PoolConfig`.
	- Spawn theo key.
	- Despawn theo key hoặc theo instance.
	- DespawnAllActive khi end game.
4. Chuẩn bị tương thích cho tile prefab và effect prefab dùng chung pipeline.

Definition of Done (Giai đoạn 2):
1. Có thể prewarm toàn bộ pool trước gameplay.
2. Tile spawn/despawn hoạt động ổn định trong 2-3 phút liên tục không phình GC bất thường.
3. Khi TriggerGameOver, toàn bộ object active được thu hồi về pool.
4. Không có compile error trong các file thuộc Gameplay/Pooling.

### Giai đoạn 3 - Tile Spawner 4 Lane
Mục tiêu:
- Xây SpawnPatternProvider có kiểm soát randomness.
- Tránh tạo pattern gây unfair hoặc quá trống.

Đầu ra:
- Spawner sinh lane ổn định, mượt và đúng nhịp.

### Giai đoạn 4 - Input, Hit/Miss, Fail State
Mục tiêu:
- Xử lý input theo lane và hit window.
- Xử lý miss, wrong tap, game over.

Đầu ra:
- Cảm giác chơi chính xác và phản hồi nhanh.

### Giai đoạn 5 - Visual Polish + 3 Ý tưởng cải tiến
Mục tiêu:
- Thêm 3 hạng mục nâng trải nghiệm.

Đề xuất 3 cải tiến:
1. Perfect Streak FX: combo đủ mốc thì bật trail và pulse lane.
2. Dynamic Beat Camera: camera rung nhẹ theo nhịp hit chuẩn.
3. Near Miss Indicator: cảnh báo lane sắp miss bằng glow ngắn.

Đầu ra:
- Trải nghiệm bắt mắt hơn và giữ người chơi lâu hơn trong ad.

### Giai đoạn 6 - Responsive + Compression + Build Pipeline
Mục tiêu:
- Tối ưu texture, audio, shader, stripping code.
- Cấu hình build cho 3 network và HTML preview.
- Chuẩn bị Luna package khi cần.

Đầu ra:
- 03 bản WebGL previewable cho IronSource, AppLovin, Mintegral.
- Bộ HTML preview local cho QA nhanh.

## 6. Kế hoạch đóng gói Deliverable

### 6.1 Cấu trúc thư mục bàn giao
1. UnityProject
2. Builds
3. Previews
4. Optional_LunaPackage

### 6.2 Chi tiết đầu ra
1. Builds/ironsource_webgl
2. Builds/applovin_webgl
3. Builds/mintegral_webgl
4. Previews/ironsource_preview.html
5. Previews/applovin_preview.html
6. Previews/mintegral_preview.html
7. README hướng dẫn chạy local preview và checklist QA

## 7. Checklist tối ưu WebGL
1. Bật compression phù hợp cho WebGL build.
2. Dùng sprite atlas, giảm texture size theo nhu cầu hiển thị.
3. Giảm số draw call và tránh material dư thừa.
4. Tránh alloc GC trong Update và input loop.
5. Tắt các package Unity không dùng.
6. Cân nhắc IL2CPP stripping level phù hợp.
7. Dùng object pooling cho tile và hiệu ứng tái sử dụng được.

## 8. QA và nghiệm thu
1. Load time: đo thời gian vào playable trên thiết bị test.
2. FPS stability: giữ khung hình ổn định trong suốt session.
3. Responsive: test nhiều tỉ lệ màn hình dọc.
4. Input accuracy: không miss ảo hoặc nhận sai lane.
5. Asset fit: không méo, không cắt nội dung quan trọng.
6. Network preview: mở được đúng trên 3 HTML preview.
7. CTA flow: đúng timing và không phá gameplay.

## 9. Dependencies cần bạn cung cấp để chốt triển khai
1. Link Sample Playable chính xác để benchmark.
2. Link Brief Playable full asset.
3. Yêu cầu cụ thể về CTA, end card, logo, font brand.
4. Giới hạn dung lượng mục tiêu cho từng network nếu có.

## 10. Lộ trình thực hiện đề xuất
1. Tuần 1: Hoàn tất Giai đoạn 1, 2, 3.
2. Tuần 2: Hoàn tất Giai đoạn 4, 5.
3. Tuần 3: Hoàn tất Giai đoạn 6, QA và đóng gói deliverable.