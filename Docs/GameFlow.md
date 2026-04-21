# 📄 TÀI LIỆU THIẾT KẾ GAMEPLAY (GDD)
**Dự án:** Playable Ad - Rhythm Game (Theme: Squid Game)

**Nền tảng:** Unity WebGL

**Thời lượng dự kiến:** 15 - 30 giây

---

## 1. TỔNG QUAN (OVERVIEW)
Đây là một phiên bản rút gọn mang tính chất quảng cáo tương tác (Playable Ad) dựa trên cơ chế của thể loại game Rhythm (tương tự Piano Tiles). Trò chơi kết hợp nhịp điệu âm nhạc dồn dập với giao diện thiết kế theo chủ đề lính gác màu hồng của "Squid Game". 

**Mục tiêu thiết kế:** Đề cao tính giải trí nhanh, hình ảnh phản hồi thị giác cực mạnh (satisfying) và có tính tha thứ cao (forgiving) nhằm giữ chân người chơi đi đến màn hình Call-To-Action (CTA) cuối cùng.

---

## 2. LUẬT CHƠI CỐT LÕI (CORE RULES)
* **Giao diện:** Màn hình dọc, chia thành các làn (lanes). Các chướng ngại vật (Tile) trượt từ trên xuống dưới theo nhịp điệu bài hát.
* **Cơ chế tha thứ (Forgiving):** Việc chạm (tap) nhầm vào làn trống hoặc khoảng không trên màn hình sẽ **không bị phạt** (không Game Over, nhạc vẫn tiếp tục).
* **Điều kiện Thua (Game Over):** Trò chơi kết thúc ngay lập tức nếu người chơi bỏ lỡ bất kỳ Tile nào (để Tile trượt ra khỏi cạnh dưới của màn hình mà chưa được xử lý xong).

---

## 3. HỆ THỐNG CHƯỚNG NGẠI VẬT (TILE SYSTEM)

Toàn bộ các Tile trong game đều mang hình dáng của lính gác Squid Game, bao gồm 2 phần: **Đầu (Head)** mặc định có ký hiệu hình Tam Giác và **Thân (Body)** màu hồng.

### 3.1. Phân loại Tile
1. **Start Tile (Tile Khởi động):**
   * **Hiển thị:** Phần thân có chữ "START".
   * **Cơ chế:** Nằm cố định ở đầu màn hình. Chạm vào để bắt đầu phát nhạc và kích hoạt màn hình cuộn xuống.
2. **Short Tile (Tile Ngắn):**
   * **Hiển thị:** Lính gác dáng lùn (Thân ngắn). Xuất hiện ở các nhịp điệu nhanh.
3. **Long Tile (Tile Dài):**
   * **Hiển thị:** Lính gác dáng cao (Thân dài). Xuất hiện ở các đoạn nhạc ngân dài hoặc điệp khúc.

### 3.2. Cơ chế Tương tác Đồng nhất: "Hold to Eliminate"
Dù là Short Tile hay Long Tile, người chơi đều phải áp dụng **một thao tác duy nhất là Chạm & Giữ (Hold)** khi phần đáy của Tile chạm vào vùng bấm (Hit Zone) ở dưới màn hình. Sự khác biệt chỉ nằm ở thời gian giữ ngón tay ngắn hay dài.

---

## 4. TƯƠNG TÁC & HỆ THỐNG ĐIỂM (INTERACTION & SCORING)

Đây là yếu tố "Game Sense" trọng tâm, áp dụng cơ chế rủi ro - phần thưởng (Risk & Reward) để tạo sự thỏa mãn:

* **Trạng thái Mặc định:** Tile trượt xuống với phần thân màu Hồng và khuôn mặt Tam Giác.
* **Trạng thái Đang giữ (Holding):** * Ngay khi ngón tay chạm vào Tile, toàn bộ phần thân (Body) từ từ đổi sang **Màu Vàng (Yellow)**. 

Hệ thống sẽ ghi nhận điểm số dựa trên thời điểm người chơi thả ngón tay ra (Release):

### 🎯 Trường hợp 1: Good Hit (Nhả tay sớm)
* **Hành động:** Người chơi chạm, giữ (thân chuyển vàng) nhưng nhả ngón tay ra *trước khi* phần Đầu (Head) của lính gác trôi xuống vùng Hit Zone.
* **Kết quả:** *
  * Nhận **Điểm Cơ bản** (Ví dụ: +3 điểm).
  * Text nổi lên: *"Good!"*
  * Trò chơi tiếp tục bình thường.

### 🌟 Trường hợp 2: Perfect Hit (Giữ đến cùng - Đổi mặt XX)
* **Hành động:** Người chơi giữ ngón tay liên tục. Phần thân màu vàng trượt dần xuống cho đến khi **phần Đầu (Head)** chạm đúng vào vùng Hit Zone.
* **Kết quả bùng nổ (Climax):**
  * Ngay khoảnh khắc chạm đến đầu, **Khuôn mặt Tam Giác lập tức chuyển đổi (Swap) sang Khuôn mặt XX (Thè lưỡi, có máu)**.
  * Nhận **Điểm Cao** (Ví dụ: +6 điểm).
  * Text nổi lên, phóng to: *"PERFECT!"* hoặc *"AWESOME!"*

---

## 5. YÊU CẦU KỸ THUẬT & BÀN GIAO (OBJECTIVE & DELIVERABLES)

### 5.1. Yêu cầu Kỹ thuật (Technical Requirements)
* **Tối ưu hóa:** Tải nhanh (Fast loading) và dung lượng nhẹ (Lightweight). Chỉ sử dụng 1 phiên bản Unity build (WebGL).
* **Trải nghiệm:** Gameplay mượt mà (Seamless gameplay), phản hồi tốt và tự động thích ứng (responsive) trên nhiều kích thước màn hình thiết bị di động khác nhau.
* **Đồ họa:** Tất cả asset đồ họa phải đảm bảo hiển thị đúng tỷ lệ, đáp ứng chuẩn yêu cầu (brief) và được xử lý nén (compression) đúng cách để tối ưu dung lượng.
* **Luna Playable:** Tương thích với các tiêu chuẩn đóng gói của Luna Playable (nếu có kinh nghiệm áp dụng).

### 5.2. Sản phẩm Bàn giao (Deliverables)
* **Build Files:** 03 file build WebGL đã được tối ưu hóa và có thể xem trước (previewable) cho các network quảng cáo: **IronSource, AppLovin, Mintegral**.
* **Đóng gói:** Format sẵn sàng để tích hợp Luna (nếu áp dụng).
* **Thư mục dự án:** Bao gồm thư mục chứa HTML preview hoạt động tốt và toàn bộ thư mục Unity Project (Source code).

---