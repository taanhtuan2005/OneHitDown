# ĐẶC TẢ KỸ THUẬT — MODULE 3: INTERACTIVE GADGETS & ENVIRONMENTAL MECHANICS

> **Loại tài liệu:** Implementation Spec (đặc tả thi hành) cho `senior-dev`, `auto-tester`, `qa-inspector`.
> **Nguồn sự thật duy nhất (Single Source of Truth):** `Docs/SCHEMA_CHI_TIET.md` §1, §3 (Module 3 & Module 1), §4; `Docs/Phan_cong.md` Mục III (Người 3); `CLAUDE.md`.
> Mọi con số, tên class, tên hàm, Tag, Layer trong tài liệu này **đã được đối chiếu trực tiếp** với 3 tài liệu trên và với file asset thật trong `Assets/Art/`. Không có giá trị nào được tự đặt thêm. Mục nào Schema chưa quy định được tập trung tại **§9 — ĐIỂM CẦN CHỐT**, không được tự suy diễn khi code.

---

## 1. TỔNG QUAN & METADATA

| Trường | Giá trị |
| :--- | :--- |
| Tính năng | Module 3 — 6 Cơ quan tương tác & Cơ chế môi trường |
| Người phụ trách | Trần Văn Tuyên — MSV `2351060495` |
| Nhánh Git | `feature/interactive-traps_tvt` (theo `Phan_cong.md` Mục IV) |
| Thư mục mã nguồn | `Assets/Scripts/Traps/` |
| Namespace bắt buộc | `OneHitDown.Traps` |
| Unity | `6000.6.6f1` (Unity 6, URP 2D 17.6.0), C# 9 / .NET Standard 2.1 |
| Kiến trúc | Component-Based, Event-Driven, `IResettable` Instant-Reset, Prefab-First |
| PPU chuẩn | `16` (1 Unity unit = 1 tile = 16 px) |
| Phiên bản spec | `1.0` |
| Trạng thái | Sẵn sàng thi hành 100% (hợp đồng liên module và xử lý điểm mở tại §9) |

### 1.1. Deliverables (đầu ra bắt buộc)

| # | Đường dẫn tuyệt đối tương đối từ repo root | Loại | Ghi chú |
| :--- | :--- | :--- | :--- |
| 1 | `Assets/Scripts/Traps/TrampolineTrap.cs` | Tạo mới | Schema §3 Module 3.1 |
| 2 | `Assets/Scripts/Traps/FanTrap.cs` | Tạo mới | Schema §3 Module 3.2 |
| 3 | `Assets/Scripts/Traps/FallingPlatform.cs` | Tạo mới | Schema §3 Module 3.3, `IResettable` |
| 4 | `Assets/Scripts/Traps/DeathPitTrap.cs` | Tạo mới | Schema §3 Module 3.4, `IResettable` |
| 5 | `Assets/Scripts/Traps/IceSurface.cs` | Tạo mới | Schema §3 Module 3.5 |
| 6 | `Assets/Scripts/Traps/ArrowBooster.cs` | Tạo mới | Schema §3 Module 3.6, `IResettable` |
| 7 | `Assets/Scripts/Core/IResettable.cs` | **Scaffolding có điều kiện** | Xem §2.2 — chỉ tạo nếu chưa tồn tại, nội dung copy nguyên văn từ Schema |
| 8 | `Assets/Prefabs/Traps/Trampoline.prefab`, `FanTrap.prefab`, `FallingPlatform.prefab`, `DeathPit.prefab`, `IceRoad.prefab`, `ArrowBooster.prefab` | Tạo mới | Schema §4 |

**Không được tạo:** file Scene nào (Scene thuộc quyền Người 1/2/4/5), `.meta` tự chế, Tag/Layer mới, package mới.

---

## 2. RÀNG BUỘC TÍCH HỢP LIÊN MODULE (INTEGRATION SEAMS)

Đây là phần quan trọng nhất của tài liệu. Trạng thái repo hiện tại (đã kiểm tra thực tế): **`Assets/Scripts/` và `Assets/Prefabs/` chưa tồn tại**. Module 3 phụ thuộc vào API của Module 1 (`PlayerController`) và Module 5 (`IResettable`, `GameManager`). Nếu xử lý sai, code sẽ **không biên dịch được** hoặc **gây lỗi merge** khi gộp nhánh.

### 2.1. Hợp đồng phụ thuộc một chiều

Module 3 **được phép gọi** (read-only contract) — tuyệt đối không sửa:

| API Module 1 — `OneHitDown.Player.PlayerController` | Dùng bởi | Ngữ nghĩa theo Schema |
| :--- | :--- | :--- |
| `bool IsDead { get; }` | Trampoline, Fan, Arrow | Guard chống tác dụng lên xác nhân vật |
| `Rigidbody2D Rigidbody { get; }` | Fan | Ghi vận tốc Y khi thổi dọc |
| `void GrantDoubleJumpAndBoost(Vector2 boostVelocity)` | Trampoline, Arrow | Đặt `canDoubleJump = true`, `coyoteCounter = 0f`, gán vận tốc (Schema §3 Module 1, hàm 8) |
| `void AddExternalVelocity(Vector2 extVel)` | Fan | Cộng xung lực ngang, tự giảm dần `25f/s` (Schema §3 Module 1, hàm 10) |
| `void SetOnIce(bool onIce)` | IceSurface | Gán `isOnIce` — đổi `accel` sang `iceFriction = 4f` (Schema §3 Module 1, hàm 14) |

| API Module 5 — `OneHitDown.Core` | Dùng bởi |
| :--- | :--- |
| `interface IResettable { void ResetState(); }` | FallingPlatform, DeathPitTrap, FanTrap, ArrowBooster |

Module 3 **cấm** tham chiếu: `GameManager`, `AudioManager`, `GameSession`, `PlayerVFX`, `SoulTether` — Schema không yêu cầu và sẽ tạo phụ thuộc vòng.

### 2.2. Chiến lược biên dịch khi thiếu file liên module

**Quy tắc bắt buộc cho `senior-dev`:**

1. Viết code **đúng như Schema**: `using OneHitDown.Player;` và tham số kiểu `PlayerController`. **KHÔNG** dùng `reflection`, `SendMessage`, `dynamic`, hay `GameObject.GetComponent("PlayerController")`.
2. Trước khi tạo `Assets/Scripts/Core/IResettable.cs`, **kiểm tra file đã tồn tại chưa**. Nếu có → không sửa. Nếu chưa → tạo **nguyên văn** đúng 5 dòng trong Schema §3 "Giao diện Dùng Chung" (namespace `OneHitDown.Core`, `public interface IResettable { void ResetState(); }`). Nội dung trùng khớp 100% thì merge với nhánh `feature/core-management` là fast-forward, không conflict.
3. Nếu `Assets/Scripts/Player/PlayerController.cs` chưa tồn tại → **KHÔNG tạo stub trong `Assets/`** (sẽ trùng class khi merge). Thay vào đó dùng harness tại §8.4 đặt **ngoài** `Assets/` để kiểm tra biên dịch.

### 2.3. Ma trận trạng thái khi merge

| File liên module | Nếu đã có | Nếu chưa có |
| :--- | :--- | :--- |
| `Assets/Scripts/Core/IResettable.cs` | Giữ nguyên, không sửa | Tạo bản sao nguyên văn Schema |
| `Assets/Scripts/Player/PlayerController.cs` | Biên dịch bình thường | Chỉ build-check qua harness §8.4, **không** commit stub |
| `Assets/Scripts/Core/GameManager.cs` | Không được chạm | Không tạo |

---

## 3. LUỒNG NGƯỜI DÙNG (USER FLOWS)

Quy ước trạng thái: mọi cơ quan là **máy trạng thái hữu hạn**; mọi chuyển trạng thái phải phát phản hồi thị giác ngay trong cùng frame (không có độ trễ cảm nhận).

### 3.1. `TrampolineTrap` — Đệm nhún lò xo đa hướng

**Happy Path**
1. Nhân vật rơi/chạy vào vùng `BoxCollider2D` (`isTrigger = true`, `Size (1.6, 0.5)`).
2. `OnTriggerEnter2D` nhận `Collider2D col` → `col.GetComponent<PlayerController>()`.
3. Điều kiện: `player != null && !player.IsDead` → gọi `Bounce(player)`.
4. `Bounce` gọi `player.GrantDoubleJumpAndBoost(bounceDirection * bounceForce)` → nhân vật bật theo `bounceDirection` (mặc định `(0,1)`), **đồng thời hồi 1 lượt Nhảy Đôi**.
5. Phát animation `Jump` (8 frame, `Trampoline/Jump (28x28).png`), sau khi clip kết thúc quay về `Idle`.
6. Nhân vật đang bay ở đỉnh quỹ đạo có thể bấm Nhảy Đôi ngay → đạt tổng độ cao 5.5 tiles.

**Luồng ngoại lệ**

