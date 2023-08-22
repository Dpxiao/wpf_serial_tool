using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Controls;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace SerialPortExample
{
    public class MySerialPort
    {
        private const string DllName = "MFCLibrarySerial.dll"; // 替换为实际的DLL名称

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr readData(int index);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeData(IntPtr pData);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OpenPort(int index,string portName, int baud);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SendPortString(int index,string data);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ClosePort(int index);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsOpendPort(int index);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetPortBaudrate(int index,int baud);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetPortDataBit(int index,int bitNum);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetPortStopBit(int index,int stopbit);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetPortParityBit(int index,int parityNum);
    }
    public class SerialPortManager
    {
        private Thread receiveThread;
        public bool isReceiving;
        public TextBox RecvEidt;
        public TextBlock recvStatus;
        public string logFileName;
        public bool isBlockSend = false;
        public bool block_falg = true;
        public bool isAutokSend = false;
        public bool ack_falg = true;
        public string expect_str = "OK";
        public bool isRunWiota = false;
        public bool isStamp = false;
        private int lenCount = 0;
        public long recvCount = 0;
        public int portNum = 0;
        //private object lockObject = new object();

        public bool OpenPort(int index,string portName,int baud)
        {
            return MySerialPort.OpenPort(index,portName, baud);
        }

        public bool ClosePort(int index)
        {
            //Marshal.FreeCoTaskMem(dataPointer); // 释放内存
            try
            {
                return MySerialPort.ClosePort(index);

            }
            catch (Exception ex)
            {
                // 处理其他异常
                Console.WriteLine("Exception occurred: " + ex.Message);
                return false;
            }
        }

        public bool IsOpendPort(int index)
        {
            return MySerialPort.IsOpendPort(index);
        }

        public bool SetPortBaudrate(int index,int baud = 115200)
        {
            return MySerialPort.SetPortBaudrate(index,baud);
        }

        public bool SetPortDataBit(int index,int bitNum = 8)
        {
            return MySerialPort.SetPortDataBit(index,bitNum);
        }

        public bool SetPortStopBit(int index,int stopbit = 0)
        {
            return MySerialPort.SetPortStopBit(index,stopbit);
        }

        public bool SetPortParityBit(int index,int parityNum = 0)
        {
            return MySerialPort.SetPortParityBit(index,parityNum);
        }

        public bool SendPortString(int index,string atCmd)
        {
            try
            {
                return MySerialPort.SendPortString(index, atCmd);

            }
            catch (Exception ex)
            {
                // 处理其他异常
                MessageBox.Show("端口异常：" + ex.Message);
                return false;
            }
        }

        public string ReadPortBuff(int index)
        {
            // 转换指针为字符串
            IntPtr pData = MySerialPort.readData(index);

            // 检查是否成功分配内存
            if (pData == IntPtr.Zero)
                return null;

            // 将返回的数据从非托管内存转换为字符串
            string data = Marshal.PtrToStringAnsi(pData);

            // 释放非托管内存
            MySerialPort.freeData(pData);

            return data;
        }

        public void OpenThread()
        {
            recvCount = 0;
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.Start();
        }

        public void CloseThread()
        {
            isReceiving = false;
            if (receiveThread != null)
            {
                receiveThread.Join();  // 等待线程退出
            }
        }

        private void ReceiveData()
        {
            string receivedData;
            while (isReceiving)
            {
                try
                {
                    receivedData = ReadPortBuff(portNum);
                    if (receivedData != null)
                    {
                        PrintLog(receivedData);
                        if (isRunWiota)//run协议栈的时候
                        {
                            
                            if (receivedData.Contains("OK") || receivedData.Contains("ERROR"))
                            {
                                block_falg = false;
                            }
                            else
                            {
                                block_falg = true;
                            }
                        }
                        else
                        {
                            if (isBlockSend)//期望值判断
                            {
                                if (receivedData.Contains(expect_str) || receivedData.Contains("ERROR"))
                                {
                                    block_falg = false;
                                }
                                else
                                {
                                    
                                    block_falg = true;
                                }
                            }
                        }
                        if (isAutokSend)//自动应答实现
                        {
                            if (receivedData.Contains(expect_str))
                            {
                                ack_falg = false;
                            }
                            else
                            {
                                ack_falg = true;
                            }
                        }
                       
                    }
                    Thread.Sleep(100);
                   
                }
                catch (Exception ex)
                {
                    PrintLog("串口丢失" + ex.ToString());
                }
            }
        }

        private void PrintLog(string strRecv)
        {
            lenCount += strRecv.Length;
            recvCount += strRecv.Length;

            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            string logMessage;

            if (isStamp)
            {
                logMessage = $"[{timestamp}] 接收<-:" + strRecv + "\r\r\n";
            }
            else
            {
                logMessage = strRecv;
            }

            SaveLogFile(logFileName, logMessage);

            const int maxCacheSize = 200 * 1024; // 200KB
            const int maxTotalCacheSize = 10 * 1024 * 1024; // 10MB

            if (lenCount > maxCacheSize)
            {
                ClearRecvEidt();
            }

            if (recvCount > maxTotalCacheSize)
            {
                recvCount = 0;
            }

            UpdateRecvEidt(logMessage);
        }

        private void ClearRecvEidt()
        {
            RecvEidt.Dispatcher.Invoke(new Action(() =>
            {
                RecvEidt.Clear();
                RecvEidt.Text = "";
                lenCount = 0;
            }));
        }

        private void UpdateRecvEidt(string message)
        {
            RecvEidt.Dispatcher.BeginInvoke(new Action(() =>
            {
                RecvEidt.AppendText(message);
                RecvEidt.ScrollToEnd();
            }));
        }

        private void SaveLogFile(string logFileName, string strLog)
        {
            if (logFileName != null)
            {

                // Open log file and append received data
                //lock (lockObject)
                //{
                    using (StreamWriter writer = new StreamWriter(logFileName, true))
                    {
                        writer.WriteLine($"{strLog}");
                    }

                    FileInfo fileInfo = new FileInfo(logFileName);
                    if (fileInfo.Exists && fileInfo.Length > 10 * 1024 * 1024)
                    {
                        // Backup log file with timestamp in filename
                        string backupFileName = $".\\log\\{Path.GetFileNameWithoutExtension(logFileName)}_bak_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(logFileName)}";
                        File.Move(logFileName, backupFileName);
                    }
               // }
            }
        }
    }
}
