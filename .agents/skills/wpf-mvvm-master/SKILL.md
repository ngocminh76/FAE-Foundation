---
name: wpf-mvvm-master
description: Hướng dẫn phong cách code WPF chuẩn MVVM "Master Level" - Tuyệt đối không dùng Code-Behind cho logic, sử dụng ObservableObject, ICommand và DataBinding.
---

# WPF MVVM Master Style Guide

Skill này định nghĩa phong cách viết code chuẩn mực cho WPF theo mô hình MVVM (Model-View-ViewModel) cấp độ "Master". Bất cứ khi nào tạo View hoặc thêm logic mới, bạn phải tuân thủ nghiêm ngặt các quy tắc sau:

## 1. Nguyên tắc "Zero Code-Behind" (Không viết logic trong View)
- File `*.xaml.cs` (Code-Behind) **CHỈ ĐƯỢC PHÉP** chứa hàm khởi tạo `InitializeComponent()`.
- TUYỆT ĐỐI KHÔNG viết các Event Handler (như `Button_Click`, `SelectionChanged`, `Loaded`) trong file `.xaml.cs`.
- Mọi tương tác của người dùng phải được điều hướng sang ViewModel thông qua **Data Binding** và **ICommand**.

## 2. Xây dựng ViewModel chuẩn mực
- Mọi ViewModel phải kế thừa từ một lớp cơ sở hỗ trợ `INotifyPropertyChanged` (ví dụ: `ObservableObject`).
- **Properties:** Tất cả các property có binding với UI phải sử dụng cơ chế `SetProperty` (hoặc gọi `OnPropertyChanged`) để thông báo thay đổi cho UI.
- **Collections:** Luôn sử dụng `ObservableCollection<T>` cho các danh sách động (như lưới dữ liệu `DataGrid`, danh sách thả xuống `ComboBox`) để UI tự động cập nhật khi thêm/sửa/xóa phần tử.

## 3. Quản lý Hành động bằng ICommand (RelayCommand)
- Thay vì xử lý `Click` event, hãy tạo các Property kiểu `ICommand` trong ViewModel.
- Khởi tạo command bằng `RelayCommand(ExecuteMethod, CanExecuteMethod)`.
- Ví dụ: Nút "Nạp dữ liệu" trên UI sẽ bind `Command="{Binding RunCommand}"`. Hàm `CanExecuteRun` quyết định khi nào nút được sáng lên (enable), hàm `ExecuteRun` chứa logic thực thi.

## 4. Xử lý File Dialogs và IO Operations
- Trong mô hình WPF thực chiến, để tránh code-behind, các thao tác mở hộp thoại như `OpenFileDialog` hoặc `SaveFileDialog` có thể được gọi trực tiếp bên trong `ExecuteMethod` của `ICommand` tại ViewModel (hoặc thông qua DialogService nếu dự án có sẵn).
- Khi đọc dữ liệu từ bên ngoài (ví dụ đọc Excel bằng `ExcelDataReader`), toàn bộ logic phân tích cú pháp (parsing) và bóc tách dữ liệu phải nằm ở ViewModel (hoặc lớp Service), không được dính líu đến View.

## 5. UI Binding cơ bản
- **DataContext:** Luôn gán DataContext cho View bằng XAML (`<UserControl.DataContext><vm:MyViewModel /></UserControl.DataContext>`) hoặc tiêm từ cha.
- **TwoWay Binding:** Với các control đầu vào (`TextBox`, `ComboBox`), luôn sử dụng `Mode=TwoWay` và (nếu cần thiết) `UpdateSourceTrigger=PropertyChanged` để dữ liệu đồng bộ tức thời với ViewModel.
- **DataGrid:** Đặt `AutoGenerateColumns="True"` (hoặc định nghĩa cụ thể) và bind `ItemsSource` trực tiếp vào `ObservableCollection`. Tránh thao tác trực tiếp với dữ liệu lưới thông qua tên control (`x:Name`).

## Ví dụ Workflow Thực tế: "Load Excel Data"
1. **View (XAML):** Có TextBox (chứa FilePath), Button "Browse" (gắn BrowseCommand), ComboBox chọn Sheet (gắn Sheets list, SelectedSheet), Button "Run" (gắn RunCommand), DataGrid (gắn Data list).
2. **ViewModel (C#):** 
   - Hàm `BrowseCommand` mở `OpenFileDialog`, đọc luồng Excel, lưu các sheet vào `ObservableCollection<string> Sheets`.
   - Hàm `CanExecuteRun` kiểm tra xem File và Sheet đã chọn chưa.
   - Hàm `ExecuteRun` lấy data từ `DataTable` tương ứng với `SelectedSheet`, xử lý mapping sang Object, và đổ vào `ObservableCollection<T>` mà `DataGrid` đang bind tới.

> **Tâm thế Master:** Code phải rời rạc (Decoupled), View chỉ là bộ mặt phản chiếu dữ liệu (Dumb View), toàn bộ linh hồn và luồng suy nghĩ của ứng dụng nằm ở ViewModel.
