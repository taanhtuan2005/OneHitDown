# BẢNG PHÂN CÔNG NHIỆM VỤ CHI TIẾT - DỰ ÁN "ONE HIT DOWN"
### Nhóm 505 / Nền tảng: Unity 6 (6000.x) / Quy chuẩn kiến trúc: Prefab-First, `IResettable` Instant-Reset & Event-Driven
> **Tài liệu tham chiếu kỹ thuật bắt buộc:** [`Docs/SCHEMA_CHI_TIET.md`](SCHEMA_CHI_TIET.md) (Đọc kỹ tên hàm, kiểu trả về, PPU = 16, Tag và Layer trước khi code).

---

## I. NGUYÊN TẮC PHÂN CÔNG & ĐẢM BẢO CÔNG BẰNG
1. **Kiến trúc phân rã độc lập (Zero Git Conflict):**
   * Mỗi thành viên chịu trách nhiệm 1 Module Code riêng, 1 bộ Prefabs riêng và **1 File Scene Màn chơi độc lập (`Level_01..05.unity`)**.
   * **Tuyệt đối không sửa chung 1 file Scene:** Trong lúc lập trình cơ chế, mỗi người test trên Scene nháp của mình (`Assets/Scenes/Dev/Dev_Nguoi1..5.unity`), sau khi xong chỉ đóng gói Prefab vào `Assets/Prefabs/` rồi ráp vào Scene màn chơi của mình.
2. **Đồng bộ 100% với Báo cáo & Kho Tài nguyên Đầy đủ (`Assets/Art/`):**
   * **Hồi sinh tức thì về đầu màn (`< 0.2s`):** Không dùng Checkpoint giữa màn; khi chết sẽ gọi `ResetLevel()` qua giao diện `IResettable` để khôi phục nguyên trạng Cổ Vật, Hố Tử Thần và Bẫy động, đưa nhân vật về `LevelStartPoint`.
   * **Đầy đủ 4 Skins Nhân vật, 13 Loại Bẫy/Cơ quan & 8 Loại Quả:** Toàn bộ 8 loại trái cây đều được tạo Prefab và sử dụng (mỗi màn lấy **đúng 3 quả Cổ Vật chính** để mở Cổng Thần `[3/3]`, các quả còn lại làm quả thưởng phụ).
   * **Đầy đủ Menu chọn chế độ 1P/2P, chọn Nhân vật (khóa trùng màu 2P), Chọn màn & 6 bản Nhạc nền Chiptune BGM.**
3. **Khối lượng công việc cân bằng:**
   * Mỗi thành viên đảm nhận khối lượng tương đương nhau (**12 điểm tải/người**), bao gồm **1 Module Code + 1 Bộ Prefabs + 1 Màn chơi chính + 1 Phần Báo cáo**.

---

## II. BẢNG PHÂN CÔNG CHI TIẾT 5 THÀNH VIÊN

