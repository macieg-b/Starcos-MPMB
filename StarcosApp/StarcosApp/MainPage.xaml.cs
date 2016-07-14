using StarcosApp.Model;
using StarcosApp.sources;
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

namespace StarcosApp
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {


        #region Fields
        private String _readerName;
        #endregion

        #region Constuctors
        public MainPage()
        {
            InitializeComponent();
        }
        public MainPage(string chosenReader)
        {
            InitializeComponent();
            _readerName = chosenReader;
        }

        #endregion



        #region Events
        private void Click_Register(object sender, RoutedEventArgs e)
        {
            Frame_MainPageRight.NavigationService.Navigate(new RegisterPage(_readerName));
            //Register();
        }
        #endregion


    }
}
