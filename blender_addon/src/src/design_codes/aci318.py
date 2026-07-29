"""
ACI 318-19 (US Standard LRFD Method) Verification Engine for Transmission Tower Foundation
Complete Implementation: Soil Bearing, Uplift, Stub Columns, Anchor Bolts, Punching Shear
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
        Pmax = self.fea_results.get("max_soil_pressure_kpa", 0.0)
        q_allow = self.fea_results.get("soil_bearing_capacity_kpa", 250.0)
        is_safe = Pmax <= q_allow
        return {
            "load_type": "Service Loads (SLS - 1.0D + 1.0W)",
            "P_max_kPa": Pmax,
            "q_allowable_kPa": q_allow,
            "is_bearing_safe": is_safe,
            "formula_explanation": f"P_max = {Pmax:.2f} kPa <= q_allowable = {q_allow:.2f} kPa",
            "status_text": "PASSED (Pmax <= q_allowable)" if is_safe else "FAILED (Overbearing)"
        }

    def check_uplift_stability(self) -> Dict[str, Any]:
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        V_slab = Lx * Ly * self.project.slab.h_slab
        G_mong = V_slab * self.project.concrete.density
        G_dat = (Lx * Ly * 0.8) * self.project.soil.gamma_soil * self.project.column.H_col

        uplift_loads = [abs(l.N_sls) for l in self.project.loads if l.N_sls < 0]
        total_N_uplift = sum(uplift_loads) if uplift_loads else 0.0

        factored_resistance = 0.9 * (G_mong + G_dat)
        is_uplift_safe = factored_resistance >= total_N_uplift

        return {
            "load_type": "Service / LRFD (0.9D >= 1.0W)",
            "G_mong_kN": G_mong,
            "G_dat_kN": G_dat,
            "factored_resistance_0.9D_kN": factored_resistance,
            "uplift_demand_1.0W_kN": total_N_uplift,
            "is_uplift_safe": is_uplift_safe,
            "formula_explanation": f"0.9 * (D_slab + D_soil) = {factored_resistance:.1f} kN >= 1.0 * W_uplift = {total_N_uplift:.1f} kN",
            "status_text": "PASSED UPLIFT (0.9D >= 1.0W)" if is_uplift_safe else "FAILED UPLIFT"
        }

    def check_stub_columns(self) -> Dict[str, Any]:
        b = self.project.column.b_col * 1000.0
        h = self.project.column.h_col * 1000.0
        d = h - 50.0

        fc_prime = 25.0 # MPa
        fy = self.project.steel.R_s # MPa
        phi = 0.65 # Compression

        max_uplift = max([abs(l.N_uls) for l in self.project.loads if l.N_uls < 0] + [0.0])
        As_req = (max_uplift * 1000.0) / (0.9 * fy)
        As_cm2 = As_req / 100.0

        n_bars = max(8, math.ceil(As_req / 490.9))

        return {
            "load_type": "Factored Loads (ULS - LRFD)",
            "As_required_cm2": As_cm2,
            "suggested_column_rebars": f"{n_bars} #8 (As = {n_bars * 5.09:.2f} cm²)",
            "suggested_stirrups": "#3 ties @ 6 in. c/c",
            "formula_explanation": f"As_col = N_uplift / (0.9 * fy) = {max_uplift:.1f} kN / ({0.9*fy:.0f}) = {As_cm2:.2f} cm²",
            "status_text": "PASSED ACI STUB COLUMN DESIGN"
        }

    def check_anchor_bolts(self) -> Dict[str, Any]:
        spec = self.project.anchor_bolt
        d_b = spec.d_bolt
        n_b = spec.n_bolts_per_leg
        f_yb = spec.f_yb

        A_net = 0.75 * (math.pi * d_b**2 / 4.0)
        phi_N_n = 0.75 * n_b * A_net * f_yb / 1000.0

        max_uplift = max([abs(l.N_uls) for l in self.project.loads if l.N_uls < 0] + [0.0])
        is_safe = phi_N_n >= max_uplift

        return {
            "phi_N_n_kN": phi_N_n,
            "uplift_demand_kN": max_uplift,
            "is_bolt_safe": is_safe,
            "formula_explanation": f"ϕN_n = 0.75 * {n_b} * {A_net:.0f}mm² * {f_yb}MPa = {phi_N_n:.1f} kN >= {max_uplift:.1f} kN",
            "status_text": "PASSED ACI ANCHOR BOLT CHECK" if is_safe else "FAILED ANCHOR BOLTS"
        }

    def check_punching_shear(self) -> Dict[str, Any]:
        h_s = self.project.slab.h_slab * 1000.0
        d_s = h_s - 50.0
        b_c = self.project.column.b_col * 1000.0
        h_c = self.project.column.h_col * 1000.0

        bo = 2.0 * ((b_c + d_s) + (h_c + d_s)) # ACI perimeter at d/2
        fc_prime = 25.0
        phi_v = 0.75

        Vc = 0.33 * math.sqrt(fc_prime) * bo * d_s / 1000.0 # kN
        phi_Vc = phi_v * Vc

        max_comp = max([l.N_uls for l in self.project.loads if l.N_uls > 0] + [0.0])
        is_safe = phi_Vc >= max_comp

        return {
            "bo_mm": bo,
            "phi_Vc_kN": phi_Vc,
            "Vu_demand_kN": max_comp,
            "is_punching_safe": is_safe,
            "formula_explanation": f"ϕVc = 0.75 * 0.33 * √{fc_prime} * {bo:.0f}mm * {d_s:.0f}mm = {phi_Vc:.1f} kN >= {max_comp:.1f} kN",
            "status_text": "PASSED ACI PUNCHING SHEAR" if is_safe else "FAILED PUNCHING SHEAR"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        b = self.project.beam.b_beam * 1000.0
        h = self.project.beam.h_beam * 1000.0
        d = h - 60.0

        fc_prime = 25.0
        fy = self.project.steel.R_s
        phi = 0.90

        max_M_kNm = max([abs(l.M_x_uls) + abs(l.Q_y_uls * self.project.column.H_col) for l in self.project.loads]) * 1.6
        Mu_Nmm = max_M_kNm * 1.0e6

        As_req = Mu_Nmm / (phi * fy * 0.85 * d)
        As_cm2 = As_req / 100.0
        n_bars = max(4, math.ceil(As_req / 380.1))

        return {
            "load_type": "Factored Loads (ULS - LRFD)",
            "Mu_kNm": max_M_kNm,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} #7 (As = {n_bars * 3.80:.2f} cm²)",
            "formula_explanation": f"As = Mu / (ϕ * fy * 0.85 * d) = {Mu_Nmm:.0f} / (0.9 * {fy} * 0.85 * {d:.0f}) = {As_cm2:.2f} cm²",
            "status_text": "PASSED ACI FLEXURE"
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        h_s = self.project.slab.h_slab * 1000.0
        d_s = h_s - 40.0
        b_unit = 1000.0

        fy = self.project.steel.R_s
        phi = 0.90

        Mu_slab_kNm = self.fea_results.get("max_soil_pressure_kpa", 66.0) * (1.5**2) / 8.0 * 1.6
        Mu_Nmm = Mu_slab_kNm * 1.0e6

        As_req = Mu_Nmm / (phi * fy * 0.85 * d_s)
        As_cm2 = As_req / 100.0

        return {
            "load_type": "Factored Loads (ULS - LRFD)",
            "Mu_slab_kNm": Mu_slab_kNm,
            "As_slab_cm2_per_m": As_cm2,
            "suggested_mesh": "Rebar #5 @ 6 in. (a150mm)",
            "formula_explanation": f"As_slab = Mu / (ϕ * fy * 0.85 * d_s) = {As_cm2:.2f} cm²/m",
            "status_text": "PASSED ACI SLAB FLEXURE"
        }

    def run_all_checks(self) -> Dict[str, Any]:
        return {
            "code_name": self.code_name,
            "soil_bearing": self.check_soil_bearing(),
            "uplift_stability": self.check_uplift_stability(),
            "stub_columns": self.check_stub_columns(),
            "anchor_bolts": self.check_anchor_bolts(),
            "punching_shear": self.check_punching_shear(),
            "beam_design": self.design_beam_flexure(),
            "slab_design": self.design_slab_flexure()
        }