| STT | Thành viên | MSV | Module phụ trách | Files Code & Prefabs chịu trách nhiệm | Nhiệm vụ Map & Báo cáo | Điểm tải |
| :-: | :--- | :-: | :--- | :--- | :--- | :-: |
| **1** | **Tạ Anh Tuấn** | 2351060494 | **Module 1: Player Controller, Physics, 4 Skins & VFX** | • `PlayerController.cs`, `PlayerVFX.cs`<br>• `Player1.prefab`, `Player2.prefab`<br>• 4 Animator Controllers (`NinjaFrog`, `VirtualGuy`, `PinkMan`, `MaskDude`)<br>• VFX `Appearing`, `Disappearing`, `Dust`, `Shadow` | • Thiết kế **Màn 1** (`Level_01.unity`)<br>• Cấu hình `Tags & Layers` gốc<br>• Báo cáo Phần 1, 2 & Phần 4 (Nhân vật, VFX) | **12** |
| **2** | **Trần Đức Trung** | 2351060493 | **Module 2: Lethal Traps & Dynamic Crushers (6 Bẫy Sát Thương)** | • `TrapBase.cs`, `SawSpinner.cs`, `FireTrap.cs`<br>• `RockHeadTrap.cs`, `SpikeHeadTrap.cs`, `SpikedBallPendulum.cs`<br>• Prefabs: `Spikes`, `Saw`, `FireTrap`, `RockHead`, `SpikeHead`, `SpikedBall` | • Thiết kế **Màn 2** (`Level_02.unity`)<br>• Báo cáo Phần 7 (Chụp ảnh 5 màn chơi) & Phần 8 (Bản quyền Asset) | **12** |
| **3** | **Trần Văn Tuyên** | 2351060495 | **Module 3: Interactive Gadgets & Environment (6 Cơ Quan & Hố Sập)** | • `TrampolineTrap.cs`, `FanTrap.cs`, `FallingPlatform.cs`<br>• `DeathPitTrap.cs`, `IceSurface.cs`, `ArrowBooster.cs`<br>• Prefabs: `Trampoline`, `FanTrap`, `FallingPlatform`, `DeathPit`, `IceRoad`, `ArrowBooster` | • Thiết kế **Màn 3** (`Level_03.unity`)<br>• Báo cáo Phần 3 (Gameplay & Mechanics) & Phần 6 (Mô tả thị giác, Điều khiển) | **12** |
| **4** | **Phan Văn Tú** | 2351260693 | **Module 4: Relics (8 Quả), Moving Platform, Start/Exit Altar & Menus** | • `CollectibleItem.cs`, `MovingPlatform.cs`<br>• `LevelStartPoint.cs`, `ExitGate.cs`, `KillZone.cs`<br>• `GameSession.cs`, `MainMenuController.cs`, `LevelSelectMenu.cs`<br>• 8 `Fruit_*.prefab`, `MovingPlatform.prefab`, `LevelStart.prefab`, `ExitGate.prefab` | • Thiết kế **Màn 4** (`Level_04.unity`) & `MainMenu.unity`<br>• Báo cáo Phần 5 (Màn chơi & Metrics) | **12** |
| **5** | **Hoàng Thị Giang Uyên**| 2351060497 | **Module 5: Core Manager, Instant Reset, Soul Tether, Audio & HUD** | • `IResettable.cs`, `GameManager.cs`, `CameraController.cs`<br>• `SoulTether.cs`, `AudioManager.cs`<br>• `HUDController.cs`, `PauseMenu.cs` (kèm `IrisTransition`)<br>• `SoulTether.prefab`, tích hợp 6 BGM & 382 SFX | • Thiết kế **Màn 5** (`Level_05.unity` - Endgame)<br>• Báo cáo Phần 4 (Âm thanh) & Phần 6 (Audio UI/BGM) | **12** |

---

## III. CHI TIẾT CÔNG VIỆC TỪNG NGƯỜI THEO SCHEMA

### 👤 NGƯỜI 1: TẠ ANH TUẤN (Module Player Controller, 4 Skins & VFX)
* **Thư mục code:** `Assets/Scripts/Player/` | **Nhánh Git:** `feature/player`
* **File cần code:** `PlayerController.cs`, `PlayerVFX.cs`
* **Nhiệm vụ trọng tâm:**
  1. Thiết lập `ProjectSettings/TagManager.asset` (Tags: `Hazard`, `Player`, `Item`; Layers: `8: Ground`, `9: Player`, `10: Traps`) và chuẩn hóa `PPU = 16`.
  2. Viết bộ điều khiển vật lý có **gia tốc lấy đà (`groundAcceleration = 65f`)**: Chạy (`moveSpeed = 8f`), Nhảy đơn (`jumpForce = 15.5f` -> cao 3.5 tiles), Nhảy đôi (`doubleJumpForce = 13.5f` -> tổng cao 5.5 tiles), đảm bảo nhảy có đà xa 6 tiles và nhảy đứng yên xa 3 tiles.
  3. Hỗ trợ cộng vận tốc nền `platformVelocity` (khi đứng trên `MovingPlatform`) và ngoại lực `externalVelocity` (để **không ghi đè làm mất lực kéo của Dây Xích `DistanceJoint2D`** hoặc quạt gió ngang).
  4. Bổ sung `coyoteTime = 0.1f`, `jumpBufferTime = 0.1f`, chống dính tường (`PhysicsMaterial2D` ma sát 0 + `IsTouchingWall`), và guard chống gọi đúp/GodMode trong `Die()`.
  5. Hỗ trợ 2 người chơi trên cùng bàn phím (`Player1`: `A/D/W/Space`; `Player2`: `←/→/↑`) và hàm `ApplySkin(CharacterSkin)` cho đủ **4 Skins** (`NinjaFrog`, `VirtualGuy`, `PinkMan`, `MaskDude`).
  6. Viết `PlayerVFX.cs`: Bóng dưới chân `Shadow.png` (`16×16`), bụi `Dust Particle.png` (`16×16`), hiệu ứng hồi sinh `Appearing (96x96).png` và tan biến `Desappearing (96x96).png` (7 frames).

