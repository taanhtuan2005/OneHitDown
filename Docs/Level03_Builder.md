# Level 03 Scene Builder — Hướng dẫn vận hành

**Phạm vi:** `Assets/Editor/Level03SceneBuilder.cs`, `Assets/Editor/Level03BuildSafety.cs`
**Scene đích:** `Assets/Scenes/Level_03.unity`
**Người phụ trách Scene:** Trần Văn Tuyên (theo `Docs/Phan_cong.md`)

---

## 1. Tổng quan

`Level03SceneBuilder` là **generator xóa-sạch-dựng-lại** Scene `Assets/Scenes/Level_03.unity`.
Mỗi lần chạy, builder:

1. Gọi `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)` — tạo Scene trống.
2. Dựng lại toàn bộ Background, Camera, Player, Terrain, Prefab Module 3, Mock (nếu còn), HUD labels.
3. Lưu đè lên `Assets/Scenes/Level_03.unity`.

Đây là thao tác **DESTRUCTIVE**: mọi chỉnh tay trong Scene (vị trí object, tham chiếu Inspector, thay đổi thủ công) sẽ **bị xóa vĩnh viễn** và không thể khôi phục ngoài bản sao lưu hoặc Git.

> Builder **không** tạo Scene mới có tên khác. Nó luôn ghi đè đúng đường dẫn `Assets/Scenes/Level_03.unity`.

---

## 2. Cách chạy

**Menu:** `Tools > One Hit Down > Build Level 03 Scene (Catacombs of Ice & Ember)`

**Luồng thực thi:**

| Bước | Hành động | Thất bại thì sao |
|---|---|---|
| 1. Backup | `Level03BuildSafety.BackupBeforeBuild(ScenePath)` — copy Scene hiện có sang `Assets/Scenes/_Backup/Level_03_{yyyyMMdd_HHmmss}.unity` | Trả `false` → builder **hủy dựng**, không đụng vào Scene |
| 2. Confirm | `Level03BuildSafety.ConfirmDestructiveRebuild()` — hộp thoại "Xây lại / Hủy" (tự true ở batch mode) | Người dùng bấm **Hủy** → builder dừng |
| 3. Missing prefab | `Level03BuildSafety.ReportMissingPrefabs(...)` — kiểm tra đủ prefab chính thức | Thiếu prefab → `Debug.LogError` + **hủy dựng** |
| 4. Dựng | Dựng toàn bộ nội dung Scene | Có exception → log lỗi, Scene có thể ở trạng thái dở dang |
| 5. Lưu | `EditorSceneManager.SaveScene` ghi đè `Level_03.unity` | — |

**Dựng lần đầu:** nếu `Level_03.unity` chưa tồn tại, bước backup trả `true` và bỏ qua (không có gì để sao lưu).

**Batch mode / CI:** `Application.isBatchMode == true` khiến bước confirm tự trả `true`, không hiện hộp thoại. Backup vẫn chạy bình thường.

---

## 3. Cảnh báo

- Chạy builder sẽ **XÓA mọi chỉnh tay** trong `Level_03.unity`.
- **Luôn commit hoặc backup trước khi chạy builder.** Nếu Scene đang có thay đổi chưa commit, hãy `git add` + `git commit` trước.
- Không chạy builder khi đang có người khác cùng làm việc trên Scene (tránh conflict merge Scene).
- Không sửa tay `.meta` của Scene sau khi builder ghi đè — GUID phải được giữ nguyên để các tham chiếu Scene trong Build Settings và Prefab không bị gãy.
- Builder ghi đè Scene đang mở trong Editor. Mọi thay đổi chưa lưu của Scene hiện tại sẽ mất — lưu trước nếu cần.

---

## 4. Restore backup

**Cách 1 — dùng bản sao lưu tự động:**

```bash
# Liệt kê các bản sao lưu (mới nhất ở cuối tên file theo timestamp)
ls Assets/Scenes/_Backup/

# Ghi đè Scene hiện tại bằng bản sao lưu mong muốn
cp Assets/Scenes/_Backup/Level_03_20261030_143012.unity Assets/Scenes/Level_03.unity
```

Sau khi copy, mở Unity để `AssetDatabase` import lại, hoặc gọi `Assets > Refresh` (Ctrl+R).
**Giữ nguyên** `Assets/Scenes/Level_03.unity.meta` — chỉ thay nội dung `.unity`, không thay `.meta`.

**Cách 2 — dùng Git (khuyến nghị nếu Scene đã commit):**

```bash
git checkout -- Assets/Scenes/Level_03.unity
```

Hoặc quay về một commit cụ thể:

```bash
git checkout <commit-hash> -- Assets/Scenes/Level_03.unity
```

**Trạng thái `.gitignore` hiện tại:**

- File `.gitignore` ở repo **KHÔNG** có mục loại trừ `Assets/Scenes/_Backup/`.
- Đã kiểm tra bằng `git check-ignore -v Assets/Scenes/_Backup/x.unity` → **không bị ignore** (exit code 1).
- **Hệ quả:** các bản sao lưu `.unity` trong `_Backup/` sẽ **xuất hiện trong `git status`** và có thể bị commit nhầm.
- **Khuyến nghị (chưa áp dụng, cần người phụ trách `.gitignore` duyệt):** thêm dòng `Assets/Scenes/_Backup/` vào `.gitignore`. Không tự ý sửa `.gitignore` trong tài liệu này.

