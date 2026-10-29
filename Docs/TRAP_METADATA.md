# Metadata cho Module 2

Commit `c4e60ba` thiếu metadata và animation phụ thuộc của 6 prefab bẫy. Bản sửa bổ sung các file đang có trong project, giữ nguyên GUID, mã nguồn và nội dung prefab.

## Liên kết script

| Prefab | Script trong `Assets/Scripts/Traps/` | GUID của script |
| --- | --- | --- |
| FireTrap | FireTrap.cs | `bc3b9b6f17379db40bf0ff2f941c2e09` |
| Spikes | TrapBase.cs | `567dc8c46612eea4297e71cb138f57c7` |
| SpikedBall | SpikedBallPendulum.cs | `ab5bbb576119821409efbc5d70459ae1` |
| RockHead | RockHeadTrap.cs | `83ffab685e23e5d4f85f6377d98c2924` |
| Saw | SawSpinner.cs | `d6dc71798ae6b824b8fa7e9018dc2baf` |
| SpikeHead | SpikeHeadTrap.cs | `6c9c080e6d868b0449aa91ed18397a9d` |

`Spikes.prefab` ghi `Assembly-CSharp::OneHitDown.Traps.TrapBase` trong `m_EditorClassIdentifier`. Gai tĩnh dùng trực tiếp lớp cơ sở theo schema; không cần tạo `Spikes.cs`.

Bản sửa gồm metadata của 6 prefab, 6 script bẫy, `IResettable.cs` và các thư mục liên quan. Hai Animator Controller RockHead/SpikeHead cùng 12 Animation Clip và metadata cũng được bổ sung vì prefab đang tham chiếu tới chúng.

## Pull trên máy đã mở Unity

1. Đóng Unity trước khi đồng bộ.
2. Nếu Git báo các file `.meta` untracked sẽ bị ghi đè, sao lưu và di chuyển đúng những file được báo ra ngoài project, rồi chạy lại `git pull origin Trung`. Git thường từ chối pull trong trường hợp này; không tự động thay thế chúng.
3. Không commit metadata có GUID mới do Unity sinh trên máy nhận. Không xóa `.meta` vừa pull về; mở lại Unity để import.
4. Kiểm tra component trên cả 6 prefab theo bảng trên, cùng Animator của RockHead/SpikeHead.

## Phụ thuộc biên dịch

`TrapBase.cs` sử dụng `OneHitDown.Player.PlayerController`, với `IsDead` và `Die()`. Nhánh Trung tại `c4e60ba` chưa chứa Module 1, nên cần tích hợp Module Player thật để biên dịch và kiểm tra trong Unity. Bản sửa metadata không thêm Player giả hoặc thay đổi cơ chế va chạm.

## Kiểm tra bản sửa

Đã kiểm tra nội dung trong Git index: 6/6 prefab trỏ đúng script và namespace; toàn bộ 80 tham chiếu GUID/fileID trong cây phụ thuộc được đối chiếu, không thiếu GUID, không trùng GUID và không sai fileID sprite. Hai Animator Controller đều có đủ Idle, Blink, Top Hit, Bottom Hit, Left Hit và Right Hit. Material Sprite-Lit-Default được cung cấp bởi package URP đã khai báo.

Kiểm tra biên dịch 7 script bằng compiler/reference assembly của Unity 6000.6.4f1 xác nhận lỗi phụ thuộc hiện có: `TrapBase.cs(2,18): CS0234`, thiếu namespace `OneHitDown.Player`. Chưa kiểm thử hiển thị hoặc runtime trong Unity sau bản sửa metadata.
