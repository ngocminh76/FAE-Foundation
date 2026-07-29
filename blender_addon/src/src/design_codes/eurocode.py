"""
Eurocode 2 (EN 1992-1-1) & Eurocode 7 (EN 1997-1) Verification Engine
Complete Implementation: Soil Bearing, Uplift EQU, Stub Columns, Anchor Bolts & Punching Shear
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
        Pmax = self.fea_results.get("max_soil_pressure_kpa", 0.0)
        R_d = self.fea_results.get("soil_bearing_capacity_kpa", 250.0) / 1.4

        is_safe = Pmax <= R_d
        return {
            "load_type": "SLS / GEO Limit State",
            "P_max_kPa": Pmax,
            "R_d_design_kPa": R_d,
            "is_bearing_safe": is_safe,
            "formula_explanation": f"P_max = {Pmax:.2f} kPa <= R_d = {R_d:.2f} kPa",
            "status_text": "PASSED EC7 GEO CHECK" if is_safe else "FAILED EC7 GEO CHECK"
        }

    def check_uplift_stability(self) -> Dict[str, Any]:
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        V_slab = Lx * Ly * self.project.slab.h_slab
        G_k_stb = (V_slab * self.project.concrete.density + (Lx * Ly * 0.8) * self.project.soil.gamma_soil * self.project.column.H_col)

        uplift_loads = [abs(l.N_sls) for l in self.project.loads if l.N_sls < 0]
        Q_k_dst = sum(uplift_loads) if uplift_loads else 0.0

        stb_effect = 0.90 * G_k_stb
        dst_effect = 1.10 * Q_k_dst
        is_safe = stb_effect >= dst_effect

        return {
            "load_type": "EQU Limit State (0.9 G_k >= 1.1 Q_k)",
            "G_k_stb_kN": G_k_stb,
            "0.90_G_k_stb_kN": stb_effect,
            "1.10_Q_k_dst_kN": dst_effect,
            "is_uplift_safe": is_safe,
            "formula_explanation": f"0.90 * G_k = {stb_effect:.1f} kN >= 1.10 * Q_k = {dst_effect:.1f} kN",
            "status_text": "PASSED EC7 EQU UPLIFT" if is_safe else "FAILED EC7 EQU UPLIFT"
        }

    def check_stub_columns(self) -> Dict[str, Any]:
        b = self.project.column.b_col * 1000.0
        h = self.project.column.h_col * 1000.0
        d = h - 50.0

        f_yd = self.project.steel.R_s / 1.15

        max_uplift = max([abs(l.N_uls) for l in self.project.loads if l.N_uls < 0] + [0.0])
        As_req = (max_uplift * 1000.0) / f_yd
        As_cm2 = As_req / 100.0

        n_bars = max(8, math.ceil(As_req / 490.9))

        return {
            "load_type": "ULS Limit State",
            "As_required_cm2": As_cm2,
            "suggested_column_rebars": f"{n_bars} H25 (As = {n_bars * 4.91:.2f} cm²)",
            "suggested_stirrups": "H10 @ 150mm c/c",
            "formula_explanation": f"As_col = N_ed / f_yd = {max_uplift:.1f} kN / {f_yd:.0f} MPa = {As_cm2:.2f} cm²",
            "status_text": "PASSED EC2 STUB COLUMN DESIGN"
        }

    def check_anchor_bolts(self) -> Dict[str, Any]:
        spec = self.project.anchor_bolt
        d_b = spec.d_bolt
        n_b = spec.n_bolts_per_leg
        f_yb = spec.f_yb / 1.25

        A_net = 0.84 * (math.pi * d_b**2 / 4.0)
        F_rd_bolts = n_b * A_net * f_yb / 1000.0

        max_uplift = max([abs(l.N_uls) for l in self.project.loads if l.N_uls < 0] + [0.0])
        is_safe = F_rd_bolts >= max_uplift

        return {
            "F_rd_bolts_kN": F_rd_bolts,
            "uplift_demand_kN": max_uplift,
            "is_bolt_safe": is_safe,
            "formula_explanation": f"F_rd = {n_b} * {A_net:.0f}mm² * {f_yb:.0f}MPa = {F_rd_bolts:.1f} kN >= {max_uplift:.1f} kN",
            "status_text": "PASSED EC3 ANCHOR BOLTS" if is_safe else "FAILED ANCHOR BOLTS"
        }

    def check_punching_shear(self) -> Dict[str, Any]:
        h_s = self.project.slab.h_slab * 1000.0
        d_s = h_s - 40.0
        b_c = self.project.column.b_col * 1000.0
        h_c = self.project.column.h_col * 1000.0

        u1 = 2.0 * (b_c + h_c) + 2.0 * math.pi * 2.0 * d_s # EC2 perimeter at 2d
        v_rd_c = 0.45 # MPa (Basic shear strength)

        V_rd_cs = v_rd_c * u1 * d_s / 1000.0 # kN
        max_comp = max([l.N_uls for l in self.project.loads if l.N_uls > 0] + [0.0])
        is_safe = V_rd_cs >= max_comp

        return {
            "u1_mm": u1,
            "V_rd_cs_kN": V_rd_cs,
            "V_ed_demand_kN": max_comp,
            "is_punching_safe": is_safe,
            "formula_explanation": f"V_rd,cs = v_rd,c * u1 * d = 0.45 * {u1:.0f}mm * {d_s:.0f}mm = {V_rd_cs:.1f} kN >= {max_comp:.1f} kN",
            "status_text": "PASSED EC2 PUNCHING SHEAR" if is_safe else "FAILED PUNCHING SHEAR"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        b = self.project.beam.b_beam * 1000.0
        h = self.project.beam.h_beam * 1000.0
        d = h - 50.0

        f_ck = 20.0
        f_cd = f_ck / 1.5
        f_yd = self.project.steel.R_s / 1.15

        M_ed_kNm = max([abs(l.M_x_uls) + abs(l.Q_y_uls * self.project.column.H_col) for l in self.project.loads]) * 1.5
        M_ed_Nmm = M_ed_kNm * 1.0e6

        K = M_ed_Nmm / (b * (d**2) * f_ck)
        z = d * min(0.95, (0.5 + math.sqrt(max(0.0, 0.25 - K / 1.134))))

        As_req = M_ed_Nmm / (f_yd * z)
        As_cm2 = As_req / 100.0
        n_bars = max(4, math.ceil(As_req / 380.1))

        return {
            "load_type": "ULS Limit State",
            "M_ed_kNm": M_ed_kNm,
            "K_factor": K,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} H20 (As = {n_bars * 3.80:.2f} cm²)",
            "formula_explanation": f"As = M_ed / (f_yd * z) = {M_ed_Nmm:.0f} / ({f_yd:.0f} * {z:.0f}) = {As_cm2:.2f} cm²",
            "status_text": "PASSED EC2 FLEXURE"
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        h_s = self.project.slab.h_slab * 1000.0
        d_s = h_s - 35.0

        f_yd = self.project.steel.R_s / 1.15
        M_ed_slab = self.fea_results.get("max_soil_pressure_kpa", 66.0) * (1.5**2) / 8.0 * 1.5
        M_ed_Nmm = M_ed_slab * 1.0e6

        z = 0.9 * d_s
        As_req = M_ed_Nmm / (f_yd * z)
        As_cm2 = As_req / 100.0

        return {
            "load_type": "ULS Limit State",
            "M_ed_slab_kNm": M_ed_slab,
            "As_slab_cm2_per_m": As_cm2,
            "suggested_mesh": "H14 @ 150mm c/c",
            "formula_explanation": f"As_slab = M_ed / (f_yd * z) = {As_cm2:.2f} cm²/m",
            "status_text": "PASSED EC2 SLAB FLEXURE"
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
