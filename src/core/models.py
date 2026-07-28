"""
Core Data Models for Transmission Tower Foundation Analysis App
Supporting explicit separation between SLS (Service Loads for Geotechnical) and ULS (Factored Loads for Concrete Design)
"""

from dataclasses import dataclass, field
from typing import List, Dict

@dataclass
class ConcreteMaterial:
    name: str = "B25"
    f_ck: float = 20.0        # Cường độ chịu nén đặc trưng mẫu trụ (MPa)
    R_b: float = 14.5         # Cường độ chịu nén tính toán (MPa - TCVN 5574:2018)
    R_bt: float = 1.05        # Cường độ chịu kéo tính toán (MPa - TCVN 5574:2018)
    E_b: float = 30.0e3       # Modul đàn hồi (MPa -> 30*10^6 kN/m2)
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
    phi: float = 18.0         # Góc ma sát trong (độ)
    c: float = 16.0           # Lực dính (kPa)

@dataclass
class RaftSlabGeometry:
    L_x: float = 8.0          # Chiều dài bản móng theo trục X (m)
    L_y: float = 8.0          # Chiều rộng bản móng theo trục Y (m)
    h_slab: float = 0.4       # Chiều dày bản móng bè (m)
    h_lean: float = 0.1       # Chiều dày bê tông lót (m)

@dataclass
class RibBeamGeometry:
    b_beam: float = 0.4       # Bề rộng dầm sườn (m)
    h_beam: float = 0.8       # Chiều cao dầm sườn (m)
    full_length_x: bool = True
    full_length_y: bool = True

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
    # Tải trọng Tiêu Chuẩn - SLS (Dùng tính Nền đất & Chống nhổ & Độ lún)
    N_sls: float = 0.0        # Lực dọc tiêu chuẩn (kN)
    Q_x_sls: float = 0.0      # Lực cắt X tiêu chuẩn (kN)
    Q_y_sls: float = 0.0      # Lực cắt Y tiêu chuẩn (kN)
    M_x_sls: float = 0.0      # Mô men X tiêu chuẩn (kNm)
    M_y_sls: float = 0.0      # Mô men Y tiêu chuẩn (kNm)

    # Tải trọng Tính Toán - ULS (Dùng tính Cốt thép bê tông Dầm/Bản)
    N_uls: float = 0.0        # Lực dọc tính toán (kN)
    Q_x_uls: float = 0.0      # Lực cắt X tính toán (kN)
    Q_y_uls: float = 0.0      # Lực cắt Y tính toán (kN)
    M_x_uls: float = 0.0      # Mô men X tính toán (kNm)
    M_y_uls: float = 0.0      # Mô men Y tính toán (kNm)

    @property
    def N(self) -> float:
        """Thuộc tính mặc định trả về N_uls"""
        return self.N_uls if self.N_uls != 0.0 else self.N_sls

    @property
    def Q_x(self) -> float:
        return self.Q_x_uls if self.Q_x_uls != 0.0 else self.Q_x_sls

    @property
    def Q_y(self) -> float:
        return self.Q_y_uls if self.Q_y_uls != 0.0 else self.Q_y_sls

    @property
    def M_x(self) -> float:
        return self.M_x_uls if self.M_x_uls != 0.0 else self.M_x_sls

    @property
    def M_y(self) -> float:
        return self.M_y_uls if self.M_y_uls != 0.0 else self.M_y_sls

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
            # Tổ hợp tải mặc định với cả SLS (Tiêu chuẩn - hệ số 1.0) và ULS (Tính toán - hệ số ~1.2)
            self.loads = [
                ColumnLoad(leg_id=1, N_sls=-400.0, Q_x_sls=70.0, Q_y_sls=50.0, M_x_sls=100.0, M_y_sls=75.0,
                                     N_uls=-480.0, Q_x_uls=85.0, Q_y_uls=60.0, M_x_uls=120.0, M_y_uls=90.0),
                ColumnLoad(leg_id=2, N_sls=-350.0, Q_x_sls=65.0, Q_y_sls=45.0, M_x_sls=95.0,  M_y_sls=70.0,
                                     N_uls=-420.0, Q_x_uls=80.0, Q_y_uls=55.0, M_x_uls=115.0, M_y_uls=85.0),
                ColumnLoad(leg_id=3, N_sls=-120.0, Q_x_sls=35.0, Q_y_sls=30.0, M_x_sls=50.0,  M_y_sls=35.0,
                                     N_uls=-150.0, Q_x_uls=45.0, Q_y_uls=35.0, M_x_uls=60.0,  M_y_uls=45.0),
                ColumnLoad(leg_id=4, N_sls=2040.0, Q_x_sls=95.0, Q_y_sls=75.0, M_x_sls=145.0, M_y_sls=110.0,
                                     N_uls=2450.0, Q_x_uls=115.0,Q_y_uls=90.0, M_x_uls=175.0, M_y_uls=135.0)
            ]
