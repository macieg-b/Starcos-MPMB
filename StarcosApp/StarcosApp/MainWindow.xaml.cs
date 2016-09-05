using System.Windows;

namespace StarcosApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {    
        public MainWindow()
        {
            InitializeComponent();
            _mainFrame.NavigationService.Navigate(new WelcomePage());
        }
    }
}