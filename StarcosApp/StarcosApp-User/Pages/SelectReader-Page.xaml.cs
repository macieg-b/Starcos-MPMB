using StarcosApp.sources;
using StarcosApp_User.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace StarcosApp_User.Pages
{
    /// <summary>
    /// Interaction logic for Welcome_Page.xaml
    /// </summary>
    public partial class SelectReader_Page : Page
    {

        #region Fields
        CardsReader firstReader = new CardsReader();
        public ObservableCollection<String> readerListObservableCollection = new ObservableCollection<String>();
        #endregion


        #region Constructors
        public SelectReader_Page()
        {
            InitializeComponent();
            LoadReader();
        }
        #endregion


        #region Methods
        public void LoadReader()
        {
            List<string> temporary = firstReader.getListReaders();
            foreach (string element in temporary)
                readerListObservableCollection.Add(element);
            lvReaders.ItemsSource = temporary;
        }

        #endregion


        #region Events
        private void Click_Select(object sender, RoutedEventArgs e)
        {
            if (lvReaders.SelectedItem != null)
            {
                string chosenReader = lvReaders.SelectedItem.ToString();
                this.NavigationService.Navigate(new Authentication_Page());
            }
        }
        #endregion
    }
}
