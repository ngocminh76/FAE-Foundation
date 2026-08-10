using System.Collections.Generic;

namespace WPF.UI.Interface
{
    public interface IDynamicFieldProvider
    {
        Dictionary<string, string> DynamicFields { get; }
        string this[string key] { get; set; }
    }
}
