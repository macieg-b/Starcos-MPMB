using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarcosApp.Model
{
    public class Utilities
    {
        public class Log
        {
            // time - jeżeli true to w logu dodawana jest akutalna data i czas
            // text - treść jaką chcemu zapisać w logu

            public void LogToFile(string text, bool time)
            {
                String destinationPath = Directory.GetCurrentDirectory() + "\\System_Log.txt";

                using (StreamWriter logFile = File.AppendText(destinationPath))
                {
                    String timeStamp;
                    DateTime currentDate;
                    if (time)
                    {
                        currentDate = DateTime.Now;
                        timeStamp = currentDate.ToString() + "\t";
                    }
                    else
                    {
                        timeStamp = "";
                    }
                    logFile.Write("\n" + timeStamp + text + "\n");

                }
            }
        }
        public class XML
        {
            private static XML instance = null;

            private XML() { }

            public static XML Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new XML();
                    }
                    return instance;
                }
            }
            public List<PersonRecord> LoadXml()
            {
                List<PersonRecord> returnList = new List<PersonRecord>();
                if (File.Exists("personXml.xml"))
                {
                    var input = XDocument.Load("personXml.xml");
                    foreach (var data in input.Descendants("PersonRecord"))
                    {
                        returnList.Add(new PersonRecord(data.Element("Name").Value,
                            data.Element("Surname").Value,
                            data.Element("DateOfBirth").Value,
                            data.Element("IdNumber").Value,
                            data.Element("CertificateCipher").Value,
                            data.Element("CertificateSign").Value));
                    }
                }
                return returnList;
            }

            public void SerializeAndSavePersonRecord(List<PersonRecord> PersonList, String Name, String Surname, String BirthDate, String IdNumber, String certificateCipher, String certificateSign)
            {
                try
                {
                    PersonRecord tmpPersonRecord = new PersonRecord(Name, Surname, BirthDate, IdNumber, certificateCipher, certificateSign);
                    PersonList.Add(tmpPersonRecord);
                    //LoadXml();

                    String path = "personXml.xml";

                    XDocument doc = new XDocument(new XElement("PersonRecords",
                    from data in PersonList
                    select new XElement("PersonRecord",
                    new XElement("Name", data.Name),
                    new XElement("Surname", data.Surname),
                    new XElement("DateOfBirth", data.BirthDate),
                    new XElement("IdNumber", data.IdNumber),
                    new XElement("CertificateCipher", data.CertificateCipher),
                    new XElement("CertificateSign", data.CertificateSign))
                    ));
                    doc.Save(path);

                }
                catch (Exception e)
                {
                    String error = e.ToString();
                }
            }

            public void SerializeAndSaveList(List<PersonRecord> PersonList)
            {
                try
                {
                    String path = "personXml.xml";

                    XDocument doc = new XDocument(new XElement("PersonRecords",
                    from data in PersonList
                    select new XElement("PersonRecord",
                    new XElement("Name", data.Name),
                    new XElement("Surname", data.Surname),
                    new XElement("DateOfBirth", data.BirthDate),
                    new XElement("IdNumber", data.IdNumber),
                    new XElement("CertificateCipher", data.CertificateCipher),
                    new XElement("CertificateSign", data.CertificateSign))
                    ));
                    doc.Save(path);

                }
                catch (Exception e)
                {
                    String error = e.ToString();
                }
            }
        }

        public class MySQLDatabase
        {
            private String _connectionString = null;
            private MySqlConnection _mySqlConnection = null;
            private MySqlCommand _mySqlCommand = null;

            public MySQLDatabase(String connectionString)
            {
                this._connectionString = connectionString;
            }

            public void OpenDatabaseConnection()
            {
                try
                {
                    _mySqlConnection = new MySqlConnection();
                    _mySqlConnection.ConnectionString = _connectionString;
                    _mySqlConnection.Open();
                }
                catch (MySqlException e)
                {
                    String error = e.Message;
                }
            }
            public void CloseDatabaseConnection()
            {
                try
                {
                    if (_mySqlConnection != null)
                        _mySqlConnection.Close();
                }
                catch (MySqlException e)
                {
                    String error = e.Message;
                }
            }
            public void ExecuteMysqlQuery(String queryString)
            {
                var state = _mySqlConnection.State.ToString();
                if (state == "Open")
                {
                    try
                    {
                        _mySqlCommand = new MySqlCommand();
                        _mySqlCommand.Connection = _mySqlConnection;
                        _mySqlCommand.CommandText = queryString;
                        _mySqlCommand.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        _mySqlConnection.Close();
                    }
                }
            }
        }
    }
}