---

### 👤 NGƯỜI 2: TRẦN ĐỨC TRUNG (Module Lethal Traps & Dynamic Crushers)
* **Thư mục code:** `Assets/Scripts/Traps/` | **Nhánh Git:** `feature/lethal-traps`
* **File cần code:** `TrapBase.cs`, `SawSpinner.cs`, `FireTrap.cs`, `RockHeadTrap.cs`, `SpikeHeadTrap.cs`, `SpikedBallPendulum.cs`
* **Nhiệm vụ trọng tâm:**
  1. Viết lớp cơ sở `TrapBase.cs` (cài đặt `IResettable`): Gắn Tag `"Hazard"`, Layer 10 (`Traps`), hỗ trợ tuần tra `PingPong` ngang/dọc và `ResetState()` đưa bẫy về vị trí đầu khi hồi sinh.
  2. Viết `SawSpinner.cs`: Xoay lưỡi cưa `720 độ/s` và di chuyển dọc đường ray xích `Saw/Chain.png`.
  3. Viết `FireTrap.cs`: Chu kỳ **2.0s tắt (`Off.png`) -> 0.35s cảnh báo (`Hit 16x32`) -> 1.0s bùng lửa sát thương (`On 16x32`)** khớp kịch bản Màn 3.
  4. Viết `RockHeadTrap.cs` (Màn 4): Cảm biến tiệm cận `BoxCast` dưới đáy, nháy mắt `Blink` trong `0.25s` rồi đập sập xuống sàn, sau đó rút từ từ lên trần.
  5. Viết `SpikeHeadTrap.cs` (Màn 4): Khối đá gai phóng ngang/dọc tốc độ cao, đập đối xứng trong hành lang hẹp.
  6. Viết `SpikedBallPendulum.cs` (Màn 5): Quả cầu gai xích sắt đung đưa theo hàm điều hòa quanh trục treo.

---

### 👤 NGƯỜI 3: TRẦN VĂN TUYÊN (Module Interactive Gadgets & Environment)
* **Thư mục code:** `Assets/Scripts/Traps/` | **Nhánh Git:** `feature/interactive-traps`
* **File cần code:** `TrampolineTrap.cs`, `FanTrap.cs`, `FallingPlatform.cs`, `DeathPitTrap.cs`, `IceSurface.cs`, `ArrowBooster.cs`
* **Nhiệm vụ trọng tâm:**
  1. `TrampolineTrap.cs`: Đệm lò xo đẩy nhân vật theo vector `bounceDirection` (`bounceForce = 20f`, hỗ trợ đặt thẳng hoặc nghiêng ở Màn 2) và hồi lượt Nhảy Đôi.
  2. `FanTrap.cs`: Cối xay quạt gió hỗ trợ cả thổi bổng theo trục Y và thổi ngang xuôi/ngược hành lang (`windDirection`, `windForce = 15f` ở Màn 4).
  3. `FallingPlatform.cs` (`IResettable`): Bệ đá treo rung `0.4s` khi giẫm lên rồi sụp rơi; giữ trạng thái sập suốt lượt chơi để tạo **Ác Mộng Quay Đầu**, khôi phục lại khi `ResetLevel()`.
  4. `DeathPitTrap.cs` (`IResettable`): Khối gạch ngụy trang `Blocks/Idle.png` (Root Layer `Ground`, `Untagged`), giẫm lên rung `0.2s` rồi vỡ nát, bật Child `DeathTriggerZone` (Tag `"Hazard"`) thành **Hố Tử Thần Vĩnh Viễn** rộng tối đa 3 tiles; khôi phục lại khi `ResetLevel()`.
  5. `IceSurface.cs`: Bề mặt băng `Sand Mud Ice` làm giảm ma sát (`SetOnIce(true/false)`).
  6. `ArrowBooster.cs` (`IResettable`): Bong bóng mũi tên `Arrow (18x18)` đặt giữa vực thẳm; khi chạm vào đẩy bật nhân vật (`boostForce = 14f`) và **hồi ngay 1 lượt Nhảy Đôi (`canDoubleJump = true`)**, phát anim `Hit` rồi hồi lại sau `2.5s`.

