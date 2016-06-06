using MifareApplication.sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace StarcosApp
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {


        #region Fields
        CardsReader firstReader = new CardsReader();
        #endregion


        #region Constuctors
        public RegisterPage()
        {
            InitializeComponent();
        }
        public RegisterPage(string chosenReader)
        {
            InitializeComponent();
            firstReader.ReaderName = chosenReader;
        }
        #endregion

        
        #region Methods

        #endregion
        
        
        #region Events

        #endregion


    }
}
