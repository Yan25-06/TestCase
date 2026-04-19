---
name: setup_skill
description: Hướng dẫn AI luôn bắt buộc phải kèm theo các bước thiết lập chi tiết trong Unity Editor (Hierarchy, Components, Inspector, Tags) mỗi khi cung cấp code C#.
---

Đối với MỖI đoạn code hoặc tác vụ bạn cung cấp, nếu nó yêu cầu bất kỳ thiết lập cấu hình nào trong Unity Editor, bạn PHẢI bao gồm một phần riêng biệt có tên "**🛠️ Unity Setup Guide**" (Hướng dẫn Thiết lập Unity) ở trước hoặc sau đoạn code.

Hướng dẫn Thiết lập của bạn phải nêu chi tiết:
1. **Hierarchy (Hệ thống phân cấp):** Script này nên được gắn vào GameObject nào? (ví dụ: "Gắn vào prefab Player", "Tạo một Empty GameObject có tên là GameManager").
2. **Components (Thành phần):** Cần có những component nào khác trên GameObject đó? (ví dụ: "Cần một Rigidbody2D với Gravity Scale bằng 0", "Cần một BoxCollider2D được đánh dấu IsTrigger").
3. **Inspector Assignments (Gán biến trong Inspector):** Những biến nào cần được liên kết trong cửa sổ Inspector? (ví dụ: "Kéo MainCamera vào trường `cam`", "Thiết lập `moveSpeed` bằng 5").
4. **Tags & Layers (Thẻ & Lớp):** Đoạn code này có phụ thuộc vào các Tag hoặc Layer cụ thể nào không? (ví dụ: "Đảm bảo object mặt đất được gắn tag là 'Ground'").

KHÔNG ĐƯỢC chỉ cung cấp mỗi đoạn code. Hãy cung cấp cho tôi đoạn code VÀ các thao tác thiết lập chính xác trong Editor để nó có thể hoạt động.