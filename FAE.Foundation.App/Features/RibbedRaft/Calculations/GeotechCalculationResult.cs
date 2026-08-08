using System;
using System.Collections.Generic;

namespace FAE.Foundation.App.Features.RibbedRaft.Calculations
{
    public class SettlementSublayer
    {
        public int Id { get; set; }
        public double Ratio2ZB { get; set; }
        public double Z { get; set; }
        public double K { get; set; }
        public double Ei { get; set; }
        public double Beta { get; set; }
        public double SigmaZi { get; set; }
        public double SumGammaHi { get; set; }
        public double Si { get; set; }
    }

    public class GeotechCalculationResult
    {
        // =========================================================
        // A. BẢNG TẢI TRỌNG TIÊU CHUẨN (CẢ 2 TỔ HỢP)
        // =========================================================
        // Tổ hợp 1 (thường là Gió 45°)
        public string TH1_Name { get; set; } = string.Empty;
        public double TH1_N { get; set; }
        public double TH1_Qx { get; set; }
        public double TH1_Qy { get; set; }
        public double TH1_Mx { get; set; }
        public double TH1_My { get; set; }
        public double TH1_Mx_Base { get; set; }
        public double TH1_My_Base { get; set; }

        // Tổ hợp 2 (thường là Gió 90°)
        public string TH2_Name { get; set; } = string.Empty;
        public double TH2_N { get; set; }
        public double TH2_Qx { get; set; }
        public double TH2_Qy { get; set; }
        public double TH2_Mx { get; set; }
        public double TH2_My { get; set; }
        public double TH2_Mx_Base { get; set; }
        public double TH2_My_Base { get; set; }

        // =========================================================
        // B. BIỆN LUẬN CHỌN TỔ HỢP CHI PHỐI (ĐỘNG)
        // =========================================================

        // -- Biện luận ứng suất nền: chọn TH có σmax LỚN HƠN --
        public double TH1_SigmaMax_Compare { get; set; }
        public double TH2_SigmaMax_Compare { get; set; }
        public bool IsStress_TH1_Governs { get; set; }  // true = TH1 chi phối ứng suất
        public string StressGoverns_Name => IsStress_TH1_Governs ? TH1_Name : TH2_Name;
        public string StressGoverns_Text => IsStress_TH1_Governs
            ? $"σmax({TH1_Name}) = {TH1_SigmaMax_Compare:F2} > σmax({TH2_Name}) = {TH2_SigmaMax_Compare:F2} T/m² → Dùng {TH1_Name} kiểm tra ứng suất nền"
            : $"σmax({TH2_Name}) = {TH2_SigmaMax_Compare:F2} > σmax({TH1_Name}) = {TH1_SigmaMax_Compare:F2} T/m² → Dùng {TH2_Name} kiểm tra ứng suất nền";

        // -- Biện luận chống lật: chọn TH có Mlật LỚN HƠN (bất lợi nhất = Kcl nhỏ nhất) --
        public double TH1_Mlat { get; set; }
        public double TH2_Mlat { get; set; }
        public bool IsOvt_TH2_Governs { get; set; }    // true = TH2 chi phối lật
        public string OvtGoverns_Name => IsOvt_TH2_Governs ? TH2_Name : TH1_Name;
        public string OvtGoverns_Text => IsOvt_TH2_Governs
            ? $"Mlật({TH2_Name}) = {TH2_Mlat:F2} > Mlật({TH1_Name}) = {TH1_Mlat:F2} T.m → Dùng {TH2_Name} kiểm tra chống lật"
            : $"Mlật({TH1_Name}) = {TH1_Mlat:F2} > Mlật({TH2_Name}) = {TH2_Mlat:F2} T.m → Dùng {TH1_Name} kiểm tra chống lật";

