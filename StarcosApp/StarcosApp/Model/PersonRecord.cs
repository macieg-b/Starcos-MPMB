using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcosApp.Model
{
    public class PersonRecord
    {
        public String Name { get; set; }
        public String Surname { get; set; }
        public String CertificateCipher { get; set; }
        public String CertificateSign { get; set; }
        public String BirthDate { get; set; }
        public String IdNumber { get; set; }

        public PersonRecord(String Name, String Surname, String BirthDate, String IdNumber, String certificateCipher, String certificateSign)
        {
            this.Name = Name;
            this.Surname = Surname;
            this.BirthDate = BirthDate;
            this.IdNumber = IdNumber;
            this.CertificateCipher = certificateCipher;
            this.CertificateSign = certificateSign;
        }
    }
}
