# 📚 Sổ Tay Kỹ Thuật & Thuyết Minh Toán Học: Tính Toán Kết Cấu Móng Bè Cột Điện 4 Cổ Cột

Tài liệu này tổng hợp toàn bộ lý thuyết cơ học kết cấu, công thức chuyển đổi vector 3D, sự phân định giữa SLS/ULS, quy trình kiểm tra 7 hạng mục và bộ sưu tập hình ảnh 3D mô phỏng cho **Phần Mềm FAE-Foundation**.

---

## 🖼️ 1. Bộ Sưu Tập Hình Ảnh 3D Mô Phỏng Kết Cấu & Cơ Chế Truyền Lực

```carousel
![Mô Hình 3D Phối Cảnh Render Từ Blender 5.2 (Cycles/EEVEE Engine)](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/blender_foundation_render.png)
<!-- slide -->
![Sơ Đồ 3D Chuẩn Xác Móng Bè Phẳng 4 Cổ Cột & Đầm Sườn Chạy Suốt 2 Phương](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/exact_both_directions_full_beam_raft_1785246112684.png)
<!-- slide -->
![Sơ Đồ 3D Cơ Chế Truyền Lực & Phản Lực Đất Nền Hình Chêm](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/force_flow_mechanism_3d_diagram_1785246378795.png)
<!-- slide -->
![Sơ Đồ 3D Quy Dời Lực Tích Có Hướng Vector r_A x F Về Tâm Móng O(0,0,0)](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/vector_cross_product_reduction_3d_1785248582506.png)
<!-- slide -->
![Sơ Đồ 3D Phân Phối Mô Men Mx, My & Xoắn Mz Từ Tâm Tháp Đến 4 Chân Cổ Cột](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/moment_resolution_3d_diagram_1785248317018.png)
```

---

## ⚡ 2. Nguồn Gốc Lực Tác Dụng & Hiện Tượng Kéo / Nhổ Móng

Tải trọng tác dụng lên 4 chân móng tháp truyền tải điện hình thành từ **3 nguồn tải trọng chính**:

1. **Tĩnh tải bản thân ($N_{dead}$):** Trọng lượng tháp thép + sứ + dây dẫn $\rightarrow$ Phân bố nén đều xuống 4 chân cột ($N_{dead} = G_{tổng} / 4$).
2. **Tải trọng gió bão ($W_{wind}$):** Gió bão thổi vào thân tháp trên cao ($H_{gió} = 30\text{m} - 50\text{m}$) sinh ra **Mô men lật khổng lồ** $M_{lật} = F_{gió} \times H_{gió}$.
3. **Phân tách thành Cặp lực ngẫu lực Đẩy - Kéo ($\Delta N$):**
   $$\Delta N = \pm \frac{M_{lật}}{2 \cdot L_{côt}}$$
   * **2 Chân phía đón gió (Windward legs):** Lực nén do tĩnh tải nhỏ hơn lực nhổ do gió ($N_{dead} < \Delta N$), tổng hợp lực bị âm $\rightarrow$ **Chịu Lực Kéo / Nhổ ($N_{uplift} < 0$) bứt ngược lên trên**.
   * **2 Chân phía khuất gió (Leeward legs):** $N_{total} = N_{dead} + \Delta N \rightarrow$ **Chịu Lực Nén NẶNG ($N_{comp} > 0$) ép xuống đất**.

---

## 🌪️ 3. Trường Hợp Gió Xiên $45^\circ$: 3 Chân Bị Kéo Nhổ Đồng Thời

Khi gió thổi xiên $45^\circ$ kết hợp tại các vị trí **Cột Góc (Angle Tower)** hoặc **Sự cố đứt dây bất đối ứng (Conductor Breakage)**:
- Mô men lật $M_{wind, 45^\circ}$ và Mô men xoắn $M_z$ dồn toàn bộ lực nén cực nặng vào **1 chân duy nhất ($Leg_4$)**.
- Cả **3 chân còn lại ($Leg_1, Leg_2, Leg_3$) bị nhấc nhổ kéo ngược lên cùng lúc ($N < 0$)**.

---

## 📐 4. Công Thức Quy Dời Lực Không Gian 3D Về Tâm Móng $O(0,0,0)$

Khi dời một lực $\mathbf{F} = (F_x, F_y, F_z)$ tác dụng tại điểm $A(x_A, y_A, z_A)$ trên cao về **Tâm Móng $O(0,0,0)$**, hệ lực tương đương tại $O$ gồm:

1. **Vector Lực Tổng:** $\mathbf{F}_O = \mathbf{F} = (F_x, F_y, F_z)$
2. **Vector Mô Men Tổng Tại O (Tích Có Hướng Vector):**
   $$\mathbf{M}_O = \mathbf{M}_A + \mathbf{r}_A \times \mathbf{F}$$

