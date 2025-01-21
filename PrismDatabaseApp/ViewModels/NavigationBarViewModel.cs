using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PrismDatabaseApp.ViewModels
{
    public class NavigationBarViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; }

        public NavigationBarViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // Command 초기화
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                _regionManager.RequestNavigate("ContentRegion", viewName);
            }
        }
    }
}
