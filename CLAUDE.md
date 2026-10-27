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
  2. Một yêu cầu còn bỏ ngỏ hoặc có thể triển khai theo nhiều hướng kỹ thuật khác nhau (ví dụ: cách đọc phím qua `Input.GetKey` hay new `Input System`).
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

---

## 5. CHUẨN API UNITY 6 & C# (BẮT BUỘC)

Dự án chạy trên **Unity 6 (6000.x)**. Tuyệt đối không dùng các API đã bị deprecated:

| Cấm (deprecated) | Dùng thay thế |
| :--- | :--- |
| `rb.velocity` | `rb.linearVelocity` |
| `FindObjectsOfType<T>()` | `FindObjectsByType<T>(FindObjectsSortMode.None)` |
| `FindObjectOfType<T>()` | `FindFirstObjectByType<T>()` |

- **Đóng gói dữ liệu:** mọi biến cần chỉnh trong Inspector phải khai báo `[SerializeField] private Type tenBien;` — **cấm** `public Type tenBien;`. Ngoại lệ duy nhất: các field `[SerializeField] protected` của lớp cơ sở như `TrapBase` được giữ nguyên đúng theo Schema để các lớp con kế thừa đọc trực tiếp. Script khác truy cập dữ liệu qua thuộc tính biểu thức: `public Type TenThuocTinh => tenBien;` (đúng mẫu `public bool IsDead => isDead;` trong Schema).
- **Không cấp phát rác trong vòng lặp (Zero GC Alloc):** cấm gọi `GetComponent<T>()`, `Find*`, LINQ (`.OfType<>()`, `.ToArray()`,...), `new List/Array` hoặc ghép chuỗi động bên trong `Update()` / `FixedUpdate()`. Toàn bộ tham chiếu phải cache trong `Awake()` / `Start()`. **Ngoại lệ duy nhất:** format text HUD — chỉ dựng lại chuỗi khi giá trị số thực sự thay đổi.
- **So sánh Tag:** dùng `CompareTag("Hazard")` thay vì `tag == "Hazard"` (tránh cấp phát chuỗi).
- **Kiểm tra va chạm:** theo đúng đặc tả của từng file trong Schema (PlayerController và CollectibleItem so Tag qua `CompareTag("Hazard")` / `CompareTag("Player")`; bẫy và cơ quan kiểm tra va chạm với `PlayerController`). Không tự đổi cơ chế này.

---

## 6. AN TOÀN GIT & FILE `.meta` (CHỐNG HỎNG ASSET)

- **Cấm commit file rác / sinh tự động:** tuyệt đối không commit các thư mục và file sinh tự động: `Library/`, `Temp/`, `obj/`, `Logs/`, `Build/`, `UserSettings/`, `*.csproj`, `*.sln` (chỉ commit các thư mục mã nguồn `Assets/`, `Packages/`, `ProjectSettings/`, `Docs/`, tài liệu dự án và file cấu hình theo phân công).
- **Cấm tự ý thêm Tag/Layer mới:** chỉ sử dụng danh mục Tag & Layer chuẩn đã định nghĩa tại Mục 1. Chỉ Người 1 phụ trách cấu hình và commit `ProjectSettings/TagManager.asset`.
- **Cấm sửa tay `.meta`:** mỗi file `.cs`, `.png`, `.prefab`, `.unity`, `.asset` đều có file `.meta` đi kèm giữ GUID. Không tự tạo, xóa, đổi GUID hay chỉnh nội dung `.meta` khi người dùng không yêu cầu rõ ràng.
- **Đổi tên / di chuyển asset:** phải thực hiện trong Unity Editor, hoặc di chuyển nguyên cặp `<file>` + `<file>.meta`. Cấm để `.meta` mồ côi hoặc tạo asset trùng GUID.
- **Line Endings:** giữ nguyên định dạng dòng mặc định của môi trường/file hiện tại (CRLF trên Windows), tuyệt đối không tự ý chuyển đổi hàng loạt (convert line endings) gây nhiễu Git diff.
- **Bảo vệ Scene (Zero Conflict):**
  - Không tự mở, sửa hay lưu file Scene màn chơi của người khác. Quyền sở hữu theo `Phan_cong.md` Mục II & IV:
    - `Level_01.unity` — Tuấn | `Level_02.unity` — Trung | `Level_03.unity` — Tuyên | `Level_04.unity` + `MainMenu.unity` — Tú | `Level_05.unity` — Uyên.
  - Scene nháp cá nhân để test cơ chế: `Assets/Scenes/Dev/Dev_Nguoi1..5.unity`.
  - Chỉ commit mã nguồn `.cs`, `.prefab` và file Scene thuộc quyền phụ trách.

---

## 7. PREFAB-FIRST — TÍNH TỰ CHỨA (SELF-CONTAINED)

- Script gắn trên Prefab (Bẫy, Cơ quan, Quả, Bệ) **không được** giữ tham chiếu cứng tới GameObject nằm ngoài chính Prefab đó. Tham chiếu tới Collider con bên trong Prefab là hợp lệ (ví dụ child `flameHitbox` của `FireTrap` hoặc child `DeathTriggerZone` của `DeathPit`).
- Giao tiếp với nhân vật và hệ thống phải thông qua Component hoặc Tag/Collision — không kéo-thả Scene object.
- **Ngoại lệ duy nhất (đúng theo Schema):** `GameManager` được giữ tham chiếu Scene (`startPoint`, `exitGate`, `player1Prefab`, `player2Prefab`, `soulTetherPrefab`) và truy cập `GameManager.Instance`; các script Menu/UI thuộc Module 4 & 5 được phép tham chiếu Scene theo đặc tả. Ngoài các trường hợp này, cấm hard-reference chéo Scene.
- Mọi chỉnh sửa thông số Bẫy / Nhân vật / Quả phải **Apply vào Prefab gốc** tại `Assets/Prefabs/`, không chỉnh Instance trong Scene.

---

## 8. QUY CHUẨN COMMENT & VỊ TRÍ FILE

- Comment tiếng Việt có dấu, ngắn gọn, nêu mục đích hàm và sự kiện phát ra; field `[SerializeField]` ghi chú đơn vị/ý nghĩa khớp mô tả trong `SCHEMA_CHI_TIET.md`.
- Giữ nguyên toàn bộ comment kỹ thuật / docstring sẵn có của người khác.
- File đặt đúng thư mục theo Module: `Assets/Scripts/Core/`, `Assets/Scripts/Player/`, `Assets/Scripts/Traps/`, `Assets/Scripts/Items/`, `Assets/Scripts/Level/`, `Assets/Scripts/UI/`.

---

## 9. HÀNH VI CỦA AI KHI SINH MÃ

- **Cấm code rút gọn:** không dùng `// ... existing code ...`, không để `// TODO` thay cho logic, không bỏ sót nhánh xử lý. File script xuất ra phải đầy đủ và biên dịch được.
- **Chỉ dùng API có sẵn:** Unity built-in + các package đã khai báo trong `Packages/manifest.json`. Cấm thêm package / plugin / asset ngoài `Assets/Art/` khi chưa được duyệt.
- **Báo cáo sau mỗi script** (đủ 4 mục, ngắn gọn):
  1. Đường dẫn file và Namespace.
  2. Script gắn vào GameObject / Prefab nào.
  3. Tag & Layer cần đặt.
  4. Bảng field Inspector — **lấy đúng giá trị từ Mục 3 & Mục 4 của `SCHEMA_CHI_TIET.md`**, không tự đặt số mới. Nếu Schema không có giá trị tương ứng, dừng lại và hỏi theo Mục 3.
