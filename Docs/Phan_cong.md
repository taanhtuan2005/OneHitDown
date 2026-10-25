# BẢNG PHÂN CÔNG NHIỆM VỤ CHI TIẾT - DỰ ÁN "ONE HIT DOWN"
### Nhóm 505 / Nền tảng: Unity 6 (6000.x) / Quy chuẩn kiến trúc: Prefab-First & Event-Driven
> **Tài liệu tham chiếu kỹ thuật bắt buộc:** [`Docs/SCHEMA_CHI_TIET.md`](SCHEMA_CHI_TIET.md) (Đọc kỹ tên hàm, kiểu trả về, Tag và Layer trước khi code).

---

## I. NGUYÊN TẮC PHÂN CÔNG & ĐẢM BẢO CÔNG BẰNG
1. **Kiến trúc phân rã độc lập (Zero Git Conflict):**
   * Mỗi thành viên chịu trách nhiệm 1 thư mục code riêng và 1 bộ Prefabs riêng.
   * **Tuyệt đối không sửa chung 1 file Scene:** Mỗi người test trên Scene riêng của mình (`TestScene_NguoiX.unity`), sau khi xong chỉ nộp Prefab vào `Assets/Prefabs/`.
2. **Khối lượng công việc cân bằng:**
   * Mỗi thành viên đảm nhận khối lượng tương đương nhau (~10 - 12 điểm tải), bao gồm **1 Module Code + 1 Bộ Prefabs + 1 Phần Báo cáo & Thiết kế màn chơi**.

---

## II. BẢNG PHÂN CÔNG CHI TIẾT 5 THÀNH VIÊN

| STT | Thành viên | MSV | Module phụ trách | Files Code & Prefabs chịu trách nhiệm | Nhiệm vụ phụ (Map & Báo cáo) | Điểm tải |
| :-: | :--- | :-: | :--- | :--- | :--- | :-: |
| **1** | **Tạ Anh Tuấn** | 2351060494 | **Module 1: Player Controller & Physics** | • `PlayerController.cs`<br>• `Player1.prefab` (NinjaFrog)<br>• `Player2.prefab` (MaskDude) | • Thiết kế **Màn 1** (Tutorial)<br>• Hỗ trợ tích hợp Git chung | **11** |
| **2** | **Trần Đức Trung** | 2351060493 | **Module 2: Lethal Traps & Hazards** | • `TrapBase.cs`<br>• `SawSpinner.cs`<br>• `FireTrap.cs`<br>• `Spikes.prefab`, `Saw.prefab` | • Thiết kế **Màn 2**<br>• Báo cáo Phần 7 (Hình ảnh) & Quay Video Demo | **11** |
| **3** | **Trần Văn Tuyên** | 2351060495 | **Module 3: Interactive Gadgets & Environment** | • `TrampolineTrap.cs`<br>• `FallingPlatform.cs`<br>• `DeathPitTrap.cs`<br>• `FanTrap.cs`<br>• `IceSurface.cs`<br>• Prefabs bẫy tương tác | • Thiết kế **Màn 3**<br>• Báo cáo Phần 3 (Gameplay & Mechanics) | **11.5** |
| **4** | **Phan Văn Tú** | 2351260693 | **Module 4: Items, Objectives & Checkpoints** | • `CollectibleItem.cs`<br>• `ExitGate.cs`<br>• `Checkpoint.cs`<br>• `Fruit_*.prefab`<br>• `ExitGate.prefab`<br>• `Checkpoint.prefab` | • Thiết kế **Màn 4**<br>• QA / Kiểm thử chéo & Test Co-op 2 người | **11** |
| **5** | **Hoàng Thị Giang Uyên**| 2351060497 | **Module 5: Core Manager, Camera & Co-op Tether** | • `GameManager.cs`<br>• `CameraController.cs`<br>• `SoulTether.cs`<br>• `SoulTether.prefab` | • Thiết kế **Màn 5** (Boss/Endgame)<br>• Quản lý Âm thanh (SFX & BGM)<br>• Báo cáo Phần 6 (UI/Giao diện) | **11.5** |

