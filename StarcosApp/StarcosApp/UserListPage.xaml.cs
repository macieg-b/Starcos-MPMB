using StarcosApp.Model;
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
using static StarcosApp.Model.Utilities;

namespace StarcosApp
{
    /// <summary>
    /// Interaction logic for UserListPage.xaml
    /// </summary>
    public partial class UserListPage : Page
    {
        private List<PersonRecord> _personList = null;
        private PersonRecord _hoverPerson = null;
        private Frame mainFrame;
        XML xmlManager = XML.Instance;
        public UserListPage()
        {
            InitializeComponent();
            _personList = xmlManager.LoadXml();
            lvUsers.ItemsSource = _personList;
        }

        private void Click_Select(object sender, MouseButtonEventArgs e)
        {
            if (lvUsers.SelectedItem != null)
            {
                PersonRecord chosenReader = (PersonRecord)lvUsers.SelectedItem;
                this.NavigationService.Navigate(new UserSinglePage(chosenReader));
            }
        }

        private void lvUsers_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem test = (ListViewItem)sender;
            _hoverPerson = (PersonRecord)test.Content;
        }

        private void Click_DeleteUser(object sender, RoutedEventArgs e)
        {
            if (_hoverPerson != null)
            {
                _personList.Remove(_hoverPerson);
                xmlManager.SerializeAndSaveList(_personList);
                this.NavigationService.Navigate(new UserListPage());
            }

        }
    }
}
