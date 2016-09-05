using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace StarcosApp_User.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class Main_Page : Page
    {


        #region Fields
        private String _readerName;
        #endregion

        #region Constuctors
        public Main_Page()
        {
            InitializeComponent();
        }
        public Main_Page(string chosenReader)
        {
            InitializeComponent();
            _readerName = chosenReader;
        }

        #endregion



        #region Events
        private void Click_Decipher(object sender, RoutedEventArgs e)
        {

        }

        private void Click_Encipher(object sender, RoutedEventArgs e)
        {
            Frame_MainPageRight.NavigationService.Navigate(new Encipher_Page());
        }

        private void Click_Sign(object sender, RoutedEventArgs e)
        {

        }

        private void Click_VerifySign(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
