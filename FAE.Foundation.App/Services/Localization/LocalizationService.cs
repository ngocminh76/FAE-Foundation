using System.Collections.Generic;
using FAE.Foundation.App.Core;

namespace FAE.Foundation.App.Services.Localization
{
    public class LocalizationService : ObservableObject
    {
        private static LocalizationService _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private string _currentLanguage = "vi-VN";
        private Dictionary<string, string> _strings;

        // Binds to UI
        public Dictionary<string, string> Strings
        {
            get => _strings;
            private set => SetProperty(ref _strings, value);
        }

        private LocalizationService()
        {
            SetLanguage("vi-VN");
        }

        public void SetLanguage(string languageCode)
        {
            _currentLanguage = languageCode;
            
            if (languageCode == "en-US")
            {
                Strings = new Dictionary<string, string>
                {
                    {"AppTitle", "Foundation Analysis & Engineering (FAE)"},
                    {"InputParams", "1. GEOMETRIC PARAMETERS"},
                    {"SpanX", "Column Span X (L_span_X):"},
                    {"ConsLX", "Left Console X (L_cons_L):"},
                    {"ConsRX", "Right Console X (L_cons_R):"},
                    {"TotalLength", "=> Total Length (L_mong):"},
                    {"SpanY", "Column Span Y (L_span_Y):"},
                    {"ConsTY", "Top Console Y (L_cons_Top):"},
                    {"ConsBY", "Bot Console Y (L_cons_Bot):"},
                    {"TotalWidth", "=> Total Width (B_mong):"},
                    {"SlabThickness", "Slab Thickness (h_ban):"},
                    {"RibWidth", "Rib Width (b_dam):"},
                    {"RibHeight", "Rib Height (h_dam):"},
                    {"ColWidth", "Column Width (b_cot):"},
                    {"Depth", "Depth (D_f):"},
                    
                    {"EnvParams", "2. ENVIRONMENT & CUSHION"},
                    {"HasSand", "Sand Cushion"},
                    {"Thick", "Thick:"},
                    {"HasMound", "Counterweight Mound"},
                    {"High", "High:"},
                    {"HasWater", "Groundwater Table"},
                    {"Elev", "Elev:"},
                    
                    {"BtnDesign", "DESIGN & VERIFY"},
                    {"PlanView", "PLAN VIEW"},
                    {"SectionX", "SECTION X"},
                    {"SectionY", "SECTION Y"},
                    {"View3D", "3D VIEW (Drag to rotate)"}
                };
            }
            else // Default: vi-VN
            {
                Strings = new Dictionary<string, string>
                {
                    {"AppTitle", "Phần mềm Thiết kế Móng (FAE)"},
                    {"InputParams", "1. THÔNG SỐ KÍCH THƯỚC"},
                    {"SpanX", "Nhịp cột phương X (L_span_X):"},
                    {"ConsLX", "Độ vươn Console Trái (L_cons_L):"},
                    {"ConsRX", "Độ vươn Console Phải (L_cons_R):"},
                    {"TotalLength", "=> Chiều dài móng (L_móng):"},
                    {"SpanY", "Nhịp cột phương Y (L_span_Y):"},
                    {"ConsTY", "Console Trên (L_cons_Top):"},
                    {"ConsBY", "Console Dưới (L_cons_Bot):"},
                    {"TotalWidth", "=> Chiều rộng móng (B_móng):"},
                    {"SlabThickness", "Chiều dày bản móng (h_bản):"},
                    {"RibWidth", "Bề rộng dầm sườn (b_dầm):"},
                    {"RibHeight", "Chiều cao dầm sườn (h_dầm):"},
                    {"ColWidth", "Kích thước cạnh cổ cột (b_cột):"},
                    {"Depth", "Chiều sâu chôn móng (D_f):"},
                    
                    {"EnvParams", "2. MÔI TRƯỜNG & LỚP ĐỆM"},
                    {"HasSand", "Có đệm cát thay đất"},
                    {"Thick", "Dày:"},
                    {"HasMound", "Có ụ đất đối trọng"},
                    {"High", "Cao:"},
                    {"HasWater", "Có mực nước ngầm"},
                    {"Elev", "Cao độ:"},
                    
                    {"BtnDesign", "THIẾT KẾ VÀ KIỂM TOÁN"},
                    {"PlanView", "MẶT BẰNG MÓNG (PLAN VIEW)"},
                    {"SectionX", "MẶT CẮT X-X"},
                    {"SectionY", "MẶT CẮT Y-Y"},
                    {"View3D", "MÔ HÌNH 3D (Kéo chuột để xoay)"}
                };
            }
            OnPropertyChanged(nameof(Strings));
        }
    }
}
