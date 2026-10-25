# 📜 CẨM NANG THIẾT KẾ MÀN CHƠI (MAP SCHEMA)
### DÀNH CHO DESIGNER NHÓM 505 — DỰ ÁN "ONE HIT DOWN"
*(Tài liệu chuẩn hóa — Đọc và làm theo, không cần biết lập trình)*

---

## 📌 NGUYÊN TẮC CỐT LÕI (ĐỌC KỸ TRƯỚC TIÊN)
1. Bạn **không cần cài Unity**, chỉ cần mở file `.json` bằng **VS Code** (hoặc Notepad) để thiết kế.
2. Mọi tọa độ trong game đều là **SỐ NGUYÊN THEO Ô (Tile)**. Không có pixel lẻ hay số thập phân.
3. Chiều cao của mọi màn chơi **BẮT BUỘC ĐÚNG 20 DÒNG** (từ dòng `y = 0` trên đỉnh đến `y = 19` dưới đáy).
4. Mỗi màn chơi **BẮT BUỘC CÓ ĐÚNG 3 CỔ VẬT** để mở Cổng Thần ở cuối màn.
5. Mỗi người chỉ sửa đúng file màn được phân công:
   - **Uyên**: `level_01.json` và `level_02.json`
   - **Tuyên**: `level_03.json`
   - **Tú**: `level_04.json`
   - **Trung**: `level_05.json`

---

## 🗺️ PHẦN 1: HỆ TỌA ĐỘ TRONG GAME

```
        x = 0  ──►  tăng dần sang phải (chiều dài màn tùy ý)
y = 0   ┌────────────────────────────────────────────────────────┐
        │ hàng 0                                                 │
        │ hàng 1                                                 │
        │ ...                                                    │
y = 18  │ mặt đất chính                                          │
y = 19  │ đáy vực sâu                                            │
        └────────────────────────────────────────────────────────┘
```
- Góc trên cùng bên trái là ô `(x = 0, y = 0)`.
- Trục **`X`** chạy từ trái sang phải.
- Trục **`Y`** đếm từ trên xuống dưới (hàng 0 ở đỉnh màn hình, hàng 19 ở đáy màn hình).

---

## 🧱 PHẦN 2: VẼ ĐỊA HÌNH BẰNG LƯỚI KÝ TỰ (`tiles`)

Trong file JSON, khối `"tiles"` gồm **đúng 20 dòng chuỗi**. Bạn vẽ địa hình như chơi xếp chữ:

| Ký tự | Ý nghĩa | Nhân vật đi qua? | Có chết không? | Minh họa |
|:---:|---|:---:|:---:|---|
| `.` | **Không khí** | Đi xuyên | ❌ Không | Khoảng trống để nhảy |
| `#` | **Đất đá đặc** | Chặn cả 4 phía | ❌ Không | Nền đường đi, tường, trần |
| `=` | **Bệ đá 1 chiều** | Nhảy từ dưới xuyên lên được, đứng lên trên mặt được | ❌ Không | Bậc thang, bục treo |
| `^` | **Gai cắm từ sàn** | Đi qua là dính | ☠️ **CHẾT NGAY** | Cắm trên mặt sàn đá |
| `v` | **Gai treo từ trần** | Đi qua là dính | ☠️ **CHẾT NGAY** | Rủ từ trần hang |
| `<` | **Gai nhọn chĩa trái** | Đi qua là dính | ☠️ **CHẾT NGAY** | Gắn ở vách tường phải |
| `>` | **Gai nhọn chĩa phải** | Đi qua là dính | ☠️ **CHẾT NGAY** | Gắn ở vách tường trái |
| `~` | **Sàn băng** | Đi được, nhưng **ma sát = 0** | ❌ Không | Trượt vèo vèo, không phanh được |
| `%` | **Sàn bùn** | Đi được, ma sát cao (chạy chậm) | ❌ Không | Đầm lầy cản bước |
| `:` | **Sàn cát** | Đi được, ma sát trung bình | ❌ Không | Sa mạc sa thạch |

### 💡 Ví dụ một đoạn địa hình thực tế (Độ cao đúng 20 ô):
```json
"tiles": [
  "........................................", 
  "........................................", 
  "........................................", 
  "................vv......................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "........................................", 
  "..................===...................", 
  "........................................", 
  "........................................", 
  "..........^^............................", 
  "############...#########################", 
  "############...#########################"  
]
```

