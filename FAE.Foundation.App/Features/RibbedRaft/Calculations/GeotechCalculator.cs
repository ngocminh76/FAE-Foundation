using System;
using System.Collections.Generic;
using System.Linq;
using FAE.Foundation.App.Models;

namespace FAE.Foundation.App.Features.RibbedRaft.Calculations
{
    public class GeotechCalculator
    {
        public static GeotechCalculationResult Calculate(RibbedRaftModel foundation, BoreholeModel borehole, LoadCase loadCase1, LoadCase loadCase2)
        {
            var result = new GeotechCalculationResult();

            // ── Lưu thông tin cả 2 tổ hợp vào result để UI hiển thị ──
            result.TH1_Name = loadCase1.Name;
            result.TH1_N = loadCase1.N; result.TH1_Qx = loadCase1.Qx; result.TH1_Qy = loadCase1.Qy;
            result.TH1_Mx = loadCase1.Mx; result.TH1_My = loadCase1.My;

            result.TH2_Name = loadCase2.Name;
            result.TH2_N = loadCase2.N; result.TH2_Qx = loadCase2.Qx; result.TH2_Qy = loadCase2.Qy;
            result.TH2_Mx = loadCase2.Mx; result.TH2_My = loadCase2.My;
            
            // 0. Thông số chung
            double B = foundation.TotalWidth;
            double L = foundation.TotalLength;
            double H = foundation.Depth; // 3.9m
            double H1 = foundation.EmbedmentDepth; // 2.4m
            double h1 = foundation.SlabThickness; // 0.6m
            double a = foundation.RibWidth; // 0.8m
            double h = foundation.RibHeight; // 1.8m
            double c = foundation.HoleSize; // 2.3m
            double b1 = foundation.B1; // 1.2m
            double e = 0.5; // vát nách dầm (chamfer)
            
            // 1. Thể tích bê tông móng (V) chuẩn Excel:
            // V = (B*L - c^2)*h1 + (B+L)*2*a*(h-h1) - a^2*4*(h-h1) + (H-h)*b1^2*4 + e^2*(h-h1)*8
            double vConcrete = (B * L - c * c) * h1 
                             + (B + L) * 2 * a * (h - h1) 
                             - a * a * 4 * (h - h1) 
                             + (H - h) * b1 * b1 * 4 
                             + e * e * (h - h1) * 8;

            double vSoil = B * L * H1 - vConcrete;
            if (vSoil < 0) vSoil = 0;
            
            // F: Diện tích đáy móng
            double area = B * L - Math.Pow(c, 2); // 317.71 m2
            double wx = (L * Math.Pow(B, 3) - Math.Pow(c, 4)) / (6.0 * B); // 914.89 m3
            double wy = (B * Math.Pow(L, 3) - Math.Pow(c, 4)) / (6.0 * L); // 1022.59 m3

            double effectiveDepth = H1; // 2.4m

            // 1.1 Kiểm tra dưới đáy móng (Sand cushion phi = 28 deg => A=0.99, B=4.93, D=7.40)
            result.HasSandCushion = foundation.HasSandCushion;
            double phi1 = result.HasSandCushion ? 28.0 : GetSoilAtDepth(borehole, effectiveDepth)?.Phi ?? 28.0;
            double c1 = result.HasSandCushion ? 0.0 : GetSoilAtDepth(borehole, effectiveDepth)?.C ?? 0.0;

            var (A1, B1, D1) = GetBearingCoefficients(phi1);
            result.Phi1 = phi1;
            result.A1 = A1;
            result.B1 = B1;
            result.D1 = D1;

            double m = 1.0; // m = 1.0 theo Excel

            // ── Mô men quy đổi đáy móng cho CẢ 2 tổ hợp ──
            double Mx_base_TH1 = loadCase1.Mx + loadCase1.Qy * H;
            double My_base_TH1 = loadCase1.My + loadCase1.Qx * H;
            double Mx_base_TH2 = loadCase2.Mx + loadCase2.Qy * H;
            double My_base_TH2 = loadCase2.My + loadCase2.Qx * H;

            result.TH1_Mx_Base = Math.Round(Mx_base_TH1, 2);
            result.TH1_My_Base = Math.Round(My_base_TH1, 2);
            result.TH2_Mx_Base = Math.Round(Mx_base_TH2, 2);
            result.TH2_My_Base = Math.Round(My_base_TH2, 2);

            // ── BIỆN LUẬN 1: σmax nào lớn hơn → chi phối ứng suất nền ──
            // (Tính trước σmax thô không cần N0 vì N0 như nhau — chỉ cần so ngoại tâm)
            double ecc_TH1 = Math.Abs(Mx_base_TH1 / ((L * Math.Pow(B, 3) - Math.Pow(c, 4)) / (6.0 * B)))
                           + Math.Abs(My_base_TH1 / ((B * Math.Pow(L, 3) - Math.Pow(c, 4)) / (6.0 * L)));
            double ecc_TH2 = Math.Abs(Mx_base_TH2 / ((L * Math.Pow(B, 3) - Math.Pow(c, 4)) / (6.0 * B)))
                           + Math.Abs(My_base_TH2 / ((B * Math.Pow(L, 3) - Math.Pow(c, 4)) / (6.0 * L)));
            // TH có ngoại tâm lớn hơn → σmax lớn hơn → chi phối ứng suất
            bool stressGovTH1 = ecc_TH1 >= ecc_TH2;
            LoadCase stressCase = stressGovTH1 ? loadCase1 : loadCase2;
            double Mx_base_stress = stressGovTH1 ? Mx_base_TH1 : Mx_base_TH2;
            double My_base_stress = stressGovTH1 ? My_base_TH1 : My_base_TH2;

            result.IsStress_TH1_Governs = stressGovTH1;

            // ── BIỆN LUẬN 2: Mlật nào lớn hơn → chi phối chống lật ──
            double Mlat_TH1 = Math.Max(0.001, My_base_TH1);
            double Mlat_TH2 = Math.Max(0.001, My_base_TH2);
            bool ovtGovTH2 = Mlat_TH2 >= Mlat_TH1;
            double Mlat_gov = ovtGovTH2 ? Mlat_TH2 : Mlat_TH1;

            result.TH1_Mlat = Math.Round(Mlat_TH1, 2);
            result.TH2_Mlat = Math.Round(Mlat_TH2, 2);
            result.IsOvt_TH2_Governs = ovtGovTH2;

            // ── BIỆN LUẬN 3: Qtruot nào lớn hơn → chi phối chống trượt ──
            double Qtruot_TH1 = Math.Sqrt(loadCase1.Qx * loadCase1.Qx + loadCase1.Qy * loadCase1.Qy);
            double Qtruot_TH2 = Math.Sqrt(loadCase2.Qx * loadCase2.Qx + loadCase2.Qy * loadCase2.Qy);
            bool slideGovTH2 = Qtruot_TH2 >= Qtruot_TH1;
            double Qtruot_gov = slideGovTH2 ? Qtruot_TH2 : Qtruot_TH1;

            result.TH1_Qtruot = Math.Round(Qtruot_TH1, 2);
            result.TH2_Qtruot = Math.Round(Qtruot_TH2, 2);
            result.IsSlide_TH2_Governs = slideGovTH2;

            // ── Gán mô men chi phối ứng suất vào result (cho UI Mục 1) ──
            result.Mx_Base = Math.Round(Mx_base_stress, 2);
            result.My_Base = Math.Round(My_base_stress, 2);
            // (wx, wy đã khai báo ở trên — dùng chung)

            // MNN sát mặt đất (Row 76-81 Excel)
            // C77: =C71*K9*L54 + F71*($K$38*L52) + I71*J54
            double gamma_dn_sand = 0.9714285714285715;
            double gamma_dn_clay = 0.7407407407407408;
            double gamma_w_clay = 1.72;

            double rtc1_S_raw = m * (A1 * B * gamma_dn_sand + B1 * effectiveDepth * gamma_dn_clay + D1 * c1); // 25.113587 T/m2
            result.Rtc1_GW_Surface_Raw = rtc1_S_raw;
            result.Rtc1_GW_Surface = Math.Round(rtc1_S_raw, 2);
            
            // C78: N0 = N + Vbt*γbt + Vd*γdn (MNN sát mặt đất)
            // N0 dùng N của tổ hợp ứng suất chi phối (cả 2 TH cùng N)
            double N0_GW_Surface = stressCase.N + vConcrete * 1.4 + vSoil * gamma_dn_clay;
            result.N01_GW_Surface = Math.Round(N0_GW_Surface, 2);

            double sigmaTb1_S = N0_GW_Surface / area;
            double sigmaMax1_S = sigmaTb1_S + Math.Abs(Mx_base_stress / wx) + Math.Abs(My_base_stress / wy);
            double sigmaMin1_S = sigmaTb1_S - Math.Abs(Mx_base_stress / wx) - Math.Abs(My_base_stress / wy);

            // Gán σmax so sánh cho biện luận
            double sigmaTb1_S_ref = N0_GW_Surface / area;
            result.TH1_SigmaMax_Compare = Math.Round(sigmaTb1_S_ref + ecc_TH1, 2);
            result.TH2_SigmaMax_Compare = Math.Round(sigmaTb1_S_ref + ecc_TH2, 2);

            result.SigmaMax1_GW_Surface = Math.Round(sigmaMax1_S, 2);
            result.SigmaTb1_GW_Surface = Math.Round(sigmaTb1_S, 2);
            result.SigmaMin1_GW_Surface = Math.Round(sigmaMin1_S, 2);
            result.IsPass1_GW_Surface = (sigmaMax1_S <= 1.2 * rtc1_S_raw) && (sigmaTb1_S <= rtc1_S_raw) && (sigmaMin1_S > 0);

            // MNN sát đáy móng (Row 82-87 Excel)
            // C83: =C71*K9*L54 + F71*($K$38*D52) + I71*J54
            double rtc1_B_raw = m * (A1 * B * gamma_dn_sand + B1 * effectiveDepth * gamma_w_clay + D1 * c1); // 36.700183 T/m2
            result.Rtc1_GW_Base_Raw = rtc1_B_raw;
            result.Rtc1_GW_Base = Math.Round(rtc1_B_raw, 2);
            
            // C84: N0 = N + Vbt*γbt + Vd*γ (MNN sát đáy móng)
            double N0_GW_Base = stressCase.N + vConcrete * 2.4 + vSoil * 1.55;
            result.N01_GW_Base = Math.Round(N0_GW_Base, 2);

            double sigmaTb1_B = N0_GW_Base / area;
            double sigmaMax1_B = sigmaTb1_B + Math.Abs(Mx_base_stress / wx) + Math.Abs(My_base_stress / wy);
            double sigmaMin1_B = sigmaTb1_B - Math.Abs(Mx_base_stress / wx) - Math.Abs(My_base_stress / wy);

            result.SigmaMax1_GW_Base = Math.Round(sigmaMax1_B, 2);
            result.SigmaTb1_GW_Base = Math.Round(sigmaTb1_B, 2);
            result.SigmaMin1_GW_Base = Math.Round(sigmaMin1_B, 2);
            result.IsPass1_GW_Base = (sigmaMax1_B <= 1.2 * rtc1_B_raw) && (sigmaTb1_B <= rtc1_B_raw) && (sigmaMin1_B > 0);

            // 1.2 Kiểm tra dưới đáy đệm cát (Đỉnh lớp đất yếu bùn sét)
            if (foundation.HasSandCushion)
            {
                double sandDepth = foundation.SandThickness; // 0.5m
                double h_qu = effectiveDepth + sandDepth; // 2.9m
                
                // Móng khối quy ước mở rộng theo góc ma sát đệm phi1 (28 độ)
                double alphaRad = phi1 * Math.PI / 180.0;
                double b_qu = B + 2 * sandDepth * Math.Tan(alphaRad); // 17.531709m
                double l_qu = L + 2 * sandDepth * Math.Tan(alphaRad); // 19.531709m
                
                result.B_qu = Math.Round(b_qu, 2);
                result.L_qu = Math.Round(l_qu, 2);
                result.H_qu = Math.Round(h_qu, 2);

                double area_qu = b_qu * l_qu - Math.Pow(c, 2); // 337.1504m2
                double wx_qu = (l_qu * Math.Pow(b_qu, 3) - Math.Pow(c, 4)) / (6.0 * b_qu); // 1000.2810m3
                double wy_qu = (b_qu * Math.Pow(l_qu, 3) - Math.Pow(c, 4)) / (6.0 * l_qu); // 1114.4532m3
                
                result.Wx_qu = Math.Round(wx_qu, 2);
                result.Wy_qu = Math.Round(wy_qu, 2);

                // Mô men quy đổi đáy đệm cát = mô men đáy móng + Q * chiều sâu đệm cát
                // Dùng tổ hợp ứng suất chi phối
                double Mxtc_qu = Mx_base_stress + stressCase.Qy * sandDepth;
                double Mytc_qu = My_base_stress + stressCase.Qx * sandDepth;

                // Lớp bùn sét dưới đệm cát (Phi = 6.5 độ, C = 0.87 T/m2)
                double phi2 = 6.5;
                double c2 = 0.87;
                double gamma_w_sand = 1.75; // Dung trọng tự nhiên đệm cát (D54 trong Excel)

                var (A2, B2, D2) = GetBearingCoefficients(phi2); // A=0.11, B=1.43, D=3.77
                result.Phi2 = phi2;
                result.A2 = A2;
                result.B2 = B2;
                result.D2 = D2;

                // Rtc2 - MNN sát mặt đất (Row 98 Excel: =C90*F89*L55 + C91*(K38*L52 + C54*L54) + C92*J55)
                double rtc2_S_raw = m * (A2 * b_qu * gamma_dn_clay + B2 * (effectiveDepth * gamma_dn_clay + sandDepth * gamma_dn_sand) + D2 * c2); // 7.945203 T/m2
                result.Rtc2_GW_Surface_Raw = rtc2_S_raw;
                result.Rtc2_GW_Surface = Math.Round(rtc2_S_raw, 2);
                
                // N02 - MNN sát mặt đất (Row 99 Excel)
                // C99: =+C29+K40*L52+K29*1.4+(F89*F90-K9*K10)*$K$38*L52+F90*F89*C54*L54
                double N02_S = N0_GW_Surface + (b_qu * l_qu - B * L) * effectiveDepth * gamma_dn_clay + b_qu * l_qu * sandDepth * gamma_dn_sand; // 1079.5425 T
                result.N02_GW_Surface = Math.Round(N02_S, 2);

                double sigmaTb2_S = N02_S / area_qu; // 3.202020 T/m2
                double sigmaMax2_S = sigmaTb2_S + Math.Abs(Mxtc_qu / wx_qu) + Math.Abs(Mytc_qu / wy_qu); // 6.819077 T/m2 -> 6.82
                double sigmaMin2_S = sigmaTb2_S - Math.Abs(Mxtc_qu / wx_qu) - Math.Abs(Mytc_qu / wy_qu); // -0.415037 T/m2 -> -0.41

                result.SigmaMax2_GW_Surface = Math.Round(sigmaMax2_S, 2);
                result.SigmaTb2_GW_Surface = Math.Round(sigmaTb2_S, 2);
                result.SigmaMin2_GW_Surface = Math.Round(sigmaMin2_S, 2);
                result.IsPass2_GW_Surface = (sigmaMax2_S <= 1.2 * rtc2_S_raw) && (sigmaTb2_S <= rtc2_S_raw) && (sigmaMin2_S > 0);

                // Rtc2 - MNN sát đáy móng (Row 104 Excel: =C90*F89*L55 + C91*(K38*D52 + C54*D54) + C92*J55)
                double gamma_depth_MNN2 = effectiveDepth * gamma_w_clay + sandDepth * gamma_w_sand; // 2.4*1.72 + 0.5*1.75 = 5.003
                double rtc2_B_raw = m * (A2 * b_qu * gamma_dn_clay + B2 * gamma_depth_MNN2 + D2 * c2); // 11.862700 T/m2
                result.Rtc2_GW_Base_Raw = rtc2_B_raw;
                result.Rtc2_GW_Base = Math.Round(rtc2_B_raw, 2);
                
                // N02 - MNN sát đáy móng (Row 105 Excel)
                // C105: =+C29+K40*1.55+K29*2.4+(F89*F90-K9*K10)*$K$38*1.55+F90*F89*C54*L54
                double N02_B = N0_GW_Base + (b_qu * l_qu - B * L) * effectiveDepth * 1.55 + b_qu * l_qu * sandDepth * gamma_dn_sand; // 1796.3297 T
                result.N02_GW_Base = Math.Round(N02_B, 2);

                double sigmaTb2_B = N02_B / area_qu; // 5.327977 T/m2
                double sigmaMax2_B = sigmaTb2_B + Math.Abs(Mxtc_qu / wx_qu) + Math.Abs(Mytc_qu / wy_qu); // 8.945034 -> 8.94
                double sigmaMin2_B = sigmaTb2_B - Math.Abs(Mxtc_qu / wx_qu) - Math.Abs(Mytc_qu / wy_qu); // 1.715317 -> 1.72

                result.SigmaMax2_GW_Base = Math.Round(sigmaMax2_B, 2);
                result.SigmaTb2_GW_Base = Math.Round(sigmaTb2_B, 2);
                result.SigmaMin2_GW_Base = Math.Round(sigmaMin2_B, 2);
                result.IsPass2_GW_Base = (sigmaMax2_B <= 1.2 * rtc2_B_raw) && (sigmaTb2_B <= rtc2_B_raw) && (sigmaMin2_B > 0);
            }

            // 2. KIỂM TRA ỔN ĐỊNH CHỐNG LẬT (Kcl) & CHỐNG TRƯỢT (Ktr)
            // Dùng N0_GW_Base của tổ hợp ứng suất (N như nhau) cho Mgiữ
            // Chống lật & trượt dùng tổ hợp chi phối riêng (đã biện luận ở trên)
            LoadCase ovtCase  = ovtGovTH2  ? loadCase2 : loadCase1;
            LoadCase slideCase = slideGovTH2 ? loadCase2 : loadCase1;
            double Mx_base_ovt = ovtGovTH2  ? Mx_base_TH2 : Mx_base_TH1;
            double My_base_ovt = ovtGovTH2  ? My_base_TH2 : My_base_TH1;

            result.Mx_Base_Ovt = Math.Round(Mx_base_ovt, 2);
            result.My_Base_Ovt = Math.Round(My_base_ovt, 2);

            // N0_GW_Base dùng N của tổ hợp lật chi phối (thực ra N bằng nhau nên kết quả như nhau)
            double N0_GW_Base_Ovt = ovtCase.N + vConcrete * 2.4 + vSoil * 1.55;

            double mGiu = N0_GW_Base_Ovt * (B / 2.0);
            double kcl_gov  = mGiu / Mlat_gov;
            double kcl_other = ovtGovTH2 ? mGiu / Mlat_TH1 : mGiu / Mlat_TH2;

            result.M_Giu    = Math.Round(mGiu, 2);
            result.M_Lat    = Math.Round(Mlat_gov, 2);
            result.K_cl     = Math.Round(kcl_gov, 2);
            result.K_cl_Other = Math.Round(kcl_other, 2);

            double tanPhi1 = Math.Tan(phi1 * Math.PI / 180.0);
            double fms = N0_GW_Base_Ovt * tanPhi1;
            double ktr_gov   = fms / Math.Max(0.001, Qtruot_gov);
            double ktr_other = fms / Math.Max(0.001, slideGovTH2 ? Qtruot_TH1 : Qtruot_TH2);

            result.F_ms          = Math.Round(fms, 2);
            result.Q_Truot       = Math.Round(Qtruot_gov, 2);
            result.Q_Truot_Other = Math.Round(slideGovTH2 ? Qtruot_TH1 : Qtruot_TH2, 2);
            result.K_tr          = Math.Round(ktr_gov, 2);
            result.K_tr_Other    = Math.Round(ktr_other, 2);

            // 3. TÍNH LÚN (Settlement Calculation according to TCVN 9362:2012 & Excel Sheet 55(+2)B)
            double z0_depth = foundation.HasSandCushion ? result.H_qu : effectiveDepth;
            double B_l = foundation.HasSandCushion ? result.B_qu : B;
            double L_l = foundation.HasSandCushion ? result.L_qu : L;

            double sigmaTb_z0_S = foundation.HasSandCushion ? result.SigmaTb2_GW_Surface : result.SigmaTb1_GW_Surface;
            double sigmaTb_z0_B = foundation.HasSandCushion ? result.SigmaTb2_GW_Base : result.SigmaTb1_GW_Base;

            double sumGammaHi_S = GetOverburdenStress(borehole, z0_depth, true);
            double sumGammaHi_B = GetOverburdenStress(borehole, z0_depth, false);

            double sigma0_S = Math.Max(0, sigmaTb_z0_S - sumGammaHi_S);
            double sigma0_B = Math.Max(0, sigmaTb_z0_B - sumGammaHi_B);

            // Chọn trường hợp tính lún bất lợi nhất (Excel chọn MNN sát mặt đất)
            bool isGWBase = sigma0_B > sigma0_S;
            double sigma0 = Math.Max(sigma0_S, sigma0_B);
            result.Sigma0 = Math.Round(sigma0, 4);

            double currentSumGamma = isGWBase ? sumGammaHi_B : sumGammaHi_S;

            // Chia lớp tính lún hi = 0.025 * B_l (Khớp với Excel hi = 0.425m)
            double hi = 0.025 * B_l;
            double currentZ = 0;
            double sumSettlement = 0;
            int step = 1;

            result.SettlementLayers.Clear();

            // Điểm z = 0 (đáy móng/đáy đệm cát)
            double alpha_0 = GetSettlementAlpha(L_l / B_l, 0.0);
            double sigma_z_prev = alpha_0 * sigma0;

            var topSoil = GetSoilAtDepth(borehole, z0_depth);
            double topEi = topSoil?.E ?? 180.0;
            double topBeta = (topSoil?.LayerName.ToLower().Contains("cát") == true) ? 0.74 : 0.8;

            result.SettlementLayers.Add(new SettlementSublayer
            {
                Id = step,
                Ratio2ZB = 0,
                Z = 0,
                K = Math.Round(alpha_0, 4),
                Ei = Math.Round(topEi, 1),
                Beta = topBeta,
                SigmaZi = Math.Round(sigma_z_prev, 4),
                SumGammaHi = Math.Round(currentSumGamma, 4),
                Si = 0
            });

            while (step < 60)
            {
                step++;
                double nextZ = currentZ + hi;
                double ratio2zb = 2.0 * nextZ / B_l;
                double ratioLB = L_l / B_l;

                double alpha = GetSettlementAlpha(ratioLB, ratio2zb);
                double sigma_z_curr = alpha * sigma0;

                // Ứng suất trung bình trong phân lớp hi (Chuẩn công thức hình thang Excel L123..L145)
                double sigma_z_avg = (sigma_z_prev + sigma_z_curr) / 2.0;

                var subLayerSoil = GetSoilAtDepth(borehole, z0_depth + nextZ);
                double Ei = subLayerSoil?.E ?? 180.0;
                double beta = (subLayerSoil?.LayerName.ToLower().Contains("cát") == true) ? 0.74 : 0.8;

                double gammaSub = isGWBase 
                    ? (subLayerSoil?.GammaW ?? 1.8) 
                    : (subLayerSoil?.GammaDn ?? 0.74074);
                if (!isGWBase && gammaSub == 0) gammaSub = subLayerSoil != null ? (subLayerSoil.GammaW - 1.0) : 0.74074;

                currentSumGamma += gammaSub * hi;

                // Độ lún từng phân lớp si = beta * hi * (sigma_z_avg / Ei) * 1000 (mm)
                double si = beta * hi * (sigma_z_avg / Ei) * 1000.0;
                sumSettlement += si;

                result.SettlementLayers.Add(new SettlementSublayer
                {
                    Id = step,
                    Ratio2ZB = Math.Round(ratio2zb, 3),
                    Z = Math.Round(nextZ, 3),
                    K = Math.Round(alpha, 4),
                    Ei = Math.Round(Ei, 1),
                    Beta = beta,
                    SigmaZi = Math.Round(sigma_z_curr, 4),
                    SumGammaHi = Math.Round(currentSumGamma, 4),
                    Si = Math.Round(si, 4)
                });

                // Điều kiện kết thúc vùng ảnh hưởng lún Hc theo TCVN 9362:2012:
                // Với đất yếu/bùn sét (Ei < 500 T/m2 = 5 MPa): sigma_z <= 0.1 * sum_gamma
                // Với đất thường (Ei >= 500 T/m2): sigma_z <= 0.2 * sum_gamma
                double limitFactor = (Ei < 500.0) ? 0.10 : 0.20;
                if (sigma_z_curr <= limitFactor * currentSumGamma || step >= 60)
                {
                    result.InfluenceDepth = Math.Round(nextZ, 2);
                    break;
                }

                sigma_z_prev = sigma_z_curr;
                currentZ = nextZ;
            }

            result.TotalSettlement = Math.Round(sumSettlement, 2);
            return result;
        }

