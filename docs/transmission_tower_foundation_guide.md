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

## 🌪️ 9. Bản Chất Cơ Học & Phân Tích Bao Tổ Hợp Tải Trọng Gió (Gió 45° vs Gió 90°)

![Sơ Đồ Minh Họa 3D Phân Bố Ứng Suất Đất Nền Đáy Móng Giữa Gió 90° và Gió 45°](/C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/wind_45_vs_90_mechanics_1786197348139.jpg)

### A. Phân Tích Vật Lý & Sự Phân Phối Mô Men Lật:

Trong thiết kế móng tháp truyền tải điện 500kV, tải trọng gió tác dụng lên thân tháp và dây dẫn tạo ra 2 kịch bản chịu lực cơ bản:

1. **Tổ hợp GIÓ $90^\circ$ MAX (Gió Vuông Góc Tuyến Dây):**
   - Lực ngang tập trung đẩy theo trục X: $Q_x = 87.30\text{ T}, Q_y \approx 0$.
   - Mô men uốn chỉ xuất hiện ở 1 phương chính: $M_y = 3141.89\text{ T.m}, M_x = 0$.
   - **Tác động:** Móng bị **Uốn Lệch Tâm Đơn (Uniaxial Eccentricity)**. Ứng suất uốn phân bố đều theo một cạnh móng.

2. **Tổ hợp GIÓ $45^\circ$ MAX (Gió Thổi Xiên Góc $45^\circ$):**
   - Lực ngang chia đều trên cả 2 phương: $Q_x = 59.84\text{ T}, Q_y = 44.03\text{ T}$.
   - Mô men uốn xuất hiện đồng thời trên 2 phương: $M_x = 1385.81\text{ T.m}, M_y = 2014.06\text{ T.m}$.
   - **Tác động:** Móng bị **Uốn Lệch Tâm Xiên (Biaxial Eccentricity)**. Mô men $M_x$ và $M_y$ **cùng cộng hưởng nén cực đại dồn vào 1 góc móng duy nhất**.

### B. Bảng So Sánh Chỉ Tiêu Khống Chế Thiết Kế Đáy Móng:

| Chỉ tiêu kiểm tra | Tổ hợp GIÓ $45^\circ$ | Tổ hợp GIÓ $90^\circ$ | Tổ hợp Bao Nguy Hiểm Nhất (Envelope) | Tổ Hợp Chi Phối Kết Quả |
| :--- | :---: | :---: | :---: | :--- |
| **Mô men đáy móng $M_{x\text{đáy}}$** | $1557.55 \text{ T.m}$ | $0.00 \text{ T.m}$ | **$1557.55 \text{ T.m}$** | Gió $45^\circ$ |
| **Mô men đáy móng $M_{y\text{đáy}}$** | $2247.44 \text{ T.m}$ | $3141.89 \text{ T.m}$ | **$3141.89 \text{ T.m}$** | Gió $90^\circ$ |
| **Ứng suất nén max ($\sigma_{max1}$)** | **$6.67 \text{ T/m}^2$** | **$5.83 \text{ T/m}^2$** | **$\mathbf{6.67 \text{ T/m}^2}$** |  **GIÓ $45^\circ$ quyết định** |
| **Ứng suất kéo min ($\sigma_{min1}$)** | **$-1.13 \text{ T/m}^2$** | **$-0.30 \text{ T/m}^2$** | **$\mathbf{-1.13 \text{ T/m}^2}$** |  **GIÓ $45^\circ$ quyết định (Bật móng)** |
| **Ứng suất đệm cát $\sigma_{max2}$** | **$6.82 \text{ T/m}^2$** | **$6.01 \text{ T/m}^2$** | **$\mathbf{6.82 \text{ T/m}^2}$** |  **GIÓ $45^\circ$ quyết định** |
| **Hệ số chống lật móng $K_{cl}$** | $2.35$ | **$1.80$** | **$\mathbf{1.80}$** |  **GIÓ $90^\circ$ quyết định** |

### C. Nguyên Lý Đường Bao Tải Trọng (Load Envelope Principle):

- **Kiểm tra Ứng suất Đáy Móng ($\sigma_{max}, \sigma_{min}, R_{tc}$):** Tổ hợp **GIÓ $45^\circ$** cho $\sigma_{max}$ lớn nhất ($6.67\text{ T/m}^2 > 5.83\text{ T/m}^2$) và $\sigma_{min}$ nhỏ nhất (bị âm nhiều nhất $-1.13\text{ T/m}^2$). Do đó, **Gió $45^\circ$ chính là Envelope quyết định kiểm tra nền đất**.
- **Kiểm tra Chống Lật Móng ($K_{cl}$):** Tổ hợp **GIÓ $90^\circ$** cho mô men lật $M_y$ lớn nhất làm cho $K_{cl}$ giảm xuống nhỏ nhất ($1.80 < 2.35$). Do đó, **Gió $90^\circ$ chính là Envelope quyết định bài toán chống lật**.
