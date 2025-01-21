using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace PrismDatabaseApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        // RegionManager를 사용하기 위한 멤버 변수
        private readonly IRegionManager _regionManager;

        // 윈도우 제목
        private string _title = "Smart Factory";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        // 네비게이션 명령 (필요 시)
        public DelegateCommand<string> NavigateCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // NavigateCommand 초기화
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        // 네비게이션 로직
        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                _regionManager.RequestNavigate("ContentRegion", viewName);
            }
        }


    }
}
