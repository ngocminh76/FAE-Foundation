"""
Preset geological soil profiles and transmission line tower load cases (110kV, 220kV, 500kV)
Supporting SLS (Service Loads for Geotechnical) and ULS (Factored Loads for Concrete Design)
"""

from typing import Dict
from src.core.models import SoilSpring, ColumnLoad, TowerFoundationProject, RaftSlabGeometry, RibBeamGeometry, StubColumnGeometry

# 🪨 Thư viện Địa chất Mẫu chuẩn:
PRESET_SOILS: Dict[str, SoilSpring] = {
    "Sét pha dẻo cứng (Trung bình)": SoilSpring(
        K_z=22500.0,       # kN/m3
        gamma_soil=18.5,   # kN/m3
        phi=18.0,          # độ
        c=16.0             # kPa
    ),
    "Cát hạt trung chặt vừa (Tốt)": SoilSpring(
        K_z=35000.0,       # kN/m3
        gamma_soil=19.0,   # kN/m3
        phi=30.0,          # độ
        c=2.0              # kPa
    ),
    "Đất yếu / Đất bùn dẻo mềm": SoilSpring(
        K_z=12000.0,       # kN/m3
        gamma_soil=17.0,   # kN/m3
        phi=10.0,          # độ
        c=8.0              # kPa
    )
}

# ⚡ Thư viện Tải trọng Tháp Điện Mẫu (Theo EVN / ASCE 74):
PRESET_LOAD_CASES: Dict[str, Dict] = {
    "Tải tháp 110kV - Gió vuông góc (Gió 90°)": {
        "description": "2 chân đón gió bị Kéo Nhổ (-350kN), 2 chân khuất gió bị Nén nặng (+1250kN)",
        "loads": [
            ColumnLoad(leg_id=1, N_sls=-290.0, Q_x_sls=55.0, Q_y_sls=40.0, M_x_sls=70.0, M_y_sls=50.0,
                                 N_uls=-350.0, Q_x_uls=65.0, Q_y_uls=45.0, M_x_uls=85.0, M_y_uls=60.0),
            ColumnLoad(leg_id=2, N_sls=-260.0, Q_x_sls=50.0, Q_y_sls=35.0, M_x_sls=65.0, M_y_sls=45.0,
                                 N_uls=-310.0, Q_x_uls=60.0, Q_y_uls=40.0, M_x_uls=80.0, M_y_uls=55.0),
            ColumnLoad(leg_id=3, N_sls=1040.0, Q_x_sls=60.0, Q_y_sls=45.0, M_x_sls=85.0, M_y_sls=60.0,
                                 N_uls=1250.0, Q_x_uls=75.0, Q_y_uls=55.0, M_x_uls=105.0,M_y_uls=75.0),
            ColumnLoad(leg_id=4, N_sls=1000.0, Q_x_sls=55.0, Q_y_sls=40.0, M_x_sls=80.0, M_y_sls=55.0,
                                 N_uls=1210.0, Q_x_uls=70.0, Q_y_uls=50.0, M_x_uls=100.0,M_y_uls=70.0),
        ]
    },
    "Tải tháp 220kV - Gió Xiên 45° (3 Chân Nhổ, 1 Chân Nén Dồn)": {
        "description": "Gió xiên 45° kết hợp Cột Góc làm 3 chân bị Kéo Nhổ cùng lúc (-480kN), 1 chân bị Nén dồn (+2450kN)",
        "loads": [
            ColumnLoad(leg_id=1, N_sls=-400.0, Q_x_sls=70.0, Q_y_sls=50.0, M_x_sls=100.0, M_y_sls=75.0,
                                 N_uls=-480.0, Q_x_uls=85.0, Q_y_uls=60.0, M_x_uls=120.0, M_y_uls=90.0),
            ColumnLoad(leg_id=2, N_sls=-350.0, Q_x_sls=65.0, Q_y_sls=45.0, M_x_sls=95.0,  M_y_sls=70.0,
                                 N_uls=-420.0, Q_x_uls=80.0, Q_y_uls=55.0, M_x_uls=115.0, M_y_uls=85.0),
            ColumnLoad(leg_id=3, N_sls=-120.0, Q_x_sls=35.0, Q_y_sls=30.0, M_x_sls=50.0,  M_y_sls=35.0,
                                 N_uls=-150.0, Q_x_uls=45.0, Q_y_uls=35.0, M_x_uls=60.0,  M_y_uls=45.0),
            ColumnLoad(leg_id=4, N_sls=2040.0, Q_x_sls=95.0, Q_y_sls=75.0, M_x_sls=145.0, M_y_sls=110.0,
                                 N_uls=2450.0, Q_x_uls=115.0,Q_y_uls=90.0, M_x_uls=175.0, M_y_uls=135.0)
        ]
    },
    "Tải tháp 500kV - Sự cố Đứt Dây (Conductor Breakage)": {
        "description": "Đứt dây pha sinh ra Mô men xoắn Mz và lực cắt bẻ cực mạnh tại các chân móng",
        "loads": [
            ColumnLoad(leg_id=1, N_sls=-540.0, Q_x_sls=115.0, Q_y_sls=80.0, M_x_sls=175.0, M_y_sls=130.0,
                                 N_uls=-650.0, Q_x_uls=140.0, Q_y_uls=95.0, M_x_uls=210.0, M_y_uls=155.0),
            ColumnLoad(leg_id=2, N_sls=-480.0, Q_x_sls=105.0, Q_y_sls=70.0, M_x_sls=160.0, M_y_sls=115.0,
                                 N_uls=-580.0, Q_x_uls=130.0, Q_y_uls=85.0, M_x_uls=195.0, M_y_uls=140.0),
            ColumnLoad(leg_id=3, N_sls=2580.0, Q_x_sls=130.0, Q_y_sls=95.0, M_x_sls=205.0, M_y_sls=150.0,
                                 N_uls=3100.0, Q_x_uls=160.0, Q_y_uls=115.0, M_x_uls=250.0,M_y_uls=185.0),
            ColumnLoad(leg_id=4, N_sls=2450.0, Q_x_sls=120.0, Q_y_sls=85.0, M_x_sls=195.0, M_y_sls=140.0,
                                 N_uls=2950.0, Q_x_uls=150.0, Q_y_uls=105.0, M_x_uls=235.0,M_y_uls=170.0),
        ]
    }
}

def create_sample_project(load_case_name: str = "Tải tháp 220kV - Gió Xiên 45° (3 Chân Nhổ, 1 Chân Nén Dồn)") -> TowerFoundationProject:
    """Tạo dự án mẫu hoàn chỉnh với các tham số địa chất và tải trọng giả định chuẩn kỹ thuật."""
    load_data = PRESET_LOAD_CASES.get(load_case_name, list(PRESET_LOAD_CASES.values())[0])
    
    return TowerFoundationProject(
        name=f"Dự Án Móng Cột Điện ({load_case_name})",
        soil=PRESET_SOILS["Sét pha dẻo cứng (Trung bình)"],
        slab=RaftSlabGeometry(L_x=8.0, L_y=8.0, h_slab=0.4, h_lean=0.1),
        beam=RibBeamGeometry(b_beam=0.4, h_beam=0.8),
        column=StubColumnGeometry(spacing_x=3.5, spacing_y=3.5, b_col=0.6, h_col=0.6, H_col=1.8),
        loads=load_data["loads"]
    )
