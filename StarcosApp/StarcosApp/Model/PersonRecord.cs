using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcosApp.Model
{
    public class PersonRecord
    {
        public String Name;
        public String Surname;
        public String DecipherKey;
        public String SigningKey;
        public String BirthDate;
        public String IdNumber;

        public PersonRecord(String Name, String Surname, String BirthDate, String IdNumber, String DecipherKey, String SigningKey)
        {
            this.Name = Name;
            this.Surname = Surname;
            this.BirthDate = BirthDate;
            this.IdNumber = IdNumber;
            this.DecipherKey = DecipherKey;
            this.SigningKey = SigningKey;
        }
    }
}
