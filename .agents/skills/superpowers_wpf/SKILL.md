---
name: superpowers-wpf
description: Hướng dẫn áp dụng quy trình phát triển Superpowers (TDD, YAGNI, DRY) và thiết kế giao diện WPF tối giản.
---

# 1. Superpowers Methodology (Quy trình Phát triển)

Khi nhận được yêu cầu xây dựng tính năng hoặc ứng dụng mới, bạn **BẮT BUỘC** phải tuân thủ nghiêm ngặt quy trình sau đây:

1. **Không code vội (Understand Before Coding):** Tuyệt đối không được nhảy vào viết code ngay lập tức. Hãy lùi lại, phân tích và hỏi người dùng để làm rõ các yêu cầu (spec) của hệ thống.
2. **Chốt Spec từng phần (Digestible Specs):** Trình bày các yêu cầu kỹ thuật và thiết kế cho người dùng xem theo từng phần nhỏ, ngắn gọn, dễ đọc và dễ tiêu hóa để người dùng xác nhận.
3. **Lập Kế hoạch Triển khai (Implementation Plan):** 
   - Sau khi người dùng đồng ý (sign-off) với thiết kế, hãy lập ra một bản kế hoạch triển khai chi tiết đến mức một "junior engineer" cũng có thể làm theo.
   - Kế hoạch phải nhấn mạnh các nguyên tắc: **Red/Green TDD** (Test-Driven Development), **YAGNI** (You Aren't Gonna Need It), và **DRY** (Don't Repeat Yourself).
4. **Phát triển dựa trên Subagent (Subagent-Driven Development):** 
   - Sau khi người dùng ra lệnh "Go", hãy chia nhỏ task và (nếu cần) sử dụng subagent để thực thi từng nhiệm vụ kỹ thuật.
   - Thường xuyên kiểm tra, review lại code của subagent để đảm bảo đi đúng hướng kế hoạch đã vạch ra ban đầu.

# 2. WPF UI Design Rules (Quy tắc thiết kế giao diện)

Khi thiết kế hoặc code giao diện (đặc biệt là WPF), phải gạt bỏ mọi yêu cầu về giao diện màu mè bóng bẩy và tuân thủ các luật sau:

1. **Tối giản (Minimalism):** Giao diện phải TỐI GIẢN HẾT MỨC CÓ THỂ. Không được sử dụng quá nhiều màu sắc, không rườm rà.
2. **Tận dụng Style có sẵn (Built-in Styles):** 
   - CHỈ sử dụng các Control và Style mặc định có sẵn từ thư viện WPF chuẩn.
   - TUYỆT ĐỐI KHÔNG tự bịa ra các template phức tạp, không viết các custom style màu mè không cần thiết.
3. **Ưu tiên Công năng (Function over Form):** Tập trung hoàn toàn vào việc bố trí layout sao cho gọn gàng, hợp lý, dễ nhìn và hoạt động chính xác.

# 3. Đặc thù Ứng dụng Tính toán (Calculation App Rules)

Bởi vì đây là một ứng dụng liên quan đến tính toán, bạn phải tuyệt đối tuân thủ:
1. **Chính xác tuyệt đối:** Mọi con số, công thức đưa ra hoặc sử dụng trong code phải rõ ràng và chính xác.
2. **Không bịa dữ liệu:** Không được tự ý bịa số liệu ảo (hallucinate numbers) hoặc dùng số liệu không có căn cứ.
3. **Không tự ý phát minh:** Không tự ý sáng chế ra công thức hay tính năng mới nếu chưa được thảo luận và đồng ý từ người dùng. Mọi logic tính toán phải dựa trên yêu cầu thực tế (Spec).

> **Lưu ý:** Skill này ghi đè bất kỳ hướng dẫn mặc định nào yêu cầu phải làm giao diện bóng bẩy (Rich Aesthetics / WOW factor). Tiêu chí cao nhất ở đây là: Hệ thống cấu trúc rõ ràng, Code dễ mở rộng, Logic tính toán chuẩn xác 100%, và UI tối giản thuần WPF.
