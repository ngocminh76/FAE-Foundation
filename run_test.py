"""
Full Automated Calculation & Multi-Standard Verification Test Script for Transmission Tower Foundation
"""

import sys
import os

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from src.core.presets import create_sample_project, PRESET_LOAD_CASES
from src.fea.opensees_tower import TowerFoundationFEASolver
from src.design_codes.manager import CodeCheckerManager

def main():
    print("=" * 80)
    print("⚡ TÍNH TOÁN PHÂN TÍCH MÓNG BÈ CỘT ĐIỆN MULTI-STANDARD (TCVN / ACI 318 / EUROCODE) ⚡")
    print("=" * 80)

    # 1. Khởi tạo dự án mẫu: Gió xiên 45 độ (3 Chân Nhổ, 1 Chân Nén Dồn)
    project = create_sample_project("Tải tháp 220kV - Gió Xiên 45° (3 Chân Nhổ, 1 Chân Nén Dồn)")
    
    print(f"📌 Tên dự án: {project.name}")
    print(f"📐 Kích thước móng bè: Lx = {project.slab.L_x}m, Ly = {project.slab.L_y}m, h_slab = {project.slab.h_slab}m")
    print(f"🧱 Dầm sườn nổi 2 phương: b = {project.beam.b_beam}m, h = {project.beam.h_beam}m")
    print(f"🏛️ 4 Cổ cột: lcx = {project.column.spacing_x}m, lcy = {project.column.spacing_y}m, H_col = {project.column.H_col}m")
    print(f"🪨 Địa chất giả định: Kz = {project.soil.K_z} kN/m3, Gamma_soil = {project.soil.gamma_soil} kN/m3")
    print("-" * 80)
    print("📊 BẢNG TẢI TRỌNG GIÓ XIÊN 45° TẠI 4 ĐỈNH CỔ CỘT (CỤM BU-LÔNG NEO):")
    for load in project.loads:
        type_str = "🔴 KÉO/NHỔ" if load.N < 0 else "🔵 NÉN DỒN NẶNG"
        print(f"   ► Chân Leg {load.leg_id}: N = {load.N:7.1f} kN ({type_str}), Qx = {load.Q_x:4.1f} kN, Qy = {load.Q_y:4.1f} kN, Mx = {load.M_x:5.1f} kNm, My = {load.M_y:5.1f} kNm")

    print("-" * 80)
    print("🚀 1. ĐANG GIẢI HỆ PHƯƠNG TRÌNH PHẦN TỬ HỮU HẠN FEA...")
    solver = TowerFoundationFEASolver(project=project, mesh_size=0.5)
    fea_results = solver.run_analysis()

    print(f"\n✅ KẾT QUẢ PHÂN TÍCH CHUYỂN VỊ & NỀN ĐẤT:")
    print(f"   • Độ lún lớn nhất (Max Settlement): {fea_results.get('max_settlement_mm'):.2f} mm")
    print(f"   • Áp lực đất lớn nhất Pmax:            {fea_results.get('max_soil_pressure_kpa'):.2f} kPa")

    print("\n" + "=" * 80)
    print("📋 2. SO SÁNH TÍNH TOÁN THEO 3 TIÊU CHUẨN (MULTI-STANDARD CHECK):")
    print("=" * 80)

    code_manager = CodeCheckerManager(project, fea_results)
    multi_results = code_manager.compare_all_standards()

    for code_key, res in multi_results.items():
        print(f"\n🏛️ TIÊU CHUẨN: {res['code_name']}")
        print("-" * 60)
        
        # Áp lực đất
        sb = res['soil_bearing']
        print(f"   1. Kiểm tra Áp lực Đất Nền:  {sb['status_text']}")
        print(f"      - Áp lực Pmax: {sb.get('P_max', sb.get('P_max_kPa', 0)):.2f} kPa | Giới hạn: {sb.get('allowable_Pmax', sb.get('R_tc', sb.get('q_allowable_kPa', 0))):.2f} kPa")
        
        # Chống nhổ
        up = res['uplift_stability']
        print(f"   2. Kiểm tra Chống Nhổ Móng:  {up['status_text']}")
        print(f"      - Tổng lực giữ (Móng+Đất): {up.get('G_mong_kN', 0) + up.get('G_dat_kN', 0):.1f} kN | Tổng lực nhổ kéo: {up.get('total_uplift_N_kN', up.get('uplift_demand_1.0W_kN', 0)):.1f} kN")
        
        # Cốt thép dầm sườn
        bm = res['beam_design']
        print(f"   3. Tính Cốt Thép Dầm Sườn:    {bm['status_text']}")
        print(f"      - Mô men thiết kế M_max: {bm.get('M_max_kNm', bm.get('Mu_kNm', bm.get('M_ed_kNm', 0))):.1f} kNm")
        print(f"      - Diện tích thép yêu cầu As: {bm['As_required_cm2']:.2f} cm²  --> Bố trí: {bm['suggested_rebars']}")

        # Cốt thép bản bè
        sl = res['slab_design']
        print(f"   4. Tính Cốt Thép Bản Móng Bè: {sl['status_text']}")
        print(f"      - Diện tích thép bản yêu cầu: {sl['As_slab_cm2_per_m']:.2f} cm²/m  --> Bố trí: {sl['suggested_mesh']}")

    print("\n" + "=" * 80)
    print("🎉 HOÀN THÀNH TÍNH TOÁN VÀ KIỂM TRA ĐA TIÊU CHUẨN THÀNH CÔNG!")
    print("=" * 80)

if __name__ == "__main__":
    main()
