using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Timers;
using StarcosApp.Model;
using static StarcosApp.Model.Utilities;

namespace StarcosApp.sources
{
    public class CardsReader
    {
        IntPtr hContext;                                        //Context Handle value
        string readerName;                                      //Global Reader Variable
        int retval;                                             //Return Value
        uint dwscope;                                           //Scope of the resource manager context
        bool IsAuthenticated;                                //Boolean variable to check the authentication
        bool release_flag;                                   //Flag to release 
        IntPtr hCard;                                           //Card handle
        IntPtr protocol;                                        //Protocol used currently
        byte[] ATR = new byte[33];                              //Array stores Card ATR
        //int card_Type;                                          //Stores the card type
        byte[] sendBuffer = new byte[255];                        //Send Buffer in SCardTransmit
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x16)]
        // public byte receiveBuffer;
        byte[] receiveBuffer = new byte[258];                   //Receive Buffer in SCardTransmit
        int sendbufferlen, receivebufferlen;                    //Send and Receive Buffer length in SCardTransmit
        byte bcla;                                             //Class Byte
        byte bins;                                             //Instruction Byte
        byte bp1;                                              //Parameter Byte P1
        byte bp2;                                              //Parameter Byte P2
        byte len;                                              //Lc/Le Byte
        byte[] data = new byte[255];                            //Data Bytes
        HiDWinscard.SCARD_READERSTATE ReaderState;              //Object of SCARD_READERSTATE
        int value_Timeout;                                      //The maximum amount of time to wait for an action
        uint ReaderCount;                                       //Count for number of readers
        string ReaderList;                                      //List Of Reader
        //System.Object sender1;                                  //Object of the Sender
        //System.Windows.RoutedEventArgs e1;                      //Object of the Event
        byte currentBlock;                                      //Stores the current block selected
        //String keych;                                           //Stores the string in key textbox
        int discarded;                                          //Stores the number of discarded character
        Log logManager = new Utilities.Log();
        //public delegate void DelegateTimer();                   //delegate of the Timer

        //private System.Timers.Timer timer;                      //Object of the Timer
        //public bool bTxtWrongInputChange;                       //Variable to check the wrong input in key textbox. Used in text change event
        //bool read_pressed;

        /* Added */
        private bool _isConnected = false;

        public bool IsConnected
        {
            get { return _isConnected;  }
            set { _isConnected = value; }
        }

        public string ReaderName
        {
            get { return readerName; }
            set { readerName = value; }
        }
        public byte[] ATRprop
        {
            get { return ATR; }
            set { ATR = value; }
        }
        public List<string> readers = new List<string>();

        public List<string> getListReaders()
        {
            uint pcchReaders = 0;
            int nullindex = -1;
            char nullchar = (char)0;
            dwscope = 2;

            // Establish context.
            retval = HID.SCardEstablishContext(dwscope, IntPtr.Zero, IntPtr.Zero, out hContext);
            if (retval != 0)
            {
                throw new Exception("Błąd wykonania polecenia HID.SCardEstablishContext");
            }
            retval = HID.SCardListReaders(hContext, null, null, ref pcchReaders);
            byte[] mszReaders = new byte[pcchReaders];

            // Fill readers buffer with second call.
            retval = HID.SCardListReaders(hContext, null, mszReaders, ref pcchReaders);

            // Populate List with readers.
            string currbuff = Encoding.ASCII.GetString(mszReaders);
            ReaderList = currbuff;

            int len = (int)pcchReaders;

            if (len > 0)
            {
                while (currbuff[0] != nullchar)
                {
                    nullindex = currbuff.IndexOf(nullchar);   // Get null end character.
                    string reader = currbuff.Substring(0, nullindex);
                    readers.Add(reader);
                    len = len - (reader.Length + 1);
                    currbuff = currbuff.Substring(nullindex + 1, len);
                }
            }
            return readers;
        }

        public void LoadKey(int keynum, string key, out string status)
        {

            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;

            if (key.Length == 12)
            {
                Byte[] str3 = HexToBytenByteToHex.GetBytes(key, out discarded); //Encoding.ASCII.GetBytes(keych1);    
                bcla = 0xFF;
                bins = 0x82;
                bp1 = 0x20;
                bp2 = (byte)keynum;
                len = 0x6;
                sendBuffer[0] = bcla;
                sendBuffer[1] = bins;
                sendBuffer[2] = bp1;
                sendBuffer[3] = bp2;
                sendBuffer[4] = len;
                for (int k = 0; k <= str3.Length - 1; k++)
                    sendBuffer[k + 5] = str3[k];
                sendbufferlen = 0xB;
                receivebufferlen = 255;
                retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retval == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {

                        status = "> LOAD KEY ( No. " + keynum + " )   Successful \n";
                    }
                    else
                    {
                        status = "> Load Key" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
                else
                {
                    status = "> Load Key" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
                }

            }
            else
            {
                status = "Klucz nie ma odpowiedniej długości!";
            }
        }

        public void Authentication(byte blockNumber, int keynum, out string status)
        {
            status = "";

            currentBlock = blockNumber;

            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;
            //'********************************************************************
            // '           For Authentication using key number
            // '*********************************************************************

            bcla = 0xFF;
            bins = 0x86;
            bp1 = 0x0;
            bp2 = 0x0; //currentBlock
            len = 0x5;
            sendBuffer[0] = bcla;
            sendBuffer[1] = bins;
            sendBuffer[2] = bp1;
            sendBuffer[3] = bp2;

            sendBuffer[4] = len;
            sendBuffer[5] = 0x1;           //Version
            sendBuffer[6] = 0x0;           //Address MSB
            sendBuffer[7] = currentBlock;  //Address LSB

            sendBuffer[8] = 0x60; //Key Type A

            sendBuffer[9] = (byte)keynum;  //Key Number

            sendbufferlen = 0xA;
            receivebufferlen = 255;
            retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
            if (retval == 0)
            {
                if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                {

                    IsAuthenticated = true;
                    status = "> General Authenticate" + "   Successful \n";

                }
                else
                {
                    status = "> General Authenticate" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                }
            }
            else
            {
                status = "> General Authenticate" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
            }
        }

        public bool Connect(out string status)
        {
           
          

            status = "";

            logManager.LogToFile("============================================", false);
            logManager.LogToFile("Start new session", true);
            logManager.LogToFile("============================================", false);

            retval = HID.SCardConnect(hContext, readerName, HiDWinscard.SCARD_SHARE_SHARED, HiDWinscard.SCARD_PROTOCOL_T1,
                               ref hCard, ref protocol
                                );       //Command to connect the card ,protocol T=1

            ReaderState.RdrName = readerName;
            ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            value_Timeout = 0;
            ReaderCount = 1;

            if (retval == 0)
            {
                status = "> SCardConnect" + "   Successful \n";
                retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);
                _isConnected = true;
                //if (ReaderState.ATRValue[ReaderState.ATRLength - 0x6].Equals(1))
                //{
                //    card_Type = 1;
                //    //ATR_UID(card_Type);
                //}
                //else if (ReaderState.ATRValue[ReaderState.ATRLength - 0x6].Equals(2))
                //{
                //    card_Type = 2;
                //    //ATR_UID(card_Type);
                //}
                //else
                //{
                //    card_Type = 3;
                //    //ATR_UID(card_Type);
                //}

                //timer.Enabled = false;
                logManager.LogToFile("Connected to card", true);
                return true;
            }

            else //if (retval != 0)
            {
                logManager.LogToFile("Error in connecting to card", true);
                status = "> SCardConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
                //timer.Enabled = true;
                return false;
            }
        }

        public void Disconnect(out string status)
        {
            status = "";

            retval = HID.SCardDisconnect(hCard, HiDWinscard.SCARD_UNPOWER_CARD); //Command to disconnect the card
            if (retval == 0)
            {
                status = "> SCardDisconnect" + "   Successful \n";
                //HID.Beep(2000, 200);

                //Thread.Sleep(1500);

                //timer.Enabled = true;
                _isConnected = false;
            }
            else //if (retval != 0)
            {
                try
                {
                    status = "> SCardDisConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}");
                    //timer.Enabled = true;}


                }
                catch { }
            }
        }

        public void sCardEstablishContext(out string status)
        {
            status = "";

            try
            {
                dwscope = 2;
                if (readerName != "" && readerName != null)
                {
                    retval = HID.SCardEstablishContext(dwscope, IntPtr.Zero, IntPtr.Zero, out hContext);
                    if (retval == 0)
                    {
                        IsAuthenticated = false;
                        status = "> SCardEstablishContext" + " Successful";
                        release_flag = true;
                    }
                    else
                    {
                        status = "> SCardEstablishContext" + " Failed... " + " Error Code: " + String.Format("{0:x}", retval) + "H\n";
                        //timer.Enabled = false;
                    }
                }
                else
                {
                    status = "Failed... " + String.Format("{0:x}", retval) + "H\n";
                    //timer.Enabled = false;
                }
            }
            catch { }
        }

        public string Read(byte blockNumber, out string status)
        {
            status = "";

            String read_str;
            //writeLabel.Content = "";
            if (IsAuthenticated == true)
            {
                HiDWinscard.SCARD_IO_REQUEST sioreq;
                sioreq.dwProtocol = 0x2;
                sioreq.cbPciLength = 8;
                HiDWinscard.SCARD_IO_REQUEST rioreq;
                rioreq.cbPciLength = 8;
                rioreq.dwProtocol = 0x2;

                bcla = 0xFF;
                bins = 0xB0;
                bp1 = 0x0;
                bp2 = blockNumber;
                sendBuffer[0] = bcla;
                sendBuffer[1] = bins;
                sendBuffer[2] = bp1;
                sendBuffer[3] = bp2;
                sendBuffer[4] = 0x0;
                sendbufferlen = 0x5;
                receivebufferlen = 0x12;
                retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retval == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        //read_pressed = true;
                        read_str = HexToBytenByteToHex.ToString(receiveBuffer);
                        
                        status = "> READ BINARY         ( Block " + blockNumber.ToString() + " )  Successful\n";
                        
                        return read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                    }
                    else
                    {
                        status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
                    }
                }
                else
                {
                    status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
                }
            }
            return "";
        }

        public int GetStatusChange(out string status)
        {
            status = "";

            ReaderState.RdrName = readerName;
            ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            value_Timeout = 0;
            ReaderCount = 1;

            if (ReaderList == "")
            {
                status = "SmartCard Removed";
                return 1;
            }
            else
            {
                retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);
                if ((ReaderState.ATRLength == 0) || (retval != 0))
                {
                    status = "SmartCard Removed";
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

    //    public void APDU_getMF(out string status)
    //    {
    //        status = "";

    //        String read_str;

    //        HiDWinscard.SCARD_IO_REQUEST sioreq;
    //        sioreq.dwProtocol = 0x2;
    //        sioreq.cbPciLength = 8;
    //        HiDWinscard.SCARD_IO_REQUEST rioreq;
    //        rioreq.cbPciLength = 8;
    //        rioreq.dwProtocol = 0x2;

    //        sendBuffer[0] = 0x00;
    //        sendBuffer[1] = 0xA4;
    //        sendBuffer[2] = 0x00;
    //        sendBuffer[3] = 0x0C;
    //        sendBuffer[4] = 0x02;
    //        sendBuffer[5] = 0x3F;
    //        sendBuffer[6] = 0x00;

    //        sendbufferlen = 0x7;
    //        receivebufferlen = 0x0A;

    //        retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
    //        if (retval == 0)
    //        {
    //            if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
    //            {
    //                //read_pressed = true;
    //                read_str = HexToBytenByteToHex.ToString(receiveBuffer);
    //                status = "> MF SELECTED  Successful\n";

    //            }
    //            else
    //            {
    //                status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + "\n";
    //            }
    //        }
    //        else
    //        {
    //            status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
    //        }

    //    }

    //    public void APDU_getAIDD6160000300101(out string status)
    //{
    //        status = "";

    //        String read_str;
           
    //        HiDWinscard.SCARD_IO_REQUEST sioreq;
    //        sioreq.dwProtocol = 0x2;
    //        sioreq.cbPciLength = 8;
    //        HiDWinscard.SCARD_IO_REQUEST rioreq;
    //        rioreq.cbPciLength = 8;
    //        rioreq.dwProtocol = 0x2;

    //        sendBuffer[0] = 0x00;
    //        sendBuffer[1] = 0xA4;
    //        sendBuffer[2] = 0x04;
    //        sendBuffer[3] = 0x0C;
    //        sendBuffer[4] = 0x07;
    //        sendBuffer[5] = 0xD6;
    //        sendBuffer[6] = 0x16;
    //        sendBuffer[7] = 0x00;
    //        sendBuffer[8] = 0x00;
    //        sendBuffer[9] = 0x30;
    //        sendBuffer[10] = 0x01;
    //        sendBuffer[11] = 0x01;

    //        sendbufferlen = 0x0C;
    //        receivebufferlen = 0x0A;

    //        retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
    //        if (retval == 0)
    //        {
    //            if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
    //            {
    //                //read_pressed = true;
    //                read_str = HexToBytenByteToHex.ToString(receiveBuffer);

    //                status = "> AIDD6160000300101 SELECTED         Successful\n";

    //            }
    //            else
    //            {
    //                status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
    //            }
    //        }
    //        else
    //        {
    //            status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
    //        }
    //    }

    //    public void APDU_FID0002(out string status)
    //    {
    //        status = "";

    //        String read_str;

    //        HiDWinscard.SCARD_IO_REQUEST sioreq;
    //        sioreq.dwProtocol = 0x2;
    //        sioreq.cbPciLength = 8;
    //        HiDWinscard.SCARD_IO_REQUEST rioreq;
    //        rioreq.cbPciLength = 8;
    //        rioreq.dwProtocol = 0x2;

    //        sendBuffer[0] = 0x00;
    //        sendBuffer[1] = 0xA4;
    //        sendBuffer[2] = 0x02;
    //        sendBuffer[3] = 0x0C;
    //        sendBuffer[4] = 0x02;
    //        sendBuffer[5] = 0x00;
    //        sendBuffer[6] = 0x02;

    //        sendbufferlen = 0x07;
    //        receivebufferlen = 0x0A;

    //        retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
    //        if (retval == 0)
    //        {
    //            if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
    //            {
    //                //read_pressed = true;
    //                read_str = HexToBytenByteToHex.ToString(receiveBuffer);

    //                status = "> FID0002 SELECTED           Successful\n";

    //            }
    //            else
    //            {
    //                status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
    //            }
    //        }
    //        else
    //        {
    //            status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
    //        }

    //    }

    //    public byte[] APDU_ReadBinary(int index, int count, out string status)
    //    {
    //        status = "";

    //        String read_str;

    //        HiDWinscard.SCARD_IO_REQUEST sioreq;
    //        sioreq.dwProtocol = 0x2;
    //        sioreq.cbPciLength = 8;
    //        HiDWinscard.SCARD_IO_REQUEST rioreq;
    //        rioreq.cbPciLength = 8;
    //        rioreq.dwProtocol = 0x2;

    //        sendBuffer[0] = 0x00;
    //        sendBuffer[1] = 0xB0;
    //        sendBuffer[2] = 0x00;
    //        sendBuffer[3] = 0x25;
    //        sendBuffer[4] = 0xBF;

    //        sendbufferlen = 0x05;
    //        receivebufferlen = 0xC1;

    //        retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
    //        if (retval == 0)
    //        {
    //            if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
    //            {
    //                //read_pressed = true;
    //                read_str = HexToBytenByteToHex.ToString(receiveBuffer);

    //                //System.IO.FileStream oFileStream = null;
    //                //oFileStream = new System.IO.FileStream("001.txt", System.IO.FileMode.Create);
    //                //oFileStream.Write(receiveBuffer, 0, receivebufferlen);
    //                //oFileStream.Close();

    //                status = "> READ BINARY         Successful\n";
    //            }
    //            else
    //            {
    //                status = "> SCardTransmit" + "   Failed(SW1 SW2 =" + BitConverter.ToString(receiveBuffer, (receivebufferlen - 2), 1) + " " + BitConverter.ToString(receiveBuffer, (receivebufferlen - 1), 1) + ")\n";
    //            }
    //        }
    //        else
    //        {
    //            status = "> SCardTransmit" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
    //        }

    //        byte[] outBuffer = new byte[receivebufferlen];
    //        Array.Copy(receiveBuffer, outBuffer, receivebufferlen);
    //        return outBuffer;
    //    }

        public void ReleaseContext(out string status)
        {
            status = "";

            if (release_flag == true)
            {
                retval = HID.SCardReleaseContext(hContext);
                if (retval == 0)
                {
                    status = "> SCardReleaseContext" + "   Successful \n";
                }
                else
                {
                    status = "> SCardReleaseContext" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
                }
            }
        }

        //public void Connect_T1(out string status)
        //{
        //    status = "";

        //    retval = HID.SCardConnect(hContext, readerName, HiDWinscard.SCARD_SHARE_SHARED, HiDWinscard.SCARD_PROTOCOL_T1,
        //                       ref hCard, ref protocol
        //                        );       //Command to connect the card ,protocol T=0

        //    ReaderState.RdrName = readerName;
        //    ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
        //    ReaderState.RdrEventState = 0;
        //    //ReaderState.UserData = "Mifare Card";
        //    value_Timeout = 0;
        //    ReaderCount = 1;

        //    if (retval == 0)
        //    {
        //        status = "> SCardConnect" + "   Successful \n";
        //        retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);
        //    }

        //    else if (retval != 0)
        //    {
        //        status = "> SCardConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
        //    }
        //}

        public void Connect(int dwProtocol, out string status)
        {
            status = "";

            retval = HID.SCardConnect(hContext, readerName, HiDWinscard.SCARD_SHARE_SHARED, (uint)dwProtocol,
                               ref hCard, ref protocol
                                );       //Command to connect the card ,protocol T=0

            ReaderState.RdrName = readerName;
            ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Starcos Card";
            value_Timeout = 0;
            ReaderCount = 1;

            if (retval == 0)
            {
                status = "> SCardConnect" + "   Successful \n";
                retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);
            }

            else if (retval != 0)
            {
                status = "> SCardConnect" + "   Failed... " + "   Error Code: " + String.Format("{0:x}", retval) + "H\n";
            }
        }

        //public byte[] APDU_SendCommand_T0(byte[] sendBuffer, int sendbufferlen, int receivebufferlen)
        //{
        //    HiDWinscard.SCARD_IO_REQUEST sioreq;
        //    sioreq.dwProtocol = HiDWinscard.SCARD_PROTOCOL_T0;
        //    sioreq.cbPciLength = 8;
        //    HiDWinscard.SCARD_IO_REQUEST rioreq;
        //    rioreq.cbPciLength = 8;
        //    rioreq.dwProtocol = HiDWinscard.SCARD_PROTOCOL_T0;

        //    for (byte i = 0; i < sendbufferlen; i++)
        //    {
        //        this.sendBuffer[i] = sendBuffer[i];
        //    }

        //    this.sendbufferlen = sendbufferlen;
        //    this.receivebufferlen = receivebufferlen;

        //    retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
            
        //    if (retval == 0)
        //    {
        //        return receiveBuffer;
        //    }
        //    else
        //    {
        //        throw new Exception("Błąd wykonania polecenia.");
        //    }
        //}

        public byte[] APDU_SendCommand(byte[] sendBuffer, int sendbufferlen, int receivebufferlen, int dwProtocol)
        {
            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = dwProtocol;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = dwProtocol;

            Array.Clear(this.sendBuffer,0,this.sendBuffer.Length);
            Array.Clear(this.receiveBuffer, 0, this.receiveBuffer.Length);
            
            for (byte i = 0; i < sendbufferlen; i++)
            {
                this.sendBuffer[i] = sendBuffer[i];
            }

            this.sendbufferlen = sendbufferlen;
            this.receivebufferlen = receivebufferlen;

            retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);

            if (retval == 0)
            {
                return receiveBuffer;
            }
            else
            {
                throw new Exception("Błąd wykonania polecenia.");
            }
        }

        public string getATR()
        {
            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = HiDWinscard.SCARD_PROTOCOL_T0;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = HiDWinscard.SCARD_PROTOCOL_T0;

            string atr_temp = "";
            string s = "";
            atr_temp = "";

            StringBuilder hex = new StringBuilder(ReaderState.ATRValue.Length * 2);

            foreach (byte b in ReaderState.ATRValue)
            {
                hex.AppendFormat("{0:X2}", b);
            }
            
            atr_temp = hex.ToString();
            atr_temp = atr_temp.Substring(0, ((int)(ReaderState.ATRLength)) * 2);

            for (int k = 0; k <= ((ReaderState.ATRLength) * 2 - 1); k += 2)
            {
                s = s + atr_temp.Substring(k, 2) + " ";
            }

            return s.Remove(s.Length - 1);
        }

    }
}

