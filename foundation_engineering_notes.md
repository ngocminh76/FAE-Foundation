# 📔 Sổ Tay Kiến Thức Kỹ Thuật: Thiết Kế Nền Móng & FEA

Tài liệu này tổng hợp lại những gì chúng ta đã thảo luận, giúp bạn dễ dàng lưu trữ và đọc lại bất cứ lúc nào.

## 1. Nền tảng Lý thuyết (FEA, SBVL & CHKC)
*   **Sức bền vật liệu (SBVL) & Cơ học kết cấu (CHKC):** Là trái tim của Phương pháp phần tử hữu hạn (FEA).
*   **FEA (Finite Element Analysis):** Máy tính chia nhỏ một kết cấu phức tạp thành hàng ngàn phần tử (Mesh), áp dụng "Phương pháp chuyển vị" của CHKC để tính ra chuyển vị tại các nút, sau đó dùng các định luật của SBVL (như định luật Hooke) để truy ngược ra ứng suất và nội lực bên trong.

## 2. Các Lực Tác Dụng Xuống Móng Thực Tế
Trong thực tế, chân cột không chỉ truyền lực đè thẳng đứng mà truyền một tổ hợp gồm 3 nội lực chính:
*   🔴 **Lực dọc ($N$):** Đè thẳng đứng. Gây ra áp lực lún phân bố đều ($P = N/F$).
*   🔵 **Lực cắt ($Q$):** Đẩy ngang do gió/động đất. Có xu hướng làm trượt móng và sinh ra mô men phụ.
*   🟢 **Mô men uốn ($M$):** Bẻ xoay móng. Làm áp lực đáy móng phân bố không đều (hình thang/tam giác), sinh ra áp lực lớn nhất ($P_{max}$) nguy cơ phá hoại đất, và áp lực nhỏ nhất ($P_{min}$) nguy cơ lật móng.
*   **Công thức kinh điển:** $P_{max, min} = \frac{\Sigma N}{F} \pm \frac{\Sigma M}{W}$

## 3. Sự Khác Biệt Giữa Các Tiêu Chuẩn Thiết Kế
*   **🇻🇳 TCVN (Việt Nam):** Dựa trên Phương pháp Trạng thái giới hạn. Đánh giá cấp độ bền bê tông (B20, B25) qua mẫu lập phương. Thiên về an toàn (bảo thủ) trong tính toán chịu cắt/chọc thủng.
*   **🇺🇸 ACI 318 (Mỹ):** Dựa trên phương pháp LRFD. Sử dụng mẫu bê tông hình trụ ($f'_c$). Mang tính thực nghiệm cao, dễ hiểu, dùng hệ số giảm cường độ $\phi$ nhân thẳng vào sức chịu tải tổng thể.
*   **🇪🇺 Eurocode (Châu Âu):** Hệ thống hóa cực kỳ chặt chẽ và phức tạp (Đặc biệt là Eurocode 7 về Địa kỹ thuật với 3 cách tiếp cận thiết kế). Dùng phương pháp Hệ số riêng phần.

## 4. Đặc Trưng Các Loại Móng
*   **Móng Đơn:** Tính toán đơn giản bằng đại số, chịu tải ít. Đáy móng ép trực tiếp lên đất.
*   **Móng Băng:** Đỡ một hàng cột/tường. Là bài toán **"Dầm trên nền đàn hồi"**. Phải giải quyết sự lún không đều dọc theo chiều dài móng.
*   **Móng Cọc:** Khác biệt hoàn toàn. Tải trọng dồn về các cọc bê tông cắm sâu xuống đất cứng. Không xét sức kháng ở mặt đáy đài móng. Phải kiểm tra đâm thủng cực kỳ phức tạp (cột đâm thủng đài, cọc đâm thủng đài).
*   **Móng Bè (Raft Foundation):** Đỡ toàn bộ tòa nhà. Là bài toán **"Bản trên nền đàn hồi"** (Phải giải bằng FEA 2D/3D).

## 5. Móng Bè Có Sườn (Ribbed Raft Foundation)
Đây là giải pháp tối ưu hóa cực kỳ thông minh:
1.  **Cấu tạo:** Cột -> Tựa lên các dầm móng (Sườn) nối với nhau thành khung lưới -> Tựa lên một tấm bản móng mỏng. Tất cả nằm úp lên đất.
2.  **Cơ chế truyền lực (Ngược với sàn nhà):** 
    *   Đất nền đẩy áp lực ngược lên trên.
    *   Tấm bản móng (mỏng) chịu áp lực này, uốn võng và truyền lực tựa vào các dầm móng.
    *   Các dầm móng (tiết diện lớn, độ cứng chống uốn cao) gánh phần lớn nội lực từ bản truyền vào, rồi mới khóa lại tại vị trí các cột.
3.  **Lợi ích:** Tránh việc phải đổ bê tông một tấm móng bè phẳng quá dày, tiết kiệm vật liệu nhưng vẫn đảm bảo độ cứng không gian khổng lồ chống lún lệch.