---

### 👤 NGƯỜI 4: PHAN VĂN TÚ (Module Relics 8 Quả, Moving Platform, Start/Exit Altar & Menus)
* **Thư mục code:** `Assets/Scripts/Items/`, `Assets/Scripts/Level/`, `Assets/Scripts/UI/` | **Nhánh Git:** `feature/items-objectives`
* **File cần code:** `CollectibleItem.cs`, `MovingPlatform.cs`, `LevelStartPoint.cs`, `ExitGate.cs`, `KillZone.cs`, `GameSession.cs`, `MainMenuController.cs`, `LevelSelectMenu.cs`
* **Nhiệm vụ trọng tâm:**
  1. `CollectibleItem.cs` (`IResettable`): Đóng gói đủ **8 Prefabs trái cây** (`Apple`, `Bananas`, `Kiwi`, `Orange`, `Pineapple`, `Cherries`, `Melon`, `Strawberry`). Mỗi màn đặt đúng 3 quả chính (`countsAsRelic = true`) để mở cổng và các quả phụ (`countsAsRelic = false`) thưởng điểm. Khi nhặt chỉ ẩn đi (không `Destroy`) để `ResetState()` khôi phục tức thì khi người chơi chết.
  2. `MovingPlatform.cs` (`IResettable`): Bệ nâng di động (`Platforms/Grey On` & `Brown On`), truyền vận tốc `SetPlatformVelocity` cho người chơi đứng trên bệ.
  3. `LevelStartPoint.cs` & `KillZone.cs`: Bệ xuất phát đầu màn (`Start (Moving)`) chứa điểm hồi sinh cho `P1` & `P2`, cùng đáy vực `KillZone` dưới màn hình.
  4. `ExitGate.cs` (`IResettable`): Cổng Thần cuối màn hiển thị nhãn `[LOCKED 0/3 -> OPEN 3/3]`, bắn pháo hoa `Confetti (16x16).png`, kiểm tra `HashSet<PlayerController>` (1 người cho 1P, đủ 2 người cho 2P Co-op) ngay cả khi cổng mở lúc người chơi đang đứng sẵn trong cổng.
  5. `GameSession.cs`, `MainMenuController.cs`, `LevelSelectMenu.cs`: Màn hình chọn chế độ `1P / 2P`, chọn 4 Skins nhân vật (**khóa trùng màu ở 2P** kèm âm báo `error_003.ogg`), và chọn 5 màn chơi (`01.png..05.png` kèm icon khóa `locked.png` / `unlocked.png`).

---

