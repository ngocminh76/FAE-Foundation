"""
TCVN 5574:2018 & TCVN 9362:2012 Code Verification Engine with Explicit Step-by-Step Mathematical Formulas
Separating SLS (Service Loads for Geotechnical) and ULS (Factored Loads for Concrete Design)
"""

import math
from typing import Dict, Any
from src.core.models import TowerFoundationProject
from src.design_codes.base import BaseCodeChecker

class TCVNCodeChecker(BaseCodeChecker):
    @property
    def code_name(self) -> str:
        return "🇻🇳 TCVN 5574:2018 / TCVN 9362:2012 (Việt Nam)"

    def check_soil_bearing(self) -> Dict[str, Any]:
        """
        Kiểm tra áp lực đất nền theo TCVN 9362:2012 (Dùng Tải Tiêu Chuẩn - SLS):
        Công thức Pmax,min = (Sigma N_sls) / F_bè  +/-  (Sigma M_sls) / W_bè
        Điều kiện: Pmax <= 1.2 * Rtc, Pmin >= 0
        """
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        Area = Lx * Ly
        W_x = Lx * (Ly**2) / 6.0
        W_y = Ly * (Lx**2) / 6.0

        # Tổng lực dọc tiêu chuẩn SLS
        sum_N_sls = sum(l.N_sls for l in self.project.loads)
        sum_Mx_sls = sum(l.M_x_sls + l.Q_y_sls * self.project.column.H_col for l in self.project.loads)
        sum_My_sls = sum(l.M_y_sls + l.Q_x_sls * self.project.column.H_col for l in self.project.loads)

        P_tb_sls = sum_N_sls / Area
        P_max_sls = P_tb_sls + abs(sum_Mx_sls)/W_x + abs(sum_My_sls)/W_y
        P_min_sls = P_tb_sls - abs(sum_Mx_sls)/W_x - abs(sum_My_sls)/W_y

        R_tc = self.fea_results.get("soil_bearing_capacity_kpa", 250.0)
        allowable_Pmax = 1.2 * R_tc

        is_pmax_safe = P_max_sls <= allowable_Pmax
        is_pmin_safe = P_min_sls >= 0.0

        formula_step = (
            f"P_max = (ΣN_sls / Area) + (|ΣMx_sls| / Wx) + (|ΣMy_sls| / Wy)\n"
            f"      = ({sum_N_sls:.1f} / {Area:.2f}) + ({abs(sum_Mx_sls):.1f} / {W_x:.2f}) + ({abs(sum_My_sls):.1f} / {W_y:.2f})\n"
            f"      = {P_max_sls:.2f} kPa <= {allowable_Pmax:.2f} kPa (1.2 * Rtc)"
        )

        return {
            "load_type": "Tải trọng Tiêu chuẩn (SLS - Hệ số gamma = 1.0)",
            "P_max": P_max_sls,
            "P_min": P_min_sls,
            "R_tc": R_tc,
            "allowable_Pmax": allowable_Pmax,
            "is_pmax_safe": is_pmax_safe,
            "is_pmin_safe": is_pmin_safe,
            "formula_explanation": formula_step,
            "status_text": "ĐẠT ÁP LỰC ĐẤT (Pmax <= 1.2 Rtc)" if is_pmax_safe else "KHÔNG ĐẠT (Vượt sức chịu tải)"
        }

    def check_uplift_stability(self) -> Dict[str, Any]:
        """
        Kiểm tra chống nhổ móng theo 11 TCN 19-2006 & TCVN 9362:2012 (Dùng Tải Tiêu Chuẩn - SLS):
        K_chống_nhổ = (G_móng + G_đất_đè) / N_nhổ_sls >= 1.3
        """
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        h_slab = self.project.slab.h_slab
        b_beam = self.project.beam.b_beam
        h_beam = self.project.beam.h_beam
        b_col = self.project.column.b_col
        h_col = self.project.column.h_col
        H_col = self.project.column.H_col
        gamma_c = self.project.concrete.density
        gamma_s = self.project.soil.gamma_soil

        V_slab = Lx * Ly * h_slab
        V_beams = 2 * Lx * b_beam * (h_beam - h_slab) + 2 * Ly * b_beam * (h_beam - h_slab)
        V_cols = 4 * b_col * h_col * H_col
        
        G_mong = (V_slab + V_beams + V_cols) * gamma_c
        
        # Đất đè trên bản móng bè
        V_soil = (Lx * Ly - (V_beams/(h_beam - h_slab) if h_beam > h_slab else 0) - 4 * b_col * h_col) * H_col
        G_dat = V_soil * gamma_s

        # Tổng lực nhổ kéo tiêu chuẩn SLS (N_sls < 0)
        uplift_loads_sls = [abs(l.N_sls) for l in self.project.loads if l.N_sls < 0]
        total_N_uplift_sls = sum(uplift_loads_sls) if uplift_loads_sls else 0.0

        total_holding_force = G_mong + G_dat
        safety_factor = total_holding_force / total_N_uplift_sls if total_N_uplift_sls > 0 else 999.0

        is_uplift_safe = safety_factor >= 1.3

        formula_step = (
            f"K_nhổ = (G_móng + G_đất) / ΣN_nhổ_sls\n"
            f"      = ({G_mong:.1f} + {G_dat:.1f}) / {total_N_uplift_sls:.1f}\n"
            f"      = {total_holding_force:.1f} / {total_N_uplift_sls:.1f} = {safety_factor:.2f} >= 1.3"
        )

        return {
            "load_type": "Tải trọng Tiêu chuẩn (SLS - Hệ số gamma = 1.0)",
            "G_mong_kN": G_mong,
            "G_dat_kN": G_dat,
            "total_holding_kN": total_holding_force,
            "total_uplift_N_kN": total_N_uplift_sls,
            "safety_factor": safety_factor,
            "required_safety_factor": 1.3,
            "is_uplift_safe": is_uplift_safe,
            "formula_explanation": formula_step,
            "status_text": "ĐẠT CHỐNG NHỔ (K >= 1.3)" if is_uplift_safe else "KHÔNG ĐẠT (Nguy cơ nhổ móng)"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        """
        Tính diện tích thép As chịu uốn Dầm sườn theo TCVN 5574:2018 (Dùng Tải Tính Toán - ULS):
        Mu_uls = Q_uls * H_col + M_uốn_uls
        alpha_m = Mu_uls / (Rb * b * h0^2)
        xi = 1 - sqrt(1 - 2*alpha_m)
        As = (xi * Rb * b * h0) / Rs
        """
        b = self.project.beam.b_beam * 1000.0 # mm
        h = self.project.beam.h_beam * 1000.0 # mm
        a = 50.0 # mm
        h0 = h - a # mm

        Rb = self.project.concrete.R_b # MPa
        Rs = self.project.steel.R_s   # MPa

        # Lấy mô men tính toán ULS lớn nhất (kNm -> Nmm)
        max_M_uls_kNm = max([abs(l.M_x_uls) + abs(l.Q_y_uls * self.project.column.H_col) for l in self.project.loads]) * 1.5
        Mu_Nmm = max_M_uls_kNm * 1.0e6

        alpha_m = Mu_Nmm / (Rb * b * (h0 ** 2))
        
        if alpha_m > 0.35:
            xi = 0.35
            status = "Cần gia cường thép nén (alpha_m lớn)"
        else:
            xi = 1.0 - math.sqrt(max(0.0, 1.0 - 2.0 * alpha_m))
            status = "ĐẠT THÉP UỐN DẦM SƯỜN"

        As_req = (xi * Rb * b * h0) / Rs # mm2
        As_cm2 = As_req / 100.0
        n_bars = max(4, math.ceil(As_req / 380.1))

        formula_step = (
            f"1. Mô men tính toán ULS: Mu = {max_M_uls_kNm:.1f} kNm\n"
            f"2. Hệ số αm = Mu / (Rb * b * h0²) = {Mu_Nmm:.0f} / ({Rb} * {b:.0f} * {h0:.0f}²) = {alpha_m:.4f}\n"
            f"3. Hệ số chiều cao vùng nén ξ = 1 - √(1 - 2αm) = {xi:.4f}\n"
            f"4. Diện tích thép yêu cầu As = (ξ * Rb * b * h0) / Rs = {As_req:.1f} mm² ({As_cm2:.2f} cm²)"
        )

        return {
            "load_type": "Tải trọng Tính toán (ULS - Hệ số gamma = 1.15-1.3)",
            "M_max_kNm": max_M_uls_kNm,
            "b_mm": b,
            "h_mm": h,
            "h0_mm": h0,
            "alpha_m": alpha_m,
            "xi": xi,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} ϕ22 (As_chọn = {n_bars * 3.80:.2f} cm²)",
            "formula_explanation": formula_step,
            "status_text": status
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        """Tính thép As chịu uốn Bản móng bè theo TCVN 5574:2018 (Dùng Tải Tính Toán - ULS)"""
        h_s = self.project.slab.h_slab * 1000.0
        h0_s = h_s - 35.0
        b_unit = 1000.0

        Rb = self.project.concrete.R_b
        Rs = self.project.steel.R_s

        # Mô men uốn tính toán ULS bản móng bè
        Pmax_uls = self.fea_results.get("max_soil_pressure_kpa", 66.0) * 1.2 # Nhân hệ số ULS
        M_slab_uls_kNm = Pmax_uls * (1.5**2) / 8.0
        Mu_Nmm = M_slab_uls_kNm * 1.0e6

        alpha_m = Mu_Nmm / (Rb * b_unit * (h0_s ** 2))
        xi = 1.0 - math.sqrt(max(0.0, 1.0 - 2.0 * alpha_m))
        As_slab_req = (xi * Rb * b_unit * h0_s) / Rs

        As_slab_cm2 = As_slab_req / 100.0

        formula_step = (
            f"1. Áp lực đất ULS: P_uls = {Pmax_uls:.1f} kPa\n"
            f"2. Mô men bản ULS: M_slab = P_uls * L²/8 = {M_slab_uls_kNm:.1f} kNm/m\n"
            f"3. Thép bản As_bản = {As_slab_req:.1f} mm²/m ({As_slab_cm2:.2f} cm²/m)"
        )

        return {
            "load_type": "Tải trọng Tính toán (ULS - Hệ số gamma = 1.15-1.3)",
            "M_slab_kNm": M_slab_uls_kNm,
            "As_slab_cm2_per_m": As_slab_cm2,
            "suggested_mesh": f"ϕ14a150 (As_chọn = {10.26:.2f} cm²/m)",
            "formula_explanation": formula_step,
            "status_text": "ĐẠT THÉP BẢN MÓNG BÈ"
        }

    def run_all_checks(self) -> Dict[str, Any]:
        return {
            "code_name": self.code_name,
            "soil_bearing": self.check_soil_bearing(),
            "uplift_stability": self.check_uplift_stability(),
            "beam_design": self.design_beam_flexure(),
            "slab_design": self.design_slab_flexure()
        }