| Mã | Tình huống | Hành vi bắt buộc |
| :--- | :--- | :--- |
| TR-EX-1 | Nhân vật đã chết (`IsDead == true`) rơi trúng đệm | Bỏ qua, không bật, không phát anim |
| TR-EX-2 | Collider không phải Player (quả, mảnh vỡ, bẫy khác) | Bỏ qua im lặng, không log mỗi frame |
| TR-EX-3 | Đệm đặt nghiêng ở Màn 2 (`bounceDirection = (0.6, 0.8)`) | `bounceDirection` được `.Normalize()` trong `Awake()` → lực bật không bị khuếch đại theo độ dài vector nhập |
| TR-EX-4 | `bounceDirection` nhập `(0,0)` trong Inspector | `Awake()` phát `Debug.LogWarning` và giữ hướng mặc định `(0,1)`; không để nhân vật kẹt tại chỗ |
| TR-EX-5 | Bật liên tiếp 2 lần trong cùng frame (2 collider con của Player) | Chỉ nhận collider có `PlayerController`; nếu vẫn trúng 2 lần, lần 2 ghi đè cùng giá trị vận tốc nên **không cộng dồn** (idempotent) |
| TR-EX-6 | Nhân vật chết ngay sau khi bật (rơi vào Hố Tử Thần) | `ResetLevel()` reset vị trí; đệm không giữ trạng thái nên không cần `ResetState()` |

**Trạng thái UI/Visual:** `Idle` (tĩnh) → `Jumping` (8 frame) → `Idle`. Không có trạng thái lỗi hiển thị cho người chơi.

### 3.2. `FanTrap` — Cối xay quạt gió (Màn 4)

**Happy Path — thổi dọc (`windDirection = (0,1)`)**
1. Nhân vật bước vào `WindZone` (trigger con) → `OnTriggerStay2D` chạy **mỗi FixedUpdate** khi còn bên trong.
2. `isActive == true`, `player != null`, `!player.IsDead`.
3. `Mathf.Abs(windDirection.y) > 0.5f` → `rb.linearVelocity = new Vector2(v.x, Mathf.Max(v.y, windDirection.y * windForce))` với `windForce = 15f`.
4. Nhân vật lơ lửng/nâng bổng — bấm nhảy vẫn được cộng thêm lực nhảy.

**Happy Path — thổi ngang (`windDirection = (1,0)` hoặc `(-1,0)`)**
1. Nhánh `else`: gọi `player.AddExternalVelocity(new Vector2(windDirection.x * windForce * 0.35f, 0f))` → giá trị xung lực `±5.25`.
2. Chạy xuôi chiều gió: `externalVelocity` cộng vào `newVx` → chạy nhanh hơn `moveSpeed` một đoạn ngắn.
3. Quay đầu Backtrack: lực gió ngược chiều, `externalVelocity` chống lại `targetVx` → tốc độ thực giảm mạnh, tạo áp lực phải giữ đà.

**Luồng ngoại lệ**

| Mã | Tình huống | Hành vi bắt buộc |
| :--- | :--- | :--- |
| FA-EX-1 | `isActive == false` (đã `SetActive(false)`) | `OnTriggerStay2D` return ngay trước khi `GetComponent`; quạt hiển thị `Fan/Off.png` |
| FA-EX-2 | Player chết trong luồng gió | Bỏ qua (`IsDead`), không giữ nhân vật lơ lửng trên không |
| FA-EX-3 | Rời `WindZone` khi đang thổi dọc | Không làm gì thêm — nhân vật tiếp tục quỹ đạo parabol nhờ trọng lực `gravityScale = 3.5f` |
| FA-EX-4 | `windDirection = (0,0)` | `Awake()` `Debug.LogWarning`, coi như `(0,1)`; không gọi `AddExternalVelocity` với vector 0 mỗi frame |
| FA-EX-5 | Vector gió chéo `(0.7, 0.7)` | Ưu tiên nhánh dọc (điều kiện `Abs(y) > 0.5f`) — đúng thứ tự rẽ nhánh Schema; **không** cộng cả hai nhánh |
| FA-EX-6 | Vector gió chéo `(0.8, -0.3)` (thổi chếch xuống) | `Abs(y) = 0.3 <= 0.5` → nhánh ngang; `Mathf.Max` với số âm không kéo nhân vật xuống quá vận tốc hiện tại |
| FA-EX-7 | 2 người chơi cùng trong luồng gió | `OnTriggerStay2D` xử lý riêng từng `col` → mỗi người nhận lực độc lập, không chia đôi |

**Trạng thái UI/Visual:** `On` (4 frame `Fan/On (24x8)`) khi `isActive`; `Off` (1 frame) khi tắt. Chuyển sprite phải diễn ra **ngay** trong `SetActive`.

### 3.3. `FallingPlatform` — Bệ đá treo sụp lún

**Happy Path (chế độ vĩnh viễn — `isPermanentInRun = true`, mặc định Màn 1 & 5)**
1. Nhân vật đứng lên mặt bệ → `OnCollisionEnter2D(Collision2D col)`.
2. Điều kiện: `col` có `PlayerController` và `!IsDead` **và** platform đang `Idle`/`Solid` → `TriggerCollapse()`.
3. `CollapseSequence()`: rung tại chỗ `shakeDuration = 0.4f` (biên độ rung nhỏ ±2 px = ±0.125 unit, tần số 25 Hz — đủ để mắt nhận ra nhưng không làm nhân vật trượt khỏi bệ).
4. Hết 0.4s → tắt va chạm solid ngay trong frame đó (`solidCollider.enabled = false`), đổi sprite sang `Off.png`, cho `transform` rơi xuống với `fallSpeed = 12f` mỗi `Update`.
5. Bệ rơi khỏi vùng nhìn thấy → tự tắt (`gameObject.SetActive(false)`) hoặc dừng ở ngoài khung hình. **Trạng thái sập giữ nguyên suốt lượt chơi** — tạo cơ chế "Ác Mộng Quay Đầu".

**Happy Path (chế độ luyện tập — `isPermanentInRun = false`, `respawnDuration = 3.0f`)**
- Sau 3.0s kể từ lúc sập, tự hồi: về `startPosition`, bật lại `solidCollider`, trả sprite `On`, về trạng thái `Solid`.

**Happy Path (hồi sinh)**
- Nhân vật chết bất kỳ đâu → `GameManager.OnAnyPlayerDied` → `ResetLevel()` → gọi `ResetState()` trên bệ → **tức thì** về `startPosition`, `solidCollider.enabled = true`, sprite `On`, dừng mọi coroutine/dịch chuyển. Không có animation hồi — phải tức thì (< 1 frame).

**Luồng ngoại lệ**

| Mã | Tình huống | Hành vi bắt buộc |
| :--- | :--- | :--- |
| FP-EX-1 | Đang rung, nhân vật bước ra rồi bước lại | Không khởi động lại coroutine (guard `isCollapsing`); đã rung thì sập đúng 0.4s |
| FP-EX-2 | `OnCollisionEnter2D` từ cạnh bên hoặc từ dưới | Chỉ kích hoạt khi `col` chạm **mặt trên** — kiểm tra `contact.normal.y < -0.5f` (pháp tuyến hướng xuống theo hệ trục collision Unity), tránh sập khi nhân vật đập đầu từ dưới |
| FP-EX-3 | `OnCollisionEnter2D` từ mảnh vỡ / bẫy khác | Bỏ qua nếu không có `PlayerController` |
| FP-EX-4 | Nhân vật đứng trên bệ lúc bệ sập | Nhân vật rơi tự do theo trọng lực — không gắn cha (`SetParent`) nên không bị kéo theo |
| FP-EX-5 | `ResetState()` gọi lúc đang chạy coroutine | `StopAllCoroutines()` **trước** khi khôi phục, nếu không coroutine sẽ tiếp tục làm bệ rơi lại sau reset |
| FP-EX-6 | `ResetState()` gọi 2 lần liên tiếp (chết chùm 2P) | Phải idempotent — gọi 10 lần vẫn cho cùng trạng thái `Solid` tại `startPosition` |
| FP-EX-7 | `respawnDuration <= 0` khi `isPermanentInRun == false` | `Awake()` cảnh báo và **không** đặt coroutine hồi (tương đương vĩnh viễn) để tránh vòng lặp hồi tức thì vô hạn |

**Trạng thái UI/Visual:** `Solid` (sprite `Falling Platforms/On (32x10)`, có va chạm) → `Shaking` (0.4s, vẫn solid) → `Falling` (sprite `Off.png`, không va chạm) → `Vanished` / `Respawning`.

### 3.4. `DeathPitTrap` — Hố Tử Thần Vĩnh Viễn ngụy trang bằng `Blocks`

**Happy Path**
1. Sàn gạch trông y hệt sàn thường (`Blocks/Idle.png`), Root Tag `"Untagged"`, Layer `Ground (8)`, `BoxCollider2D solidCollider` `isTrigger = false`.
2. Nhân vật giẫm lên mặt trên → `OnCollisionEnter2D` → `TriggerCollapse()`.
3. `CollapseRoutine()`: đổi sprite sang `HitTop (22x22)` (2 frame), rung lắc `0.2s`.
4. Tung 2 mảnh vỡ `Part 1 (22x22)` và `Part 2 (22x22)` (dùng `Rigidbody2D` + lực ngẫu nhiên nhỏ, hoặc chỉ đổi sprite — xem §9.3).
5. Tắt `solidCollider.enabled = false`, ẩn `blockRenderer.enabled = false`, bật `deathTrigger.gameObject.SetActive(true)`.
6. Rơi xuống → chạm `DeathTriggerZone` (Tag `"Hazard"`, `isTrigger = true`) → `PlayerController.OnTriggerEnter2D` gọi `Die()`.
7. Hố **giữ nguyên trạng thái mở suốt lượt chơi** — không tự đóng.

