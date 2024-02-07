using itas109;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SerialPort_itas109
{
    public class SerialPortClass
    {
        public TextBox RecvEidt;
        private Thread receiveThread;
        CSerialPort sp;
        public bool isReceiving = true;
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
        private byte[] data = new byte[1024]; // 将data定义为类的成员变量
        public void  Display_Port()
        {
            SerialPortInfoVector spInfoVec = new SerialPortInfoVector();
            spInfoVec = CSerialPortInfo.availablePortInfos();
            for (int i = 1; i <= spInfoVec.Count; ++i)
            {
                Console.WriteLine("{0} - {1} {2}", i, spInfoVec[i - 1].portName, spInfoVec[i - 1].description);
            }
        }

        public bool OpenPort(string portName,int baudRate)
        {
            sp = new CSerialPort();
            sp.init(portName,             // windows:COM1 Linux:/dev/ttyS0
                       baudRate,                 // baudrate
                       Parity.ParityNone,    // parity
                       DataBits.DataBits8,   // data bit
                       StopBits.StopOne,     // stop bit
                       FlowControl.FlowNone, // flow
                       4096                  // read buffer size
                       );
            sp.setReadIntervalTimeout(0); // read interval timeout
            sp.open();
            if (sp.isOpen())
            {
                return true;
            }
            return false;
        }


        public string ReadPortBuff()
        {
            uint readBufferLen = sp.getReadBufferUsedLen();
            if (readBufferLen > 0)
            {
                Array.Resize(ref data, (int)readBufferLen); // 调整字节数组的大小以适应读取的数据量
                sp.readAllData(data);
                string str = Encoding.Default.GetString(data);
                return str;
            }
            return null;
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

        public void ClosePort()
        {
            sp.close();
        }

        public void SetPortBaudrate(int buadRate)
        {
           sp.setBaudRate(buadRate);
        }

        private void ReceiveData()
        {
            string receivedData;
            while (isReceiving)
            {
                try
                {
                    
                    receivedData = ReadPortBuff();
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
                    Thread.Sleep(200);
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
                logMessage = $"[{timestamp}] 接收<-:" + strRecv + "\r";
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
                //}
            }
        }


        public int SendPortString(string command)
        {
            byte[] data = Encoding.ASCII.GetBytes(command); // 将命令转换为字节数组
            int sentBytes = sp.writeData(data, data.Length); // 发送数据并获取发送的字节数
            return sentBytes;
        }

    }
}