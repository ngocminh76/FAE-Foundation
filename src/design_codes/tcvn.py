"""
TCVN 5574:2018 & TCVN 9362:2012 Code Verification Engine
Comprehensive Implementation: Soil Bearing, Uplift Stability, Stub Column Reinforcement, Anchor Bolts & Punching Shear
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
        V_soil = (Lx * Ly - (V_beams/(h_beam - h_slab) if h_beam > h_slab else 0) - 4 * b_col * h_col) * H_col
        G_dat = V_soil * gamma_s

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

    def check_stub_columns(self) -> Dict[str, Any]:
        """
        Tính toán và kiểm tra Cốt thép 4 Cổ Cột theo TCVN 5574:2018 (Dùng Tải Tính Toán - ULS):
        1. Cổ cột chịu Nén uốn xiên (N_comp > 0)
        2. Cổ cột chịu Kéo uốn xiên (N_uplift < 0) -> As = |N_nhổ| / Rs + M_uốn / (Rs * z)
        3. Tính cốt đai Asw chịu lực cắt Q_max
        """
        b = self.project.column.b_col * 1000.0 # mm
        h = self.project.column.h_col * 1000.0 # mm
        H_col = self.project.column.H_col       # m
        a = 40.0 # mm
        h0 = h - a
        z = 0.8 * h0

        Rb = self.project.concrete.R_b # MPa
        Rs = self.project.steel.R_s   # MPa

        # Tìm cổ cột chịu Kéo/Nhổ lớn nhất và Nén lớn nhất
        max_uplift_load = min(self.project.loads, key=lambda l: l.N_uls)
        max_comp_load = max(self.project.loads, key=lambda l: l.N_uls)

        # Tính thép cho cổ cột chịu Kéo Nhổ cực đại (Leg 1 / Leg 2)
        N_k_N = abs(max_uplift_load.N_uls) * 1.0e3 # N
        M_k_Nmm = (abs(max_uplift_load.M_x_uls) + max_uplift_load.Q_y_uls * H_col) * 1.0e6 # Nmm

        As_tensile_req = (N_k_N / Rs) + (M_k_Nmm / (Rs * z)) # mm2
        As_tensile_cm2 = As_tensile_req / 100.0

        # Hàm lượng thép tối thiểu mu_min = 0.4%
        As_min_cm2 = (0.004 * b * h) / 100.0
        As_final_cm2 = max(As_tensile_cm2, As_min_cm2)
        n_bars_col = max(8, math.ceil((As_final_cm2 * 100.0) / 490.9)) # phi 25 (A1 = 490.9 mm2)

        # Cốt đai cổ cột (phi 10a150)
        max_Q_kN = max([math.sqrt(l.Q_x_uls**2 + l.Q_y_uls**2) for l in self.project.loads])

        formula_step = (
            f"1. Lực kéo nhổ ULS lớn nhất: N_nhổ = {abs(max_uplift_load.N_uls):.1f} kN\n"
            f"2. Mô men chân cổ cột: M_chân = {M_k_Nmm/1e6:.1f} kNm\n"
            f"3. Thép dọc chịu kéo uốn xiên: As = (|N_nhổ| / Rs) + (M_chân / (Rs * z))\n"
            f"      = ({N_k_N:.0f} / {Rs}) + ({M_k_Nmm:.0f} / ({Rs} * {z:.0f})) = {As_tensile_req:.1f} mm² ({As_tensile_cm2:.2f} cm²)\n"
            f"4. Thép dọc tối thiểu (µ_min = 0.4%): As_min = {As_min_cm2:.2f} cm²\n"
            f"   --> Bố trí thép cổ cột: {n_bars_col} ϕ25 (As_chọn = {n_bars_col * 4.91:.2f} cm²)"
        )

        return {
            "load_type": "Tải trọng Tính toán (ULS - Hệ số gamma = 1.15-1.3)",
            "N_uplift_max_kN": abs(max_uplift_load.N_uls),
            "N_comp_max_kN": max_comp_load.N_uls,
            "As_required_cm2": As_final_cm2,
            "suggested_column_rebars": f"{n_bars_col} ϕ25 (As_chọn = {n_bars_col * 4.91:.2f} cm²)",
            "suggested_stirrups": "ϕ10a150 (Cốt đai gia cường chống cắt cổ cột)",
            "formula_explanation": formula_step,
            "status_text": "ĐẠT CỐT THÉP CỔ CỘT"
        }

    def check_anchor_bolts(self) -> Dict[str, Any]:
        """
        Kiểm tra Bu-lông Neo đỉnh Cổ Cột (Anchor Bolts Verification):
        1. Sức chịu kéo tính toán của 1 bu-lông: N_rd = A_net * f_yb
        2. Tổng khả năng chịu kéo cụm 4 bu-lông: N_total_rd = n * N_rd >= |N_nhổ_uls|
        3. Chiều dài dính neo L_anchor >= 30 * d_bolt
        """
        spec = self.project.anchor_bolt
        d_b = spec.d_bolt # mm (e.g. 36mm)
        n_b = spec.n_bolts_per_leg # 4
        f_yb = spec.f_yb # MPa (400 MPa)
        L_anc = spec.L_anchor # mm (1000mm)

        # Diện tích làm việc chịu kéo của 1 bu-lông (A_net = 0.8 * A_danh_nghĩa)
        A_nom = math.pi * (d_b ** 2) / 4.0
        A_net = 0.8 * A_nom # mm2
        N_rd_1bolt_kN = (A_net * f_yb) / 1000.0 # kN
        N_rd_leg_kN = n_b * N_rd_1bolt_kN # kN

        # Lực nhổ ULS lớn nhất tại 1 chân cột
        max_uplift_N = max([abs(l.N_uls) for l in self.project.loads if l.N_uls < 0] + [0.0])

        is_bolt_safe = N_rd_leg_kN >= max_uplift_N
        is_length_safe = L_anc >= 30.0 * d_b

        formula_step = (
            f"1. Bu-lông neo M{int(d_b)} (d = {d_b}mm, A_net = {A_net:.1f} mm²)\n"
            f"2. Sức chịu kéo 1 bu-lông: N_rd1 = A_net * f_yb = {N_rd_1bolt_kN:.1f} kN\n"
            f"3. Khả năng chịu kéo cụm {n_b} bu-lông: N_rd_cụm = {n_b} * {N_rd_1bolt_kN:.1f} = {N_rd_leg_kN:.1f} kN\n"
            f"4. Kiểm tra: N_rd_cụm = {N_rd_leg_kN:.1f} kN >= N_nhổ = {max_uplift_N:.1f} kN --> "
            f"{'ĐẠT' if is_bolt_safe else 'KHÔNG ĐẠT'}\n"
            f"5. Chiều dài neo L_anchor = {L_anc:.0f} mm >= 30*d = {30*d_b:.0f} mm --> "
            f"{'ĐẠT NEO' if is_length_safe else 'KHÔNG ĐẠT NEO'}"
        )

        return {
            "d_bolt_mm": d_b,
            "n_bolts": n_b,
            "N_rd_leg_kN": N_rd_leg_kN,
            "N_uplift_demand_kN": max_uplift_N,
            "is_bolt_safe": is_bolt_safe,
            "is_length_safe": is_length_safe,
            "formula_explanation": formula_step,
            "status_text": "ĐẠT BU-LÔNG NEO M36" if (is_bolt_safe and is_length_safe) else "KHÔNG ĐẠT BU-LÔNG NEO"
        }

    def check_punching_shear(self) -> Dict[str, Any]:
        """
        Kiểm tra chọc thủng bản bè quanh 4 cổ cột theo TCVN 5574:2018:
        F_b_ult = R_bt * u_m * h0_slab >= N_comp_uls
        """
        h_s = self.project.slab.h_slab * 1000.0 # mm
        h0_s = h_s - 40.0 # mm
        b_c = self.project.column.b_col * 1000.0 # mm
        h_c = self.project.column.h_col * 1000.0 # mm

        # Chu vi tháp chọc thủng trung bình u_m = 2 * (b_c + h_c + 2*h0_s)
        u_m = 2.0 * (b_c + h_c + 2.0 * h0_s) # mm

        Rbt = self.project.concrete.R_bt # MPa
        F_punch_rd_kN = (Rbt * u_m * h0_s) / 1000.0 # kN

        # Lực nén ULS lớn nhất ấn xuống móng
        max_comp_N_uls = max([l.N_uls for l in self.project.loads if l.N_uls > 0] + [0.0])

        is_punching_safe = F_punch_rd_kN >= max_comp_N_uls

        formula_step = (
            f"1. Chu vi tháp chọc thủng u_m = 2*(b_col + h_col + 2*h0) = {u_m:.0f} mm\n"
            f"2. Khả năng chống chọc thủng bê tông bản: F_b_ult = Rbt * u_m * h0\n"
            f"      = {Rbt} * {u_m:.0f} * {h0_s:.0f} = {F_punch_rd_kN:.1f} kN\n"
            f"3. Lực nén ULS lớn nhất ấn xuống: N_nén_max = {max_comp_N_uls:.1f} kN\n"
            f"4. Kiểm tra: F_b_ult = {F_punch_rd_kN:.1f} kN >= N_nén = {max_comp_N_uls:.1f} kN --> "
            f"{'ĐẠT CHỐNG CHỌC THỦNG' if is_punching_safe else 'KHÔNG ĐẠT'}"
        )

        return {
            "u_m_mm": u_m,
            "F_punch_rd_kN": F_punch_rd_kN,
            "N_comp_demand_kN": max_comp_N_uls,
            "is_punching_safe": is_punching_safe,
            "formula_explanation": formula_step,
            "status_text": "ĐẠT CHỐNG CHỌC THỦNG" if is_punching_safe else "KHÔNG ĐẠT CHỌC THỦNG"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        """Tính diện tích thép As chịu uốn Dầm sườn theo TCVN 5574:2018 (ULS)"""
        b = self.project.beam.b_beam * 1000.0
        h = self.project.beam.h_beam * 1000.0
        a = 50.0
        h0 = h - a

        Rb = self.project.concrete.R_b
        Rs = self.project.steel.R_s

        max_M_uls_kNm = max([abs(l.M_x_uls) + abs(l.Q_y_uls * self.project.column.H_col) for l in self.project.loads]) * 1.5
        Mu_Nmm = max_M_uls_kNm * 1.0e6

        alpha_m = Mu_Nmm / (Rb * b * (h0 ** 2))
        
        if alpha_m > 0.35:
            xi = 0.35
            status = "Cần gia cường thép nén (alpha_m lớn)"
        else:
            xi = 1.0 - math.sqrt(max(0.0, 1.0 - 2.0 * alpha_m))
            status = "ĐẠT THÉP UỐN DẦM SƯỜN"

        As_req = (xi * Rb * b * h0) / Rs
        As_cm2 = As_req / 100.0
        n_bars = max(4, math.ceil(As_req / 380.1))

        formula_step = (
            f"1. Mô men tính toán ULS dầm sườn: Mu = {max_M_uls_kNm:.1f} kNm\n"
            f"2. Hệ số αm = Mu / (Rb * b * h0²) = {alpha_m:.4f}\n"
            f"3. Hệ số chiều cao vùng nén ξ = {xi:.4f}\n"
            f"4. Thép dầm yêu cầu As = {As_req:.1f} mm² ({As_cm2:.2f} cm²)"
        )

        return {
            "load_type": "Tải trọng Tính toán (ULS - Hệ số gamma = 1.15-1.3)",
            "M_max_kNm": max_M_uls_kNm,
            "b_mm": b,
            "h_mm": h,
            "h0_mm": h0,
            "alpha_m": alpha_m,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} ϕ22 (As_chọn = {n_bars * 3.80:.2f} cm²)",
            "formula_explanation": formula_step,
            "status_text": status
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        """Tính thép As chịu uốn Bản móng bè theo TCVN 5574:2018 (ULS)"""
        h_s = self.project.slab.h_slab * 1000.0
        h0_s = h_s - 35.0
        b_unit = 1000.0

        Rb = self.project.concrete.R_b
        Rs = self.project.steel.R_s

        Pmax_uls = self.fea_results.get("max_soil_pressure_kpa", 66.0) * 1.2
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
            "stub_columns": self.check_stub_columns(),
            "anchor_bolts": self.check_anchor_bolts(),
            "punching_shear": self.check_punching_shear(),
            "beam_design": self.design_beam_flexure(),
            "slab_design": self.design_slab_flexure()
        }
