using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvertCToCSharp
{
    public class SerialPortImplementation
    {
        SerialPort hComm;
        string pcCommPort;
        string SerialBuffer;
        string cmd1 = "CMD1";
        public SerialPortImplementation(string port)
        {

            pcCommPort = port;
            
        }

        public int main()
        {
            var returnValue = SerialPort_Init();
            if (returnValue == 0)
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
                hComm.Close();
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

        public int SerialPort_Init()
        {
             hComm = new SerialPort(pcCommPort, 115200, Parity.None, 8, StopBits.One);


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
            hComm.ReadTimeout = 5000;
            hComm.WriteTimeout = 1000;
            

            return 0;
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

        public void SerialPort_Read(string data, UInt32 len)
        {
            char TempChar = '\0';

            while (true)
            {
                try
                {

                    Console.WriteLine("Receiving Data");
                    var a = new byte[100];

                    string readExisting = hComm.ReadExisting();
                    Console.WriteLine($"readExisting buffer is {readExisting}");
                    
                    var b = hComm.Read(a, 0, 3);
                    Console.WriteLine($"a buffer is {a}");
                    Console.WriteLine($"size is {b}");
                    SerialBuffer = hComm.ReadLine();
                    Console.WriteLine($"serial buffer is {SerialBuffer}");

                    if ((SerialBuffer.Contains(data)) || (SerialBuffer.Length == len))
                    {
                        Console.WriteLine("Required string found: {0}", data);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"exception in catch {ex.Message}");
                   
                }
            }
        }
    }
}
