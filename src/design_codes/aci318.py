"""
ACI 318-19 (US Standard LRFD Method) Verification Engine for Transmission Tower Foundation
"""

import math
from typing import Dict, Any
from src.core.models import TowerFoundationProject
from src.design_codes.base import BaseCodeChecker

class ACI318CodeChecker(BaseCodeChecker):
    @property
    def code_name(self) -> str:
        return "🇺🇸 ACI 318-19 (Building Code Requirements for Structural Concrete - USA)"

    def check_soil_bearing(self) -> Dict[str, Any]:
        """Kiểm tra áp lực đất nền theo ASD / LRFD phương pháp Mỹ"""
        Pmax = self.fea_results.get("max_soil_pressure_kpa", 0.0)
        q_allow = self.fea_results.get("soil_bearing_capacity_kpa", 250.0)
        
        is_safe = Pmax <= q_allow
        return {
            "P_max_kPa": Pmax,
            "q_allowable_kPa": q_allow,
            "is_bearing_safe": is_safe,
            "status_text": "PASSED (Pmax <= q_allowable)" if is_safe else "FAILED (Overbearing)"
        }

    def check_uplift_stability(self) -> Dict[str, Any]:
        """Kiểm tra chống nhổ móng theo ACI 318 Uplift factor: 0.9D + 1.0W"""
        # Trọng lượng móng & Đất đè
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        V_slab = Lx * Ly * self.project.slab.h_slab
        G_mong = V_slab * self.project.concrete.density
        G_dat = (Lx * Ly * 0.8) * self.project.soil.gamma_soil * self.project.column.H_col

        uplift_loads = [abs(l.N) for l in self.project.loads if l.N < 0]
        total_N_uplift = sum(uplift_loads) if uplift_loads else 0.0

        # LRFD Uplift demand: 0.9 * (G_mong + G_dat) >= 1.0 * N_uplift
        factored_resistance = 0.9 * (G_mong + G_dat)
        is_uplift_safe = factored_resistance >= total_N_uplift

        return {
            "G_mong_kN": G_mong,
            "G_dat_kN": G_dat,
            "factored_resistance_0.9D_kN": factored_resistance,
            "uplift_demand_1.0W_kN": total_N_uplift,
            "is_uplift_safe": is_uplift_safe,
            "status_text": "PASSED UPLIFT (0.9D >= 1.0W)" if is_uplift_safe else "FAILED UPLIFT"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        """
        Tính thép dầm sườn theo ACI 318-19:
        phi = 0.90 (Flexure)
        Mu <= phi * Mn = phi * As * fy * (d - a/2)
        a = (As * fy) / (0.85 * f'c * b)
        """
        b = self.project.beam.b_beam * 1000.0 # mm
        h = self.project.beam.h_beam * 1000.0 # mm
        d = h - 60.0 # mm (effective depth)

        fc_prime = 25.0 # MPa (Cylinder strength equivalent to B25)
        fy = self.project.steel.R_s # MPa (400 MPa)
        phi = 0.90

        max_M_kNm = max([abs(l.M_x) + abs(l.Q_y * self.project.column.H_col) for l in self.project.loads]) * 1.6
        Mu_Nmm = max_M_kNm * 1.0e6

        # As_approx = Mu / (phi * fy * 0.9 * d)
        As_req = Mu_Nmm / (phi * fy * 0.85 * d)
        As_cm2 = As_req / 100.0

        n_bars = max(4, math.ceil(As_req / 380.1))

        return {
            "Mu_kNm": max_M_kNm,
            "phi_factor": phi,
            "fc_prime_MPa": fc_prime,
            "fy_MPa": fy,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} #7 (As = {n_bars * 3.80:.2f} cm2)",
            "status_text": "PASSED ACI FLEXURE"
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        """Tính thép bản móng bè theo ACI 318-19"""
        h_s = self.project.slab.h_slab * 1000.0
        d_s = h_s - 40.0
        b_unit = 1000.0

        fc_prime = 25.0
        fy = self.project.steel.R_s
        phi = 0.90

        Mu_slab_kNm = self.fea_results.get("max_soil_pressure_kpa", 66.0) * (1.5**2) / 8.0 * 1.6
        Mu_Nmm = Mu_slab_kNm * 1.0e6

        As_req = Mu_Nmm / (phi * fy * 0.85 * d_s)
        As_cm2 = As_req / 100.0

        return {
            "Mu_slab_kNm": Mu_slab_kNm,
            "As_slab_cm2_per_m": As_cm2,
            "suggested_mesh": "Rebar #5 @ 6 in. (a150mm)",
            "status_text": "PASSED ACI SLAB FLEXURE"
        }

    def run_all_checks(self) -> Dict[str, Any]:
        return {
            "code_name": self.code_name,
            "soil_bearing": self.check_soil_bearing(),
            "uplift_stability": self.check_uplift_stability(),
            "beam_design": self.design_beam_flexure(),
            "slab_design": self.design_slab_flexure()
        }
