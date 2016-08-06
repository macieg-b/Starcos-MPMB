using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcosApp.Model
{
    public class Utilities
    {
        public class LogManager
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

        public class SecurityManager
        {
            public void CreateCertificate()
            {

            }
        }
    }
}
