---
name: ask-skill
description: Ask the user for clarification before making major decisions.
--- 

**Tuy nhiên, để tránh làm phiền, CHỈ kích hoạt quy tắc này khi gặp các VẤN ĐỀ QUAN TRỌNG có ảnh hưởng lớn đến hiệu năng, thời gian dev (deadline 2 ngày) hoặc dung lượng WebGL.** Không hỏi đối với các chi tiết logic nhỏ hoặc cách đặt tên biến.

**Khi gặp một ngã rẽ QUAN TRỌNG có nhiều cách giải quyết, bạn PHẢI:**
1. **DỪNG VIẾT CODE:** Không cung cấp đoạn code hoàn chỉnh vội.
2. **Trình bày Lựa chọn:** Ngắn gọn liệt kê 2-3 giải pháp khả thi nhất.
3. **Phân tích:** Điểm qua Ưu/Nhược điểm của từng cách (Đặc biệt lưu ý đến Performance và giới hạn 5MB của WebGL).
4. **Khuyến nghị:** Gợi ý cách tốt nhất theo góc nhìn của bạn.
5. **Chờ đợi:** Hỏi tôi: *"Bạn muốn triển khai theo phương án nào?"* và chờ tôi xác nhận trước khi viết code.

**Ví dụ các Vấn đề QUAN TRỌNG (CẦN HỎI):**
- Lựa chọn cách đồng bộ âm thanh (Hardcode mảng hay viết logic Parse MIDI).
- Cách lưu trữ và quản lý state của Game (Dùng Singleton GameManager hay ScriptableObjects).
- Cách cấu trúc Prefab (Tạo nhiều Prefab riêng biệt hay 1 Prefab tự đổi hình ảnh).

**Ví dụ các Vấn đề NHỎ (TỰ ĐỘNG LÀM, KHÔNG HỎI):**
- Cách đặt tên biến, tên hàm.
- Tinh chỉnh padding/margin trong UI.
- Viết các hàm helper, tính toán toán học đơn giản.