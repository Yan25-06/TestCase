# 🎵 Rhythm Game Playable Ad (Squid Game Theme)

Một game nhịp điệu (Rhythm Game) tối ưu hóa cho nền tảng WebGL, được thiết kế dưới dạng Playable Ad (Quảng cáo chơi được) với chủ đề Squid Game. Game sở hữu hệ thống tự động căn chỉnh giao diện theo chiều xoay thiết bị (ngang/dọc), hệ thống tile pooling tối ưu, và hệ thống beatmap tùy biến.

## 🔗 Chơi thử (Play the Game)

▶️ **[Chơi thử trên Unity Play tại đây](https://play.unity.com/en/games/db44e27c-ca39-4f26-817b-6c4be1719aa8/testcase4)**

## 🌟 Tính Năng Nổi Bật

- **Responsive Design**: Tự động nhận diện thiết bị đang xoay dọc (Portrait) hay ngang (Landscape) để tự động căn chỉnh kích thước Camera, Background, và độ rộng các Lane cho phù hợp.
- **Tối ưu WebGL**: 
  - Sử dụng **Object Pooling** cho toàn bộ hệ thống Tile (Short/Long/Start) để loại bỏ việc tạo/hủy object liên tục (Garbage Collection), giúp game chạy mượt trên các thiết bị yếu.
  - Tối ưu dung lượng để đảm bảo build size dưới 5MB phục vụ cho Ad Networks.
- **Hệ Thống Beatmap Tùy Biến**: Sử dụng ScriptableObject `GameConfig` cho phép thiết kế các nhịp beatmap thủ công (hỗ trợ cả chạm thả ngắn và giữ dài).
- **Điều Khiển Đa Nền Tảng**:
  - **Mobile**: Hỗ trợ cảm ứng đa điểm (Multi-touch) thông qua EnhancedTouch API của Unity (mỗi ngón tay cho một lane).
  - **Desktop**: Hỗ trợ nhấp chuột cho việc test và chơi trên PC.
- **UI & Hiệu Ứng Trực Quan**:
  - Màn hình Intro có hướng dẫn chạm nhấp nháy, bàn tay chỉ dẫn.
  - Phản hồi trực quan (Good / PERFECT) kết hợp hiệu ứng rung màn hình.
  - Game Over có hiệu ứng Slow-motion (cảnh báo chớp đỏ) trước khi chuyển sang màn hình tải game.
  - Màn hình chiến thắng có hiệu ứng nảy "CLEARED!".
  - Màn hình Call-To-Action (CTA) với nút Download nổi bật, kêu gọi hành động.

## 🏗️ Tổng Quan Kiến Trúc Code

Project được cấu trúc theo dạng các module độc lập, giao tiếp với nhau bằng Event-driven (C# events):
- **`GameManager`**: Máy trạng thái chính (State Machine) điều phối luồng Intro → Playing → GameOver / GameCleared → CTA.
- **`OrientationManager`**: Tính toán và căn chỉnh tỷ lệ màn hình, chiều rộng lane dựa vào tỉ lệ hiển thị thực tế.
- **`TileSpawner` & `TilePool`**: Xử lý logic sinh ra các nốt nhạc dựa trên mốc thời gian của Beatmap.
- **`InputHandler`**: Chuyển đổi tọa độ màn hình (Touch/Mouse) sang các lane cụ thể để xử lý va chạm.
- **`UIManager` & `CTAManager`**: Quản lý việc bật/tắt các lớp giao diện và xử lý nút mở Link Download ở màn hình cuối.

*Để xem bản phân tích luồng code chi tiết và sâu hơn, vui lòng xem tại [`Docs/game_analysis.md`](Docs/game_analysis.md).*
