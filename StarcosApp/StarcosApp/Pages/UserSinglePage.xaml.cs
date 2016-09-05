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

namespace StarcosApp
{
    /// <summary>
    /// Interaction logic for UserSinglePage.xaml
    /// </summary>
    public partial class UserSinglePage : Page
    {
        private PersonRecord _person = null;
        public UserSinglePage()
        {
            InitializeComponent();
        }
        public UserSinglePage(PersonRecord person)
        {
            InitializeComponent();
            _person = person;
            if(person!=null)
            {
                Name.Text = person.Name;
                Surname.Text = person.Surname;
                BirthDate.Text = person.BirthDate;
                IdNumber.Text = person.IdNumber;
                DecipherCertificate.Text = person.CertificateCipher;
                SigningCertificate.Text = person.CertificateSign;
            }
        }
    }
}
