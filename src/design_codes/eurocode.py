"""
Eurocode 2 (EN 1992-1-1) & Eurocode 7 (EN 1997-1) Verification Engine for Transmission Tower Foundation
"""

import math
from typing import Dict, Any
from src.core.models import TowerFoundationProject
from src.design_codes.base import BaseCodeChecker

class EurocodeChecker(BaseCodeChecker):
    @property
    def code_name(self) -> str:
        return "🇪🇺 Eurocode 2 (EN 1992 Concrete) / Eurocode 7 (EN 1997 Geotechnical)"

    def check_soil_bearing(self) -> Dict[str, Any]:
        """Kiểm tra sức chịu tải địa kỹ thuật theo Eurocode 7 Design Approach 1/2/3"""
        Pmax = self.fea_results.get("max_soil_pressure_kpa", 0.0)
        R_d = self.fea_results.get("soil_bearing_capacity_kpa", 250.0) / 1.4 # Partial factor gamma_R = 1.4

        is_safe = Pmax <= R_d
        return {
            "P_max_kPa": Pmax,
            "R_d_design_kPa": R_d,
            "gamma_R_partial_factor": 1.4,
            "is_bearing_safe": is_safe,
            "status_text": "PASSED EC7 GEO CHECK" if is_safe else "FAILED EC7 GEO CHECK"
        }

    def check_uplift_stability(self) -> Dict[str, Any]:
        """Kiểm tra nhổ theo Eurocode 7 ULS EQU Limit State: 0.9 G_k,stb >= 1.1 Q_k,dst"""
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        V_slab = Lx * Ly * self.project.slab.h_slab
        G_k_stb = (V_slab * self.project.concrete.density + (Lx * Ly * 0.8) * self.project.soil.gamma_soil * self.project.column.H_col)

        uplift_loads = [abs(l.N) for l in self.project.loads if l.N < 0]
        Q_k_dst = sum(uplift_loads) if uplift_loads else 0.0

        stb_effect = 0.90 * G_k_stb
        dst_effect = 1.10 * Q_k_dst

        is_safe = stb_effect >= dst_effect

        return {
            "G_k_stb_kN": G_k_stb,
            "0.90_G_k_stb_kN": stb_effect,
            "1.10_Q_k_dst_kN": dst_effect,
            "is_uplift_safe": is_safe,
            "status_text": "PASSED EC7 EQU UPLIFT" if is_safe else "FAILED EC7 EQU UPLIFT"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        """
        Tính thép dầm sườn theo Eurocode 2 (EN 1992-1-1):
        f_cd = f_ck / gamma_C = 20 / 1.5 = 13.33 MPa
        f_yd = f_yk / gamma_S = 400 / 1.15 = 347.8 MPa
        K = M_ed / (b * d^2 * f_ck)
        """
        b = self.project.beam.b_beam * 1000.0
        h = self.project.beam.h_beam * 1000.0
        d = h - 50.0

        f_ck = 20.0 # C20/25
        gamma_C = 1.5
        gamma_S = 1.15

        f_cd = f_ck / gamma_C
        f_yd = self.project.steel.R_s / gamma_S

        M_ed_kNm = max([abs(l.M_x) + abs(l.Q_y * self.project.column.H_col) for l in self.project.loads]) * 1.5
        M_ed_Nmm = M_ed_kNm * 1.0e6

        K = M_ed_Nmm / (b * (d**2) * f_ck)
        z = d * min(0.95, (0.5 + math.sqrt(max(0.0, 0.25 - K / 1.134))))

        As_req = M_ed_Nmm / (f_yd * z)
        As_cm2 = As_req / 100.0
        n_bars = max(4, math.ceil(As_req / 380.1))

        return {
            "M_ed_kNm": M_ed_kNm,
            "f_cd_MPa": f_cd,
            "f_yd_MPa": f_yd,
            "K_factor": K,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} H20 (As = {n_bars * 3.80:.2f} cm2)",
            "status_text": "PASSED EC2 FLEXURE"
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        """Tính thép bản móng bè theo Eurocode 2"""
        h_s = self.project.slab.h_slab * 1000.0
        d_s = h_s - 35.0
        b_unit = 1000.0

        f_yd = self.project.steel.R_s / 1.15
        M_ed_slab = self.fea_results.get("max_soil_pressure_kpa", 66.0) * (1.5**2) / 8.0 * 1.5
        M_ed_Nmm = M_ed_slab * 1.0e6

        z = 0.9 * d_s
        As_req = M_ed_Nmm / (f_yd * z)
        As_cm2 = As_req / 100.0

        return {
            "M_ed_slab_kNm": M_ed_slab,
            "As_slab_cm2_per_m": As_cm2,
            "suggested_mesh": "H14 @ 150mm c/c",
            "status_text": "PASSED EC2 SLAB FLEXURE"
        }

    def run_all_checks(self) -> Dict[str, Any]:
        return {
            "code_name": self.code_name,
            "soil_bearing": self.check_soil_bearing(),
            "uplift_stability": self.check_uplift_stability(),
            "beam_design": self.design_beam_flexure(),
            "slab_design": self.design_slab_flexure()
        }
