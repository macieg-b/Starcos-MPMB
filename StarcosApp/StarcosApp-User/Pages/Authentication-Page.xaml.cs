using StarcosApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StarcosApp_User.Pages
{
    /// <summary>
    /// Interaction logic for Autentication_Page.xaml
    /// </summary>
    public partial class Authentication_Page : Page
    {
        public Authentication_Page()
        {
            InitializeComponent();
        }

        private void Click_ExternalAuthentication(object sender, RoutedEventArgs e)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#FF2BAE2B");
            Circle_External.Foreground = new SolidColorBrush(color);
        }

        private void Click_InternalAuthentication(object sender, RoutedEventArgs e)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#FF2BAE2B");
            Circle_Internal.Foreground = new SolidColorBrush(color);
        }

        private void Click_LogIn(object sender, RoutedEventArgs e)
        {
            Click_ExternalAuthentication(sender,  e);
            Click_InternalAuthentication(sender, e);
            NavigationService.Navigate(new Main_Page());
        }
    }
}