---

## III. CHI TIẾT CÔNG VIỆC TỪNG NGƯỜI THEO SCHEMA

### 👤 NGƯỜI 1: TẠ ANH TUẤN (Module Player Controller)
* **Thư mục code:** `Assets/Scripts/Player/`
* **File cần code:** `PlayerController.cs`
* **Nhiệm vụ trọng tâm:**
  1. Viết bộ điều khiển vật lý: Chạy (`moveSpeed = 8f`), Nhảy đơn (`jumpForce = 14f`), Nhảy đôi (`doubleJumpForce = 13.5f`).
  2. Bổ sung cơ chế ân hạn mép vực (`coyoteTime = 0.1f`) và bấm trước khi tiếp đất (`jumpBufferTime = 0.1f`).
  3. Xử lý triệt tiêu ma sát dính tường: Dùng `PhysicsMaterial2D` ma sát 0 và hàm `IsTouchingWall()`.
  4. Hỗ trợ 2 người chơi: Viết hàm `ReadInput()` nhận diện enum `PlayerType`:
     * `Player1`: Dùng phím `A`, `D`, `Space`/`W`.
     * `Player2`: Dùng phím Mũi tên `←`, `→`, `↑`.
  5. Đóng gói Prefab: `Player1.prefab` (Sprite NinjaFrog) và `Player2.prefab` (Sprite MaskDude), gán Tag `"Player"`, Layer 9 (`Player`).

---

### 👤 NGƯỜI 2: TRẦN ĐỨC TRUNG (Module Lethal Traps)
* **Thư mục code:** `Assets/Scripts/Traps/`
* **File cần code:** `TrapBase.cs`, `SawSpinner.cs`, `FireTrap.cs`
* **Nhiệm vụ trọng tâm:**
  1. Viết lớp bẫy cơ sở `TrapBase.cs`: Gắn Tag `"Hazard"`, Layer 10 (`Traps`), bắt va chạm `OnTriggerEnter2D` / `OnCollisionEnter2D` gọi `player.Die()`.
  2. Tích hợp tính năng tuần tra: Hỗ trợ chế độ `PingPong` di chuyển qua lại mượt mà theo trục ngang/dọc.
  3. Viết `SawSpinner.cs`: Tự động xoay lưỡi cưa với tốc độ `rotationSpeed = 720 độ/s`.
  4. Viết `FireTrap.cs`: Phun lửa định thời (Bật 2s -> Tắt 2s) kèm cảnh báo nhấp nháy.
  5. Đóng gói Prefab: `Spikes.prefab` (Chông gai), `Saw.prefab` (Lưỡi cưa tuần tra).

---

### 👤 NGƯỜI 3: TRẦN VĂN TUYÊN (Module Interactive Gadgets & Environment)
* **Thư mục code:** `Assets/Scripts/Traps/`
* **File cần code:**
  * `TrampolineTrap.cs`: Đệm nhún lò xo đẩy tung người chơi lên cao (`bounceForce = 18f`), kích hoạt trigger hoạt ảnh `"Jump"`.
  * `FallingPlatform.cs`: Bàn nâng rung lắc 0.4s khi dẫm vào -> rơi tự do -> tự hồi sinh sau 3s.
  * `DeathPitTrap.cs`: Hố sập ngụy trang (bẫy lún sập vĩnh viễn thành hố tử thần `"Hazard"`).
  * `FanTrap.cs`: Quạt gió thổi luồng hơi nâng nhân vật bay lơ lửng (`windForce = 16f`).
  * `IceSurface.cs`: Bề mặt đường băng làm giảm ma sát (`SetOnIce(true/false)`).
* **Đóng gói Prefabs:** `Trampoline.prefab`, `FallingPlatform.prefab`, `DeathPit.prefab`, `FanTrap.prefab`, `IceRoad.prefab`.

---