**Happy Path (hồi sinh)**
- `ResetLevel()` → `ResetState()` → `solidCollider.enabled = true`, `blockRenderer.enabled = true`, `deathTrigger.SetActive(false)`, `isTriggered = false`. Tức thì.

**Luồng ngoại lệ**

| Mã | Tình huống | Hành vi bắt buộc |
| :--- | :--- | :--- |
| DP-EX-1 | Nhân vật đứng trên khối lúc nó biến mất | Không gắn cha; nhân vật rơi tự do vào `DeathTriggerZone` → chết đúng thiết kế |
| DP-EX-2 | `TriggerCollapse()` gọi 2 lần (2 người chơi giẫm cùng lúc) | Guard `isTriggered` — lần 2 return ngay, không chạy 2 coroutine rung |
| DP-EX-3 | `ResetState()` khi coroutine đang chạy | `StopAllCoroutines()` trước khi khôi phục |
| DP-EX-4 | Va chạm từ cạnh bên / từ dưới | Chỉ kích hoạt khi pháp tuyến mặt trên (`contact.normal.y < -0.5f`) |
| DP-EX-5 | Nhân vật đã chết vẫn còn trong trigger | `PlayerController.Die()` có guard `isDead` → không nhân đôi sự kiện chết |
| DP-EX-6 | Khối `Blocks` đặt sát nhau thành dải dài | Mỗi khối là 1 prefab độc lập → sập riêng lẻ; hố mở rộng tối đa 3 tiles theo Schema |
| DP-EX-7 | `deathTrigger` chưa gán trong Inspector | `Awake()` `Debug.LogError` nêu rõ tên field; disable component để không gây NullReference mỗi frame |

**Trạng thái UI/Visual:** `Intact` (`Blocks/Idle`) → `Cracking` (0.2s, `Blocks/HitTop`) → `Broken` (`Part 1` + `Part 2` rơi, `HitSide` cho mép) → `Open` (không còn sprite sàn, trigger Hazard hoạt động).

### 3.5. `IceSurface` — Sàn băng trơn trượt (Màn 3)

**Happy Path**
1. Nhân vật bước vào vùng băng → `OnTriggerEnter2D` → `player.SetOnIce(true)`.
2. `PlayerController.FixedUpdate` đổi `accel` sang `iceFriction = 4f` → nhân vật giữ quán tính, trượt dài, khó dừng và khó đổi hướng.
3. Có thể phát hạt `Ice Particle.png` tại chân (xem §9.4).
4. Rời vùng băng → `OnTriggerExit2D` → `player.SetOnIce(false)`, quay lại `groundAcceleration = 65f` / `groundDeceleration = 80f`.

**Luồng ngoại lệ**

| Mã | Tình huống | Hành vi bắt buộc |
| :--- | :--- | :--- |
| IC-EX-1 | Nhân vật chết khi đang đứng trên băng | `rb.simulated = false` → `OnTriggerExit2D` **không** phát. Không cần xử lý thêm vì `PlayerController.Respawn` đã đặt `isOnIce = false` (Schema hàm 13) |
| IC-EX-2 | 2 người chơi cùng trên băng | Trạng thái `isOnIce` là **thuộc tính của từng PlayerController**, không phải của băng → gọi độc lập, không dùng biến đếm chung |
| IC-EX-3 | Băng liền kề nhau (2 prefab sát nhau) | Người chơi đi từ băng A sang băng B: `Exit(A)` và `Enter(B)` xảy ra cùng frame. Thứ tự có thể là Exit trước Enter → nháy `isOnIce = false` 1 frame. **Chấp nhận được** (1 frame = 0.02s) và không được xử lý bằng biến đếm toàn cục |
| IC-EX-4 | Băng dốc (`EdgeCollider2D` / `BoxCollider2D` xoay) | Không cần code khác — `SetOnIce` chỉ đổi hệ số gia tốc, trọng lực vẫn kéo nhân vật xuống dốc |
| IC-EX-5 | Prefab bị disable khi nhân vật đang đứng trên | `OnDisable` phải nhả trạng thái cho mọi player đang tiếp xúc (xem §5.4) |
| IC-EX-6 | Va chạm với quả/vật phẩm lăn trên băng | Chỉ xử lý collider có `PlayerController`; vật khác bỏ qua |

**Trạng thái UI/Visual:** Không có sprite trạng thái — băng là sprite tĩnh. Phản hồi duy nhất là **vật lý** (quán tính) + hạt băng.

### 3.6. `ArrowBooster` — Bong bóng mũi tên đẩy khí & hồi Nhảy Đôi

**Happy Path**
1. Mũi tên lơ lửng (Animator chạy `Idle`, 10 frame `Arrow/Idle (18x18)`), `CircleCollider2D` `Radius 0.6`, `isTrigger = true`.
2. Nhân vật nhảy chạm vào → `OnTriggerEnter2D` → `player != null && !player.IsDead`.
3. Guard: chỉ kích hoạt khi `isReady == true` (chống bấm đúp / chạm lại trong lúc cooldown).
4. `player.GrantDoubleJumpAndBoost(boostDirection * boostForce)` với `boostForce = 14f` → hất nhẹ lên + **hồi ngay 1 lượt Nhảy Đôi**.
5. Phát animation `Hit` (4 frame `Arrow/Hit (18x18)`), ẩn `SpriteRenderer` + tắt `collider.enabled` sau khi anim chạy xong.
6. `CooldownRoutine()` chờ `reactivateDelay = 2.5s` → bật lại renderer + collider, về `Idle`.

**Luồng ngoại lệ**

| Mã | Tình huống | Hành vi bắt buộc |
| :--- | :--- | :--- |
| AB-EX-1 | Nhân vật chạm lại khi đang cooldown | `collider.enabled = false` chặn vật lý **và** guard `isReady` chặn logic — hai lớp bảo vệ |
| AB-EX-2 | `ResetState()` khi đang cooldown | `StopAllCoroutines()`, bật lại collider + renderer **ngay**, `isReady = true`. Người chơi hồi sinh không phải chờ 2.5s |
| AB-EX-3 | Nhân vật chết ngay sau khi bật | Xong — `ResetState()` ở lượt sau khôi phục |
| AB-EX-4 | 2 người chơi chạm cùng frame | Lần đầu kích hoạt, tắt collider ngay trong cùng frame → lần 2 không xảy ra |
| AB-EX-5 | Mũi tên đặt sát Hố Tử Thần | Không xử lý đặc biệt — người chơi bật lên nhưng vẫn có thể rơi lại; đúng thiết kế "phải canh thời điểm" |
| AB-EX-6 | `boostDirection` mặc định | Schema chỉ nêu "đẩy bật theo hướng mũi tên" — hướng mặc định `(0,1)` thẳng đứng (xem §9.5) |

**Trạng thái UI/Visual:** `Ready` (Idle 10 frame loop) → `Fired` (Hit 4 frame, 1 lần) → `Hidden` (2.5s) → `Ready`.

---

## 4. VALIDATION & DATA SCHEMA

Quy định chặt chẽ cho toàn bộ 6 script. Mọi field phải khai báo đúng danh pháp, đúng kiểu, đúng default value từ Schema §3 & §4. Mọi kiểm tra hợp lệ được thực hiện trong `OnValidate()` (khi chỉnh trong Inspector) và/hoặc `Awake()` (khi game chạy) với thông báo lỗi rõ ràng.

### 4.1. Bảng thuộc tính Inspector chuẩn

