"""
Input Panels Widget for PySide6 Desktop GUI App
"""

from PySide6.QtWidgets import (
    QWidget, QVBoxLayout, QHBoxLayout, QFormLayout, QGroupBox,
    QLabel, QLineEdit, QComboBox, QDoubleSpinBox, QTabWidget,
    QPushButton, QTableWidget, QTableWidgetItem, QHeaderView
)
from PySide6.QtCore import Signal
from src.core.models import TowerFoundationProject, ColumnLoad
from src.core.presets import PRESET_SOILS, PRESET_LOAD_CASES

class InputPanelsWidget(QWidget):
    # Signal phát ra khi người dùng đổi dữ liệu đầu vào hoặc bấm nút Tính Toán
    calculate_requested = Signal(TowerFoundationProject, str)

    def __init__(self, parent=None):
        super().__init__(parent)
        self.project = TowerFoundationProject()
        self.init_ui()

    def init_ui(self):
        layout = QVBoxLayout(self)

        # Tab Widget cho các mục nhập liệu
        self.tabs = QTabWidget()
        
        # Tab 1: Hình học Móng
        self.tab_geometry = QWidget()
        self.init_geometry_tab()
        self.tabs.addTab(self.tab_geometry, "📐 Hình Học Móng")

        # Tab 2: Địa Chất & Nền
        self.tab_soil = QWidget()
        self.init_soil_tab()
        self.tabs.addTab(self.tab_soil, "🪨 Địa Chất")

        # Tab 3: Tải Trọng 4 Chân
        self.tab_loads = QWidget()
        self.init_loads_tab()
        self.tabs.addTab(self.tab_loads, "📊 Tải Trọng Cột")

        # Tab 4: Tiêu Chuẩn Thiết Kế
        self.tab_code = QWidget()
        self.init_code_tab()
        self.tabs.addTab(self.tab_code, "📜 Tiêu Chuẩn")

        layout.addWidget(self.tabs)

        # Nút bấm Thực Thi Tính Toán
        self.btn_calc = QPushButton("⚡ THỰC THI TÍNH TOÁN & PHÂN TÍCH FEA")
        self.btn_calc.setStyleSheet("""
            QPushButton {
                background-color: #007acc;
                color: white;
                font-weight: bold;
                font-size: 14px;
                padding: 10px;
                border-radius: 5px;
            }
            QPushButton:hover {
                background-color: #0098ff;
            }
        """)
        self.btn_calc.clicked.connect(self.on_calculate_clicked)
        layout.addWidget(self.btn_calc)

    def init_geometry_tab(self):
        layout = QVBoxLayout(self.tab_geometry)
        
        # Bản móng bè
        group_slab = QGroupBox("1. Bản Móng Bè Phẳng")
        form_slab = QFormLayout(group_slab)
        self.spin_Lx = QDoubleSpinBox()
        self.spin_Lx.setRange(2.0, 30.0)
        self.spin_Lx.setValue(8.0)
        self.spin_Lx.setSuffix(" m")

        self.spin_Ly = QDoubleSpinBox()
        self.spin_Ly.setRange(2.0, 30.0)
        self.spin_Ly.setValue(8.0)
        self.spin_Ly.setSuffix(" m")

        self.spin_hslab = QDoubleSpinBox()
        self.spin_hslab.setRange(0.2, 2.0)
        self.spin_hslab.setValue(0.4)
        self.spin_hslab.setSingleStep(0.05)
        self.spin_hslab.setSuffix(" m")

        form_slab.addRow("Chiều dài Lx:", self.spin_Lx)
        form_slab.addRow("Chiều rộng Ly:", self.spin_Ly)
        form_slab.addRow("Dày bản h_slab:", self.spin_hslab)
        layout.addWidget(group_slab)

        # Dầm sườn
        group_beam = QGroupBox("2. Dầm Sườn Nổi 2 Phương (Chạy Suốt)")
        form_beam = QFormLayout(group_beam)
        self.spin_bbeam = QDoubleSpinBox()
        self.spin_bbeam.setRange(0.2, 1.5)
        self.spin_bbeam.setValue(0.4)
        self.spin_bbeam.setSuffix(" m")

        self.spin_hbeam = QDoubleSpinBox()
        self.spin_hbeam.setRange(0.3, 2.5)
        self.spin_hbeam.setValue(0.8)
        self.spin_hbeam.setSuffix(" m")

        form_beam.addRow("Rộng dầm b_beam:", self.spin_bbeam)
        form_beam.addRow("Cao dầm h_beam:", self.spin_hbeam)
        layout.addWidget(group_beam)

        # Cổ cột
        group_col = QGroupBox("3. 4 Cổ Cột Cao (4 Stub Columns)")
        form_col = QFormLayout(group_col)
        self.spin_lcx = QDoubleSpinBox()
        self.spin_lcx.setRange(1.0, 20.0)
        self.spin_lcx.setValue(3.5)
        self.spin_lcx.setSuffix(" m")

        self.spin_lcy = QDoubleSpinBox()
        self.spin_lcy.setRange(1.0, 20.0)
        self.spin_lcy.setValue(3.5)
        self.spin_lcy.setSuffix(" m")

        self.spin_Hcol = QDoubleSpinBox()
        self.spin_Hcol.setRange(0.5, 5.0)
        self.spin_Hcol.setValue(1.8)
        self.spin_Hcol.setSuffix(" m")

        form_col.addRow("Khoảng cách cổ cột X (lcx):", self.spin_lcx)
        form_col.addRow("Khoảng cách cổ cột Y (lcy):", self.spin_lcy)
        form_col.addRow("Chiều cao cổ cột H_col:", self.spin_Hcol)
        layout.addWidget(group_col)
        layout.addStretch()

    def init_soil_tab(self):
        layout = QVBoxLayout(self.tab_soil)
        
        # Mẫu địa chất
        group_preset = QGroupBox("Chọn Mẫu Địa Chất Chuẩn")
        layout_preset = QVBoxLayout(group_preset)
        self.combo_soil_preset = QComboBox()
        self.combo_soil_preset.addItems(list(PRESET_SOILS.keys()))
        self.combo_soil_preset.currentIndexChanged.connect(self.on_soil_preset_changed)
        layout_preset.addWidget(self.combo_soil_preset)
        layout.addWidget(group_preset)

        group_soil_detail = QGroupBox("Thông Số Nền Đất")
        form_soil = QFormLayout(group_soil_detail)
        
        self.spin_Kz = QDoubleSpinBox()
        self.spin_Kz.setRange(1000.0, 200000.0)
        self.spin_Kz.setValue(22500.0)
        self.spin_Kz.setSuffix(" kN/m³")

        self.spin_gamma_soil = QDoubleSpinBox()
        self.spin_gamma_soil.setRange(10.0, 25.0)
        self.spin_gamma_soil.setValue(18.5)
        self.spin_gamma_soil.setSuffix(" kN/m³")

        self.spin_Rtc = QDoubleSpinBox()
        self.spin_Rtc.setRange(50.0, 1000.0)
        self.spin_Rtc.setValue(250.0)
        self.spin_Rtc.setSuffix(" kPa")

        # Mực nước ngầm
        self.spin_GWT = QDoubleSpinBox()
        self.spin_GWT.setRange(0.0, 20.0)
        self.spin_GWT.setValue(10.0)
        self.spin_GWT.setSuffix(" m (0=Ngập mặt)")

        form_soil.addRow("Hệ số nền Winkler (Kz):", self.spin_Kz)
        form_soil.addRow("Dung trọng đất đè (gamma):", self.spin_gamma_soil)
        form_soil.addRow("Sức chịu tải đất (Rtc):", self.spin_Rtc)
        form_soil.addRow("Chiều sâu Nước ngầm (GWT):", self.spin_GWT)
        layout.addWidget(group_soil_detail)
        
        # Phương pháp giải
        group_engine = QGroupBox("Phương Pháp Giải (Engine)")
        layout_engine = QVBoxLayout(group_engine)
        self.combo_engine = QComboBox()
        self.combo_engine.addItems([
            "1. Tính tay Bóc tách Ô bản (Theo Cẩm nang PECC5)",
            "2. Giải tích Lưới lò xo Winkler (Phương pháp FEM)"
        ])
        layout_engine.addWidget(self.combo_engine)
        layout.addWidget(group_engine)
        
        layout.addStretch()

    def init_loads_tab(self):
        layout = QVBoxLayout(self.tab_loads)
        
        # Mẫu tải tháp điện
        group_preset_load = QGroupBox("Chọn Mẫu Tải Trọng Tháp Điện")
        layout_preset_load = QVBoxLayout(group_preset_load)
        self.combo_load_preset = QComboBox()
        self.combo_load_preset.addItems(list(PRESET_LOAD_CASES.keys()))
        self.combo_load_preset.currentIndexChanged.connect(self.on_load_preset_changed)
        layout_preset_load.addWidget(self.combo_load_preset)
        layout.addWidget(group_preset_load)

        # Bảng hiển thị/sửa tải trọng 4 chân
        self.table_loads = QTableWidget(4, 5)
        self.table_loads.setHorizontalHeaderLabels(["N (kN)", "Qx (kN)", "Qy (kN)", "Mx (kNm)", "My (kNm)"])
        self.table_loads.setVerticalHeaderLabels(["Chân 1 (Leg 1)", "Chân 2 (Leg 2)", "Chân 3 (Leg 3)", "Chân 4 (Leg 4)"])
        self.table_loads.horizontalHeader().setSectionResizeMode(QHeaderView.Stretch)
        self.update_loads_table_from_preset()
        layout.addWidget(self.table_loads)

    def init_code_tab(self):
        layout = QVBoxLayout(self.tab_code)
        group_code = QGroupBox("Chọn Tiêu Chuẩn Tính Toán Cốt Thép & Nhổ Móng")
        layout_code = QVBoxLayout(group_code)
        
        self.combo_code = QComboBox()
        self.combo_code.addItems([
            "ALL - So Sánh Cùng Lúc 3 Tiêu Chuẩn (TCVN / ACI 318 / Eurocode)",
            "TCVN - Tiêu chuẩn Việt Nam (TCVN 5574:2018 / TCVN 9362:2012)",
            "ACI318 - Tiêu chuẩn Mỹ (ACI 318-19 LRFD Method)",
            "EUROCODE - Tiêu chuẩn Châu Âu (Eurocode 2 & Eurocode 7)"
        ])
        layout_code.addWidget(self.combo_code)
        layout.addWidget(group_code)
        layout.addStretch()

    def on_soil_preset_changed(self, index):
        name = self.combo_soil_preset.currentText()
        soil = PRESET_SOILS.get(name)
        if soil:
            self.spin_Kz.setValue(soil.K_z)
            self.spin_gamma_soil.setValue(soil.gamma_soil)

    def on_load_preset_changed(self, index):
        self.update_loads_table_from_preset()

    def update_loads_table_from_preset(self):
        name = self.combo_load_preset.currentText()
        case_data = PRESET_LOAD_CASES.get(name, list(PRESET_LOAD_CASES.values())[0])
        loads = case_data["loads"]

        for idx, load in enumerate(loads):
            self.table_loads.setItem(idx, 0, QTableWidgetItem(f"{load.N:.1f}"))
            self.table_loads.setItem(idx, 1, QTableWidgetItem(f"{load.Q_x:.1f}"))
            self.table_loads.setItem(idx, 2, QTableWidgetItem(f"{load.Q_y:.1f}"))
            self.table_loads.setItem(idx, 3, QTableWidgetItem(f"{load.M_x:.1f}"))
            self.table_loads.setItem(idx, 4, QTableWidgetItem(f"{load.M_y:.1f}"))

    def get_current_project(self) -> TowerFoundationProject:
        """Thu thập dữ liệu từ giao diện và trả về đối tượng TowerFoundationProject"""
        self.project.slab.L_x = self.spin_Lx.value()
        self.project.slab.L_y = self.spin_Ly.value()
        self.project.slab.h_slab = self.spin_hslab.value()

        self.project.beam.b_beam = self.spin_bbeam.value()
        self.project.beam.h_beam = self.spin_hbeam.value()

        self.project.column.spacing_x = self.spin_lcx.value()
        self.project.column.spacing_y = self.spin_lcy.value()
        self.project.column.H_col = self.spin_Hcol.value()

        self.project.soil.K_z = self.spin_Kz.value()
        self.project.soil.gamma_soil = self.spin_gamma_soil.value()

        # Thu thập bảng tải trọng
        loads = []
        for i in range(4):
            try:
                N_val = float(self.table_loads.item(i, 0).text())
                Qx_val = float(self.table_loads.item(i, 1).text())
                Qy_val = float(self.table_loads.item(i, 2).text())
                Mx_val = float(self.table_loads.item(i, 3).text())
                My_val = float(self.table_loads.item(i, 4).text())
                
                # Tải tiêu chuẩn SLS (gamma = 1.0) và Tải tính toán ULS (gamma ~ 1.2)
                loads.append(ColumnLoad(
                    leg_id=i+1,
                    N_sls=N_val / 1.2 if N_val > 0 else N_val / 1.15,
                    Q_x_sls=Qx_val / 1.2, Q_y_sls=Qy_val / 1.2,
                    M_x_sls=Mx_val / 1.2, M_y_sls=My_val / 1.2,
                    N_uls=N_val, Q_x_uls=Qx_val, Q_y_uls=Qy_val,
                    M_x_uls=Mx_val, M_y_uls=My_val
                ))
            except (ValueError, AttributeError):
                loads.append(ColumnLoad(leg_id=i+1, N_sls=100.0, N_uls=120.0))

        self.project.loads = loads
        return self.project

    def get_selected_code_key(self) -> str:
        text = self.combo_code.currentText()
        if "TCVN" in text and "ALL" not in text:
            return "TCVN"
        elif "ACI318" in text:
            return "ACI318"
        elif "EUROCODE" in text:
            return "EUROCODE"
        return "ALL"

    def bind_live_updates(self):
        """Bind all input changes to automatically trigger calculation"""
        spinboxes = [
            self.spin_Lx, self.spin_Ly, self.spin_hslab, 
            self.spin_bbeam, self.spin_hbeam, 
            self.spin_lcx, self.spin_lcy, self.spin_Hcol,
            self.spin_Kz, self.spin_gamma_soil, self.spin_Rtc, self.spin_GWT
        ]
        for spin in spinboxes:
            spin.valueChanged.connect(lambda val: self.on_calculate_clicked())
            
        self.combo_engine.currentIndexChanged.connect(self.on_calculate_clicked)

    def on_calculate_clicked(self):
        project = self.get_current_project()
        code_key = self.get_selected_code_key()
        self.calculate_requested.emit(project, code_key)
