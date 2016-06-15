using StarcosApp.sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        CardsReader _firstReader = new CardsReader();
        private string _status = String.Empty;
        private string _AtrString = String.Empty;

        System.Timers.Timer timer = new System.Timers.Timer();
        #endregion

        #region Constuctors
        public RegisterPage()
        {
            InitializeComponent();
        }
        public RegisterPage(string chosenReader)
        {
            InitializeComponent();
            _firstReader.ReaderName = chosenReader;
            StartConnection();
        }
        



        #endregion

        #region Methods
        private bool StartConnection()
        {
            try
            {
                if (!String.IsNullOrEmpty(_firstReader.ReaderName))
                {
                    _firstReader.sCardEstablishContext(out _status);
                    timer.Interval = 100;
                    timer.Elapsed += new ElapsedEventHandler(timer_Tick);
                    timer.Start();
                }
                return true;
            }
            catch
            {
                return false;
            }
           
        }
        private void StopConnection()
        {
            _firstReader.Disconnect(out _status);
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (_firstReader.GetStatusChange(out _status) == 0)
            {
                timer.Stop();
                _firstReader.Connect(out _status);
                _AtrString = _firstReader.getATR();
                HID.Beep(3000, 200);
                Thread.Sleep(2000);
            }
        }

        private byte[] SendMessage(string APDUmessage)
        {
            byte[] response = null;
            int messageLength, outVar;
            if (_firstReader.GetStatusChange(out _status) == 0)
            {
                outVar = 0;
                APDUmessage = APDUmessage.Replace(" ", "");
                messageLength = HexToBytenByteToHex.GetByteCount(APDUmessage);
                if (!String.IsNullOrEmpty(APDUmessage))
                {
                    response = _firstReader.APDU_SendCommand((HexToBytenByteToHex.GetBytes(APDUmessage, out outVar)),
                                                                                       messageLength,
                                                                                       258,
                                                                                       HiDWinscard.SCARD_PROTOCOL_T1);
                }             
            }
            return response;
        }
        private void Register()
        {
            List <String> responsesDD01 = new List<String>(), responsesDD02 = new List<String>();
            //It's a kind of magic in DF DD01
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 01 0C 02 DD 01"))); //Select DF
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 20 00 81 08 31 32 33 34 35 36 37 38"))); //Verify
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 22 41 B8 08 84 03 80 01 00 89 01 41"))); // Set security environment
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 46 00 02"))); // Generate RSA keys
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 03 0C"))); //Back to MF
            
            //It's a kind of magic ind DF DD02
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 01 0C 02 DD 02"))); //Select DF
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 20 00 81 08 31 32 33 34 35 36 37 38"))); //Verify
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 22 41 B6 08 84 03 80 01 00 89 01 41"))); // Set security 
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 46 00 02"))); // Generate RSA keys

            StopConnection();
        }
        #endregion

        #region Events
        private void Click_Register(object sender, RoutedEventArgs e)
        {
            Register();
        }
        #endregion


    }
}