### 👤 NGƯỜI 5: HOÀNG THỊ GIANG UYÊN (Core Manager, Instant Reset, Soul Tether, Audio & HUD)
* **Thư mục code:** `Assets/Scripts/Core/`, `Assets/Scripts/Player/`, `Assets/Scripts/UI/` | **Nhánh Git:** `feature/core-management`
* **File cần code:** `IResettable.cs`, `GameManager.cs`, `CameraController.cs`, `SoulTether.cs`, `AudioManager.cs`, `HUDController.cs`, `PauseMenu.cs`
* **Nhiệm vụ trọng tâm:**
  1. `GameManager.cs`: Quản lý danh sách người chơi (`1P` hoặc `2P`), đếm `3/3` Cổ Vật mở cổng, xử lý chết chùm Co-op và **Hồi sinh tức thì (`0.18s`) về `LevelStartPoint` đầu màn** kèm gọi `ResetState()` cho toàn bộ đối tượng `IResettable` trong màn. Hỗ trợ đầy đủ phím tắt (`R` Reset màn, `ESC/P` Pause, `G` GodMode, `H` Hitbox, `T` TimeScale, `1..5` đổi màn).
  2. `CameraController.cs`: Bám mượt theo `Player1` (1P) hoặc trung điểm của `Player1 & Player2` (2P).
  3. `SoulTether.cs` (`IResettable`): Dây xích vật lý `DistanceJoint2D` (`maxDistance = 4f` = 64px), hỗ trợ truyền lực kéo Mỏ Neo Sàn & Đu Dây, vẽ `LineRenderer` đổi 3 màu theo độ căng (`Cyan < 48px`, `Orange 48–60px`, `Red >= 60px` kèm tia lửa `spark_01.png` và âm cảnh báo).
  4. `AudioManager.cs`: Quản lý **6 bản nhạc nền Chiptune BGM** (`bgm_menu`, `bgm_level1..5` trong `06_AmThanh/BGM_ChiptuneLoops/`) và 382 files SFX/Jingles theo từng địa hình màn chơi.
  5. `HUDController.cs` & `PauseMenu.cs`: Hiển thị bộ đếm `[x/3]`, `TIME`, `DEATHS`, bảng tạm dừng và hiệu ứng màn tròn `IrisTransition` (`Transition.png` 44×44).

---

## IV. QUY TẮC PHỐI HỢP GIT TRÁNH XUNG ĐỘT (MERGE CONFLICT)

1. **Nhánh Git thống nhất (Khớp 100% với `SCHEMA_CHI_TIET.md`):**
   * Nhánh chính: `main` (chỉ dùng để release sản phẩm hoàn chỉnh).
   * Nhánh tích hợp: `dev` (gộp tính năng hàng ngày).
   * 5 nhánh tính năng riêng:
     * `feature/player` (Người 1 - Tuấn)
     * `feature/lethal-traps` (Người 2 - Trung)
     * `feature/interactive-traps` (Người 3 - Tuyên) - feature/interactive-traps_tvt
     * `feature/items-objectives` (Người 4 - Tú)
     * `feature/core-management` (Người 5 - Uyên)

2. **Quy tắc "Mỗi người sở hữu Scene riêng":**
   * Khi test code cơ chế: Dùng `Assets/Scenes/Dev/Dev_Nguoi1..5.unity`.
   * Khi thiết kế màn chơi chính: Mỗi người chỉ mở và lưu file Scene màn chơi được giao (`Level_01.unity` .. `Level_05.unity`).
   * Chỉ commit mã nguồn **`.cs`**, **`.prefab`** và file Scene thuộc quyền phụ trách của mình.

---

## V. MA TRẬN TEST CHÉO (QA MATRIX)

Để đảm bảo khách quan, không thành viên nào tự nghiệm thu màn chơi do mình thiết kế:
* **Màn 1 (`Level_01` - Tuấn làm):** Do **Trung** kiểm thử độ nhạy điều khiển, gia tốc đà nhảy, 4 Skins và `ArrowBooster`.
* **Màn 2 (`Level_02` - Trung làm):** Do **Tuyên** kiểm thử lưỡi cưa trượt ray, đệm nhún nghiêng và bẫy Hố Tử Thần.
* **Màn 3 (`Level_03` - Tuyên làm):** Do **Tú** kiểm thử quán tính dốc băng, chu kỳ lửa 2s-1s và bệ nâng `MovingPlatform`.
* **Màn 4 (`Level_04` & `MainMenu` - Tú làm):** Do **Uyên** kiểm thử `RockHead`, `SpikeHead`, quạt gió ngang, chọn nhân vật 2P và mở Cổng Thần `[3/3]`.
* **Màn 5 (`Level_05` - Uyên làm):** Do **Tuấn** kiểm thử tổng thể con lắc `SpikedBall`, cơ chế `ResetLevel()` đầu màn, âm thanh BGM/SFX và Đu Dây Co-op 2 người.
