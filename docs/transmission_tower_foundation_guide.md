# 📚 Sổ Tay Kỹ Thuật & Thuyết Minh Toán Học: Tính Toán Kết Cấu Móng Bè Cột Điện 4 Cổ Cột

Tài liệu này tổng hợp toàn bộ lý thuyết cơ học kết cấu, công thức chuyển đổi vector 3D, sự phân định giữa SLS/ULS, quy trình kiểm tra 7 hạng mục và bộ sưu tập hình ảnh 3D mô phỏng cho **Phần Mềm FAE-Foundation**.

---

## 🖼️ 1. Bộ Sưu Tập Hình Ảnh 3D Mô Phỏng Kết Cấu & Cơ Chế Truyền Lực

```carousel
![Sơ Đồ 3D Chuẩn Xác Móng Bè Phẳng 4 Cổ Cột & Đầm Sườn Chạy Suốt 2 Phương](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/exact_both_directions_full_beam_raft_1785246112684.png)
<!-- slide -->
![Sơ Đồ 3D Cơ Chế Truyền Lực & Phản Lực Đất Nền Hình Chêm](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/force_flow_mechanism_3d_diagram_1785246378795.png)
<!-- slide -->
![Sơ Đồ 3D Quy Dời Lực Tích Có Hướng Vector r_A x F Về Tâm Móng O(0,0,0)](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/vector_cross_product_reduction_3d_1785248582506.png)
<!-- slide -->
![Sơ Đồ 3D Phân Phối Mô Men Mx, My & Xoắn Mz Từ Tâm Tháp Đến 4 Chân Cổ Cột](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/moment_resolution_3d_diagram_1785248317018.png)
<!-- slide -->
![Trường Hợp Đặc Biệt: Gió Xiên 45° - 3 Chân Bị Kéo Nhổ & 1 Chân Nén Dồn](file:///C:/Users/qnbk1/.gemini/antigravity/brain/532fcab8-feeb-4929-b8e3-e0fffa788c40/three_legs_uplift_diagonal_wind_1785246597295.png)
```

---

## ⚡ 2. Nguồn Gốc Lực Tác Dụng & Hiện Tượng Kéo / Nhổ Móng

Tải trọng tác dụng lên 4 chân móng tháp truyền tải điện hình thành từ **3 nguồn tải trọng chính**:

1. **Tĩnh tải bản thân ($N_{dead}$):** Trọng lượng tháp thép + sứ + dây dẫn $\rightarrow$ Phân bố nén đều xuống 4 chân cột ($N_{dead} = G_{tổng} / 4$).
2. **Tải trọng gió bão ($W_{wind}$):** Gió bão thổi vào thân tháp trên cao ($H_{gió} = 30\text{m} - 50\text{m}$) sinh ra **Mô men lật khổng lồ** $M_{lật} = F_{gió} \times H_{gió}$.
3. **Phân tách thành Cặp lực ngẫu lực Đẩy - Kéo ($\Delta N$):**
   $$\Delta N = \pm \frac{M_{lật}}{2 \cdot L_{côt}}$$
   * **2 Chân phía đón gió (Windward legs):** Lực nén do tĩnh tải nhỏ hơn lực nhổ do gió ($N_{dead} < \Delta N$), tổng hợp lực bị âm $\rightarrow$ **Chịu Lực Kéo / Nhổ ($N_{uplift} < 0$) bứt ngược lên trên**.
   * **2 Chân phía khuất gió (Leeward legs):** $N_{total} = N_{dead} + \Delta N \rightarrow$ **Chịu Lực Nén Nặng ($N_{comp} > 0$) ép xuống đất**.

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

* **Mô men uốn cổ cột ($H_{col}$):**
  $$M_{x,i} = Q_{y,i} \times H_{col}$$
  $$M_{y,i} = Q_{x,i} \times H_{col}$$

---

## ⚖️ 6. Phân Định Giữa Tải Trọng Tiêu Chuẩn (SLS) Và Tải Trọng Tính Toán (ULS)