        // -- Biện luận chống trượt: chọn TH có Qtruot LỚN HƠN (bất lợi nhất = Ktr nhỏ nhất) --
        public double TH1_Qtruot { get; set; }
        public double TH2_Qtruot { get; set; }
        public bool IsSlide_TH2_Governs { get; set; }  // true = TH2 chi phối trượt
        public string SlideGoverns_Name => IsSlide_TH2_Governs ? TH2_Name : TH1_Name;
        public string SlideGoverns_Text => IsSlide_TH2_Governs
            ? $"Qtruốt({TH2_Name}) = {TH2_Qtruot:F2} > Qtruốt({TH1_Name}) = {TH1_Qtruot:F2} T → Dùng {TH2_Name} kiểm tra chống trượt"
            : $"Qtruốt({TH1_Name}) = {TH1_Qtruot:F2} > Qtruốt({TH2_Name}) = {TH2_Qtruot:F2} T → Dùng {TH1_Name} kiểm tra chống trượt";

        // =========================================================
        // 1.1 KIỂM TRA DƯỚI ĐÁY MÓNG (Tổ hợp ứng suất chi phối)
        // =========================================================
        public double Phi1 { get; set; }
        public double A1 { get; set; }
        public double B1 { get; set; }
        public double D1 { get; set; }

        public double Mx_Base { get; set; }   // Mô men quy đổi đáy móng - tổ hợp chi phối ứng suất
        public double My_Base { get; set; }

        // MNN sát mặt đất
        public double Rtc1_GW_Surface_Raw { get; set; }
        public double Rtc1_GW_Surface { get; set; }
        public double Rtc1_GW_Surface_12 => Math.Round(Rtc1_GW_Surface_Raw * 1.2, 2);
        public double N01_GW_Surface { get; set; }
        public double SigmaMax1_GW_Surface { get; set; }
        public double SigmaTb1_GW_Surface { get; set; }
        public double SigmaMin1_GW_Surface { get; set; }
        public bool IsPass1_GW_Surface { get; set; }

        // MNN sát đáy móng
        public double Rtc1_GW_Base_Raw { get; set; }
        public double Rtc1_GW_Base { get; set; }
        public double Rtc1_GW_Base_12 => Math.Round(Rtc1_GW_Base_Raw * 1.2, 2);
        public double N01_GW_Base { get; set; }
        public double SigmaMax1_GW_Base { get; set; }
        public double SigmaTb1_GW_Base { get; set; }
        public double SigmaMin1_GW_Base { get; set; }
        public bool IsPass1_GW_Base { get; set; }

        // =========================================================
        // 1.2 KIỂM TRA DƯỚI ĐÁY ĐỆM CÁT (Tổ hợp ứng suất chi phối)
        // =========================================================
        public bool HasSandCushion { get; set; }
        public double Phi2 { get; set; }
        public double A2 { get; set; }
        public double B2 { get; set; }
        public double D2 { get; set; }

        public double B_qu { get; set; }
        public double L_qu { get; set; }
        public double H_qu { get; set; }
        public double Wx_qu { get; set; }
        public double Wy_qu { get; set; }

        // MNN sát mặt đất
        public double Rtc2_GW_Surface_Raw { get; set; }
        public double Rtc2_GW_Surface { get; set; }
        public double Rtc2_GW_Surface_12 => Math.Round(Rtc2_GW_Surface_Raw * 1.2, 2);
        public double N02_GW_Surface { get; set; }
        public double SigmaMax2_GW_Surface { get; set; }
        public double SigmaTb2_GW_Surface { get; set; }
        public double SigmaMin2_GW_Surface { get; set; }
        public bool IsPass2_GW_Surface { get; set; }

        // MNN sát đáy móng
        public double Rtc2_GW_Base_Raw { get; set; }
        public double Rtc2_GW_Base { get; set; }
        public double Rtc2_GW_Base_12 => Math.Round(Rtc2_GW_Base_Raw * 1.2, 2);
        public double N02_GW_Base { get; set; }
        public double SigmaMax2_GW_Base { get; set; }
        public double SigmaTb2_GW_Base { get; set; }
        public double SigmaMin2_GW_Base { get; set; }
        public bool IsPass2_GW_Base { get; set; }