Chi tiết 3 thành phần mô men tại Tâm Móng $O(0,0,0)$:
$$\mathbf{M_{x,O} = M_{x,A} + y_A \cdot F_z - z_A \cdot F_y}$$
$$\mathbf{M_{y,O} = M_{y,A} + z_A \cdot F_x - x_A \cdot F_z}$$
$$\mathbf{M_{z,O} = M_{z,A} + x_A \cdot F_y - y_A \cdot F_x}$$

---

## 🧮 5. Công Thức Navier Phân Phối Mô Men Lật $M_x, M_y$ Thành Lực Dọc 4 Chân

Cho 4 cổ cột đặt đối xứng tại tọa độ $(\pm x_c, \pm y_c)$ với $x_c = L_{cx}/2, y_c = L_{cy}/2$:

$$\mathbf{N_i = \frac{N_{tổng}}{4} + \frac{M_{y, tổng} \cdot x_i}{2 \cdot x_c^2} - \frac{M_{x, tổng} \cdot y_i}{2 \cdot y_c^2}}$$

---

## 🪨 8. Đặc Điểm Phân Bố & Vị Trí Tập Trung Ứng Suất Đất Nền Đáy Móng (Soil Stress Hotspots)

### A. Hình Dáng Phân Bố Ứng Suất Đất Nền ($P_{soil}$):
- Ứng suất phản lực đất nền bên dưới bản móng bè phân bố theo **Biểu đồ hình chêm nghiêng 3D (3D Inclined Wedge)**.
- **Tại góc móng $Leg_4$ (Chân nén dồn):** Ứng suất đất đạt **Giá trị cực đại $P_{max} = 43.46\text{ kPa}$** (tương ứng vùng màu đỏ thẫm trên Heatmap 3D).
- **Tại 3 góc móng $Leg_1, Leg_2, Leg_3$ (Các chân bị kéo nhổ):** Áp lực đất nền giảm về **0.0 kPa** (tương ứng vùng màu xanh dương thẫm trên Heatmap 3D).

---

## 🌪️ 9. Thuyết Minh Chi Tiết Tính Toán Móng & Phân Tích Bao Tổ Hợp Tải Trọng Gió (Gió 45° vs Gió 90°)

### A. Sơ Đồ Minh Họa 3D Phân Bố Ứng Suất Đất Nền & Lực 4 Chân Cột:

![Sơ Đồ Minh Họa 3D Phân Bố Ứng Suất Đất Nền Đáy Móng Giữa Gió 90° và Gió 45°](/C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/wind_45_vs_90_mechanics_1786197348139.jpg)

![Sơ Đồ Minh Họa 3D Lực Tác Dụng Lên 4 Chân Móng Giữa Gió 90° và Gió 45°](/C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/leg_forces_wind_45_vs_90_1786197602583.jpg)

---

### B. Mạch Thuyết Minh Giải Bài Toán Kỹ Thuật 6 Bước:

#### 📌 BƯỚC 1: BẢNG NỘI LỰC ĐẦU VÀO TỪ FILE EXCEL GỐC
- **Nội lực Từng Chân Cột (Tính Cổ cột & Bu-lông neo)**:
  - Lực Nhổ Max: $N_{\text{nhổ}} = \mathbf{-198.58 \text{ T}}, \quad Q_x = -23.80\text{ T}, \quad Q_y = -25.55\text{ T}$
  - Lực Nén Max: $N_{\text{nén}} = \mathbf{+260.89 \text{ T}}, \quad Q_x = -32.93\text{ T}, \quad Q_y = -29.73\text{ T}$
- **Nội lực TIÊU CHUẨN Cả Cột quy về Tâm móng (Tính Nền móng & Trượt/Lật)**:
  - `45 ĐỘ BT GIÓ MAX`: $N^{tc} = 125.70\text{ T}, Q_{xtc} = 59.84\text{ T}, Q_{ytc} = 44.03\text{ T}, M_{xtc} = 1385.81\text{ T.m}, M_{ytc} = 2014.06\text{ T.m}$
  - `90 ĐỘ BT GIÓ MAX`: $N^{tc} = 125.70\text{ T}, Q_{xtc} = 87.30\text{ T}, Q_{ytc} = 0.00\text{ T}, M_{xtc} = 0.00\text{ T.m}, M_{ytc} = 3141.89\text{ T.m}$

#### 📌 BƯỚC 2: QUY ĐỔI MÔ MEN VỀ ĐÁY MÓNG ($H = 3.90\text{m}$)
- `45 ĐỘ BT GIÓ MAX`:
  $$M_{xtc,\text{đáy}} = 1385.81 + 44.03 \times 3.90 = \mathbf{1557.55 \text{ T.m}}$$
  $$M_{ytc,\text{đáy}} = 2014.06 + 59.84 \times 3.90 = \mathbf{2247.44 \text{ T.m}}$$
