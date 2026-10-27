# QUY TẮC CỐT LÕI CHO DỰ ÁN "ONE HIT DOWN"
# (PROJECT STRICT CONSTRAINTS & BEHAVIORAL GUIDELINES)

Mọi phản hồi, phân tích, đề xuất và thao tác viết code trong dự án này BẮT BUỘC phải tuân thủ nghiêm ngặt các điều khoản dưới đây:

---

## 1. NGUYÊN TẮC "NGUỒN SỰ THẬT DUY NHẤT" (SINGLE SOURCE OF TRUTH)
- **Tài liệu chuẩn mực tối thượng:**
  1. `Docs/SCHEMA_CHI_TIET.md`
  2. `Docs/Phan_cong.md`
  3. Báo cáo đồ án: `G:/Slides_BaiGiangTLU/ThietKeGame/document/Báo cáo Game One Hit Down nhóm 505.pdf` (và `.docx`)
- **Tuân thủ danh pháp & thông số 100%:**
  - Tên class, struct, enum, interface (`IResettable`, `GameSession`, `PlayerController`,...).
  - Tên biến thành viên, phương thức, kiểu trả về, danh sách tham số.
  - Tên Namespace: `OneHitDown.Core`, `OneHitDown.Player`, `OneHitDown.Traps`, `OneHitDown.Items`, `OneHitDown.Level`, `OneHitDown.UI`.
  - Tags (`"Hazard"`, `"Player"`, `"Item"`, `"Untagged"`).
  - Layers (Layer 0: `Default`, Layer 8: `Ground`, Layer 9: `Player`, Layer 10: `Traps`).
  - Toàn bộ thông số vật lý & thiết kế: `PPU = 16`, `moveSpeed = 8f`, `groundAcceleration = 65f`, `defaultGravityScale = 3.5f`, `jumpForce = 15.5f`, `doubleJumpForce = 13.5f`, `maxDistance = 4f`,...

---

## 2. TUYỆT ĐỐI KHÔNG TỰ SUY DIỄN / KHÔNG BỊA ĐẶT (ZERO EXTRAPOLATION)
- **NGHIÊM CẤM:**
  - Không tự ý thêm tính năng, cơ chế, bẫy, vật phẩm hoặc quy tắc gameplay nằm ngoài tài liệu.
  - Không tự ý đổi tên file, đổi thư mục code, đổi cấu trúc Prefab so với Mục 4 của `SCHEMA_CHI_TIET.md`.
  - Không tự tiện chọn thư viện ngoài, plugin ngoài hoặc asset không có trong `Assets/Art/`.
- Khi viết mã hoặc giải thích, **phải luôn đối chiếu trực tiếp** với mục tương ứng trong `SCHEMA_CHI_TIET.md`.

---

## 3. NGUYÊN TẮC "CHƯA RÕ THÌ PHẢI HỎI" (ASK BEFORE ACTING)
- Nếu phát hiện:
  1. Bất kỳ sự mâu thuẫn nào giữa các tài liệu.
  2. Một yêu cầu còn bỏ ngỏ hoặc có thể triển khai theo nhiều hướng kỹ thuật khác nhau.
  3. Thiếu thông tin chi tiết mà tài liệu chưa đặc tả.
- **HÀNH ĐỘNG BẮT BUỘC:** 
  - DỪNG LẠI NGAY LẬP TỨC.
  - Liệt kê ngắn gọn điểm chưa rõ và các phương án khả thi.
  - Đặt câu hỏi trực tiếp cho người dùng và CHỜ người dùng phản hồi/chốt phương án trước khi viết bất kỳ dòng code nào.

---

## 4. BẢO VỆ TOÀN VẸN CẤU TRÚC CODE & GIT
- Tuân thủ phân công công việc của từng thành viên theo `Phan_cong.md`.
- Mỗi thành viên chỉ chỉnh sửa trong thư mục code, bộ Prefab và Scene của riêng mình.
- Giữ nguyên toàn bộ comment kỹ thuật, docstrings và quy chuẩn kiến trúc: Component-based, Prefab-First, Event-Driven, `IResettable`.