        // =========================================================
        // HELPER LOGIC
        // =========================================================
        public bool IsPass1_Nen_Surface => SigmaMax1_GW_Surface <= Rtc1_GW_Surface_12 && SigmaTb1_GW_Surface <= Rtc1_GW_Surface;
        public bool IsPass1_Nen_Base => SigmaMax1_GW_Base <= Rtc1_GW_Base_12 && SigmaTb1_GW_Base <= Rtc1_GW_Base;

        public bool IsOverallPass => HasSandCushion
            ? (IsPass1_Nen_Surface && IsPass1_Nen_Base && IsPass2_GW_Surface && IsPass2_GW_Base && IsPass_Kcl && IsPass_Ktr)
            : (IsPass1_GW_Surface && IsPass1_GW_Base && IsPass_Kcl && IsPass_Ktr);

        public string SigmaMin1_GW_Surface_Text => HasSandCushion
            ? (SigmaMin1_GW_Surface >= 0 ? " > 0 (Thỏa mãn)" : " < 0 (Không thỏa mãn đất tự nhiên → Đã dùng Đệm cát gia cố)")
            : (SigmaMin1_GW_Surface >= 0 ? " > 0 (Thỏa mãn)" : " < 0 (Không thỏa mãn!)");
        public string SigmaMin1_GW_Base_Text => SigmaMin1_GW_Base >= 0 ? " > 0 (Thỏa mãn)" : " < 0 (Không thỏa mãn!)";
        public string SigmaMin2_GW_Surface_Text => SigmaMin2_GW_Surface >= 0 ? " > 0 (Thỏa mãn)" : " < 0 (Không thỏa mãn!)";
        public string SigmaMin2_GW_Base_Text => SigmaMin2_GW_Base >= 0 ? " > 0 (Thỏa mãn)" : " < 0 (Không thỏa mãn!)";

        public string SigmaMin1_GW_Surface_Color => HasSandCushion
            ? (SigmaMin1_GW_Surface >= 0 ? "#16A34A" : "#D97706")
            : (SigmaMin1_GW_Surface >= 0 ? "#16A34A" : "#DC2626");
        public string SigmaMin1_GW_Base_Color => SigmaMin1_GW_Base >= 0 ? "#16A34A" : "#DC2626";
        public string SigmaMin2_GW_Surface_Color => SigmaMin2_GW_Surface >= 0 ? "#16A34A" : "#DC2626";
        public string SigmaMin2_GW_Base_Color => SigmaMin2_GW_Base >= 0 ? "#16A34A" : "#DC2626";

        // =========================================================
        // 2. KIỂM TRA CHỐNG LẬT (Kcl) & CHỐNG TRƯỢT (Ktr)
        //    (Tổ hợp lật/trượt chi phối - động)
        // =========================================================
        public double Mx_Base_Ovt { get; set; }  // Mô men quy đổi - tổ hợp chi phối lật/trượt
        public double My_Base_Ovt { get; set; }

        public double M_Giu { get; set; }
        public double M_Lat { get; set; }         // Mlật chi phối
        public double K_cl { get; set; }           // Kcl chi phối (nhỏ nhất)
        public double K_cl_Other { get; set; }     // Kcl tổ hợp còn lại
        public bool IsPass_Kcl => K_cl >= 1.50;

        public double F_ms { get; set; }
        public double Q_Truot { get; set; }        // Qtruot chi phối (lớn nhất)
        public double Q_Truot_Other { get; set; }  // Qtruot tổ hợp còn lại
        public double K_tr { get; set; }           // Ktr chi phối (nhỏ nhất)
        public double K_tr_Other { get; set; }     // Ktr tổ hợp còn lại
        public bool IsPass_Ktr => K_tr >= 1.30;

        // =========================================================
        // 3. BẢNG DỰ TÍNH ĐỘ LÚN
        // =========================================================
        public List<SettlementSublayer> SettlementLayers { get; set; } = new List<SettlementSublayer>();
        public double Sigma0 { get; set; }
        public double TotalSettlement { get; set; } // in mm
        public double InfluenceDepth { get; set; }  // Hc in m
    }
}