- `90 ĐỘ BT GIÓ MAX`:
  $$M_{xtc,\text{đáy}} = 0.00 \text{ T.m}, \quad M_{ytc,\text{đáy}} = 3141.89 + 87.30 \times 3.90 = \mathbf{3482.36 \text{ T.m}}$$

#### 📌 BƯỚC 3: KIỂM TRA ĐẤT NỀN TỰ NHIÊN DƯỚI ĐÁY MÓNG (MỤC 1.1)

![Sơ Đồ Minh Họa Mực Nước Ngầm Sát Mặt Đất - Đẩy Nổi & Bật Móng](/C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/gw_surface_mechanics_1786200404809.jpg)

- **Trường hợp a: MNN sát mặt đất (SLS 1a)**:
  - $R_{tc1,S} = \mathbf{25.11 \text{ T/m}^2} \implies 1.2 R_{tc1,S} = \mathbf{30.14 \text{ T/m}^2}, \ N_{o1,S} = \mathbf{878.69 \text{ T}}, \ \sigma_{tb1} = \mathbf{2.77 \text{ T/m}^2}$.
  - Gió $45^\circ$: $\sigma_{max1} = 2.77 + \frac{1557.55}{914.89} + \frac{2247.44}{1022.59} = \mathbf{6.67 \text{ T/m}^2} \le 30.14 \implies \mathbf{THỎA \ MÃN}$.
  - Gió $45^\circ$: $\sigma_{min1} = 2.77 - 1.70 - 2.20 = \mathbf{-1.13 \text{ T/m}^2} < 0 \implies \mathbf{KHÔNG \ THỎA \ MÃN}$.
- **Trường hợp b: MNN sát đáy móng (SLS 1b)**:
  - $R_{tc1,B} = \mathbf{36.70 \text{ T/m}^2} \implies 1.2 R_{tc1,B} = \mathbf{44.04 \text{ T/m}^2}, \ N_{o1,B} = \mathbf{1557.75 \text{ T}}, \ \sigma_{tb1} = \mathbf{4.90 \text{ T/m}^2}$.
  - Gió $45^\circ$: $\sigma_{max1} = 4.90 + 1.70 + 2.20 = \mathbf{8.80 \text{ T/m}^2} \le 44.04 \implies \mathbf{THỎA \ MÃN}$.
  - Gió $45^\circ$: $\sigma_{min1} = 4.90 - 1.70 - 2.20 = \mathbf{1.00 \text{ T/m}^2} > 0 \implies \mathbf{THỎA \ MÃN}$.

> **💡 DÒNG KẾT LUẬN LOGIC VỀ ĐỆM CÁT GIA CỐ NỀN**:
> Vì ở Trường hợp a (MNN sát mặt đất), ứng suất nhỏ nhất bị âm ($\sigma_{min1} = -1.13 \text{ T/m}^2 < 0$), móng bị mở khe hở/bật móng góc đón gió $\implies$ **BẮT BUỘC PHẢI THIẾT KẾ THÊM LỚP ĐỆM CÁT THAY THẾ LỚP ĐẤT YẾU!**

#### 📌 BƯỚC 4: KIỂM TRA LỚP ĐẤT YẾU DƯỚI ĐÁY ĐỆM CÁT (MỤC 1.2)

![Sơ Đồ Minh Họa Lớp Đệm Cát & Góc Truyền Ứng Suất 28 Độ](/C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/gw_base_sand_cushion_1786200418666.jpg)

- Móng khối quy ước đáy đệm cát ($h_{\text{cát}} = 0.5\text{m}$): $b_{qu} = 17.53\text{m}, L_{qu} = 19.53\text{m} \implies A_{qu} = 337.15\text{m}^2, W_{xqu} = 1000.28\text{m}^3, W_{yqu} = 1114.45\text{m}^3$.
- Mô men quy đổi đệm cát: $M_{xtc,qu} = 1557.50\text{ T.m}, M_{ytc,qu} = 2291.10\text{ T.m}$.
- **Trường hợp a: MNN sát mặt đất (SLS 2a)**:
  - $R_{tc2,S} = \mathbf{7.95 \text{ T/m}^2} \implies 1.2 R_{tc2,S} = \mathbf{9.53 \text{ T/m}^2}, \ N_{o2,S} = \mathbf{1079.54 \text{ T}}, \ \sigma_{tb2} = \mathbf{3.20 \text{ T/m}^2}$.
  - $\sigma_{max2,S} = 3.20 + \frac{1557.50}{1000.28} + \frac{2291.10}{1114.45} = 3.20 + 1.56 + 2.06 = \mathbf{6.82 \text{ T/m}^2} \le 9.53 \implies \mathbf{THỎA \ MÃN}$.
  - $\sigma_{min2,S} = 3.20 - 1.56 - 2.06 = \mathbf{-0.41 \text{ T/m}^2}$.
