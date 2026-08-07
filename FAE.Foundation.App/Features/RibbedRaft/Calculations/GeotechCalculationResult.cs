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
        public double Rtc1_GW_Surface { get; set; }
        public double N01_GW_Surface { get; set; }
        public double SigmaMax1_GW_Surface { get; set; }
        public double SigmaTb1_GW_Surface { get; set; }
        public double SigmaMin1_GW_Surface { get; set; }
        public bool IsPass1_GW_Surface { get; set; }

        // Trường hợp MNN sát đáy móng
        public double Rtc1_GW_Base { get; set; }
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
        public double Rtc2_GW_Surface { get; set; }
        public double N02_GW_Surface { get; set; }
        public double SigmaMax2_GW_Surface { get; set; }
        public double SigmaTb2_GW_Surface { get; set; }
        public double SigmaMin2_GW_Surface { get; set; }
        public bool IsPass2_GW_Surface { get; set; }

        // Trường hợp MNN sát đáy móng
        public double Rtc2_GW_Base { get; set; }
        public double N02_GW_Base { get; set; }
        public double SigmaMax2_GW_Base { get; set; }
        public double SigmaTb2_GW_Base { get; set; }
        public double SigmaMin2_GW_Base { get; set; }
        public bool IsPass2_GW_Base { get; set; }

        public bool IsOverallPass => IsPass1_GW_Surface && IsPass1_GW_Base && 
                                     (!HasSandCushion || (IsPass2_GW_Surface && IsPass2_GW_Base));

        // 3. Bảng dự tính độ lún
        public List<SettlementSublayer> SettlementLayers { get; set; } = new List<SettlementSublayer>();
        public double Sigma0 { get; set; }
        public double TotalSettlement { get; set; } // in mm
        public double InfluenceDepth { get; set; } // Hc in m
    }
}
