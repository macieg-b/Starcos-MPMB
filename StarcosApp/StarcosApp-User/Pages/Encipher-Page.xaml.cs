using StarcosApp.Model;
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
using static StarcosApp.Model.Utilities;

namespace StarcosApp_User.Pages
{
    /// <summary>
    /// Interaction logic for Encipher_Page.xaml
    /// </summary>
    public partial class Encipher_Page : Page
    {
        List<PersonRecord> _personList = null;
        PersonRecord _hoverPerson = null;
        public Encipher_Page()
        {
            InitializeComponent();
            XML xmlManager = XML.Instance;
            _personList = xmlManager.LoadXml();
            lvUsers.ItemsSource = _personList;
        }

        private void lvUsers_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem test = (ListViewItem)sender;
            _hoverPerson = (PersonRecord)test.Content;
        }
    }
}