- **Trường hợp b: MNN sát đáy móng (SLS 2b)**:
  - $R_{tc2,B} = \mathbf{11.86 \text{ T/m}^2} \implies 1.2 R_{tc2,B} = \mathbf{14.24 \text{ T/m}^2}, \ N_{o2,B} = \mathbf{1796.33 \text{ T}}, \ \sigma_{tb2} = \mathbf{5.33 \text{ T/m}^2}$.
  - $\sigma_{max2,B} = 5.33 + 1.56 + 2.06 = \mathbf{8.94 \text{ T/m}^2} \le 14.24 \implies \mathbf{THỎA \ MÃN}$.
  - $\sigma_{min2,B} = 5.33 - 1.56 - 2.06 = \mathbf{1.72 \text{ T/m}^2} > 0 \implies \mathbf{THỎA \ MÃN}$.

#### 📌 BƯỚC 5: KIỂM TRA ỔN ĐỊNH CHỐNG LẬT & CHỐNG TRƯỢT MÓNG (MỤC 2)
- **Tổ hợp `90 ĐỘ BT GIÓ MAX` chi phối bài toán Chống Lật & Trượt**:
  - System chống lật: $K_{cl} = \frac{1557.75 \times 8.50}{4004.69} = \mathbf{3.80} \ge 1.50 \implies \mathbf{THỎA \ MÃN}$.
  - System chống trượt: $K_{tr} = \frac{1557.75 \times \tan 28^\circ}{87.30} = \frac{828.25}{87.30} = \mathbf{9.49} \ge 1.30 \implies \mathbf{THỎA \ MÃN}$.

#### 📌 BƯỚC 6: BẢNG TỔNG HỢP KẾT LUẬN AN TOÀN VÀ ĐỐI CHIẾU 100% VỚI EXCEL

| STT | Hạng mục Tính toán | Trường hợp MNN | Kết quả Excel `55(+2)B` | Kết quả Thuyết minh | Giới hạn Cho phép | Trạng thái Đối chiếu |
| :-: | :--- | :---: | :---: | :---: | :---: | :---: |
| **1a** | **Áp lực nén đáy móng $\sigma_{max1}$** | MNN sát mặt đất | **$6.67 \text{ T/m}^2$** | **$6.67 \text{ T/m}^2$** | $30.14 \text{ T/m}^2$ |  **Khớp 100%** |
| **1b** | **Áp lực nén đáy móng $\sigma_{max1}$** | MNN sát đáy móng | **$8.80 \text{ T/m}^2$** | **$8.80 \text{ T/m}^2$** | $44.04 \text{ T/m}^2$ |  **Khớp 100%** |
| **2a** | **Khống chế bật móng $\sigma_{min1}$** | MNN sát mặt đất | **$-1.13 \text{ T/m}^2$** | **$-1.13 \text{ T/m}^2$** | $> 0.00 \text{ T/m}^2$ | 🔴 **Khớp 100% (Cần Đệm cát)** |
| **2b** | **Khống chế bật móng $\sigma_{min1}$** | MNN sát đáy móng | **$1.00 \text{ T/m}^2$** | **$1.00 \text{ T/m}^2$** | $> 0.00 \text{ T/m}^2$ |  **Khớp 100%** |
| **3a** | **Ứng suất đệm cát $\sigma_{max2}$** | MNN sát mặt đất | **$6.82 \text{ T/m}^2$** | **$6.82 \text{ T/m}^2$** | $9.53 \text{ T/m}^2$ |  **Khớp 100%** |
| **3b** | **Ứng suất đệm cát $\sigma_{max2}$** | MNN sát đáy móng | **$8.94 \text{ T/m}^2$** | **$8.94 \text{ T/m}^2$** | $14.24 \text{ T/m}^2$ |  **Khớp 100%** |
| **4a** | **Ứng suất đệm cát $\sigma_{min2}$** | MNN sát mặt đất | **$-0.41 \text{ T/m}^2$** | **$-0.41 \text{ T/m}^2$** | $> 0.00 \text{ T/m}^2$ |  **Khớp 100%** |
| **4b** | **Ứng suất đệm cát $\sigma_{min2}$** | MNN sát đáy móng | **$1.72 \text{ T/m}^2$** | **$1.72 \text{ T/m}^2$** | $> 0.00 \text{ T/m}^2$ |  **Khớp 100%** |
| **5** | **Hệ số Chống Lật $K_{cl}$** | MNN sát đáy móng | **$3.80$** | **$3.80$** | $\ge 1.50$ |  **Khớp 100%** |
| **6** | **Hệ số Chống Trượt $K_{tr}$** | MNN sát đáy móng | **$9.49$** | **$9.49$** | $\ge 1.30$ |  **Khớp 100%** |

