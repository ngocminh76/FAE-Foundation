"""
Core Data Models for Transmission Tower Foundation Analysis App
"""

from dataclasses import dataclass, field
from typing import List, Dict

@dataclass
class ConcreteMaterial:
    name: str = "B25"
    f_ck: float = 20.0        # Cường độ chịu nén đặc trưng (MPa)
    R_b: float = 14.5         # Cường độ chịu nén tính toán (MPa - TCVN 5574:2018)
    R_bt: float = 1.05        # Cường độ chịu kéo tính toán (MPa - TCVN 5574:2018)
    E_b: float = 30.0e3       # Modul đàn hồi (MPa = N/mm2 = 10^3 kN/m2 -> 30*10^6 kN/m2)
    density: float = 25.0     # Trọng lượng riêng bê tông (kN/m3)

@dataclass
class SteelMaterial:
    name: str = "CB400-V"
    R_s: float = 350.0        # Cường độ chịu kéo tính toán (MPa)
    R_sw: float = 280.0       # Cường độ cốt đai (MPa)
    E_s: float = 200.0e3      # Modul đàn hồi thép (MPa)

@dataclass
class SoilSpring:
    K_z: float = 22500.0      # Hệ số nền Winkler (kN/m3)
    gamma_soil: float = 18.5  # Dung trọng đất đè móng (kN/m3)
    phi: float = 20.0         # Góc ma sát trong (độ)
    c: float = 15.0           # Lực dính (kPa)

@dataclass
class RaftSlabGeometry:
    L_x: float = 8.0          # Chiều dài bản móng theo trục X (m)
    L_y: float = 8.0          # Chiều rộng bản móng theo trục Y (m)
    h_slab: float = 0.4       # Chiều dày bản móng bè (m)
    h_lean: float = 0.1       # Chiều dày bê tông lót (m)

@dataclass
class RibBeamGeometry:
    b_beam: float = 0.4       # Bề rộng dầm sườn (m)
    h_beam: float = 0.8       # Chiều cao dầm sườn (m - tính từ mặt đáy bản hoặc mặt trên bản)
    full_length_x: bool = True # Dầm X chạy dài từ mép sang mép (L_beam_X = L_x)
    full_length_y: bool = True # Dầm Y chạy dài từ mép sang mép (L_beam_Y = L_y)

@dataclass
class StubColumnGeometry:
    spacing_x: float = 3.5    # Khoảng cách giữa 2 chân cột theo trục X (m)
    spacing_y: float = 3.5    # Khoảng cách giữa 2 chân cột theo trục Y (m)
    b_col: float = 0.6        # Bề rộng cổ cột (m)
    h_col: float = 0.6        # Bề sâu cổ cột (m)
    H_col: float = 1.8        # Chiều cao cổ cột nhô lên trên mặt dầm (m)

@dataclass
class ColumnLoad:
    leg_id: int               # ID chân cột (1, 2, 3, 4)
    N: float                  # Lực dọc (kN) - Âm là Kéo/Nhổ, Dương là Nén
    Q_x: float = 0.0          # Lực cắt phương X (kN)
    Q_y: float = 0.0          # Lực cắt phương Y (kN)
    M_x: float = 0.0          # Mô men uốn quanh trục X (kNm)
    M_y: float = 0.0          # Mô men uốn quanh trục Y (kNm)

@dataclass
class TowerFoundationProject:
    name: str = "Móng Cột Điện Truyền Tải"
    concrete: ConcreteMaterial = field(default_factory=ConcreteMaterial)
    steel: SteelMaterial = field(default_factory=SteelMaterial)
    soil: SoilSpring = field(default_factory=SoilSpring)
    slab: RaftSlabGeometry = field(default_factory=RaftSlabGeometry)
    beam: RibBeamGeometry = field(default_factory=RibBeamGeometry)
    column: StubColumnGeometry = field(default_factory=StubColumnGeometry)
    loads: List[ColumnLoad] = field(default_factory=list)

    def __post_init__(self):
        if not self.loads:
            # Tạo tổ hợp tải ví dụ tiêu chuẩn cho 4 chân cột điện:
            # Leg 1 & 2: Windward (Nhổ kéo)
            # Leg 3 & 4: Leeward (Nén nặng)
            self.loads = [
                ColumnLoad(leg_id=1, N=-480.0, Q_x=85.0, Q_y=60.0, M_x=120.0, M_y=90.0),
                ColumnLoad(leg_id=2, N=-420.0, Q_x=80.0, Q_y=55.0, M_x=115.0, M_y=85.0),
                ColumnLoad(leg_id=3, N=1850.0, Q_x=95.0, Q_y=70.0, M_x=140.0, M_y=110.0),
                ColumnLoad(leg_id=4, N=1790.0, Q_x=90.0, Q_y=65.0, M_x=135.0, M_y=105.0)
            ]
