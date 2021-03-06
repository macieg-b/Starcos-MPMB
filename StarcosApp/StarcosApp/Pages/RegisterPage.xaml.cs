﻿using StarcosApp.Model;
using StarcosApp.sources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using static StarcosApp.Model.Cryptography;
using static StarcosApp.Model.Utilities;

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
        private List<PersonRecord> personList = null;
        Log logManager = new Log();
        XML xmlManager = XML.Instance;

        System.Timers.Timer timer = new System.Timers.Timer();
        #endregion

        #region Constuctors
        public RegisterPage()
        {
            InitializeComponent();
        }
        public RegisterPage(string Arg)
        {
            _firstReader.ReaderName = Arg;
            InitializeComponent();
            StartConnection();
            personList = xmlManager.LoadXml();
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
                var apduOld = APDUmessage;
                APDUmessage = APDUmessage.Replace(" ", "");
                messageLength = HexToBytenByteToHex.GetByteCount(APDUmessage);
                if (!String.IsNullOrEmpty(APDUmessage))
                {
                    response = _firstReader.APDU_SendCommand((HexToBytenByteToHex.GetBytes(APDUmessage, out outVar)),
                                                                                       messageLength,
                                                                                       258,
                                                                                       HiDWinscard.SCARD_PROTOCOL_T1);
                }

                logManager.LogToFile("-> Send to card: " + apduOld, false);
                logManager.LogToFile("<- Response: " + HexToBytenByteToHex.ToString(response), false);
            }
            return response;
        }
        private void Register(String Name, String Surname, String BirthDate, String IdNumber)
        {
            /* Local variables */
            List<String> responsesDD01 = new List<String>(), responsesDD02 = new List<String>();
            Char[] decipherKeyArray = new Char[256], signingKeyArray = new Char[256];
            String decipherKeyString, signingKeyString;

            /*  DF 0010 - file to decipher */
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 01 0C 02 00 10")));                   //Select DF
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 02 0C 02 0E 01")));                   //Select file with public key
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 B0 00 00 F0")));                         //Read bytes (from public key transparent file)
            Array.Copy(responsesDD01[2].ToCharArray(), 8, decipherKeyArray, 0, 256);                                //Cut only these bytes, which belong to decpiher key
            decipherKeyString = new String(decipherKeyArray);
            responsesDD01.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 03 0C")));                            //Back to MF

            /*  DF 0020 - file to signing */
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 01 0C 02 00 20")));                   //Select DF
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 20 00 81 08 31 32 33 34 35 36 37 38"))); //Verify
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 02 0C 02 0E 01")));                   //Select file with public key
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 B0 00 00 F0")));                         //Read bytes (from public key transparent file)         
            Array.Copy(responsesDD02[3].ToCharArray(), 8, signingKeyArray, 0, 256);                                 //Cut only these bytes, which belong to decpiher key
            signingKeyString = new String(signingKeyArray);
            responsesDD02.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 03 0C")));                            //Back to MF

           
            /* Creating certificate based on public key generated on SmartCard */
            Certificate clientDecipherCertificate = new Certificate(), clientSigningCertificate = new Certificate(), caCertificate = new Certificate();
            Cryptography.Key clientDecpiherKey = new Cryptography.Key(), clientSigningKey = new Cryptography.Key(), caKey = new Cryptography.Key();

            clientDecpiherKey.ReadPublicKeyFromCardResponse(decipherKeyString);
            clientSigningKey.ReadPublicKeyFromCardResponse(signingKeyString);

            caKey.ReadCaPrivateKeyFromFile();
            System.Security.Cryptography.X509Certificates.X509Certificate2 caCert = caCertificate.LoadPKCS12FromFile();
            clientDecipherCertificate.CreateSubjectX509Name(TextBox_Name.Text, TextBox_Surname.Text, TextBox_IdNumber.Text);
            clientSigningCertificate.CreateSubjectX509Name(TextBox_Name.Text, TextBox_Surname.Text, TextBox_IdNumber.Text);
            clientDecipherCertificate.CreateCertificate(clientDecpiherKey, caKey, caCert);
            clientSigningCertificate.CreateCertificate(clientSigningKey, caKey, caCert);

            /* Serialize data and save to file */
            xmlManager.SerializeAndSavePersonRecord(personList, Name, Surname, BirthDate, IdNumber, clientDecipherCertificate.GetCertificateAsString, clientSigningCertificate.GetCertificateAsString);
            /* Stop connection to card */
            //StopConnection();
        }

        private void Sign(byte[] input)
        {
            List<String> responsesDD02sign = new List<String>();
            responsesDD02sign.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 01 0C 02 00 20"))); //Select DF
            responsesDD02sign.Add(HexToBytenByteToHex.ToString(SendMessage("00 20 00 81 08 31 32 33 34 35 36 37 38"))); //Verify

            //manage security environment: signing with algorithm id = 13 23 10 (Signature procedure PKCS#1 with RSA with SHA-1)
            //using key: id=01 version=00 
            responsesDD02sign.Add(HexToBytenByteToHex.ToString(SendMessage("00 22 41 B6 0A 84 03 80 01 00 89 03 13 23 10")));
            responsesDD02sign.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 02 0C 02 0F 01"))); //Select file with private key

            //w programie (NIE NA KARCIE) obliczamy skrót SHA-1 z pliku (dowolnego)
            //skrót z tekstu "Marta Pardon Maciej Bartlomiejczyk" wynosi: fb 2d 6d d3 79 c9 0f a7 0f b1 13 cb 7e e6 9c ac 45 5b f7 36
            string myhash = "FB 2D 6D D3 79 C9 0F A7 0F B1 13 CB 7E E6 9C AC 45 5B F7 36";
            //myhash = input.ToString();
            //podpisujemy, jako Data wprowadzamy skrót, Le=F0 tzn ile bajtów w response
            responsesDD02sign.Add(HexToBytenByteToHex.ToString(SendMessage("00 2A 9E 9A " + myhash + " 19")));
            //co robimy z podpisem? od razu zapisujemy w pliku? wypisujemy na ekranie?
            //obie te rzeczy (według mnie ta opcja jest najlepsza; można też od razu żądać podania nazwy dla pliku z podpisem)

            responsesDD02sign.Add(HexToBytenByteToHex.ToString(SendMessage("00 A4 03 0C"))); //Back to MF
        }
      
        #endregion

        #region Events
        private void Click_Register(object sender, RoutedEventArgs e)
        {
            Register(TextBox_Name.Text, TextBox_Surname.Text, TextBox_BirthData.Text, TextBox_IdNumber.Text);
            this.NavigationService.Navigate(new UserListPage());
        }
        #endregion
    }
}