> **LƯU Ý:**
> - Mọi dòng trong `tiles` **PHẢI DÀI BẰNG NHAU CHẶN CHẶT**.
> - Không dùng phím **Tab** hoặc để khoảng trắng thừa ở cuối dòng.
> - Hố tử thần hay vực sâu giữa 2 mỏm đá **tối đa rộng 3 ô** (để khi quay đầu backtrack nhân vật vẫn nhảy đôi qua được).

---

## 💎 PHẦN 3: ĐẶT 3 CỔ VẬT (`relics`), CỔNG (`gate`) VÀ XUẤT PHÁT (`spawn`)

Mỗi màn bắt buộc có đúng 3 quả Cổ Vật. Cổng chỉ mở khi người chơi nhặt đủ cả 3:

```json
  "relics": [
    { "x": 25, "y": 14 },  
    { "x": 60, "y": 10 },  
    { "x": 110, "y": 13 }  
  ],

  "gate": { "x": 130, "y": 16 }, 

  "spawn": {
    "p1": { "x": 2, "y": 16 },   
    "p2": { "x": 4, "y": 16 }    
  }
```

---

## ⚙️ PHẦN 4: DANH SÁCH BẪY CƠ HỌC VÀ CÁCH COPY-PASTE (`entities`)

### 1. Cưa máy trượt ngang qua lại (`SAW`)
```json
{
  "id": "m1_saw_01",
  "type": "SAW",
  "x": 30, "y": 16, "w": 2, "h": 2,
  "patrol": { "mode": "LINEAR", "axis": "X", "range": 6, "speed": 4, "phase": 0 },
  "trigger": { "mode": "ALWAYS" },
  "damage": 1
}
```

### 2. Bẫy Hố Tử Thần Vĩnh Viễn (`DEATH_PIT`)
```json
{
  "id": "m1_deathpit_01",
  "type": "DEATH_PIT",
  "x": 45, "y": 18, "w": 3, "h": 1,
  "trigger": { "mode": "ON_TOUCH", "delay": 0.2 },
  "params": { "permanent": true },
  "damage": 1
}
```

### 3. Đệm nhún lò xo bật nảy cao (`TRAMPOLINE`)
```json
{
  "id": "m2_trampoline_01",
  "type": "TRAMPOLINE",
  "x": 20, "y": 17, "w": 2, "h": 1,
  "trigger": { "mode": "ON_TOUCH" },
  "params": { "force": 22 },
  "damage": 0
}
```

### 4. Vạc dầu phun lửa ngầm (`FIRE`)
```json
{
  "id": "m3_fire_01",
  "type": "FIRE",
  "x": 50, "y": 16, "w": 1, "h": 2,
  "trigger": { "mode": "ON_TIMER", "onTime": 1.0, "offTime": 2.0 },
  "damage": 1
}
```

### 5. Tượng đá đập từ trần xuống (`ROCK_HEAD`)
```json
{
  "id": "m4_rockhead_01",
  "type": "ROCK_HEAD",
  "x": 70, "y": 6, "w": 3, "h": 3,
  "patrol": { "mode": "LINEAR", "axis": "Y", "range": 9, "speed": 18, "phase": 0 },
  "trigger": { "mode": "ON_PROXIMITY", "radius": 6 },
  "params": { "holdTime": 0.25 },
  "damage": 1
}
```

---

## 🚀 PHẦN 5: QUY TRÌNH 5 BƯỚC ĐỂ THIẾT KẾ 1 MÀN

1. **Bước 1:** Vẽ phác thảo sơ bộ ra giấy kẻ ô li (chia làm 4 đoạn A/B/C/D, đánh dấu chỗ đặt 3 Cổ Vật).
2. **Bước 2:** Copy file `_TEMPLATE.json` và đổi tên thành file của bạn (ví dụ `level_01.json`).
3. **Bước 3:** Mở file bằng VS Code, vẽ đất đá, chông gai vào `tiles`, copy bẫy vào `entities`.
4. **Bước 4:** Mở terminal chạy lệnh kiểm tra lỗi:
   ```bash
   python D:\game\THIET_KE\tools\validate.py levels/level_01.json
   ```
   - Thấy hiện chữ xanh `[PASS]` nghĩa là màn hoàn toàn hợp lệ!
   - Thấy báo `[FAIL]` dòng nào thì mở ra sửa lại dòng đó.
5. **Bước 5:** Báo Tuấn mở game, bấm phím **`R`** trên bàn phím là màn chơi của bạn hiện lên ngay lập tức!
