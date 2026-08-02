using FAE.Foundation.App.Core;

namespace FAE.Foundation.App.Models
{
    public abstract class FoundationBase : ObservableObject
    {
        public abstract string FoundationType { get; }
    }
}
