using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ConvertCToCSharp
{
    public class CSharpCodeNew
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetCommMask(SerialPort hFile, uint lpEvtMask);

        [DllImport("kernel32.dll")]
        static extern bool WaitCommEvent(SerialPort hFile, out uint lpEvtMask, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll")]
        private static extern int ReadFile(SerialPort hFile, out string lpBuffer, UInt32 nNumberOfBytesToRead, out UInt32 lpNumberOfBytesRead, UIntPtr lpOverlapped);

        [DllImport("Kernel32.dll")]
        private static extern string GetLastError();

        [DllImport("kernel32.dll")]
        public static extern bool GetCommTimeouts(SerialPort hFile, out COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll")]
        public static extern bool SetCommTimeouts(SerialPort hFile, out COMMTIMEOUTS lpCommTimeouts);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern int CloseHandle(SerialPort hObject);

        [DllImport("kernel32.dll")]
        static extern bool GetCommState(SerialPort hFile, out SerialPort lpDCB);

        [DllImport("kernel32.dll")]
        static extern bool SetCommState(SerialPort hFile, out SerialPort lpDCB);


        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern UIntPtr CreateFileW(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, UIntPtr lpSecurityAttributes,
                                             UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, UIntPtr hTemplateFile);

        SerialPort hComm;
        bool Read_Status;
        bool Write_Status;
        string pcCommPort = "COM3";
        SerialPort dcbSerialParams = new SerialPort();
        string cmd1= "CMD1";
        string cmd2 = "CMD2";
        string cmd3 = "CMD3";
        string SerialBuffer = "";
        uint EV_RXCHAR = 0x0001;
        COMMTIMEOUTS timeouts = new COMMTIMEOUTS();
        byte[] crc8_table = new byte[] {
        0x00, 0x9B, 0xAD, 0x36, 0xC1, 0x5A, 0x6C, 0xF7,
        0x19, 0x82, 0xB4, 0x2F, 0xD8, 0x43, 0x75, 0xEE,
        0x32, 0xA9, 0x9F, 0x04, 0xF3, 0x68, 0x5E, 0xC5,
        0x2B, 0xB0, 0x86, 0x1D, 0xEA, 0x71, 0x47, 0xDC,
        0x64, 0xFF, 0xC9, 0x52, 0xA5, 0x3E, 0x08, 0x93,
        0x7D, 0xE6, 0xD0, 0x4B, 0xBC, 0x27, 0x11, 0x8A,
        0x56, 0xCD, 0xFB, 0x60, 0x97, 0x0C, 0x3A, 0xA1,
        0x4F, 0xD4, 0xE2, 0x79, 0x8E, 0x15, 0x23, 0xB8,
        0xC8, 0x53, 0x65, 0xFE, 0x09, 0x92, 0xA4, 0x3F,
        0xD1, 0x4A, 0x7C, 0xE7, 0x10, 0x8B, 0xBD, 0x26,
        0xFA, 0x61, 0x57, 0xCC, 0x3B, 0xA0, 0x96, 0x0D,
        0xE3, 0x78, 0x4E, 0xD5, 0x22, 0xB9, 0x8F, 0x14,
        0xAC, 0x37, 0x01, 0x9A, 0x6D, 0xF6, 0xC0, 0x5B,
        0xB5, 0x2E, 0x18, 0x83, 0x74, 0xEF, 0xD9, 0x42,
        0x9E, 0x05, 0x33, 0xA8, 0x5F, 0xC4, 0xF2, 0x69,
        0x87, 0x1C, 0x2A, 0xB1, 0x46, 0xDD, 0xEB, 0x70,
        0x92, 0x09, 0x3F, 0xA4, 0x53, 0xC8, 0xFE, 0x65,
        0x8B, 0x10, 0x26, 0xBD, 0x4A, 0xD1, 0xE7, 0x7C,
        0xA0, 0x3B, 0x0D, 0x96, 0x61, 0xFA, 0xCC, 0x57,
        0xB9, 0x22, 0x14, 0x8F, 0x78, 0xE3, 0xD5, 0x4E,
        0xF6, 0x6D, 0x5B, 0xC0, 0x37, 0xAC, 0x9A, 0x01,
        0xEF, 0x74, 0x42, 0xD9, 0x2E, 0xB5, 0x83, 0x18,
        0xC4, 0x5F, 0x69, 0xF2, 0x05, 0x9E, 0xA8, 0x33,
        0xDD, 0x46, 0x70, 0xEB, 0x1C, 0x87, 0xB1, 0x2A,
        0x5A, 0xC1, 0xF7, 0x6C, 0x9B, 0x00, 0x36, 0xAD,
        0x43, 0xD8, 0xEE, 0x75, 0x82, 0x19, 0x2F, 0xB4,
        0x68, 0xF3, 0xC5, 0x5E, 0xA9, 0x32, 0x04, 0x9F,
        0x71, 0xEA, 0xDC, 0x47, 0xB0, 0x2B, 0x1D, 0x86,
        0x3E, 0xA5, 0x93, 0x08, 0xFF, 0x64, 0x52, 0xC9,
        0x27, 0xBC, 0x8A, 0x11, 0xE6, 0x7D, 0x4B, 0xD0
    };

        public  byte CalculateCRC8(byte[] data, int len)
        {
            byte crc = 0;
            for (int i = 0; i < len; i++)
            {
                crc = crc8_table[crc ^ data[i]];
            }
            return crc;
        }

        public  void Byte2Hex(byte[] byteArray, int byteLength, char[] hexString)
        {
            for (int i = 0; i < byteLength; i++)
            {
                hexString[i * 2] = byteArray[i].ToString("X2")[0];
                hexString[i * 2 + 1] = byteArray[i].ToString("X2")[1];
            }
        }

        public  void ByteArrayToHexString(byte[] byteArray, byte[] destBuf)
        {
            int byteLength = byteArray.Length - 1;
            Console.WriteLine($"\n\rKey Length: {byteLength}\n");
            // Calculate the length of the resulting hex string
            int hexStringLength = byteLength * 2 + 1; // Each byte is represented by 2 characters, plus 1 for the null terminator
                                                      // Allocate memory for the hex string
            char[] hexString = new char[hexStringLength];
            // Convert the byte array to a hex string
            Byte2Hex(byteArray, byteLength, hexString);
            // Print the hex string
            Console.WriteLine($"\n\rHex string: {new string(hexString)}\n");
            Array.Copy(Encoding.ASCII.GetBytes(hexString), destBuf, hexStringLength);
        }

        public  void SerialPort_Read(string data, UInt32 len)
        {
            char TempChar = '\0';
            Read_Status = SetCommMask(hComm, EV_RXCHAR);
            Read_Status = WaitCommEvent(hComm, out uint dwEventMask, IntPtr.Zero);
            while (true)
            {
                ReadFile(hComm, out SerialBuffer, len, out UInt32 NumOfBytes, UIntPtr.Zero);
                if ((SerialBuffer.Contains(data)) || (SerialBuffer.Length == len))
                {
                    Console.WriteLine("Required string found: {0}", data);
                    break;
                }
            }
        }

        public void SerialPort_Write(string data, int len)
        {
            /*----------------------------- Writing a Character to Serial Port----------------------------------------*/
            int dNoOFBytestoWrite = len;
            try
            {
                if (hComm.IsOpen)
                {
                    hComm.Write(data);
                    Console.WriteLine("\n {0} bytes written to {1}", len, pcCommPort);
                    if (Write_Status == true)
                        Console.WriteLine("Written data {0} to {1}\n\r", data, pcCommPort);
                    else
                        Console.WriteLine("\n\n   Error {0} in Writing to Serial Port", GetLastError());
                }
                else
                {
                    Console.WriteLine("Serial Port is not open");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing text to {0}: {1}", pcCommPort, ex.Message);
            }
        }

        public int SerialPort_Init()
        {
            SerialPort hComm = new SerialPort(pcCommPort, 9600, Parity.None, 8, StopBits.One);
          

            try
            {
                hComm.Open();
                Console.WriteLine("Opening serial port successful");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("cannot open port!");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"invalid handle value! with exception as {ex.Message}");
                return 1;
            }

            //dcbSerialParams. = Marshal.SizeOf(dcbSerialParams.DcbLength);
            Write_Status = GetCommState(hComm, out dcbSerialParams);
            if (Write_Status == false)
            {
                Console.WriteLine("\n   Error! in GetCommState()");
                hComm.Close();
                int ret = 1;
                return ret;
            }

            dcbSerialParams.BaudRate = 115200;
            dcbSerialParams.DataBits = 8;
            dcbSerialParams.StopBits = StopBits.One;
            dcbSerialParams.Parity = Parity.None;

            Write_Status = SetCommState(hComm, out dcbSerialParams);
            if (Write_Status == false)
            {
                Console.WriteLine("Error! in Setting DCB Structure");
                hComm.Close();
                int ret = 1;
                return ret;
            }
            else
            {
                Console.WriteLine("Setting DCB Structure Successful");
            }
            timeouts.ReadIntervalTimeout = 50;
            timeouts.ReadTotalTimeoutConstant = 50;
            timeouts.ReadTotalTimeoutMultiplier = 10;
            timeouts.WriteTotalTimeoutConstant = 100;
            timeouts.WriteTotalTimeoutMultiplier = 100;
            if (SetCommTimeouts(hComm, out timeouts))
            {
                Console.WriteLine("Error setting timeouts");
                CloseHandle(hComm);
                return 1;
            }
            return 0;
        }

        public int main()
        {
            var returnValue = SerialPort_Init();
            if(returnValue == 0)
            {
                SerialPort_Write(cmd1, cmd1.Length); // Command to read the DevEUI of LoRa device
                SerialPort_Read("ACK", 3); // Receive the ACK for the command
                Console.WriteLine($"serial buffer is {SerialBuffer}"); // print ACK for debug
                if (SerialBuffer.Equals("ACK"))
                {
                    //Memset(SerialBuffer, '\0', SerialBuffer.Length);
                    SerialPort_Read(" ", 16);
                    Console.WriteLine($"serial buffer after ack check is {SerialBuffer}");
                    //memset(SerialBuffer, '\0', sizeof(SerialBuffer));
                }
                CloseHandle(hComm);//Closing the Serial Port
                Console.WriteLine("Program ended unexpectedly");
                return 1;
            }
            else
            {
                Console.WriteLine("Serial port init failed");
                return 1;
            }

            Console.WriteLine("Program reached end of main function");
            return -1;
        }

       
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMMTIMEOUTS
    {
        public int ReadIntervalTimeout;
        public int ReadTotalTimeoutMultiplier;
        public int ReadTotalTimeoutConstant;
        public int WriteTotalTimeoutMultiplier;
        public int WriteTotalTimeoutConstant;
    }
}
