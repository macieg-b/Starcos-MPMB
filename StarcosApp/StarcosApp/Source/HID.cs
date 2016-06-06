using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Timers;


namespace MifareApplication.sources
{
    class HID
    {
        [DllImport("kernel32.dll")]
        public static extern bool Beep(int BeepFreq, int BeepDuration);

        [DllImport("WinScard.dll")]
        public static extern int SCardEstablishContext(uint dwScope,
        IntPtr notUsed1,
        IntPtr notUsed2,
        out IntPtr phContext);

        [DllImport("WinScard.dll")]
        public static extern int SCardReleaseContext(IntPtr phContext);

        [DllImport("WinScard.dll")]
        public static extern int SCardConnect(IntPtr hContext,
        string cReaderName,
        uint dwShareMode,
        uint dwPrefProtocol,
        ref IntPtr hCard,
        ref IntPtr ActiveProtocol);

        [DllImport("WinScard.dll")]
        public static extern int SCardDisconnect(IntPtr hCard, int Disposition);

        [DllImport("WinScard.dll", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
        public static extern int SCardListReaders(
          IntPtr hContext,
          byte[] mszGroups,
          byte[] mszReaders,
          ref UInt32 pcchReaders
          );

        [DllImport("WinScard.dll")]
        public static extern int SCardState(IntPtr hCard, ref IntPtr state, ref IntPtr protocol, ref Byte[] ATR, ref int ATRLen);

        [DllImport("WinScard.dll")]
        public static extern int SCardTransmit(IntPtr hCard, ref HiDWinscard.SCARD_IO_REQUEST pioSendRequest,
                                                           Byte[] SendBuff,
                                                           int SendBuffLen,
                                                           ref HiDWinscard.SCARD_IO_REQUEST pioRecvRequest,
                                                           Byte[] RecvBuff, ref int RecvBuffLen);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public static extern int SCardGetStatusChange(IntPtr hContext,
        int value_Timeout,
        ref HiDWinscard.SCARD_READERSTATE ReaderState,
        uint ReaderCount);
    }

    public class HiDWinscard
    {
        public const int SCARD_STATE_UNAWARE = 0x0;

        public const int SCARD_SHARE_SHARED = 2;

        public const int SCARD_UNPOWER_CARD = 2;

        public const int SCARD_PROTOCOL_T0 = 0x1;
        public const int SCARD_PROTOCOL_T1 = 0x2;
        public const int SCARD_PROTOCOL_UNDEFINED = 0x0;

        public struct SCARD_IO_REQUEST
        {
            public int dwProtocol;
            public int cbPciLength;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SCARD_READERSTATE
        {
            public string RdrName;
            public string UserData;
            public uint RdrCurrState;
            public uint RdrEventState;
            public uint ATRLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24, ArraySubType = UnmanagedType.U1)]
            public byte[] ATRValue;
        }
        public const int card_Type_Mifare_1K = 1;
        public const int card_Type_Mifare_4K = 2;
    }
}