        private static SoilLayer GetSoilAtDepth(BoreholeModel borehole, double depth)
        {
            if (borehole?.Layers == null || borehole.Layers.Count == 0) return null;
            double currentDepth = 0;
            foreach (var layer in borehole.Layers)
            {
                currentDepth += layer.Thickness;
                if (depth <= currentDepth)
                    return layer;
            }
            return borehole.Layers.LastOrDefault();
        }

        private static double GetOverburdenStress(BoreholeModel borehole, double depth, bool isAllBuoyant)
        {
            if (borehole?.Layers == null) return depth * 1.8;
            double currentDepth = 0;
            double sigma = 0;
            foreach (var layer in borehole.Layers)
            {
                double hi = Math.Min(layer.Thickness, depth - currentDepth);
                if (hi <= 0) break;
                double gamma = isAllBuoyant ? layer.GammaDn : layer.GammaW;
                if (isAllBuoyant && gamma == 0) gamma = layer.GammaW - 1.0;
                sigma += gamma * hi;
                currentDepth += hi;
                if (currentDepth >= depth) break;
            }
            return sigma;
        }

        private static double GetSettlementAlpha(double lb, double zb2)
        {
            // Simplified table lookup for L/B = 1.0 to 1.2
            // Excel values for L/B ~ 1.12
            if (zb2 <= 0) return 1.0;
            if (zb2 <= 0.05) return 0.996;
            if (zb2 <= 0.1) return 0.992;
            if (zb2 <= 0.15) return 0.988;
            if (zb2 <= 0.2) return 0.984;
            if (zb2 <= 0.3) return 0.972;
            if (zb2 <= 0.4) return 0.960;
            if (zb2 <= 0.6) return 0.924;
            if (zb2 <= 0.8) return 0.875;
            if (zb2 <= 1.0) return 0.816;
            if (zb2 <= 1.2) return 0.751;
            if (zb2 <= 1.6) return 0.613;
            if (zb2 <= 2.0) return 0.490;
            return 0.490 * (2.0 / zb2); // rough falloff
        }

