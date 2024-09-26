
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace SerialPort_itas109
{
    public class SerialPortClass
    {
        public TextBox RecvEidt;
        public Button OpenButton;
        private Thread receiveThread;
        public SerialPort sp;
        public bool isReceiving = true;
        public string logFileName;
        public bool isBlockSend = false;
        public bool block_falg = true;
        public bool isAutokSend = false;
        public bool ack_falg = true;
        public bool readLine_flag = false;
        public string expect_str = "OK";
        public bool isRunWiota = false;
        public bool isStamp = false;
        public int lenCount = 0;
        public long recvCount = 0;

        public bool OpenPort(string portName,int baudRate)
        {
            sp = new SerialPort();
            sp.PortName = portName;
            sp.BaudRate = baudRate;
            try
            {
                sp.Open();
            }
            catch(Exception ex)
            {
                return false;

            }
            
            if (sp.IsOpen)
                return true;
            else
            {
                return false;
            }
        }

        public string ReadPortBuff()
        {
            try
            {
                int bytesRead = sp.BytesToRead;
                if (bytesRead > 0)
                {

                    byte[] buffer = new byte[bytesRead];
                    sp.Read(buffer, 0, bytesRead);
                    string receivedData = Encoding.ASCII.GetString(buffer);
                    return receivedData;
                }
                return null;
            }
             catch (Exception ex)
            {
                return $"Error occurred while receiving data: {ex.Message}";
            }
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

        public void ClosePort(int num)
        {
            sp.Close();
        }

        public void SetPortBaudrate(int nup,int buadRate)
        {
            sp.BaudRate = buadRate;
        }

        private void ReceiveData()
        {
            string receivedData;
            while (isReceiving)
            {
                try
                {
                    if (!sp.IsOpen)
                    {
                        PrintLog("串口设备出现异常");
                        CloseButtont();
                        break;
                    }
                    if (readLine_flag)
                    {
                        Thread.Sleep(20);
                        receivedData = sp.ReadLine();
                        if (receivedData == null || receivedData.Length < 1)
                            continue;
                   
                    }
                    else
                    {
                        Thread.Sleep(200);
                        receivedData = ReadPortBuff();
                    }
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
                logMessage = $"[{timestamp}] 接收<-:" + strRecv +"\r";
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

        private void CloseButtont()
        {
            OpenButton.Dispatcher.Invoke(new Action(() =>
            {
                OpenButton.Content = "打开串口";
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


        public void SendPortString(int port_num,string command)
        {
            sp.Write(command);
        }

    }
}