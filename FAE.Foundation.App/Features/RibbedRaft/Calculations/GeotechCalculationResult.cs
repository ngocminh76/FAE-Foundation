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
        // 1.1 Kiểm tra dưới đáy móng
        public double Phi1 { get; set; }
        public double A1 { get; set; }
        public double B1 { get; set; }
        public double D1 { get; set; }
        public double M1 { get; set; }
        
        public double Mx_Base { get; set; }
        public double My_Base { get; set; }

        // Trường hợp MNN sát mặt đất
        public double Rtc1_GW_Surface_Raw { get; set; }
        public double Rtc1_GW_Surface { get; set; }
        public double Rtc1_GW_Surface_12 => Math.Round(Rtc1_GW_Surface_Raw * 1.2, 2);
        public double N01_GW_Surface { get; set; }
        public double SigmaMax1_GW_Surface { get; set; }
        public double SigmaTb1_GW_Surface { get; set; }
        public double SigmaMin1_GW_Surface { get; set; }
        public bool IsPass1_GW_Surface { get; set; }

        // Trường hợp MNN sát đáy móng
        public double Rtc1_GW_Base_Raw { get; set; }
        public double Rtc1_GW_Base { get; set; }
        public double Rtc1_GW_Base_12 => Math.Round(Rtc1_GW_Base_Raw * 1.2, 2);
        public double N01_GW_Base { get; set; }
        public double SigmaMax1_GW_Base { get; set; }
        public double SigmaTb1_GW_Base { get; set; }
        public double SigmaMin1_GW_Base { get; set; }
        public bool IsPass1_GW_Base { get; set; }

        // 1.2 Kiểm tra dưới đáy đệm cát (Chỉ tính khi có đệm cát)
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
        
        public double Mx_SandBase { get; set; }
        public double My_SandBase { get; set; }

        // Trường hợp MNN sát mặt đất
        public double Rtc2_GW_Surface_Raw { get; set; }
        public double Rtc2_GW_Surface { get; set; }
        public double Rtc2_GW_Surface_12 => Math.Round(Rtc2_GW_Surface_Raw * 1.2, 2);
        public double N02_GW_Surface { get; set; }
        public double SigmaMax2_GW_Surface { get; set; }
        public double SigmaTb2_GW_Surface { get; set; }
        public double SigmaMin2_GW_Surface { get; set; }
        public bool IsPass2_GW_Surface { get; set; }

        // Trường hợp MNN sát đáy móng
        public double Rtc2_GW_Base_Raw { get; set; }
        public double Rtc2_GW_Base { get; set; }
        public double Rtc2_GW_Base_12 => Math.Round(Rtc2_GW_Base_Raw * 1.2, 2);
        public double N02_GW_Base { get; set; }
        public double SigmaMax2_GW_Base { get; set; }
        public double SigmaTb2_GW_Base { get; set; }
        public double SigmaMin2_GW_Base { get; set; }
        public bool IsPass2_GW_Base { get; set; }

        public bool IsPass1_Nen_Surface => SigmaMax1_GW_Surface <= Rtc1_GW_Surface_12 && SigmaTb1_GW_Surface <= Rtc1_GW_Surface;
        public bool IsPass1_Nen_Base => SigmaMax1_GW_Base <= Rtc1_GW_Base_12 && SigmaTb1_GW_Base <= Rtc1_GW_Base;

        public bool IsOverallPass => HasSandCushion 
            ? (IsPass1_Nen_Surface && IsPass1_Nen_Base && IsPass2_GW_Surface && IsPass2_GW_Base && IsPass_Kcl && IsPass_Ktr)
            : (IsPass1_GW_Surface && IsPass1_GW_Base && IsPass_Kcl && IsPass_Ktr);

        public string SigmaMin1_GW_Surface_Text => HasSandCushion
            ? (SigmaMin1_GW_Surface >= 0 ? " > 0 (Thỏa mãn)" : " < 0 (Không thỏa mãn đất tự nhiên -> Đã dùng Đệm cát gia cố)")
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

        // 2. Kiểm tra ổn định Chống Lật (Kcl) & Chống Trượt (Ktr) móng
        public double M_Giu { get; set; }
        public double M_Lat { get; set; }
        public double K_cl { get; set; }
        public bool IsPass_Kcl => K_cl >= 1.50;

        public double F_ms { get; set; }
        public double Q_Truot { get; set; }
        public double K_tr { get; set; }
        public bool IsPass_Ktr => K_tr >= 1.30;

        // 3. Bảng dự tính độ lún
        public List<SettlementSublayer> SettlementLayers { get; set; } = new List<SettlementSublayer>();
        public double Sigma0 { get; set; }
        public double TotalSettlement { get; set; } // in mm
        public double InfluenceDepth { get; set; } // Hc in m
    }
}