### 👤 NGƯỜI 4: PHAN VĂN TÚ (Module Items, Objectives & Checkpoints)
* **Thư mục code:** `Assets/Scripts/Items/`, `Assets/Scripts/Level/`
* **File cần code:**
  * `CollectibleItem.cs`: Thu thập quả hoa quả ăn điểm, tự động hủy và gọi sự kiện.
  * `Checkpoint.cs`: Cột mốc lưu vị trí, khi người chơi chạm vào thì bung cờ (`FlagOut`) và lưu tọa độ hồi sinh mới.
  * `ExitGate.cs`: Cổng hoàn thành màn chơi:
    * Chỉ mở khi thu thập đủ số lượng quả quy định (`isUnlocked == true`).
    * Hỗ trợ cờ `requireBothPlayers`: Chế độ 1 người thì 1 người vào là win; chế độ 2 người thì cần cả 2 cùng chạm cổng mới qua màn.
* **Đóng gói Prefabs:** `Fruit_Apple.prefab`, `Fruit_Kiwi.prefab`, `Checkpoint.prefab`, `ExitGate.prefab`.

---

### 👤 NGƯỜI 5: HOÀNG THỊ GIANG UYÊN (Core, Camera & Co-op Soul Tether)
* **Thư mục code:** `Assets/Scripts/Core/`, `Assets/Scripts/Player/`
* **File cần code:**
  * `GameManager.cs` (Singleton): Quản lý vòng đời trò chơi, quản lý đếm số quả, tự động hồi sinh nhân vật siêu tốc (0.15s) tại Checkpoint gần nhất, phím tắt tiện ích (`R` hồi sinh, `G` bật GodMode bất tử).
  * `CameraController.cs`: Camera bám theo mượt mà (`smoothSpeed = 8f`). Khi chơi 2 người (`target2 != null`), camera tự động lia theo trung điểm của cả 2 người chơi.
  * `SoulTether.cs`: Sợi dây liên kết Co-op 2 người: Dùng `DistanceJoint2D` giới hạn khoảng cách tối đa 4 ô gạch (`maxDistance = 4f`), vẽ tia dây `LineRenderer`, và xử lý cơ chế chết chùm (người này chết thì người kia tan biến).
* **Đóng gói Prefab:** `SoulTether.prefab`, thiết lập âm thanh SFX và nhạc nền BGM.

---

## IV. QUY TẮC PHỐI HỢP GIT TRÁNH XUNG ĐỘT (MERGE CONFLICT)

1. **Nhánh Git phân lập:**
   * Nhánh chính: `main` (chỉ dùng để release sản phẩm hoàn chỉnh).
   * 5 thành viên tạo 5 nhánh tính năng riêng:
     * `feature/player` (Người 1)
     * `feature/lethal-traps` (Người 2)
     * `feature/interactive-traps` (Người 3)
     * `feature/items-objectives` (Người 4)
     * `feature/core-management` (Người 5)

2. **Quy tắc "Không sửa chung file Scene":**
   * Trong quá trình làm, mỗi người tạo 1 Scene test riêng của mình (ví dụ `Assets/Scenes/Test_Player.unity`).
   * Không ai được mở và lưu đè lên `SampleScene.unity` của người khác.
   * Khi hoàn thành tính năng, chỉ commit các file **Script (.cs)** và file **Prefab (.prefab)** vào Git.
   * Người 5 (hoặc nhóm trưởng) sẽ là người mở Scene chính để kéo các Prefab hoàn thiện vào dựng màn chơi cuối cùng.

---

## V. MA TRẬN TEST CHÉO (QA MATRIX)

Để đảm bảo khách quan, không thành viên nào tự test màn chơi do mình thiết kế:
* **Màn 1 (Tuấn làm):** Do **Trung** kiểm thử & đánh giá độ nhạy phím.
* **Màn 2 (Trung làm):** Do **Tuyên** kiểm thử & kiểm tra độ khó của chông/cưa.
* **Màn 3 (Tuyên làm):** Do **Tú** kiểm thử các cơ chế bệ lún, quạt gió, lò xo.
* **Màn 4 (Tú làm):** Do **Uyên** kiểm thử thu thập hoa quả và mở cổng đích.
* **Màn 5 (Uyên làm):** Do **Tuấn** kiểm thử tổng thể, test sợi dây xích 2 người chơi.
