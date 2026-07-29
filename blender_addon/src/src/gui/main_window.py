"""
PySide6 Main Desktop Application Window for FAE-Foundation
"""

from PySide6.QtWidgets import (
    QMainWindow, QWidget, QHBoxLayout, QVBoxLayout, QSplitter,
    QTextEdit, QLabel, QFrame, QScrollArea, QTableWidget, QTableWidgetItem, QHeaderView
)
from PySide6.QtCore import Qt
from src.core.models import TowerFoundationProject
from src.fea.opensees_tower import TowerFoundationFEASolver
from src.design_codes.manager import CodeCheckerManager
from src.gui.input_panels import InputPanelsWidget
from src.gui.viewport_3d import Viewport3DWidget

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("FAE-Foundation | Phần Mềm Tính Toán Kết Cấu Móng Bè Cột Điện (Multi-Standard FEA)")
        self.resize(1350, 850)
        
        self.init_dark_theme()
        self.init_ui()

    def init_dark_theme(self):
        """Thiết lập giao diện Dark Mode Pro CAD sang trọng"""
        self.setStyleSheet("""
            QMainWindow, QWidget {
                background-color: #1e1e1e;
                color: #d4d4d4;
                font-family: 'Segoe UI', Arial, sans-serif;
            }
            QGroupBox {
                border: 1px solid #3c3c3c;
                border-radius: 6px;
                margin-top: 10px;
                font-weight: bold;
                color: #569cd6;
            }
            QGroupBox::title {
                subcontrol-origin: margin;
                left: 10px;
                padding: 0 5px;
            }
            QTabWidget::pane {
                border: 1px solid #3c3c3c;
            }
            QTabBar::tab {
                background-color: #252526;
                color: #cccccc;
                padding: 8px 12px;
                border: 1px solid #3c3c3c;
            }
            QTabBar::tab:selected {
                background-color: #007acc;
                color: white;
                font-weight: bold;
            }
            QLineEdit, QDoubleSpinBox, QComboBox, QTableWidget {
                background-color: #2d2d2d;
                color: #9cdcfe;
                border: 1px solid #3c3c3c;
                padding: 4px;
                border-radius: 3px;
            }
            QTextEdit {
                background-color: #141414;
                color: #dcdcaa;
                font-family: 'Consolas', 'Courier New', monospace;
                font-size: 13px;
                border: 1px solid #3c3c3c;
            }
        """)

    def init_ui(self):
        main_widget = QWidget()
        self.setCentralWidget(main_widget)
        layout = QHBoxLayout(main_widget)

        splitter = QSplitter(Qt.Horizontal)

        # Khung Nhập Liệu Trái
        self.input_widget = InputPanelsWidget()
        self.input_widget.calculate_requested.connect(self.run_calculation)
        splitter.addWidget(self.input_widget)

        # Khung Kết Quả Phải
        right_container = QWidget()
        right_layout = QVBoxLayout(right_container)

        # 3D Viewport
        self.viewport_3d = Viewport3DWidget()
        right_layout.addWidget(self.viewport_3d, stretch=1)

        # Báo cáo Text Report
        title_results = QLabel("📊 BÁO CÁO KẾT QUẢ SO SÁNH ĐA TIÊU CHUẨN")
        title_results.setStyleSheet("font-size: 14px; font-weight: bold; color: #4ec9b0; margin-top: 5px;")
        right_layout.addWidget(title_results)

        self.text_report = QTextEdit()
        self.text_report.setReadOnly(True)
        self.text_report.setPlaceholderText("Bấm nút 'THỰC THI TÍNH TOÁN & PHÂN TÍCH FEA' để xem báo cáo kết quả chi tiết...")
        right_layout.addWidget(self.text_report, stretch=1)

        splitter.addWidget(right_container)
        splitter.setSizes([450, 900])

        layout.addWidget(splitter)
        
        # Cập nhật hiển thị 3D ban đầu
        self.viewport_3d.update_scene(self.input_widget.get_current_project())

    def run_calculation(self, project: TowerFoundationProject, code_key: str):
        """Thực thi giải số FEA và chạy kiểm tra Tiêu chuẩn"""
        self.text_report.clear()
        self.text_report.append("=" * 70)
        self.text_report.append("⚡ BẮT ĐẦU QUY TRÌNH PHÂN TÍCH PHẦN TỬ HỮU HẠN & KIỂM TRA ĐA TIÊU CHUẨN ⚡")
        self.text_report.append("=" * 70)
        self.text_report.append(f"📌 Dự án: {project.name}")
        self.text_report.append(f"📐 Kích thước móng bè: {project.slab.L_x}m x {project.slab.L_y}m x {project.slab.h_slab}m")
        self.text_report.append(f"🧱 Dầm sườn nổi: {project.beam.b_beam}m x {project.beam.h_beam}m (Chạy suốt 2 phương)")
        self.text_report.append(f"🏛️ 4 Cổ cột điện: {project.column.spacing_x}m x {project.column.spacing_y}m (H_col = {project.column.H_col}m)")
        self.text_report.append(f"🪨 Hệ số nền Winkler Kz: {project.soil.K_z} kN/m³")
        self.text_report.append("-" * 70)

        # 1. Chạy FEA Solver
        solver = TowerFoundationFEASolver(project=project, mesh_size=0.5)
        fea_results = solver.run_analysis()

        # Cập nhật khung hiển thị 3D với lực tác dụng thực tế
        self.viewport_3d.update_scene(project, fea_results)

        self.text_report.append("✅ KẾT QUẢ GIẢI HỆ PHƯƠNG TRÌNH FEA:")
        self.text_report.append(f"   • Độ lún lớn nhất (Max Settlement): {fea_results.get('max_settlement_mm'):.2f} mm")
        self.text_report.append(f"   • Áp lực đất lớn nhất Pmax:            {fea_results.get('max_soil_pressure_kpa'):.2f} kPa")
        self.text_report.append("-" * 70)

        # 2. Chạy Engine Kiểm Tra Tiêu Chuẩn
        manager = CodeCheckerManager(project, fea_results)
        
        if code_key == "ALL":
            multi_res = manager.compare_all_standards()
            self.text_report.append("\n📋 SO SÁNH KẾT QUẢ TRÊN CẢ 3 TIÊU CHUẨN (TCVN / ACI 318 / EUROCODE):\n")
            for key, res in multi_res.items():
                self._format_code_result(res)
        else:
            res = manager.check_standard(code_key)
            self._format_code_result(res)

        self.text_report.append("=" * 70)
        self.text_report.append("🎉 HOÀN THÀNH TÍNH TOÁN THÀNH CÔNG!")

    def _format_code_result(self, res: dict):
        self.text_report.append(f"🏛️ TIÊU CHUẨN: {res['code_name']}")
        sb = res.get('soil_bearing', {})
        up = res.get('uplift_stability', {})
        col = res.get('stub_columns', {})
        bolt = res.get('anchor_bolts', {})
        punch = res.get('punching_shear', {})
        bm = res.get('beam_design', {})
        sl = res.get('slab_design', {})

        self.text_report.append("  ----------------------------------------------------------------------------------")
        self.text_report.append(f"   1. ÁP LỰC ĐẤT NỀN ĐÁY MÓNG ({sb.get('load_type', 'SLS')}): {sb.get('status_text', '')}")
        if "formula_explanation" in sb:
            self.text_report.append(f"      {sb['formula_explanation'].replace('\n', '\n      ')}")

        self.text_report.append(f"\n   2. ỔN ĐỊNH CHỐNG NHỔ MÓNG ({up.get('load_type', 'SLS')}): {up.get('status_text', '')}")
        if "formula_explanation" in up:
            self.text_report.append(f"      {up['formula_explanation'].replace('\n', '\n      ')}")

        if col:
            self.text_report.append(f"\n   3. TÍNH CỐT THÉP 4 CỔ CỘT ({col.get('load_type', 'ULS')}): {col.get('status_text', '')}")
            if "formula_explanation" in col:
                self.text_report.append(f"      {col['formula_explanation'].replace('\n', '\n      ')}")
            self.text_report.append(f"      --> Bố trí thép dọc cổ cột: {col.get('suggested_column_rebars', '')}")
            self.text_report.append(f"      --> Bố trí thép đai đai cột: {col.get('suggested_stirrups', '')}")

        if bolt:
            self.text_report.append(f"\n   4. KIỂM TRA CUỘC BU-LÔNG NEO M36: {bolt.get('status_text', '')}")
            if "formula_explanation" in bolt:
                self.text_report.append(f"      {bolt['formula_explanation'].replace('\n', '\n      ')}")

        if punch:
            self.text_report.append(f"\n   5. KIỂM TRA CHỐNG CHỌC THỦNG BẢN BÈ: {punch.get('status_text', '')}")
            if "formula_explanation" in punch:
                self.text_report.append(f"      {punch['formula_explanation'].replace('\n', '\n      ')}")

        self.text_report.append(f"\n   6. TÍNH CỐT THÉP DẦM SƯỜN MÓNG ({bm.get('load_type', 'ULS')}): {bm.get('status_text', '')}")
        if "formula_explanation" in bm:
            self.text_report.append(f"      {bm['formula_explanation'].replace('\n', '\n      ')}")
        self.text_report.append(f"      --> Cốt thép dọc dầm bố trí: {bm.get('suggested_rebars', '')}")

        self.text_report.append(f"\n   7. TÍNH CỐT THÉP BẢN MÓNG BÈ ({sl.get('load_type', 'ULS')}): {sl.get('status_text', '')}")
        if "formula_explanation" in sl:
            self.text_report.append(f"      {sl['formula_explanation'].replace('\n', '\n      ')}")
        self.text_report.append(f"      --> Lưới thép bản móng: {sl.get('suggested_mesh', '')}")
        self.text_report.append("  ----------------------------------------------------------------------------------\n")
