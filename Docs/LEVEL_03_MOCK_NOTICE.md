# THÔNG BÁO QUAN TRỌNG VỀ SCENE LEVEL_03 & CÁC MOCK SCRIPTS TẠM THỜI
# (DÀNH CHO TẤT CẢ THÀNH VIÊN NHÓM 505: TRUNG, TÚ, UYÊN)

**Người gửi:** Trần Văn Tuyên (Phụ trách Module 3 & Thiết kế Scene `Level_03.unity`)  
**Ngày cập nhật:** 28/10/2026  
**Mục đích:** Giải thích rõ ranh giới code và bảo vệ phân công công việc của từng thành viên khi kéo (git pull) code về.

---

## 1. PHÂN ĐỊNH RANH GIỚI TRÁCH NHIỆM (TUÂN THỦ `Phan_cong.md`)

- **File Scene `Assets/Scenes/Level_03.unity`:** Thuộc quyền phụ trách duy nhất của **Trần Văn Tuyên** (theo `Docs/Phan_cong.md` Mục II & IV). Tuyên đã dựng hoàn chỉnh 100% địa hình và bố cục chướng ngại vật Màn 3 theo đúng 5 ảnh thiết kế Báo cáo (`Anh_Minh_Hoa_Bao_Cao/màn 3/`).
- **Để Màn 3 có thể bấm Play thử nghiệm (Test-Run) được ngay**, Tuyên đã tạo một số **Mock Scripts tạm thời** đặt riêng biệt trong thư mục:
  `Assets/Scripts/Dev/Mocks/` (Namespace: `OneHitDown.Dev.Mocks`)

---

## 2. CẢNH BÁO CHO CÁC THÀNH VIÊN (TUYỆT ĐỐI CHÚ Ý):

Các script trong `Assets/Scripts/Dev/Mocks/` **CHỈ LÀ BẢN DỰNG TẠM THỜI (MOCK-SAFE)** để phục vụ kiểm thử Màn 3, **HOÀN TOÀN KHÔNG PHẢI VÀ KHÔNG THAY THẾ CHO CODE CHÍNH THỨC CỦA CÁC BẠN!**

Khi các thành viên kéo code về:
1. **Trần Đức Trung (Người 2 - Module 2):**
   - VẪN BẮT BUỘC phải viết mã nguồn chính thức tại `Assets/Scripts/Traps/`:
     - `TrapBase.cs` (Lớp cơ sở kế thừa `IResettable`)
     - `FireTrap.cs` (Vạc dầu ngầm phun lửa chu kỳ 2s tắt - 0.35s báo - 1s bật)
     - `SpikedBallPendulum.cs` (Cầu gai con lắc xích sắt)
     - `SawSpinner.cs`
   - Đóng gói Prefabs chính thức tại `Assets/Prefabs/Traps/`.
   - **KHÔNG ĐƯỢC** lấy các file `MockFireTrap.cs`, `MockSpikedBall.cs` làm bài nộp của Module 2.

2. **Phan Văn Tú (Người 4 - Module 4):**
   - VẪN BẮT BUỘC phải viết mã nguồn chính thức tại `Assets/Scripts/Items/` và `Assets/Scripts/Level/`:
     - `CollectibleItem.cs` (Hệ thống 8 loại quả, Kiwi mở cổng, Dâu tây thưởng, kế thừa `IResettable`)
     - `MovingPlatform.cs` (Bệ nâng tuần tra X/Y kế thừa `IResettable`)
     - `LevelStartPoint.cs` & `ExitGate.cs` (Cổng Thần Cổ `[3/3]` mở cổng + pháo hoa)
   - Đóng gói Prefabs chính thức tại `Assets/Prefabs/`.
   - **KHÔNG ĐƯỢC** lấy các file `MockMovingPlatform.cs`, `MockCollectible.cs` làm bài nộp của Module 4.

3. **Hoàng Thị Giang Uyên (Người 5 - Module 5):**
   - VẪN BẮT BUỘC phải viết mã nguồn chính thức tại `Assets/Scripts/Core/`:
     - `SoulTether.cs` (Dây xích co-op 2P)
     - `GameManager.cs` (Trọng tài hồi sinh tức thì)
     - `AudioManager.cs`, `HUDController.cs`

---

## 3. CÁCH THAY THẾ KHI CÁC BẠN HOÀN THÀNH SCRIPT CHÍNH THỨC

Trong Scene `Assets/Scenes/Level_03.unity`, toàn bộ các vật thể tạm thời đã được gom nhóm rõ ràng trong Hierarchy:
- `--- [MOCK FOR TEST - CHO NGUOI 2 HOAN THIEN: LUA, GAI, CAU GAI] ---`
- `--- [MOCK FOR TEST - CHO NGUOI 4 HOAN THIEN: BE NANG, KIWI, DAU TAY, DICH] ---`

Khi Người 2 và Người 4 hoàn thành Prefab hoặc Script chuẩn của mình:
- Chỉ cần kéo Prefab chính thức đè vào đúng vị trí tọa độ đó, hoặc gỡ bỏ component Mock và gán component chính thức vào. Tọa độ X, Y đã được căn chuẩn pixel theo thiết kế.
- Sau khi toàn bộ dự án hoàn thiện, thư mục `Assets/Scripts/Dev/Mocks/` sẽ được xóa sạch mà không ảnh hưởng bất kỳ điều gì đến kiến trúc gốc.
