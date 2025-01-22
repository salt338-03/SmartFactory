using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using PrismDatabaseApp.Views;
using Prism.DryIoc;
using Prism.Regions;
using PrismDatabaseApp.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismDatabaseApp.Data;
using PrismDatabaseApp.Services;
using System.Net;

namespace PrismDatabaseApp
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindowView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // View와 ViewModel 등록
            containerRegistry.RegisterForNavigation<CoatingProcessView>();
            containerRegistry.RegisterForNavigation<AlarmBarView>();
            containerRegistry.RegisterForNavigation<DryingProcessView>();
            containerRegistry.RegisterForNavigation<NavigationBarView>();
            containerRegistry.RegisterForNavigation<SlurrySupplyProcessView, SlurrySupplyProcessViewModel>();
            containerRegistry.RegisterForNavigation<NotificationInquiryView>();

            // TcpSocketService 싱글톤 등록
            var tcpSocketService = new TcpSocketService();
            tcpSocketService.Configure("IPAddress.Any", 8080); // IP와 포트 설정
            tcpSocketService.StartListening(); // 서비스 시작
            containerRegistry.RegisterInstance<ITcpSocketService>(tcpSocketService);
            
            Console.WriteLine($"TcpSocketService instance: {tcpSocketService.GetHashCode()}");
            //string sql = "Server=SUNJIN-NOTEBOOK\\MSSQLSERVERR;Database=SlurryCoatingDB;Trusted_Connection=True;TrustServerCertificate=True;";
            string sql = "Server=192.168.1.151,1433;Database=SlurryCoatingDB;User Id=1234;Password=1234;TrustServerCertificate=True;";

            containerRegistry.RegisterSingleton<AppDbContext>(() =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlServer(sql);
                return new AppDbContext(optionsBuilder.Options);
            });

            // 서비스 등록
            containerRegistry.RegisterSingleton<AlarmService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            // DI 컨테이너에서 모든 뷰모델 생성 및 초기화
            var regionManager = Container.Resolve<IRegionManager>();

            // 모든 뷰를 순차적으로 로드
            regionManager.RequestNavigate("ContentRegion", "SlurrySupplyProcessView");
            regionManager.RequestNavigate("ContentRegion", "CoatingProcessView");
            regionManager.RequestNavigate("ContentRegion", "DryingProcessView");
            regionManager.RequestNavigate("ContentRegion", "NotificationInquiryView");

            // 초기 화면으로 복원
            regionManager.RequestNavigate("ContentRegion", "SlurrySupplyProcessView");
            // 초기 View 설정
            
            regionManager.RegisterViewWithRegion("NavigationRegion", typeof(NavigationBarView));
            regionManager.RequestNavigate("ContentRegion", "SlurrySupplyProcessView");
            regionManager.RequestNavigate("AlarmRegion", "AlarmBarView");
        }
    }
}
