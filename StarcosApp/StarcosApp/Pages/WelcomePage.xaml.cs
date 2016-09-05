using StarcosApp.sources;
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

namespace StarcosApp
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {

        #region Fields
        CardsReader firstReader = new CardsReader();
        public ObservableCollection<String> readerListObservableCollection = new ObservableCollection<String>();
        #endregion


        #region Constructors
        public WelcomePage()
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
                this.NavigationService.Navigate(new MainPage(chosenReader));
            }
        }
        #endregion
    }
}