---

## 5. Bảng mapping Mock → Prefab chính thức

Builder thay thế toàn bộ object Mock (script tạm trong `Assets/Scripts/Dev/Mocks/`) bằng Prefab chính thức của Module 2 & Module 4.

| Cũ (Mock) | Mới (prefab chính thức) | Đường dẫn | Số lượng |
|---|---|---|---|
| MockFireTrap | FireTrap.prefab | Assets/Prefabs/ | 47 |
| MockMovingPlatform | MovingPlatform.prefab | Assets/Prefabs/ | 2 |
| MockSpikedBallPendulum | SpikedBall.prefab | Assets/Prefabs/ | 1 |
| MockCollectibleItem | Fruit_Kiwi.prefab | Assets/Prefabs/ | 3 |
| MockCollectibleItem | Fruit_Strawberry.prefab | Assets/Prefabs/ | 2 |
| MockGoalTrigger | ExitGate.prefab | Assets/Prefabs/ | 1 |

> Sau khi build, **không còn object Mock nào** trong Scene. Xem mục 6 để đối chiếu chỉ số.

---

## 6. Chỉ số vàng của Scene đích

Đây là bộ số chuẩn mà Scene sau khi build phải đạt. Dùng để đối chiếu thủ công hoặc qua tool audit.

| Chỉ số | Giá trị kỳ vọng |
|---|---|
| Tổng GameObject | 239 |
| Prefab instance | 79 |
| FireTrap | 47 |
| IceRoad | 11 |
| Fruit_Kiwi | 3 |
| Fruit_Strawberry | 2 |
| MovingPlatform | 2 |
| SpikedBall | 1 |
| Object Mock còn lại | 0 |
| GameObject có tag `Player` | 0 |
| GameObject có tag `Hazard` | 9 |
| Root GameObject | 7 (`01`, `02`, `03`, `04`, `05`, `07`, `KillZone`) |

**Ghi chú về root:** Scene có đúng 7 root GameObject, gồm các nhóm `--- 01 ... ---`, `--- 02 ... ---`, `--- 03 ... ---`, `--- 04 ... ---`, `--- 05 ... ---`, `--- 07 ... ---` và `KillZone`.
Tag `Player` không được gán cho bất kỳ object nào trong Scene (Player là Prefab con, tag `Player` nằm trong Prefab gốc — không tính ở cấp Scene).

---

## 7. Kiểm chứng sau khi build

Sau khi chạy builder, **bắt buộc** chạy audit để xác nhận Scene khớp 100% chỉ số vàng:

**Menu:** `Tools > One Hit Down > Audit Level 03 Parity`

Tool audit do Agent 3 cung cấp (`Assets/Editor/Level03ParityCheck.cs`), kiểm tra:

- Tổng số GameObject, số Prefab instance.
- Số lượng từng loại Prefab (FireTrap, IceRoad, Kiwi, Strawberry, MovingPlatform, SpikedBall).
- Không còn object Mock.
- Phân bố tag (`Player`, `Hazard`) và danh sách root.

**Kết quả bắt buộc: PASS toàn bộ.** Nếu có mục FAIL, **không** sửa tay Scene — chạy lại builder hoặc báo người phụ trách builder để sửa logic dựng.

---

## 8. Kiến trúc hạ tầng

### 8.1. CoreRig

`Assets/Prefabs/CoreRig.prefab` là Prefab hạ tầng dùng chung (GameManager + HUD + Audio + Transition), được đặt **ở root của Scene** (không nằm trong nhóm `01`–`07`).

Các override khi đặt vào Scene Màn 3:

| Trường | Giá trị |
|---|---|
| `levelIndexOverride` | `3` |
| `startPoint` | Tham chiếu `LevelStart` của Màn 3 |
| `exitGate` | Tham chiếu `ExitGate` của Màn 3 |
| `mainCamera` | Tham chiếu Camera 2D của Màn 3 |

> `GameManager` là ngoại lệ duy nhất được giữ tham chiếu Scene (theo Mục 7 của `CLAUDE.md`). Các Prefab bẫy/vật phẩm vẫn phải self-contained.

### 8.2. KillZone

`KillZone` là **root GameObject riêng biệt** trong Scene (không nằm trong nhóm `01`–`07`), đặt dưới đáy màn để tiêu diệt nhân vật rơi ra ngoài.

| Thuộc tính | Giá trị |
|---|---|
| Kích thước collider | `250 x 4` |
| Vai trò | Vùng chết dưới đáy màn |

`KillZone` là root thứ 7 trong danh sách root kỳ vọng ở mục 6.

---

## 9. Tóm tắt nhanh (TL;DR)

```text
Tools > One Hit Down > Build Level 03 Scene (Catacombs of Ice & Ember)
  → tự backup vào Assets/Scenes/_Backup/
  → hỏi xác nhận
  → chặn nếu thiếu prefab
  → dựng lại Level_03.unity

Sau đó:
Tools > One Hit Down > Audit Level 03 Parity
  → phải PASS 100%
```

**Luôn commit trước khi build.** `_Backup/` hiện chưa nằm trong `.gitignore` — cẩn thận khi `git add`.