| Script | Tên field (C#) | Kiểu dữ liệu | Default | Dải hợp lệ | Mục đích & Ý nghĩa | Lỗi cảnh báo khi vi phạm |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `TrampolineTrap` | `bounceForce` | `float` | `20f` | `> 0f` (chuẩn `20f`) | Lực bật toàn dự án | `"[TrampolineTrap] bounceForce phải > 0, đang là {v}. Tự reset về 20f."` |
| `TrampolineTrap` | `bounceDirection` | `Vector2` | `(0, 1)` | `sqrMagnitude > 0.001f` | Hướng đẩy (tự normalize) | `"[TrampolineTrap] bounceDirection không được là Vector2.zero. Tự gán Vector2.up."` |
| `TrampolineTrap` | `animator` | `Animator` | `null` (auto) | Không được null nếu có anim | `Animator` điều khiển `Idle` và `Jump` | `"[TrampolineTrap] animator chưa được gán trên {name}."` (Warning) |
| `FanTrap` | `windDirection` | `Vector2` | `(0, 1)` | `sqrMagnitude > 0.001f` | Hướng gió thổi (dọc/ngang) | `"[FanTrap] windDirection không được là zero. Tự gán Vector2.up."` |
| `FanTrap` | `windForce` | `float` | `15f` | `> 0f` (chuẩn `15f`) | Cường độ luồng gió | `"[FanTrap] windForce phải > 0, đang là {v}."` |
| `FanTrap` | `isActive` | `bool` | `true` | `true/false` | Trạng thái bật/tắt quạt | — |
| `FanTrap` | `animationFrames` | `Sprite[]` | `4 frames` | `Length >= 1` | Sheet `Fan/On (24x8).png` | `"[FanTrap] animationFrames trống trên {name}."` |
| `FallingPlatform` | `shakeDuration` | `float` | `0.4f` | `0.1f .. 2.0f` | Thời gian rung cảnh báo | `"[FallingPlatform] shakeDuration phải trong khoảng (0, 2], đang là {v}."` |
| `FallingPlatform` | `isPermanentInRun`| `bool` | `true` | `true/false` | Sập vĩnh viễn suốt lượt | — |
| `FallingPlatform` | `respawnDuration` | `float` | `3.0f` | `> 0f` khi `!isPermanent` | Thời gian tự hồi (chế độ tập) | `"[FallingPlatform] respawnDuration phải > 0 khi isPermanentInRun = false."` |
| `FallingPlatform` | `fallSpeed` | `float` | `12f` | `> 0f` | Vận tốc rơi | `"[FallingPlatform] fallSpeed phải > 0."` |
| `FallingPlatform` | `activeSprite` | `Sprite` | `On` | Không được null | Sprite nguyên vẹn | `"[FallingPlatform] activeSprite chưa được gán."` |
| `FallingPlatform` | `offSprite` | `Sprite` | `Off` | Không được null | Sprite sập | `"[FallingPlatform] offSprite chưa được gán."` |
| `DeathPitTrap` | `shakeDuration` | `float` | `0.2f` | `0.05f .. 1.0f` | Thời gian rung `HitTop` | `"[DeathPitTrap] shakeDuration không hợp lệ."` |
| `DeathPitTrap` | `solidCollider` | `BoxCollider2D`| `null` (auto) | Bắt buộc | Collider sàn lúc chưa sập | `"[DeathPitTrap] solidCollider chưa gán."` |
| `DeathPitTrap` | `deathTrigger` | `BoxCollider2D`| `null` (child) | Bắt buộc | Child trigger tag `"Hazard"` | `"[DeathPitTrap] deathTrigger chưa gán. Bẫy sẽ không giết được player!"` (Error) |
| `DeathPitTrap` | `blockRenderer` | `SpriteRenderer`| `null` (auto) | Bắt buộc | Renderer hiển thị gạch | `"[DeathPitTrap] blockRenderer chưa gán."` |
| `IceSurface` | `iceMaterial` | `PhysicsMaterial2D`| `null` (tùy chọn)| `friction <= 0.05f` | Physics Material trượt | — |
| `IceSurface` | `iceParticle` | `ParticleSystem`| `null` (tùy chọn)| — | Hạt băng `Ice Particle.png` | — |
| `ArrowBooster` | `boostForce` | `float` | `14f` | `> 0f` (chuẩn `14f`) | Lực đẩy mũi tên | `"[ArrowBooster] boostForce phải > 0."` |
| `ArrowBooster` | `boostDirection`| `Vector2` | `(0, 1)` | `sqrMagnitude > 0.001f` | Hướng đẩy | `"[ArrowBooster] boostDirection zero. Tự gán Vector2.up."` |
| `ArrowBooster` | `reactivateDelay`| `float` | `2.5f` | `> 0f` (chuẩn `2.5s`)| Thời gian ẩn trước khi hồi | `"[ArrowBooster] reactivateDelay phải > 0."` |
| `ArrowBooster` | `animator` | `Animator` | `null` (auto) | Bắt buộc | Hoạt ảnh Idle + Hit | `"[ArrowBooster] animator chưa gán."` |

### 4.2. Quy tắc khai báo C# (bắt buộc tuân thủ CLAUDE.md)

1. Mọi field Inspector: `[SerializeField] private Type tenBien = defaultValue;`. **Cấm** dùng `public Type tenBien;`.
2. Truy cập công khai từ bên ngoài: chỉ qua property read-only dạng expression-bodied:
   ```csharp
   public float BounceForce => bounceForce;
   public Vector2 BounceDirection => bounceDirection;
   public bool IsActive => isActive;
   public bool IsPermanentInRun => isPermanentInRun;
   public bool IsReady => isReady;
   ```
3. Mọi vector hướng (`bounceDirection`, `windDirection`, `boostDirection`) **bắt buộc được normalize** trong `Awake()` và `OnValidate()`:
   ```csharp
   if (bounceDirection.sqrMagnitude < 0.0001f) {
       bounceDirection = Vector2.up;
   } else {
       bounceDirection.Normalize();
   }
   ```
4. Kiểm tra tag va chạm: luôn dùng `col.CompareTag("Player")` hoặc `col.GetComponent<PlayerController>()`. **Cấm** dùng `col.tag == "Player"` (cấp phát chuỗi rác mỗi frame).
5. Trả về đúng mã lỗi: thông báo log theo đúng mẫu bảng trên, có tiền tố `[ClassName]` để dễ lọc trong Console.

---

## 5. EDGE CASES & SECURITY (TÍNH BỀN VỮNG & ZERO-GC)

Phần này chuyển hóa các nguyên tắc bảo mật phần mềm (idempotency, debounce, sanitization, resource leak prevention) sang môi trường game Unity.

### 5.1. Chống kích hoạt đúp (Debounce / Cooldown trong Game Loop)

Trong game 2D, người chơi có 2 chân, nhiều collider con, hoặc 2 người chơi cùng chạm vật thể trong 1 frame. Nếu không debounce:
- **`TrampolineTrap`**: Không cần cờ cooldown vì `GrantDoubleJumpAndBoost` gán đè vận tốc (`linearVelocity = boostVelocity`), không cộng dồn. Tuy nhiên, cần guard `col.GetComponent<PlayerController>()` — nếu Player có nhiều Collider con (ví dụ Hitbox + FootCheck), chỉ collider chính mang script mới kích hoạt.
- **`FanTrap`**: Sử dụng `OnTriggerStay2D` là **cố ý** (áp dụng lực liên tục mỗi FixedUpdate). Nhánh ngang gọi `AddExternalVelocity`, nhánh dọc ghi đè `linearVelocity.y`. Để không tích tụ vô hạn: `AddExternalVelocity` trong `PlayerController` tự tiêu biến `25f * Time.fixedDeltaTime`.
- **`FallingPlatform`**: Bắt buộc cờ `private bool isCollapsing = false;`. Khi `OnCollisionEnter2D` phát hiện Player:
  ```csharp
  if (isCollapsing) return;
  // Kiểm tra tiếp xúc mặt trên (Player phải đứng TRÊN bệ, không phải đập đầu từ dưới)
  bool hitFromAbove = false;
  for (int i = 0; i < col.contactCount; i++) {
      if (col.GetContact(i).normal.y < -0.5f) { hitFromAbove = true; break; }
  }
  if (!hitFromAbove) return;
  isCollapsing = true;
  StartCoroutine(CollapseSequence());
  ```
- **`DeathPitTrap`**: Bắt buộc cờ `private bool isTriggered = false;`. Khi `OnCollisionEnter2D` phát hiện Player:
  ```csharp
  if (isTriggered) return;
  bool hitFromAbove = false;
  for (int i = 0; i < col.contactCount; i++) {
      if (col.GetContact(i).normal.y < -0.5f) { hitFromAbove = true; break; }
  }
  if (!hitFromAbove) return;
  isTriggered = true;
  StartCoroutine(CollapseRoutine());
  ```
- **`ArrowBooster`**: Bắt buộc hai lớp bảo vệ:
  1. Cờ logic `private bool isReady = true;`. Khi chạm: `if (!isReady) return; isReady = false;`.
  2. Tắt collider vật lý ngay lập tức: `boosterCollider.enabled = false;`. Không có thêm va chạm vật lý nào được gửi trong suốt 2.5s.

### 5.2. Tính lũy đẳng (Idempotency) của `IResettable.ResetState()`

Trò chơi có thể kích hoạt `ResetState()` trong các tình huống nhạy cảm:
- Cả 2 người chơi cùng chết trong vòng 0.05s (chết chùm Co-op qua dây xích `SoulTether`).
- Người chơi spam phím `R` (Restart level).
- Chuyển màn chơi `Level_01 -> Level_02`.

**Yêu cầu bắt buộc cho mọi hàm `ResetState()`:**
1. Dừng mọi Coroutine đang chạy: `StopAllCoroutines();`.
2. Trả vị trí `transform.position = startPosition;` (và góc quay nếu có).
3. Trả lại toàn bộ cờ trạng thái: `isCollapsing = false;`, `isTriggered = false;`, `isReady = true;`.
4. Bật/tắt lại đúng collider và renderer.
5. Gọi `ResetState()` 1 lần, 10 lần liên tiếp, hay gọi giữa chừng coroutine đều phải cho kết quả giống hệt nhau.

```csharp
// Mẫu chuẩn FallingPlatform.ResetState()
public void ResetState() {
    StopAllCoroutines();
    transform.position = startPosition;
    isCollapsing = false;
    if (solidCollider != null) solidCollider.enabled = true;
    if (spriteRenderer != null && activeSprite != null) {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = activeSprite;
    }
}
```

```csharp
// Mẫu chuẩn DeathPitTrap.ResetState()
public void ResetState() {
    StopAllCoroutines();
    isTriggered = false;
    if (solidCollider != null) solidCollider.enabled = true;
    if (blockRenderer != null) {
        blockRenderer.enabled = true;
        blockRenderer.sprite = idleSprite;
    }
    if (deathTrigger != null) deathTrigger.gameObject.SetActive(false);
}
```

```csharp
// Mẫu chuẩn ArrowBooster.ResetState()
public void ResetState() {
    StopAllCoroutines();
    isReady = true;
    if (boosterCollider != null) boosterCollider.enabled = true;
    if (spriteRenderer != null) spriteRenderer.enabled = true;
    if (animator != null) animator.Play("Idle", 0, 0f);
}
```

### 5.3. Zero-GC trong vòng lặp vật lý (`Update` / `FixedUpdate` / `OnTriggerStay2D`)

Theo CLAUDE.md §5: cấm tuyệt đối sinh rác trong vòng lặp vì sẽ gây giật khung hình (frame drop) trong game 2D tốc độ cao.

1. **`FanTrap.OnTriggerStay2D`**:
   - **Cấm:** `col.GetComponent<PlayerController>()` mỗi frame mà không cache! Với 2 player và 50 FixedUpdate/s = 100 lần GetComponent/s rác.
   - **Giải pháp:** Sử dụng dictionary/hashset nhỏ hoặc kiểm tra nhanh tag trước:
     ```csharp
     if (!isActive) return;
     if (!col.CompareTag("Player")) return;
     // Chỉ GetComponent khi đúng tag Player
     var player = col.GetComponent<PlayerController>();
     if (player == null || player.IsDead) return;
     ```
   - **Cấm:** dùng `LINQ` (`.Where`, `.First`), `new List`, `string.Format`.
2. **`IceSurface`**:
   - Sử dụng `OnTriggerEnter2D` và `OnTriggerExit2D` (sự kiện theo biên, không chạy mỗi frame).
   - Tuyệt đối không kiểm tra trong `Update()`.
3. **`FallingPlatform.CollapseSequence`**:
   - Khi rung: tính vị trí rung từ toán học sin/cos thuần hoặc `Random.insideUnitCircle`:
     ```csharp
     // Không new Vector3 mỗi frame trong Update nếu có thể
     transform.position = startPosition + (Vector3)(Random.insideUnitCircle * shakeMagnitude);
     ```

### 5.4. Dọn dẹp an toàn khi tắt / hủy (`OnDisable` / `OnDestroy`)

1. **`IceSurface.OnDisable`**: Nếu một mảnh băng bị tắt khi Player đang đứng trên (ví dụ chuyển màn), phải nhả cờ:
   ```csharp
   private readonly HashSet<PlayerController> currentPlayers = new();
   private void OnTriggerEnter2D(Collider2D col) {
       if (col.CompareTag("Player") && col.TryGetComponent<PlayerController>(out var p)) {
           currentPlayers.Add(p);
           p.SetOnIce(true);
       }
   }
   private void OnTriggerExit2D(Collider2D col) {
       if (col.CompareTag("Player") && col.TryGetComponent<PlayerController>(out var p)) {
           currentPlayers.Remove(p);
           p.SetOnIce(false);
       }
   }
   private void OnDisable() {
       foreach (var p in currentPlayers) {
           if (p != null) p.SetOnIce(false);
       }
       currentPlayers.Clear();
   }
   ```
2. **`FallingPlatform` / `DeathPitTrap` / `ArrowBooster`**:
   - Khi `OnDisable`: dừng mọi coroutine (`StopAllCoroutines`).
   - Lưu `startPosition` trong `Awake()` thay vì `Start()` để đảm bảo luôn sẵn sàng trước khi có bất kỳ sự kiện nào xảy ra.

### 5.5. Tuân thủ Unity 6 Physics API (Cấm deprecated)

Theo CLAUDE.md §5:
- **Cấm tuyệt đối:** `rb.velocity`.
- **Bắt buộc:** `rb.linearVelocity`.
- Trong `FanTrap.OnTriggerStay2D`:
  ```csharp
  Vector2 vel = player.Rigidbody.linearVelocity;
  if (Mathf.Abs(windDirection.y) > 0.5f) {
      vel.y = Mathf.Max(vel.y, windDirection.y * windForce);
      player.Rigidbody.linearVelocity = vel;
  } else {
      player.AddExternalVelocity(new Vector2(windDirection.x * windForce * 0.35f, 0f));
  }
  ```

---

## 6. KIẾN TRÚC FILE, PREFABS & THÔNG SỐ TỪNG SCRIPT

### 6.1. Cấu trúc thư mục

```
Assets/
├── Scripts/
│   ├── Core/
│   │   └── IResettable.cs               [Scaffolding — xem §2.2]
│   └── Traps/
│       ├── TrampolineTrap.cs            [Module 3.1]
│       ├── FanTrap.cs                   [Module 3.2]
│       ├── FallingPlatform.cs           [Module 3.3]
│       ├── DeathPitTrap.cs              [Module 3.4]
│       ├── IceSurface.cs                [Module 3.5]
│       └── ArrowBooster.cs              [Module 3.6]
└── Prefabs/
    └── Traps/
        ├── Trampoline.prefab
        ├── FanTrap.prefab
        ├── FallingPlatform.prefab
        ├── DeathPit.prefab
        ├── IceRoad.prefab
        └── ArrowBooster.prefab
```

### 6.2. Đặc tả chi tiết từng Prefab & Component

Toàn bộ thông số kích thước Collider dưới đây được **tính toán trực tiếp từ pixel sprite thật** chia cho `PPU = 16`, khớp tuyệt đối với bảng Mục 4 trong `SCHEMA_CHI_TIET.md`:

#### Prefab 1: `Trampoline.prefab`
- **Root GameObject:**
  - Name: `Trampoline`
  - Tag: `"Untagged"` (Schema §1.2 dòng 49 & §4 dòng 560)
  - Layer: `Traps (10)` (Schema §1.3 dòng 58 & §4 dòng 560)
  - Components: `Transform`, `SpriteRenderer`, `Animator`, `BoxCollider2D`, `TrampolineTrap`
- **SpriteRenderer:**
  - Sprite: `Assets/Art/03_Bay/PA1_Traps/Trampoline/Idle.png` (frame `Idle_0`, kích thước 28×28, pivot `{0.5, 0}`)
- **Animator:**
  - Controller: `TrampolineAnimator` (đặt tại `Assets/Art/Animations/Traps/`). *Tên controller là do spec đặt — Schema chỉ quy định tên state, không quy định tên file controller.*
  - Clips: `Idle` (1 frame `Idle.png`), `Jump` (8 frames cắt từ `Trampoline/Jump (28x28).png`, file gốc 224×28 px → đúng 8 frame 28×28, tốc độ 20 FPS).
- **BoxCollider2D:**
  - `isTrigger`: `true`
  - `Size`: `(1.6f, 0.5f)` (khớp Schema §4 dòng 560)
  - `Offset`: `(0f, 0.25f)` (đặt trên bệ, bao trọn phần đệm co giãn)
- **TrampolineTrap:**
  - `bounceForce`: `20f`
  - `bounceDirection`: `Vector2.up` (có thể xoay nghiêng theo Transform z-rotation ở Màn 2)
  - `animator`: tham chiếu tới Animator cùng object

#### Prefab 2: `FanTrap.prefab`
- **Root GameObject:**
  - Name: `FanTrap`
  - Tag: `"Untagged"` (Schema §1.2 dòng 49 & §4 dòng 561)
  - Layer: `Traps (10)` (Schema §4 dòng 561)
  - Components: `Transform`, `SpriteRenderer`, `BoxCollider2D` (thân solid), `FanTrap`
- **SpriteRenderer:**
  - Sprite: `Assets/Art/03_Bay/PA1_Traps/Fan/Off.png` (khi tắt) hoặc `On (24x8).png` (khi bật)
- **BoxCollider2D (thân - solid):**
  - `isTrigger`: `false`
  - Layer: `Traps (10)`
  - `Size`: `(1.5f, 0.5f)` (24×8 px chia 16 PPU = 1.5×0.5 unit)
  - `Offset`: `(0f, 0f)`
- **Child GameObject `WindZone`:**
  - Name: `WindZone`
  - Tag: `"Untagged"`
  - Layer: `Traps (10)`
  - Components: `Transform`, `BoxCollider2D` (`isTrigger = true`)
  - `Size` đề xuất: `(1.5f, 4.0f)` khi thổi dọc (luồng gió cao 4 tiles); `(5.0f, 1.0f)` khi thổi ngang
- **FanTrap script:**
  - `windDirection`: `Vector2.up` (hoặc `Vector2.right` / `Vector2.left`)
  - `windForce`: `15f`
  - `isActive`: `true`
  - `animationFrames`: 4 sprites cắt từ `Fan/On (24x8).png` (`On (24x8)_0..3`)

#### Prefab 3: `FallingPlatform.prefab`
- **Root GameObject:**
  - Name: `FallingPlatform`
  - Tag: `"Untagged"` (Schema §4 dòng 562)
  - Layer: **`Ground (8)`** (Schema §1.3 dòng 56 & §4 dòng 562 — để `PlayerController.CheckGround` nhận diện được như sàn gạch thông thường)
  - Components: `Transform`, `SpriteRenderer`, `BoxCollider2D`, `FallingPlatform`
- **SpriteRenderer:**
  - Sprite: `Assets/Art/03_Bay/PA1_Traps/Falling Platforms/On (32x10).png`
- **BoxCollider2D:**
  - `isTrigger`: `false` (phải là Solid để nhân vật đứng lên)
  - `Size`: `(2.0f, 0.625f)` (32×10 px chia 16 PPU = 2.0 × 0.625 unit, khớp Schema §4 dòng 562)
  - `Offset`: `(0f, 0f)`
- **FallingPlatform script:**
  - `shakeDuration`: `0.4f`
  - `isPermanentInRun`: `true` (mặc định Màn 1 & 5)
  - `respawnDuration`: `3.0f`
  - `fallSpeed`: `12f`
  - `activeSprite`: `On (32x10)_0`
  - `offSprite`: `Off`

#### Prefab 4: `DeathPit.prefab` (Hierarchy 2 tầng theo Schema §3.4 & §4 dòng 563)
- **Root GameObject:**
  - Name: `DeathPit`
  - Tag: `"Untagged"`
  - Layer: **`Ground (8)`** (Schema §4 dòng 563 — ngụy trang làm mặt đất)
  - Components: `Transform`, `SpriteRenderer`, `BoxCollider2D` (`solidCollider`), `DeathPitTrap`
  - `SpriteRenderer`: Sprite `Blocks/Idle.png` (`Idle_0`, kích thước 22×22 px chia 16 PPU = 1.375 unit)
  - `BoxCollider2D (solidCollider)`: `isTrigger = false`, `Size: (1.375f, 1.375f)`
- **Child GameObject `DeathTriggerZone`:**
  - Name: `DeathTriggerZone`
  - Tag: **`"Hazard"`** (Bắt buộc theo Schema §1.2 dòng 46 & §4 dòng 563)
  - Layer: `Traps (10)`
  - Components: `Transform`, `BoxCollider2D` (`deathTrigger`, `isTrigger = true`)
  - `Offset`: đặt thấp hơn mặt sàn `0.8f` (Schema §3.4)
  - `Size`: `(1.375f, 1.0f)`
  - Ban đầu ở trạng thái: **`SetActive(false)`** (được bật khi khối gạch vỡ)
- **DeathPitTrap script:**
  - `shakeDuration`: `0.2f`
  - `solidCollider`: tham chiếu tới collider của Root
  - `deathTrigger`: tham chiếu tới collider của Child `DeathTriggerZone`
  - `blockRenderer`: tham chiếu tới SpriteRenderer của Root
  - `idleSprite`: `Blocks/Idle.png`
  - `crackSprite`: `Blocks/HitTop (22x22).png` (frame 0)
  - `part1Sprite`: `Blocks/Part 1 (22x22).png`
  - `part2Sprite`: `Blocks/Part 2 (22x22).png`

#### Prefab 5: `IceRoad.prefab`
- **Root GameObject:**
  - Name: `IceRoad`
  - Tag: `"Untagged"` (Schema §1.2 dòng 49 & §4 dòng 564)
  - Layer: **`Ground (8)`** (Schema §1.3 dòng 56 & §4 dòng 564 — nhân vật phải đi đứng và nhảy được trên băng)
  - Components: `Transform`, `SpriteRenderer`, `BoxCollider2D` (Solid), `BoxCollider2D` (Trigger kiểm tra đứng trên băng) hoặc 1 Trigger phủ bên trên, `IceSurface`
- **SpriteRenderer:**
  - Sprite: `Sand Mud Ice (16x6)_2` (frame 2 = Băng xanh, kích thước 49×80 px chia 16 PPU ≈ 3.06 × 5.0 unit)
- **Collider:**
  - Solid collider trên Layer `Ground (8)` để đỡ nhân vật.
  - Trigger collider (hoặc dùng `OnCollisionEnter2D/Exit2D`) để kích hoạt `SetOnIce(true/false)`.
- **IceSurface script:**
  - Gắn trên GameObject chứa collider.

#### Prefab 6: `ArrowBooster.prefab`
- **Root GameObject:**
  - Name: `ArrowBooster`
  - Tag: `"Untagged"` (Schema §1.2 dòng 49 & §4 dòng 565)
  - Layer: `Traps (10)` (Schema §4 dòng 565)
  - Components: `Transform`, `SpriteRenderer`, `Animator`, `CircleCollider2D`, `ArrowBooster`
- **SpriteRenderer:**
  - Sprite: `Arrow/Idle (18x18)_0` (18×18 px chia 16 PPU = 1.125 unit)
- **Animator:**
  - Controller: `Arrow_Controller.controller`
  - Clips: `Idle` (10 frames `Idle (18x18).png`, loop), `Hit` (4 frames `Hit (18x18).png`, không loop)
- **CircleCollider2D:**
  - `isTrigger`: `true`
  - `Radius`: `0.6f` (Schema §4 dòng 565)
  - `Offset`: `(0f, 0f)`
- **ArrowBooster script:**
  - `boostForce`: `14f` (Schema §1.4 dòng 70 & §3.6 dòng 393)
  - `boostDirection`: `Vector2.up` (mặc định)
  - `reactivateDelay`: `2.5f` (Schema §1.4 dòng 70 & §3.6 dòng 394)
  - `animator`: tham chiếu cùng object
  - `boosterCollider`: tham chiếu tới `CircleCollider2D`
  - `spriteRenderer`: tham chiếu tới `SpriteRenderer`

---

## 7. TIÊU CHUẨN ANTI-AI DESIGN TRONG GAME PIXEL 2D

Mặc dù tiêu chuẩn Anti-AI gốc được phát triển cho giao diện web/SaaS (cấm gradient tím hồng neon, cấm bo góc quá đà, cấm emoji vô nghĩa), bản chất cốt lõi của nó là: **tính kỷ luật thẩm mỹ, tôn trọng bảng màu hệ thống, phản hồi thị giác rõ ràng và không lạm dụng hiệu ứng thừa thãi**.

Dưới đây là phiên bản chuẩn hóa cho dự án **Pixel Platformer 2D "One Hit Down"**:

| Nguyên tắc web gốc | Chuyển dịch sang Pixel Art Game (One Hit Down) | Quy tắc cụ thể cho Module 3 |
| :--- | :--- | :--- |
| **Cấm gradient tím/hồng neon** | **Cấm hạt/ánh sáng phát quang lòe loẹt** | Không dùng `Light2D` màu tím/hồng neon chiếu quanh bẫy. Toàn bộ Sprite dùng màu phẳng chuẩn từ asset pack (`Assets/Art/`). Băng dùng màu cyan nhạt `#A9EAF0`, không pha màu neon. |
| **Cấm bo góc lố (`rounded-3xl`)** | **Tôn trọng lưới pixel (Pixel Grid Discipline)** | Mọi collider phải khớp với biên pixel sprite tại `PPU = 16`. Không dùng capsule collider trơn trượt trên bệ đá vuông. Bệ sập dùng `BoxCollider2D` mép sắc nét. |
| **Cấm nhét emoji rác (🚀 ✨)** | **Cấm text nổi kiểu casual mobile** | Không hiện chữ "BOOST! +1 JUMP" hay icon bay lơ lửng khi chạm đệm nhún/mũi tên. Phản hồi hoàn toàn qua: hoạt ảnh sprite (`Jump`, `Hit`), âm thanh SFX, và quán tính vật lý nhân vật. |
| **Cấm popup alert cổ điển** | **Cấm dừng game (Zero Pause)** | Khi bẫy sập hoặc nhân vật chết: không bao giờ bật popup hỏi "Bạn có muốn hồi sinh không?". Game lập tức hồi sinh sau `0.18s` mượt mà, đưa toàn bộ cơ quan về trạng thái ban đầu qua `ResetState()`. |
| **Đầy đủ 6 trạng thái UI** | **Đầy đủ trạng thái thị giác của cơ quan (Machine States)** | Mỗi cơ quan phải có đủ trạng thái nhận biết được bằng mắt: xem bảng bên dưới. Không có cơ quan nào đổi logic mà hình ảnh đứng im. |
| **Viền mỏng 1px sạch sẽ** | **Đường nét pixel 1 unit nhất quán** | Mọi sprite đều được vẽ với nét viền đen 1px nguyên bản từ asset pack. Không dùng outline shader phát sáng. |

### Bảng 6 trạng thái thị giác bắt buộc cho từng cơ quan

| Cơ quan | 1. Sẵn sàng (Default/Idle) | 2. Cảnh báo (Warning) | 3. Hoạt động (Active/Fired) | 4. Hồi chiêu (Cooldown/Hidden) | 5. Khôi phục (Reset) | 6. Vô hiệu hóa (Disabled) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `Trampoline` | Đệm xẹp tĩnh (`Idle`) | — | Lò xo bung cực đại (8 frames `Jump`) | Trở về `Idle` sau hết clip | Tức thì về `Idle` | Collider tắt |
| `FanTrap` | Cánh quạt quay liên tục (4 frames `On`) | — | Luồng gió đẩy hạt bụi bay lên | — | Giữ nguyên hoặc tắt | Cánh đứng yên (`Off.png`), trigger tắt |
| `FallingPlatform` | Bệ đá xanh nguyên vẹn (`On`) | Rung lắc 0.4s (biên độ ±2px) | Rơi tự do (`Off.png`) | Biến mất khỏi màn hình | Tức thì trở về vị trí đầu, đổi sprite `On` | `solidCollider.enabled = false` |
| `DeathPit` | Gạch đá lành lặn (`Idle`) | Nứt mặt trên 0.2s (`HitTop`) | Tung 2 mảnh vỡ (`Part 1 & 2`), mở hố | Hố mở toang suốt màn chơi | Tức thì đóng hố, sprite `Idle`, tắt hazard | — |
| `IceSurface` | Mặt băng trong suốt | — | Hạt băng `Ice Particle.png` bắn ra khi chân trượt | — | — | — |
| `ArrowBooster` | Bong bóng tên đung đưa (10 frames `Idle`) | — | Mũi tên nổ bung (4 frames `Hit`) | Ẩn hoàn toàn trong 2.5s | Tức thì hiện lại, chạy clip `Idle` | Renderer + collider tắt |

---

## 8. TEST MATRIX BẮT BUỘC (DÀNH CHO `auto-tester`)

Subagent `auto-tester` **bắt buộc** phải triển khai đầy đủ các test cases trong bảng dưới đây ở Bước 3.

### 8.1. Phân loại kiểm thử

1. **Category A — Headless Unit Tests (Chạy không cần Unity Editor):**
   - Kiểm tra các hàm tính toán toán học, vector normalize, clamping, state machine pure logic.
   - Viết bằng C# / NUnit hoặc xUnit trong thư mục `Tests/` chạy qua `dotnet test`.
2. **Category B — Headless Compilation Gate (Bắt buộc 100%):**
   - Biên dịch toàn bộ 6 file script qua `dotnet build` sử dụng project tạm chiếu tới managed DLLs của Unity:
     `UnityEngine.CoreModule.dll`, `UnityEngine.Physics2DModule.dll`.
   - Kết quả bắt buộc: **0 Warning(s), 0 Error(s)**.
3. **Category C — Mock Integration Tests:**
   - Kiểm tra tương tác giữa các script và mock `PlayerController` / mock `IResettable`.

### 8.2. Danh sách Test Cases chi tiết

| Test ID | Script mục tiêu | Tên Test Case | Điều kiện đầu vào | Kết quả kỳ vọng | Loại |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TC-TR-01` | `TrampolineTrap` | Vector Normalization | `bounceDirection = (3, 4)` | `bounceDirection.magnitude == 1.0f` và hướng là `(0.6, 0.8)` | Unit |
| `TC-TR-02` | `TrampolineTrap` | Zero Vector Fallback | `bounceDirection = (0, 0)` | Tự gán `Vector2.up` `(0, 1)`, không gây chia cho 0 | Unit |
| `TC-TR-03` | `TrampolineTrap` | Boost Velocity Calculation | `direction = (0, 1)`, `force = 20f` | `velocity == (0, 20f)` | Unit |
| `TC-TR-04` | `TrampolineTrap` | Dead Player Rejection | `player.IsDead = true` | Không gọi `GrantDoubleJumpAndBoost` | Mock |
| `TC-TR-05` | `TrampolineTrap` | Alive Player Trigger | `player.IsDead = false` | `GrantDoubleJumpAndBoost` được gọi đúng 1 lần với vector `(0, 20f)` | Mock |
| `TC-FA-01` | `FanTrap` | Upward Wind Velocity Math | `vy = 2f`, `windDir = (0, 1)`, `force = 15f` | `newVy = max(2f, 15f) == 15f` | Unit |
| `TC-FA-02` | `FanTrap` | Upward Wind Clamping | `vy = 18f`, `windDir = (0, 1)`, `force = 15f` | `newVy = max(18f, 15f) == 18f` (không làm giảm tốc khi đang nhảy cao hơn) | Unit |
| `TC-FA-03` | `FanTrap` | Horizontal Wind Force Math | `windDir = (1, 0)`, `force = 15f` | `extVx == 15f * 0.35f == 5.25f` | Unit |
| `TC-FA-04` | `FanTrap` | Horizontal Reverse Wind Math | `windDir = (-1, 0)`, `force = 15f` | `extVx == -5.25f` | Unit |
| `TC-FA-05` | `FanTrap` | Inactive Fan Rejection | `isActive = false` | Không tác dụng lực lên player | Mock |
| `TC-FA-06` | `FanTrap` | Zero Vector Guard | `windDirection = (0, 0)` | Tự fallback về `(0, 1)` | Unit |
| `TC-FP-01` | `FallingPlatform` | State Machine Transitions | Bắt đầu tại `Solid` | Gọi trigger → chuyển `Shaking` → hết 0.4s → `Falling` | Mock |
| `TC-FP-02` | `FallingPlatform` | Trigger Debounce | Đang ở `Shaking` nhận tiếp va chạm | Bỏ qua, không tạo coroutine thứ 2 | Mock |
| `TC-FP-03` | `FallingPlatform` | Bottom Hit Rejection | Va chạm từ dưới (`contact.normal.y > 0`) | Không kích hoạt rung | Mock |
| `TC-FP-04` | `FallingPlatform` | Permanent Collapse | `isPermanentInRun = true` | Sau khi rơi, không tự hồi | Mock |
| `TC-FP-05` | `FallingPlatform` | Training Respawn Mode | `isPermanentInRun = false`, `respawnDuration = 3f` | Tự hồi về `Solid` tại `startPosition` sau 3s | Mock |
| `TC-FP-06` | `FallingPlatform` | Idempotent ResetState | Gọi `ResetState()` 5 lần liên tiếp | Trạng thái luôn là `Solid`, vị trí đúng `startPosition`, collider bật | Mock |
| `TC-DP-01` | `DeathPitTrap` | Initial State Verification | Khởi tạo | `solidCollider.enabled == true`, `deathTrigger.activeSelf == false` | Mock |
| `TC-DP-02` | `DeathPitTrap` | Collapse Sequence | Player giẫm mặt trên | Rung 0.2s → `solidCollider.enabled == false` → `deathTrigger.activeSelf == true` | Mock |
| `TC-DP-03` | `DeathPitTrap` | Permanent Open In Run | Sau sập | Hố giữ nguyên trạng thái mở, không tự đóng | Mock |
| `TC-DP-04` | `DeathPitTrap` | Idempotent ResetState | Gọi `ResetState()` khi hố đang mở | `solidCollider` bật lại, `deathTrigger` tắt, `isTriggered = false` | Mock |
| `TC-DP-05` | `DeathPitTrap` | Side Contact Ignored | Va chạm từ cạnh bên | Không kích hoạt sập | Mock |
| `TC-IC-01` | `IceSurface` | Player Enter Surface | Player chạm collider | Gọi `player.SetOnIce(true)` | Mock |
| `TC-IC-02` | `IceSurface` | Player Exit Surface | Player rời collider | Gọi `player.SetOnIce(false)` | Mock |
| `TC-IC-03` | `IceSurface` | Non-Player Ignored | Vật thể không có `PlayerController` | Không làm gì | Mock |
| `TC-IC-04` | `IceSurface` | OnDisable Cleanup | 2 player đang trên băng, script bị disable | Cả 2 player đều nhận `SetOnIce(false)` | Mock |
| `TC-AB-01` | `ArrowBooster` | Boost Vector Math | `dir = (0, 1)`, `force = 14f` | `velocity == (0, 14f)` | Unit |
| `TC-AB-02` | `ArrowBooster` | Double Jump Granted | Player chạm mũi tên khi `isReady = true` | `player.GrantDoubleJumpAndBoost` được gọi đúng 1 lần | Mock |
| `TC-AB-03` | `ArrowBooster` | Cooldown Lockout | Chạm lần 2 khi đang trong 2.5s cooldown | Bỏ qua hoàn toàn, không gọi lại `GrantDoubleJumpAndBoost` | Mock |
| `TC-AB-04` | `ArrowBooster` | Reactivation After Delay | Sau 2.5s | `isReady == true`, collider và renderer được bật lại | Mock |
| `TC-AB-05` | `ArrowBooster` | Instant ResetState | Gọi `ResetState()` khi đang cooldown 1.0s | Ngay lập tức `isReady = true`, collider bật, không cần đợi nốt 1.5s | Mock |
| `TC-BLD-01` | Toàn bộ 6 scripts | Headless Compilation | Toàn bộ code trong `Assets/Scripts/Traps/` | Build thành công 0 errors, 0 warnings | Build |

### 8.3. Mẫu Harness biên dịch Headless cho `auto-tester`

Tạo file project tạm tại `Tools/CompileCheck/CompileCheck.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <Nullable>disable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="../../Assets/Scripts/Traps/*.cs" />
    <Compile Include="../../Assets/Scripts/Core/IResettable.cs" Condition="Exists('../../Assets/Scripts/Core/IResettable.cs')" />
    <!-- Stub PlayerController chỉ để build check nếu Module 1 chưa commit -->
    <Compile Include="Stubs/*.cs" Condition="!Exists('../../Assets/Scripts/Player/PlayerController.cs')" />
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>G:/Slides_BaiGiangTLU/ThietKeGame/Unity/hub/editor/6000.6.6f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.Physics2DModule">
      <HintPath>G:/Slides_BaiGiangTLU/ThietKeGame/Unity/hub/editor/6000.6.6f1/Editor/Data/Managed/UnityEngine/UnityEngine.Physics2DModule.dll</HintPath>
    </Reference>
    <Reference Include="UnityEngine.AnimationModule">
      <HintPath>G:/Slides_BaiGiangTLU/ThietKeGame/Unity/hub/editor/6000.6.6f1/Editor/Data/Managed/UnityEngine/UnityEngine.AnimationModule.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

Lệnh chạy kiểm tra:
```bash
dotnet build Tools/CompileCheck/CompileCheck.csproj -c Release
```

### 8.4. Mẫu Stub `PlayerController` tối thiểu (chỉ đặt trong `Tools/CompileCheck/Stubs/`)

File: `Tools/CompileCheck/Stubs/PlayerControllerStub.cs` (**tuyệt đối không đưa vào `Assets/`**):
```csharp
namespace OneHitDown.Player {
    using UnityEngine;
    public class PlayerController : MonoBehaviour {
        public bool IsDead => false;
        public Rigidbody2D Rigidbody => GetComponent<Rigidbody2D>();
        public void GrantDoubleJumpAndBoost(Vector2 boostVelocity) {}
        public void AddExternalVelocity(Vector2 extVel) {}
        public void SetOnIce(bool onIce) {}
    }
}
```

---

## 9. ĐIỂM CẦN CHỐT & CÂU HỎI MỞ (TUÂN THỦ CLAUDE.MD §3)

Tuân thủ nghiêm ngặt điều khoản: *"ừ Mục 3 & Mục 4 của `SCHEMA_CHI_TIET.md`, không tự đặt số mới. Nếu Schema không có giá trị tương ứng, dừng lại và hỏi theo Mục 3"*.

Dưới đây là các điểm đặc thù cần lưu ý cho `senior-dev` và Orchestrator:

### 9.1. Trạng thái asset `Sand Mud Ice (16x6).png` (Mặt cát / bùn / băng)
- **Thực tế asset:** File `Assets/Art/03_Bay/PA1_Traps/Sand Mud Ice/Sand Mud Ice (16x6).png` có kích thước 176×80 và chứa **3 frames khác nhau**:
  - Frame 0 (x: 0..49): Màu nâu/cam → **Cát (Sand)**.
  - Frame 1 (x: 63..112): Màu xanh lá đậm → **Bùn (Mud)**.
  - Frame 2 (x: 127..175): Màu lam nhạt `#A9EAF0` → **Băng (Ice)**.
- **Quy định trong Schema:** Schema §3 Module 3.5 chỉ yêu cầu script `IceSurface.cs` (Sàn băng trơn trượt) cho **Màn 3**. Chưa đặc tả cơ chế làm chậm của Bùn hay lún của Cát.
- **Quyết định thiết kế cho `senior-dev`:**
  - Script tên chính xác: `IceSurface.cs`.
  - Sprite mặc định cho prefab `IceRoad.prefab` là **frame 2 (Băng - `Sand Mud Ice (16x6)_2`)**.
  - Không tự bịa thêm script `MudSurface.cs` hay `SandSurface.cs` để tuân thủ nguyên tắc Zero Extrapolation. Nếu sau này nhóm muốn làm thêm Bùn, sẽ mở rộng qua enum sau khi thống nhất.

### 9.2. Hướng đẩy của `ArrowBooster`
- **Quy định trong Schema:** Schema §3 Module 3.6 ghi: *"đẩy bật theo hướng mũi tên (`boostForce = 14f`)"*.
- **Quyết định thiết kế cho `senior-dev`:**
  - Field: `[SerializeField] private Vector2 boostDirection = Vector2.up;`.
  - Mặc định là `Vector2.up` (thẳng đứng lên trời để cứu người chơi rơi xuống vực).
  - Khi level designer xoay GameObject trong Scene hoặc chỉnh `boostDirection` trong Inspector (ví dụ chéo lên `(1, 1).normalized`), script sẽ tự normalize và đẩy theo hướng đó.
  - Giá trị lực cố định đúng Schema: **`boostForce = 14f`**.

### 9.3. Xử lý mảnh vỡ của `DeathPitTrap`
- **Quy định trong Schema:** Schema §3 Module 3.4 ghi: *"tung 2 mảnh vỡ `Part 1 & 2`"*.
- **Quyết định thiết kế cho `senior-dev`:**
  - Không tạo thêm Prefab mảnh vỡ riêng nếu chưa có asset VFX trong Schema.
  - Cách triển khai nhẹ và đúng chuẩn: sinh 2 GameObject tạm thời chứa `SpriteRenderer` mang `Part 1 (22x22).png` và `Part 2 (22x22).png`, gắn `Rigidbody2D` tạm thời cho nảy ra 2 bên rồi tự hủy sau 1.0s, hoặc đơn giản hơn là chỉ ẩn sprite chính và mở hố (chọn cách an toàn không sinh rác).
  - Tuyệt đối không để việc thiếu VFX mảnh vỡ làm block logic mở hố.

### 9.4. Giá trị `iceFriction`
- Giá trị gia tốc trượt trên băng đã được chốt chính thức trong Schema §3 Module 1: **`iceFriction = 4f`** (so với mặt đất thông thường `groundAcceleration = 65f`, ma sát giảm hơn 16 lần, tạo cảm giác trơn trượt rõ rệt).
- Script `IceSurface.cs` **không cần chứa biến số ma sát** — nó chỉ cần gọi `player.SetOnIce(true)` và `player.SetOnIce(false)`. Logic vật lý thuộc về `PlayerController` của Người 1. Điều này đảm bảo tính đóng gói Component-Based chuẩn mực.

### 9.5. Dị thường Sprite Slicing trong Metadata hiện tại (Lưu ý quan trọng cho `senior-dev`)
Khảo sát chi tiết các file `.meta` tại `Assets/Art/03_Bay/PA1_Traps/` phát hiện một số sheet sprite có cấu hình cắt (slice) không đồng đều:
1. **`Falling Platforms/On (32x10).png`**:
   - File ảnh gốc: kích thước `128×10` px (chứa 4 frames bệ đá kích thước `32×10` px).
   - Metadata hiện tại: chỉ có **1 slice duy nhất** tên `On (32x10)_0` với kích thước `128×10` px!
   - Hậu quả nếu dùng trực tiếp: Sprite hiển thị sẽ dài 128 px (8 tiles = 8 units) trong khi Collider theo Schema chỉ rộng `2.0` units (32 px = 2 tiles)!
   - **Chỉ dẫn cho `senior-dev`:** File `Off.png` có kích thước chuẩn `32×10` px (2.0 × 0.625 units). Khi tạo prefab `FallingPlatform.prefab`, `senior-dev` nên dùng `Off.png` làm sprite tĩnh kiểm tra kích thước, hoặc trong Unity Editor dùng Sprite Editor cắt lại sheet `On (32x10).png` theo lưới `Grid By Cell Size: 32x10` (4 frames) để làm hoạt ảnh 4 frames đúng Schema. **Cấm sửa tay file `.meta` bằng text editor** theo quy tắc CLAUDE.md §6.
2. **`Arrow/Idle (18x18).png`**:
   - File ảnh gốc: `180×18` px (10 frames `18×18` px).
   - Metadata hiện tại: có 9 slices với chiều rộng không đều (do thuật toán trim alpha tự động của Unity) và pivot `{0, 0}`.
   - Khi chạy Animator, sprite có thể bị giật nhẹ nếu không căn lại pivot tâm `{0.5, 0.5}`. `senior-dev` nên lưu ý khi thiết lập clip Animation.
3. **`Trampoline/Jump (28x28).png`**:
   - Được cắt cực chuẩn: đủ 8 frames `28×28` px, pivot đều `{0.5, 0}` (đáy tâm), khớp 100% với `Idle.png` (cũng `28×28`, pivot `{0.5, 0}`). Prefab này hoàn toàn sẵn sàng không có dị thường.

---

## 10. KẾT LUẬN & HƯỚNG DẪN BÀN GIAO CHO `senior-dev`

Tài liệu đặc tả này đã hoàn tất đầy đủ 100% yêu cầu:
1. Đã khảo sát thực tế toàn bộ thư mục asset, sprite slice, layer/tag và API Unity 6.
2. Đã quy định rõ danh sách 6 script Module 3 + 1 file scaffolding `IResettable.cs`.
3. Đã đặc tả chi tiết 6 Prefabs tương ứng tại `Assets/Prefabs/Traps/`.
4. Đã thiết kế đầy đủ luồng người dùng, các trường hợp biên, chống bấm đúp, an toàn `ResetState()`.
5. Đã lập Test Matrix với 31 test cases cụ thể cho `auto-tester`.
6. Đã cung cấp công cụ `Tools/CompileCheck/` để kiểm tra biên dịch headless.

**Đường dẫn tuyệt đối của file spec:**
`G:\Slides_BaiGiangTLU\ThietKeGame\Projects\OneHitDown\Docs\specs\module3-interactive-traps-spec.md`

Orchestrator có thể chuyển giao trực tiếp tài liệu này cho Subagent `senior-dev` để tiến hành lập trình.
