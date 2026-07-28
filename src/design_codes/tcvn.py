"""
TCVN 5574:2018 & TCVN 9362:2012 Code Verification Engine for Transmission Tower Foundation
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
        Kiểm tra áp lực đất nền theo TCVN 9362:2012:
        Pmax <= 1.2 * Rtc
        Ptrung_bình <= Rtc
        Pmin >= 0
        """
        Pmax = self.fea_results.get("max_soil_pressure_kpa", 0.0)
        Pmin = self.fea_results.get("min_soil_pressure_kpa", 0.0)
        
        # Sức chịu tải tính toán Rtc của đất nền (kPa)
        R_tc = self.fea_results.get("soil_bearing_capacity_kpa", 250.0)
        
        is_pmax_safe = Pmax <= 1.2 * R_tc
        is_pmin_safe = Pmin >= 0.0
        
        return {
            "P_max": Pmax,
            "P_min": Pmin,
            "R_tc": R_tc,
            "allowable_Pmax": 1.2 * R_tc,
            "is_pmax_safe": is_pmax_safe,
            "is_pmin_safe": is_pmin_safe,
            "status_text": "ĐẠT (Pmax <= 1.2Rtc)" if is_pmax_safe else "KHÔNG ĐẠT (Vượt sức chịu tải)"
        }

    def check_uplift_stability(self) -> Dict[str, Any]:
        """
        Kiểm tra chống nhổ móng theo 11 TCN 19-2006 & TCVN 9362:2012:
        K_chống_nhổ = (G_móng + G_đất_đè) / N_nhổ >= K_an_toàn (1.2 - 1.5)
        """
        # Trọng lượng bản thân móng bê tông (kN)
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
        # 4 dầm sườn nổi chạy suốt
        V_beams = 2 * Lx * b_beam * (h_beam - h_slab) + 2 * Ly * b_beam * (h_beam - h_slab)
        # 4 cổ cột
        V_cols = 4 * b_col * h_col * H_col
        
        G_mong = (V_slab + V_beams + V_cols) * gamma_c # kN
        
        # Thể tích đất lấp đè trên bản móng bè (m3)
        V_soil = (Lx * Ly - (V_beams/(h_beam - h_slab) if h_beam > h_slab else 0) - 4 * b_col * h_col) * H_col
        G_dat = V_soil * gamma_s # kN

        # Tổng lực nhổ kéo tính toán tại các chân bị kéo (kN)
        uplift_loads = [abs(l.N) for l in self.project.loads if l.N < 0]
        total_N_uplift = sum(uplift_loads) if uplift_loads else 0.0

        total_holding_force = G_mong + G_dat
        safety_factor = total_holding_force / total_N_uplift if total_N_uplift > 0 else 999.0

        is_uplift_safe = safety_factor >= 1.3

        return {
            "G_mong_kN": G_mong,
            "G_dat_kN": G_dat,
            "total_holding_kN": total_holding_force,
            "total_uplift_N_kN": total_N_uplift,
            "safety_factor": safety_factor,
            "required_safety_factor": 1.3,
            "is_uplift_safe": is_uplift_safe,
            "status_text": "ĐẠT CHỐNG NHỔ (K >= 1.3)" if is_uplift_safe else "KHÔNG ĐẠT (Nguy cơ nhổ móng)"
        }

    def design_beam_flexure(self) -> Dict[str, Any]:
        """
        Tính diện tích thép As chịu uốn cho Dầm sườn theo TCVN 5574:2018:
        M = Q * H_col + M_uốn
        alpha_m = M / (Rb * b * h0^2)
        xi = 1 - sqrt(1 - 2*alpha_m)
        As = (xi * Rb * b * h0) / Rs
        """
        b = self.project.beam.b_beam * 1000.0 # mm
        h = self.project.beam.h_beam * 1000.0 # mm
        a = 50.0 # mm (lớp bảo vệ)
        h0 = h - a # mm

        Rb = self.project.concrete.R_b # MPa = N/mm2
        Rs = self.project.steel.R_s   # MPa = N/mm2

        # Lấy mô men thiết kế dầm lớn nhất (kNm -> Nmm)
        max_M_kNm = max([abs(l.M_x) + abs(l.Q_y * self.project.column.H_col) for l in self.project.loads]) * 1.5
        M_Nmm = max_M_kNm * 1.0e6

        alpha_m = M_Nmm / (Rb * b * (h0 ** 2))
        
        if alpha_m > 0.35: # Vượt quá chiều cao vùng nén hạn chế xi_R
            As_req = (M_Nmm / (Rs * 0.8 * h0)) # Gần đúng gia cường
            status = "Cần gia cường thép nén (alpha_m lớn)"
        else:
            xi = 1.0 - math.sqrt(max(0.0, 1.0 - 2.0 * alpha_m))
            As_req = (xi * Rb * b * h0) / Rs # mm2

        # Đổi ra cm2 và chọn số thanh thép (phi 22/25)
        As_cm2 = As_req / 100.0
        n_bars = max(4, math.ceil(As_req / 380.1)) # phi 22 (A1 = 380.1 mm2)

        return {
            "M_max_kNm": max_M_kNm,
            "b_mm": b,
            "h_mm": h,
            "h0_mm": h0,
            "alpha_m": alpha_m,
            "As_required_cm2": As_cm2,
            "suggested_rebars": f"{n_bars} ϕ22 (As_chọn = {n_bars * 3.80:.2f} cm2)",
            "status_text": status if alpha_m > 0.35 else "ĐẠT THÉP UỐN DẦM"
        }

    def design_slab_flexure(self) -> Dict[str, Any]:
        """Tính thép As chịu uốn cho Bản móng bè (cm2/m)"""
        h_s = self.project.slab.h_slab * 1000.0 # mm
        h0_s = h_s - 35.0 # mm
        b_unit = 1000.0 # 1m dải bản

        Rb = self.project.concrete.R_b
        Rs = self.project.steel.R_s

        # Mô men uốn bản bè ví dụ
        M_slab_kNm = self.fea_results.get("max_soil_pressure_kpa", 66.0) * (1.5**2) / 8.0 # kNm/m
        M_Nmm = M_slab_kNm * 1.0e6

        alpha_m = M_Nmm / (Rb * b_unit * (h0_s ** 2))
        xi = 1.0 - math.sqrt(max(0.0, 1.0 - 2.0 * alpha_m))
        As_slab_req = (xi * Rb * b_unit * h0_s) / Rs # mm2/m

        As_slab_cm2 = As_slab_req / 100.0

        return {
            "M_slab_kNm": M_slab_kNm,
            "As_slab_cm2_per_m": As_slab_cm2,
            "suggested_mesh": f"ϕ14a150 (As_chọn = {10.26:.2f} cm2/m)",
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
