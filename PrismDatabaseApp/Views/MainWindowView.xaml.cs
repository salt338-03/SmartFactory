using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Prism.Regions;

namespace PrismDatabaseApp.Views
{
    public partial class MainWindowView : Window
    {
        private readonly IRegionManager _regionManager;

        public MainWindowView(IRegionManager regionManager)
        {
            InitializeComponent();
            ModifyTheme(true);
        }
        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
    }
}