| Tiêu chí | 🪨 1. Tính Nền Đất & Chống Nhổ | 🧱 2. Tính Kết Cấu Bê Tông Cốt Thép |
| :--- | :--- | :--- |
| **Trạng Thái Giới Hạn** | Trạng thái Giới hạn II (Sử dụng - **SLS**) | Trạng thái Giới hạn I (Cường độ - **ULS**) |
| **Hệ Số Tải Trọng ($\gamma_f$)** | **$\gamma_f = 1.0$ (Tải Tiêu Chuẩn)** | **$\gamma_D \approx 1.15, \gamma_L \approx 1.2, \gamma_W \approx 1.2$ (Tải Tính Toán)** |
| **Mục Đích Tính Toán** | 1. Áp lực đất $P_{max} \le 1.2 R^{tc}$<br>2. Hệ số an toàn chống nhổ $K \ge 1.3$<br>3. Độ lún móng $S \le S_{cho\ phép}$ | 1. Diện tích thép $A_s$ chịu uốn dầm sườn<br>2. Diện tích thép lưới bản móng bè $A_{s,bản}$<br>3. Kiểm tra cổ cột & Bu-lông neo M36 |
| **Tiêu Chuẩn Áp Dụng** | TCVN 9362:2012 / Eurocode 7 (GEO) | TCVN 5574:2018 / ACI 318 / Eurocode 2 |

---

## 📊 7. Quy Trình Tính Toán 7 Hạng Mục Kết Cấu & Địa Kỹ Thuật

### 1. Áp lực đất nền đáy móng ($P_{max}, P_{min}$ - SLS):
$$P_{max, min} = \frac{\Sigma N_{sls}}{F_{bè}} \pm \frac{\Sigma M_{x,sls}}{W_x} \pm \frac{\Sigma M_{y,sls}}{W_y} \le 1.2 R^{tc}$$

### 2. Hệ số an toàn chống nhổ móng ($K_{nhổ}$ - SLS):
$$K_{nhổ} = \frac{G_{móng} + G_{đất\_đè}}{\Sigma |N_{nhổ,sls}|} \ge 1.3$$

### 3. Cốt thép 4 Cổ Cột ($A_{s,col}$ - ULS):
Chịu Kéo-Uốn xiên và Nén-Uốn xiên theo TCVN 5574:2018:
$$A_{s,col} = \frac{|N_{nhổ,uls}|}{R_s} + \frac{M_{chân,uls}}{R_s \cdot z}$$

### 4. Kiểm tra cụm 4 Bu-lông Neo M36 (ULS):
* Sức chịu kéo 1 bu-lông: $N_{rd1} = A_{net} \cdot f_{yb}$.
* Tổng khả năng chịu kéo cụm 4 bu-lông: $N_{rd,cụm} = 4 \cdot N_{rd1} \ge |N_{nhổ,uls}|$.
* Chiều dài neo: $L_{anchor} \ge 30 \cdot d_{bolt}$.

### 5. Kiểm tra chọc thủng bản móng bè (ULS):
$$F_{b,ult} = R_{bt} \cdot u_m \cdot h_{0,slab} \ge N_{nén,max,uls}$$

### 6. Cốt thép dọc Dầm Sườn Móng ($A_{s,beam}$ - ULS):
$$\alpha_m = \frac{M_{uls}}{R_b \cdot b \cdot h_0^2} \longrightarrow \xi = 1 - \sqrt{1 - 2\alpha_m} \longrightarrow A_{s,beam} = \frac{\xi R_b b h_0}{R_s}$$

### 7. Cốt thép lưới Bản Móng Bè ($A_{s,slab}$ - ULS):
$$M_{slab} = \frac{P_{uls} \cdot L_{ô}^2}{8} \longrightarrow A_{s,slab} = \frac{\xi R_b b_{unit} h_{0,slab}}{R_s}$$
