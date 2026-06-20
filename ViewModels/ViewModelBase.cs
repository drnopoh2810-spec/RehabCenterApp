using ReactiveUI;
using RehabCenterApp.Services;

namespace RehabCenterApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public LocalizationService L => LocalizationService.Instance;

    protected ViewModelBase()
    {
        LocalizationService.LanguageChanged += (s, e) =>
            this.RaisePropertyChanged(nameof(L));
    }
}