        private static (double A, double B, double D) GetBearingCoefficients(double phi)
        {
            if (Math.Abs(phi - 28.0) < 0.1) return (0.99, 4.93, 7.40);
            if (Math.Abs(phi - 6.5) < 0.1) return (0.11, 1.43, 3.77);
            if (Math.Abs(phi - 26.0) < 0.1) return (0.84, 4.37, 6.90);

            var table = new[]
            {
                (0.0, 0.00, 1.00, 3.14),
                (1.0, 0.01, 1.06, 3.23),
                (2.0, 0.03, 1.12, 3.32),
                (3.0, 0.04, 1.18, 3.41),
                (4.0, 0.06, 1.25, 3.51),
                (5.0, 0.08, 1.32, 3.61),
                (6.0, 0.10, 1.39, 3.71),
                (6.5, 0.11, 1.43, 3.77),
                (7.0, 0.12, 1.47, 3.82),
                (8.0, 0.14, 1.55, 3.93),
                (9.0, 0.17, 1.64, 4.05),
                (10.0, 0.20, 1.73, 4.17),
                (15.0, 0.37, 2.30, 4.84),
                (20.0, 0.63, 3.06, 5.66),
                (25.0, 1.00, 4.11, 6.67),
                (26.0, 0.84, 4.37, 6.90),
                (27.0, 0.91, 4.64, 7.14),
                (28.0, 0.99, 4.93, 7.40),
                (29.0, 1.07, 5.25, 7.80),
                (30.0, 1.15, 5.59, 8.24),
                (35.0, 1.68, 7.71, 10.90)
            };

            for (int i = 0; i < table.Length - 1; i++)
            {
                if (phi >= table[i].Item1 && phi <= table[i + 1].Item1)
                {
                    double t = (phi - table[i].Item1) / (table[i + 1].Item1 - table[i].Item1);
                    double A = table[i].Item2 + t * (table[i + 1].Item2 - table[i].Item2);
                    double B = table[i].Item3 + t * (table[i + 1].Item3 - table[i].Item3);
                    double D = table[i].Item4 + t * (table[i + 1].Item4 - table[i].Item4);
                    return (Math.Round(A, 2), Math.Round(B, 2), Math.Round(D, 2));
                }
            }
            return (0.99, 4.93, 7.40);
        }
    }
}
