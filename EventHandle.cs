using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;
using System.Windows.Media;
using System.Windows.Input;
using NoteWindow;
using SerialPortExample;
using System.Text;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


//保存log的路径有问题
namespace WIoTa_Serial_Tool
{
    public partial class MainWindow : Window
    {
        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    foreach (string portName in mySerial.GetPortNames())
        //    {
        //        portsComboBox1.Items.Add(portName);

        //    }

        //}

        private string GetPortNumber(string portName)
        {
            string[] parts = portName.Split(' '); // 将字符串按照空格进行分割
            string port = parts[parts.Length - 1]; // 取最后一个元素作为端口名
            port = port.Trim('(', ')');
            return port;
        }

        private void DisplayPort()
        {
            ComboBox[] portsComboBoxes = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };

            foreach (var device in new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%)'").Get())
            {
                string description = (string)device.GetPropertyValue("Caption");
                foreach (ComboBox comboBox in portsComboBoxes)
                {
                    comboBox.Items.Add(description);
                }
            }
            foreach (ComboBox comboBox in portsComboBoxes)
            {
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = SetSerialName();
                }
                else
                {
                    comboBox.Items.Add("Not Find COM Port");
                    comboBox.SelectedIndex = 0;
                }
            }   
        }

        private void DisplayPortNum(int num)
        {
            ComboBox[] portsComboBoxes = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };

            foreach (var device in new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%)'").Get())
            {
                string description = (string)device.GetPropertyValue("Caption");

                portsComboBoxes[num].Items.Add(description);
            }

            if (portsComboBoxes[num].Items.Count > 0)
            {
                portsComboBoxes[num].SelectedIndex = SetSerialName();
            }
            else
            {
                portsComboBoxes[num].Items.Add("Not Find COM Port");
                portsComboBoxes[num].SelectedIndex = 0;
            }

        }

        private int SetSerialName()
        {
            int count = portsComboBox1.Items.Count;
            
            for(int i = 0;i<count;i++)
            {
                if (selectedPorts[portNum] == portsComboBox1.Items[i].ToString())
                {
                    return i;
                }
            }
            return 0;
            //selectedPort
        }

        private string SelectComPort()
        {
            ComboBox[] portsComboBoxes = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };
            int index = portsComboBoxes[portNum].SelectedIndex;
            if (index == -1)
            {
                string portNumber = GetPortNumber(selectedPorts[portNum]);
                return portNumber;
            }
            else
            {
                selectedPorts[portNum] = portsComboBoxes[portNum].Items[index].ToString();//保存当前选取的端口。
                string portNumber = GetPortNumber(selectedPorts[portNum]);
                return portNumber;
            }
            
        }

        private void TabControlContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainGrid.ColumnDefinitions[2].ActualWidth != 0)
            {
                gridWidth = MainGrid.ColumnDefinitions[2].ActualWidth;
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["gridWidth"].Value = gridWidth.ToString();
                config.Save(ConfigurationSaveMode.Modified);
            }

        }
        
        private void Button_Grid_Click(object sender, RoutedEventArgs e)
        {
            Button[] AtCmdList_Button = { AtCmdList_Button1, AtCmdList_Button2 , AtCmdList_Button3 , AtCmdList_Button4 , AtCmdList_Button5 ,
                AtCmdList_Button6 , AtCmdList_Button7 , AtCmdList_Button8 };
            isChecked_at_grid_flag = !isChecked_at_grid_flag;
            this.SizeChanged += MainWindow_LocationChanged;
            if (isChecked_at_grid_flag)
            {
                for (int i = 0; i < 8; i++)
                {
                    AtCmdList_Button[i].Content = "指令列表扩展>>";
                }
                if(gridWidth == 0)
                {
                    MessageBox.Show("获取到指令列表窗口的值为宽度为0");
                }
                double gridWidth2 = MainGrid.ColumnDefinitions[2].ActualWidth;
                if (gridWidth2 == 0)
                {
                    //MessageBox.Show("2");
                    //宽度值不变
                }
                else
                {
                    gridWidth = gridWidth2;
                }
                
                this.Width -= gridWidth;
                MainGrid.ColumnDefinitions[2].Width = new GridLength(0);
              
                //指令列表宽度
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["gridWidth"].Value = gridWidth.ToString();
                config.Save(ConfigurationSaveMode.Modified);
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    AtCmdList_Button[i].Content = "指令列表隐藏<<";
                }

                MainGrid.ColumnDefinitions[2].Width = new GridLength(gridWidth);
                this.Width += gridWidth;
            }
            this.SizeChanged += MainWindow_LocationChanged;
        }

        private TextBox RetTextBoxObject(int port_num)
        {
            TextBox[] recvDataTextBox = { recvDataRichTextBox1, recvDataRichTextBox2,recvDataRichTextBox3, recvDataRichTextBox4, 
                recvDataRichTextBox5, recvDataRichTextBox6, recvDataRichTextBox7, recvDataRichTextBox8 };

            return recvDataTextBox[port_num];
        }

        private void OpenSerialThread(int port_num)
        {
            //这里开八个线程
            mySerial[port_num].RecvEidt = RetTextBoxObject(port_num);
            mySerial[port_num].isReceiving = true;
            mySerial[port_num].isStamp = stampCheckBox1.IsChecked ?? false;
            mySerial[port_num].OpenThread();
        }

        int Selection_index = 0;
        private void MyComboBox_DropDownOpened(object sender, EventArgs e)
        {
            // 添加 PortsComboBox_SelectionChanged 事件处理程序
            int port_num = PortTab.SelectedIndex;
            ComboBox[] portsComboBoxes = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };
            portsComboBoxes[port_num].Items.Clear();
            DisplayPortNum(port_num);
            Selection_index = 0;
        }

        private void PortsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            if (port_num == -1)
                return;
            ComboBox[] portsComboBoxes = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };
            Button[] openButton = { openButton1, openButton2, openButton3, openButton4,
                openButton5, openButton6, openButton7, openButton8 };
            Selection_index++;
            if (Selection_index == 1) //为一的时候表示，选中了该端口
            {
                int index = portsComboBoxes[port_num].SelectedIndex;
                if (index != -1)
                {
                    selectedPorts[portNum] = portsComboBoxes[port_num].Items[index].ToString();//保存当前选取的端口。
                }
               
                if (openButton[portNum].Content.ToString() == "关闭串口")
                {
                    if (mySerial[port_num] != null)
                    {
                        mySerial[port_num].ClosePort(port_num);
                    }
                    if (mySerial[port_num] != null)
                    {
                        mySerial[port_num].CloseThread();
                    }

                    if (OpenComPort())
                    {
                        OpenSerialThread(port_num);
                        openButton[portNum].Content = "关闭串口";
                    }
                    else
                    {
                        openButton[portNum].Content = "打开串口";
                    }
                }
               
            }

        }
        //波特率发生变化

        private void BaudRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            if (mySerial[portNum] != null)
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.SelectedValue != null)
                {
                    string baudRate = comboBox.SelectedValue.ToString();
                    baudRate = baudRate.Replace(" ", "");
                    string[] parts = baudRate.Split(':');

                    if (parts.Length > 1)
                    {
                        string data = parts[1].Trim();
                        
                        _ = mySerial[portNum].SetPortBaudrate(portNum, Convert.ToInt32(data));
                    }
                    
                }

            }
            if (Send_At_Edit[portNum]!= null)
            {
                Send_At_Edit[portNum].Focus();
            }
            
        }

        //打开串口点击事件
        private void Button_Open_Port_Click(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            Button[] openButton = { openButton1, openButton2, openButton3, openButton4, openButton5, openButton6, openButton7, openButton8 };
            CheckBox[] loopSendCheck = { loopSendCheck1, loopSendCheck2, loopSendCheck3, loopSendCheck4, loopSendCheck5, loopSendCheck6, loopSendCheck7, loopSendCheck8 };
            CheckBox[] timerSendCheckBox = { timerSendCheckBox1, timerSendCheckBox2, timerSendCheckBox3, timerSendCheckBox4, timerSendCheckBox5, timerSendCheckBox6, timerSendCheckBox7, timerSendCheckBox8 };

            if (openButton[portNum].Content.ToString() == "打开串口")
            {
                
                if (OpenComPort())
                {

                    OpenSerialThread(port_num);
                    openButton[portNum].Content = "关闭串口";
                }
                else
                {
                    TabItem tabItem = PortTab.SelectedItem as TabItem;
                    tabItem.Header = $"端口{ port_num + 1}";
                    openButton[portNum].Content = "打开串口";
                }
            }
            else
            {
                TabItem tabItem = PortTab.SelectedItem as TabItem;
                tabItem.Header = $"端口{ port_num + 1}";
                mySerial[port_num].CloseThread();


                isRunning_start[port_num] = false;
                isRunning_ack = false;
                isRunning_loopsend[port_num] = false;
                isRunning_looptimer[port_num] = false;

                openButton[port_num].Content = "打开串口";
                _ = mySerial[port_num].ClosePort(port_num);
                loopSendCheck[port_num].IsChecked = false;
                timerSendCheckBox[port_num].IsChecked = false;
            }
        }

        private bool OpenSerial_Clicked_Send()
        {
            int port_num = PortTab.SelectedIndex;
            Button[] openButton = { openButton1, openButton2, openButton3, openButton4, openButton5, openButton6, openButton7, openButton8 };
            if (openButton[port_num].Content.ToString() == "打开串口")
            {
               // mySerial[port_num] = new mySerialManager();
                if (OpenComPort())
                {
                    OpenSerialThread(port_num);
                    openButton[port_num].Content = "关闭串口";
                    return true;
                }
                else
                {
                    openButton[port_num].Content = "打开串口";
                    return false;
                }
            }
            return true;

        }

        private void ConfigurePort(int port_num)
        {
            _ = mySerial[port_num].SetPortParityBit(port_num, 0);
            _ = mySerial[port_num].SetPortDataBit(port_num, 8);
            _ = mySerial[port_num].SetPortStopBit(port_num, 1);
        }

        private bool OpenComPort()
        {
            ComboBox[] baudRateComboBox = {baudRateComboBox1, baudRateComboBox2, baudRateComboBox3, baudRateComboBox4, baudRateComboBox5,
            baudRateComboBox6,baudRateComboBox7,baudRateComboBox8};
            CheckBox[] savefileCheckBox = { savefileCheckBox1 , savefileCheckBox2 , savefileCheckBox3 , savefileCheckBox4 , savefileCheckBox5
            , savefileCheckBox6 , savefileCheckBox7 , savefileCheckBox8 };
            string baudRate = baudRateComboBox[portNum].Text;//由于可编辑所以要这样获取值
            string portNumber = SelectComPort();
            int port_num = PortTab.SelectedIndex;
            //ConfigurePort(port_num);
           
            if (portNumber != null)
            {
                
                mySerial[port_num] = new SerialPortManager();
                
                mySerial[port_num].portNum = port_num;
                //读取配置文件，然后将该值传入。
                ConfigurationManager.RefreshSection("appSettings");
                // 获取新的值
                mySerial[port_num].expect_str = ConfigurationManager.AppSettings["expect_read"];
                if (!mySerial[port_num].OpenPort(port_num,portNumber, Convert.ToInt32(baudRate)))
                {
                    MessageBox.Show("端口打开失败，请检查端口是否占用或不存在", "警告");
                    return false;
                }
                //事件触发会在高频率接收的情况下导致CPU大量占用
                //mySerial.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                if (savefileCheckBox[port_num].IsChecked ?? false)
                {
                    DateTime now = DateTime.Now;
                    string timestamp = now.ToString("yyyy-MM-dd_HH_mm_ss_fff");
                    string timefloder = now.ToString("yyyy-MM-dd");
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string exeFolderPath = Path.GetDirectoryName(exePath);
                    string logpath = $"{exeFolderPath}\\log\\{timefloder}";
                    CreateDirectory(logpath);
                    logFileName[portNum] = $"{logpath}\\{portNumber}_{timestamp}.log";
                    
                    mySerial[port_num].logFileName = logFileName[portNum];
                }
                else
                {
                    logFileName[port_num] = null;//为空时不保存
                    mySerial[port_num].logFileName = logFileName[portNum];
                }
                TabItem tabItem = PortTab.SelectedItem as TabItem;
                tabItem.Header = portNumber;
                return true;
            }
            return false;
        }

        public bool CreateDirectory(string path)
            {
                // 去除首位空格和尾部 \ 符号
                path = path.Trim().TrimEnd('\\');

                // 确保父目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // 如果目录不存在则创建，并返回True；否则返回False
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                else
                {
                    return false;
                }
            }

    //弹出注释窗口  这方法没法绑定数据，保存不了。
    private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button sendButton = sender as Button;
            string sendButton_Content = (string)sendButton.Content;
            MyWindow NoteWindow = new MyWindow(sendButton_Content);
            DataGridRow dataGridRow = FindVisualParentROW<DataGridRow>(sendButton);
            int index = GridTab.SelectedIndex;
            if (dataGridRow != null)
            {
                DataGrid dataGrid = FindVisualParent<DataGrid>(dataGridRow);

                if (dataGrid != null)
                {
                    int rowIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(dataGridRow);

                    // 位置在鼠标下方
                    NoteWindow.WindowStartupLocation = WindowStartupLocation.Manual;

                    // 获取鼠标光标的屏幕位置
                    System.Drawing.Point cursorPos = System.Windows.Forms.Cursor.Position;

                    // 设置注释窗口的位置为鼠标光标的位置
                    NoteWindow.Left = cursorPos.X;
                    NoteWindow.Top = cursorPos.Y;

                    if (NoteWindow.ShowDialog() == true)
                    {
                        DataTemp[rowIndex].发送 = NoteWindow.MyTextBox.Text;
                        sendButton.Content = NoteWindow.MyTextBox.Text;
                        NoteWindow.Close();
                    }
                    else
                    {
                        NoteWindow.Close();
                    }
                }
            }

           
        }

        private void GridSendButton_Click(object sender, RoutedEventArgs e)
        {
            Button sendButton = sender as Button;
            int port_num = PortTab.SelectedIndex;
            if (sendButton != null)
            {
              

                // Get the parent DataGridRow
                DataGridRow row = FindVisualParent<DataGridRow>(sendButton);

                // Get the AT指令 TextBox
                var dataItem = row.Item;

                // Cast the data item to the appropriate type
                if (dataItem is GridDataTemp)
                {
                    GridDataTemp rowData = (GridDataTemp)dataItem;

                    // Access the value of the "AT指令" property
                    string AtCmd = rowData.AT指令;
                    //替换字符中的回车换行符
                    string output = AtCmd.Replace("\\r", "\r");
                    AtCmd = output.Replace("\\n", "\n");
                    // Use the value as needed
                    if (OpenSerial_Clicked_Send())
                    {
                        //是否加回车换行
                        if (enterCheckBox1.IsChecked ?? false)
                        {
                            AtCmd = AtCmd + "\r\n";
                        }
                        Add_TimeStamp(AtCmd, port_num);
                        mySerialWrite(port_num,AtCmd);
                    }
                }

               
            }
        }

        // Helper method to find a visual child of a parent element
        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && ((FrameworkElement)child).Name == childName)
                {
                    return (T)child;
                }
                else
                {
                    T foundChild = FindVisualChild<T>(child, childName);
                    if (foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }
            return null;
        }

        // Helper method to find a parent element of a visual object
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                return null;
            }
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindVisualParent<T>(parentObject);
            }
        }

        private void DataGridCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 判断是否为第一列---无效
            if (((sender as DataGridCell)?.Column)?.DisplayIndex == 0)
            {
                // 获取当前单元格所在的行
                var dataGridRow = FindVisualParentROW<DataGridRow>(sender as DependencyObject);

                if (dataGridRow != null)
                {
                    e.Handled = true;

                    //右键时才显示菜单
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        ContextMenu contextMenu = ((FrameworkElement)sender).ContextMenu;
                        if (contextMenu != null)
                        {
                            contextMenu.PlacementTarget = (UIElement)sender;
                            contextMenu.IsOpen = true;
                        }
                    }
                }

              
            }
        }

        private static T FindVisualParentROW<T>(DependencyObject child) where T : DependencyObject
        {

            while (child is DependencyObject parent)
            {
                if (parent is T found)
                {
                    return found;
                }

                child = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        private void Send_At_Edit_KeyDown(object sender, KeyEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            if (e.Key == Key.Enter)
            {
                if (OpenSerial_Clicked_Send())
                {
                    string AtCmd = Send_At_Edit1.Text;
                    //是否加回车换行
                    if (enterCheckBox1.IsChecked ?? false)
                    {
                        AtCmd = AtCmd + "\r\n";
                    }
                    string output = AtCmd.Replace("\\r", "\r");
                    AtCmd = output.Replace("\\n", "\n");
                    Add_TimeStamp(AtCmd,port_num);
                    mySerialWrite(port_num, AtCmd);
                }
            }
           
        }
        private void Button_Click_Send_AtCmd(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            Button[] openButton = { openButton1, openButton2, openButton3, openButton4, openButton5, openButton6, openButton7, openButton8 };
            CheckBox[] loopSendCheck = { loopSendCheck1, loopSendCheck2, loopSendCheck3, loopSendCheck4, loopSendCheck5, loopSendCheck6, loopSendCheck7, loopSendCheck8 };
            CheckBox[] timerSendCheckBox = { timerSendCheckBox1, timerSendCheckBox2, timerSendCheckBox3, timerSendCheckBox4, timerSendCheckBox5, timerSendCheckBox6, timerSendCheckBox7, timerSendCheckBox8 };

            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            CheckBox[] enterCheckBox = { enterCheckBox1, enterCheckBox2, enterCheckBox3, enterCheckBox4, enterCheckBox5, enterCheckBox6, enterCheckBox7, enterCheckBox8 };
            if (OpenSerial_Clicked_Send())
            {
                string AtCmd = Send_At_Edit[portNum].Text;
                //是否加回车换行
                if (enterCheckBox[portNum].IsChecked ?? false)
                {
                    AtCmd = AtCmd + "\r\n";
                }
                string output = AtCmd.Replace("\\r", "\r");
                AtCmd = output.Replace("\\n", "\n");
                Add_TimeStamp(AtCmd,portNum);
                mySerial[portNum].SendPortString(portNum, AtCmd);
          
            }

        }
        private void mySerialWrite(int port_num,string AtCmd)
        {
            mySerial[port_num].SendPortString(port_num,AtCmd);
        }
        private void Add_TimeStamp(string AtCmd,int portNum)
        {
            TextBox[] recvDataRichTextBox = { recvDataRichTextBox1, recvDataRichTextBox2, recvDataRichTextBox3, recvDataRichTextBox4,recvDataRichTextBox5, recvDataRichTextBox6, recvDataRichTextBox7, recvDataRichTextBox8 };
            CheckBox[] stampCheckBox = { stampCheckBox1, stampCheckBox2, stampCheckBox3, stampCheckBox4, stampCheckBox5, stampCheckBox6, stampCheckBox7, stampCheckBox8 };
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            //是否加时间戳
            if (stampCheckBox[portNum].IsChecked ?? false)
            {
                recvDataRichTextBox[portNum].AppendText($"[{timestamp}] 发送->:{AtCmd}█\r");
                recvDataRichTextBox[portNum].ScrollToEnd();
                if (logFileName[portNum] != null)
                {
                    // 打开日志文件，追加接收到的数据
                    //lock (lockObject)
                    //{
                        using (StreamWriter writer = new StreamWriter(logFileName[portNum], true))
                        {
                            writer.WriteLine($"[{timestamp}] 发送->:{AtCmd}█");
                        }
                   // }
                }
            }
            else
            {
                if (logFileName != null)
                {
                    //lock (lockObject)
                    //{
                        using (StreamWriter writer = new StreamWriter(logFileName[portNum], true))
                        {
                            writer.WriteLine(AtCmd);
                        }
                    //}
                }
            }
        }

        private void Button_Clear_RecvEdit_Click(object sender, RoutedEventArgs e)
        {
            //mySerial[0].recvCount = 0;
            //recvStatus.Text = $"已接收字节数:{0}";
            // recvDataRichTextBox.Document.Blocks.Clear();
            TextBox[] recvDataTextBox = { recvDataRichTextBox1, recvDataRichTextBox2, recvDataRichTextBox3, recvDataRichTextBox4, recvDataRichTextBox5, recvDataRichTextBox6, recvDataRichTextBox7, recvDataRichTextBox8 };

            recvDataTextBox[portNum].Clear();
            recvDataTextBox[portNum].Text = "";
        }
        private void UpdateTimeDate()
        {
            DateTime now = DateTime.Now;
            string timeDateString = string.Format("{0}年{1}月{2}日 {3}:{4}:{5}",
                now.Year,
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            timeDateTextBlock.Text = timeDateString;
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeDate();
        }
       
        //表格菜单


        private void Add_Up_One_Row_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int insertIndex = DataTemp.Count;
            if (insertIndex > 199)
            {
                MessageBox.Show("当前表格行数已经超过200行，无法再继续增加行数！！", "警告");
                return;
            }

            
            UpdateGrid(GetNowGridRow());
            string TabName = (GridTab.SelectedItem as TabItem)?.Header.ToString();
            saveDataGrid(TabName, tabIndex);
        }

        private void saveDataGrid(string TabName, int Tabindex)
        {
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            var selectedItem = ATCmdComBox[Tabindex].SelectedItem as ComboBoxItem;
            if (Tabindex == -1)
                return;
            if (selectedItem != null)
            {
                int itemNum = ATCmdComBox[Tabindex].SelectedIndex;
                string databasePath = $".//config//{TabName}.db";
                CreateSQLiteTable(databasePath, $"sheet{itemNum}", Tabindex);
                InsertDataToSQLite(databasePath, $"sheet{itemNum}", DataTemp, Tabindex);
            }
        }

        private void Add_Dl_One_Row_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int insertIndex = DataTemp.Count;
            if (insertIndex > 199)
            {
                MessageBox.Show("当前表格行数已经超过200行，无法再继续增加行数！！", "警告");
                return;
            }
            UpdateGrid(GetNowGridRow()+1);
        }
        private void Add_End_One_Row_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int insertIndex = DataTemp.Count;
            if (insertIndex > 199)
            {
                MessageBox.Show("当前表格行数已经超过200行，无法再继续增加行数！！", "警告");
                return;
            }
            UpdateGrid(insertIndex);
        }

        private void Add_End_Multi_Row_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int insertIndex = DataTemp.Count-1;//后面会加一
            if (insertIndex+1 > 199)
            {
                MessageBox.Show("当前表格行数已经超过200行，无法再继续增加行数！！", "警告");
                return;
            }
            MyWindow NoteWindow = new MyWindow("3");
            NoteWindow.Title = "请输入增加的行数";
            // 位置在鼠标下方
            NoteWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // 获取鼠标光标的屏幕位置
            System.Drawing.Point cursorPos = System.Windows.Forms.Cursor.Position;

            // 设置注释窗口的位置为鼠标光标的位置
            NoteWindow.Left = cursorPos.X;
            NoteWindow.Top = cursorPos.Y;

            if (NoteWindow.ShowDialog() == true)
            {
                string userInput = NoteWindow.MyTextBox.Text;
                if (IsNumeric(userInput))
                {
                    // 输入是数字
                    int number = int.Parse(userInput);
                    if (insertIndex + number + 1 > 201)
                    {
                        MessageBox.Show("添加的行数导致表格超过200行，已给您增加为200行！！！", "警告");
                        number = 199 - insertIndex;
                    }
                    
                    for (int i = 0; i < number; i++)
                    {
                        insertIndex++;
                        UpdateGrid(insertIndex);
                    }
                    
                }
                else
                {
                    MessageBox.Show("输入的内容存在其他字符，添加多行失败！！", "警告");
                }
            }

            NoteWindow.Close();
        }

        private bool IsNumeric(string input)
        {
            return int.TryParse(input, out _);
        }

        private void Del_Sel_One_Row_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int insertIndex = DataTemp.Count;
            if (insertIndex < 60)
            {
                MessageBox.Show("当前表格行数已经低于60行，无法再继续删除！！", "警告");
                return;
            }
            int rowIndexToDelete = GetNowGridRow(); ;            
            if (rowIndexToDelete >= 0 && rowIndexToDelete < DataTemp.Count)
            {
                // 删除选中行
                DataTemp.RemoveAt(rowIndexToDelete);
                // 先从界面的数据绑定中解除绑定
                DataContext = null;
                // 然后重新绑定更新后的数据源
                DataContext = DataTemp;
            }
            // 更新所有行的行号和发送按钮文本
            for (int i = 0; i < DataTemp.Count; i++)
            {
                DataTemp[i].NumCol = i + 1;
                DataTemp[i].发送 = $"发送按钮{i + 1}";
            }
           
        }

        private int GetNowGridRow()
        {
            int tabIndex = GridTab.SelectedIndex;
            int insertIndex = 0;
         
            if (tabIndex == 0)
            {
                DataGridCellInfo currentCell = myDataGrid_1.CurrentCell;
                if (currentCell != null)
                {
                    insertIndex = myDataGrid_1.Items.IndexOf(currentCell.Item);
                    return insertIndex;
                }
            }
            if (tabIndex == 1)
            {
                DataGridCellInfo currentCell = myDataGrid_2.CurrentCell;
                if (currentCell != null)
                {
                    insertIndex = myDataGrid_2.Items.IndexOf(currentCell.Item);
                    return insertIndex;
                }
            }
            if (tabIndex == 2)
            {
                DataGridCellInfo currentCell = myDataGrid_3.CurrentCell;
                if (currentCell != null)
                {
                    insertIndex = myDataGrid_3.Items.IndexOf(currentCell.Item);
                    return insertIndex;
                }
            }
            return 0;
        }

        private void Modify_Note_Row_Click(object sender, RoutedEventArgs e)
        {
            
            NoteWindow = new MyWindow(DataTemp[GetNowGridRow()].发送);

            // 位置在鼠标下方
            NoteWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // 获取鼠标光标的屏幕位置
            System.Drawing.Point cursorPos = System.Windows.Forms.Cursor.Position;

            // 设置注释窗口的位置为鼠标光标的位置
            NoteWindow.Left = cursorPos.X;
            NoteWindow.Top = cursorPos.Y;

            if (NoteWindow.ShowDialog() == true)
            {
                DataTemp[GetNowGridRow()].发送 = NoteWindow.MyTextBox.Text;
                DataContext = null;
                // 然后重新绑定更新后的数据源
                DataContext = DataTemp;
                NoteWindow.Close();
            }
            else
            {
                NoteWindow.Close();
            }
        }

        private void UpdateGrid(int insertIndex)
        {
            DataTemp.Insert(insertIndex, new GridDataTemp { NumCol = insertIndex, Hex = false, 应答 = "OK", 延时 = "1000", AT指令 = $"AT", 发送 = $"发送按钮{insertIndex}" });
            // 更新所有行的行号和发送按钮文本
            for (int i = 0; i < DataTemp.Count; i++)
            {
                DataTemp[i].NumCol = i + 1;
                //DataTemp[i].发送 = $"发送按钮{i + 1}";
            }
            // 重新设置 DataContext，以便数据绑定更新界面
            DataContext = null;
            DataContext = DataTemp;
        }

        private void AtCmd_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateSource(); // 更新数据源
                }
            }
        }
        private void Ack_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateSource(); // 更新数据源
                }
            }
        }
        private void Delay_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateSource(); // 更新数据源
                }
            }
        }

        private string GetUsridToFile(int groupidx, int burstidx, int slotidx, int singleidx = 0)
        {
            if (groupidx >= TXT_5120_GROUP_NUM || burstidx >= TXT_5120_BURST_NUM ||
                slotidx >= TXT_5120_SLOT_NUM || singleidx >= TXT_5120_SINGLEPOS_NUM)
            {
                MessageBox.Show("Invalid indices");
                return null;
            }
            string fileScrambleIdTxt = "userid/scramble_id_set_5120.txt";
            string text = File.ReadAllText(fileScrambleIdTxt);

            MatchCollection matches = Regex.Matches(text, ", hex: 0x([\\w]{8}), scb id:");
            int totalLen = TXT_5120_GROUP_NUM * TXT_5120_BURST_NUM * TXT_5120_SLOT_NUM * TXT_5120_SINGLEPOS_NUM;
            if (totalLen != matches.Count)
            {
                MessageBox.Show("Mismatch in data length");
                return null;
            }

            int pos = groupidx * TXT_5120_BURST_NUM * TXT_5120_SLOT_NUM * TXT_5120_SINGLEPOS_NUM +
                      burstidx * TXT_5120_SLOT_NUM * TXT_5120_SINGLEPOS_NUM +
                      slotidx * TXT_5120_SINGLEPOS_NUM + singleidx;
            string usrid = matches[pos].Groups[1].Value;
            return usrid;
        }

        private void PortTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = PortTab.SelectedIndex;
            TabItem tabItem = PortTab.SelectedItem as TabItem;

            switch (index)
            {
                case 0:
                    portNum = 0;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 1:
                    portNum = 1;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 2:
                    portNum = 2;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 3:
                    portNum = 3;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 4:
                    portNum = 4;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 5:
                    portNum = 5;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 6:
                    portNum = 6;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                case 7:
                    portNum = 7;
                    if (Start_Window != null)
                    {
                        Start_Window.useridTextBox2.Text = GetUsridToFile(0, portNum, 0, 0);
                        Start_Window.useridTextBox3.Text = GetUsridToFile(0, portNum, 0, 0);
                    }
                    break;
                default:
                    break;
            }

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
           
            string TabName = (GridTab.SelectedItem as TabItem)?.Header.ToString();
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            
            if (tabIndex == -1)
            {
                return;
            }     
            if (DataTemp != null)
            {
                saveDataGrid(NowTabName, NextIndex);
            }
            else
            {
                //启动的时候为空
                return;
            }
            int sheet_index = ATCmdComBox[tabIndex].SelectedIndex;
            NowTabName = TabName;
            NextIndex = tabIndex;
 
            string filePath = $".//config//{TabName}.db";
            if (!ReadSqlData(filePath, sheet_index, tabIndex))
            {
                DataTemp.Clear();
                for (int i = 1; i <= 60; i++)
                {
                    DataTemp.Add(new GridDataTemp { NumCol = i, Hex = false, 延时 = "1000", 应答 = "OK", AT指令 = "AT", 发送 = $"发送按钮{i}" });
                }
                DataContext = DataTemp;
            }
        }

        private void LoadGridData_From_Excel()
        {
            var selectedTab = GridTab.SelectedItem as TabItem;
            int tabIndex = GridTab.SelectedIndex;
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            if (selectedTab != null)
            {
                var tab_Name = selectedTab.Header.ToString();
                var selectedItem = ATCmdComBox[tabIndex].SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                    return;
                var sheetName = selectedItem.Content;
                int sheet_index = ATCmdComBox[tabIndex].SelectedIndex;
                if (sheetName != null)
                {
                    DataTemp.Clear();
                    string filePath = $".//config//{tab_Name}.db";
                    if (!ReadSqlData(filePath, sheet_index, tabIndex))
                    {
                        DataTemp.Clear();
                        for (int i = 1; i <= 60; i++)
                        {
                            DataTemp.Add(new GridDataTemp { NumCol = i, Hex = false, 延时 = "1000", 应答 = "OK", AT指令 = "AT", 发送 = $"发送按钮{i}" });
                        }
                        DataContext = DataTemp;
                    }
                }
            }
        }
        private void ATCmdComBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            string TabName = (GridTab.SelectedItem as TabItem)?.Header.ToString();
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            if (tabIndex == -1)
                return;
            var selectedItem = ATCmdComBox[tabIndex].SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                
                int itemNum = ATCmdComBox[tabIndex].SelectedIndex;
                string databasePath = $".//config//{TabName}.db";
                CreateSQLiteTable(databasePath, $"sheet{NextIndexChild}", tabIndex);
                InsertDataToSQLite(databasePath, $"sheet{NextIndexChild}", DataTemp, tabIndex);
                NextIndexChild = itemNum;
            }
            
            LoadGridData_From_Excel();
        }



        private void LoopSendCheck_Checked(object sender, RoutedEventArgs e)
        {
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            CheckBox[] loopSendCheck = { loopSendCheck1 , loopSendCheck2 , loopSendCheck3 , loopSendCheck4 , loopSendCheck5 ,
                loopSendCheck6 , loopSendCheck7 , loopSendCheck8 };
            CheckBox[] blockSendCheck = { blockSendCheck1 , blockSendCheck2 , blockSendCheck3 , blockSendCheck4 , blockSendCheck5 ,
                blockSendCheck6 , blockSendCheck7 , blockSendCheck8 };
            int port_num = PortTab.SelectedIndex;
            if (loopSendCheck[port_num].IsChecked ?? false)
            {
                string AtCmd = Send_At_Edit[port_num].Text;
                if (OpenSerial_Clicked_Send())
                {
                    loopSendThread = new Thread(new ParameterizedThreadStart(LoopSendThread_func));
                    parameters = new LoopSendPara();
                    parameters.mySerial = mySerial[port_num];
                    parameters.PortNum = port_num;
                    isRunning_loopsend[port_num] = true;
                    mySerial[port_num].isBlockSend = blockSendCheck[port_num].IsChecked ?? false;
                    loopSendThread.Start(parameters);
                }
                else
                {
                    loopSendCheck[portNum].IsChecked = false;
                    isRunning_loopsend[port_num] = false;
                }
            }
            else
            {
                mySerial[port_num].block_falg = false;
                isRunning_loopsend[port_num] = false;
                loopSendThread.Abort();
            }
        }

        private void LoopSendThread_func(object obj)//可以简化
        {
            LoopSendPara parameters = (LoopSendPara)obj;
            SerialPortManager mySerial = parameters.mySerial;
            int portNum = parameters.PortNum;
            string loopcount = string.Empty;
            string sleep = string.Empty;
            TextBox[] loopSendNumTextBox = { loopSendNumTextBox1, loopSendNumTextBox2, loopSendNumTextBox3, loopSendNumTextBox4, loopSendNumTextBox5, loopSendNumTextBox6, loopSendNumTextBox7, loopSendNumTextBox8 };
            TextBox[] loopSendTimeTextBox = { loopSendTimeTextBox1, loopSendTimeTextBox2, loopSendTimeTextBox3, loopSendTimeTextBox4, loopSendTimeTextBox5, loopSendTimeTextBox6, loopSendTimeTextBox7, loopSendTimeTextBox8 };
            TextBox[] recvDataTextBox = { recvDataRichTextBox1, recvDataRichTextBox2,recvDataRichTextBox3, recvDataRichTextBox4,recvDataRichTextBox5, recvDataRichTextBox6, recvDataRichTextBox7, recvDataRichTextBox8 };
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            CheckBox[] enterCheckBox = { enterCheckBox1, enterCheckBox2, enterCheckBox3, enterCheckBox4, enterCheckBox5, enterCheckBox6, enterCheckBox7, enterCheckBox8 };
            CheckBox[] loopSendCheck = { loopSendCheck1, loopSendCheck2, loopSendCheck3, loopSendCheck4, loopSendCheck5, loopSendCheck6, loopSendCheck7, loopSendCheck8 };
            
            // 在UI线程上下文中访问和获取UI控件的值
            
            this.Dispatcher.Invoke(new Action(() =>
            {
                loopcount = loopSendNumTextBox[portNum].Text;
                sleep = loopSendTimeTextBox[portNum].Text;
            }));
            try
            {
                int num = Convert.ToInt32(loopcount);
                int sleep_time = Convert.ToInt32(sleep);
                for (int i = 0; i < num; i++)
                {
                    if (!isRunning_loopsend[portNum])
                    {
                        break;
                    }
                    // 在UI线程上下文中更新UI控件
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        recvDataTextBox[portNum].AppendText($"=====开始第{i + 1}次发送=====\r\n");
                        recvDataTextBox[portNum].ScrollToEnd();
                        if (logFileName[portNum] != null)
                        {
                            using (StreamWriter writer = new StreamWriter(logFileName[portNum], true))
                            {
                                writer.WriteLine($" ===== 开始第{ i + 1}次发送 =====\r\n");
                            }
                        }
                    }));

                    string AtCmd = string.Empty;

                    // 在UI线程上下文中访问和获取UI控件的值
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AtCmd = Send_At_Edit[portNum].Text;
                        string output = AtCmd.Replace("\\r", "\r");
                        AtCmd = output.Replace("\\n", "\n");
                        if (enterCheckBox[portNum].IsChecked ?? false)
                        {
                            AtCmd += "\r\n";
                        }
                        Add_TimeStamp(AtCmd, portNum);
                    }));
                    mySerialWrite(portNum, AtCmd);

                    if (mySerial.isBlockSend)
                    {
                        mySerial.block_falg = true;
                        while (mySerial.block_falg)
                        {
                            
                            if (!isRunning_loopsend[portNum])
                            {
                                break;
                            }
                            if (!mySerial.isBlockSend)
                            {
                                break;
                            }
                            Thread.Sleep(100);
                        }                
                    }
                    Thread.Sleep(sleep_time);
                }
                //在UI线程上下文中更新UI控件
          
            }
            catch (OverflowException)
            {
                MessageBox.Show("输出数值超过最大值：2147483647", "警告");
            }
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    loopSendCheck[portNum].IsChecked = false;
                    recvDataTextBox[portNum].ScrollToEnd();
                }));
            }
            catch(System.Threading.Tasks.TaskCanceledException)
            {
                return;
            }
        }

        private void TimerSendCheckBox_Check(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            CheckBox[] blockSendCheck = { blockSendCheck1 , blockSendCheck2 , blockSendCheck3 , blockSendCheck4 , blockSendCheck5 ,blockSendCheck6 , blockSendCheck7 , blockSendCheck8 };
            CheckBox[] timerSendCheckBox = { timerSendCheckBox1, timerSendCheckBox2, timerSendCheckBox3, timerSendCheckBox4, timerSendCheckBox5, timerSendCheckBox6, timerSendCheckBox7, timerSendCheckBox8 };
            if (timerSendCheckBox[port_num].IsChecked ?? false)
            {
                if (OpenSerial_Clicked_Send())
                {
                    loopTimeSendThread = new Thread(new ParameterizedThreadStart(LoopTimeSendThread_func));
                    parameters = new LoopSendPara();
                    parameters.PortNum = port_num;
                    parameters.mySerial = mySerial[port_num];
                    isRunning_looptimer[port_num] = true;
                    mySerial[port_num].isBlockSend = blockSendCheck[port_num].IsChecked ?? false;
                    loopTimeSendThread.Start(parameters);
                }
                else
                {
                    timerSendCheckBox[portNum].IsChecked = false;
                    isRunning_looptimer[port_num] = false;
                }
            }
            else
            {
                mySerial[port_num].block_falg = false;
                isRunning_looptimer[port_num] = false;
                loopTimeSendThread.Abort();
            }
        }
        private void LoopTimeSendThread_func(object obj)//可以简化
        {
            LoopSendPara parameters = (LoopSendPara)obj;
            SerialPortManager mySerial = parameters.mySerial;
            string loopcount = string.Empty;
            string sleep = string.Empty;
            int port_num = parameters.PortNum;
            // 在UI线程上下文中访问和获取UI控件的值
  
            TextBox[] timerSendTextBox = { timerSendTextBox1, timerSendTextBox2, timerSendTextBox3, timerSendTextBox4,timerSendTextBox5,timerSendTextBox6, timerSendTextBox7, timerSendTextBox8 };
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            CheckBox[] enterCheckBox = { enterCheckBox1, enterCheckBox2, enterCheckBox3, enterCheckBox4, enterCheckBox5, enterCheckBox6, enterCheckBox7, enterCheckBox8 };
      
            this.Dispatcher.Invoke(new Action(() =>
            {
                sleep = timerSendTextBox[port_num].Text;
            }));

            int sleep_time = Convert.ToInt32(sleep);

            while (isRunning_looptimer[port_num])//这里可以设置标志位来管理线程
            {
                string AtCmd = string.Empty;
                // 在UI线程上下文中访问和获取UI控件的值
                this.Dispatcher.Invoke(new Action(() =>
                {
                    AtCmd = Send_At_Edit[port_num].Text;
                    string output = AtCmd.Replace("\\r", "\r");
                    AtCmd = output.Replace("\\n", "\n");
                    if (enterCheckBox[port_num].IsChecked ?? false)
                    {
                        AtCmd = AtCmd + "\r\n";
                    }
                    Add_TimeStamp(AtCmd, port_num);
                }));
                mySerialWrite(port_num, AtCmd);
                if (mySerial.isBlockSend)
                {
                    while (mySerial.block_falg)
                    {
                        if (!isRunning_looptimer[port_num])
                        {
                            break;
                        }
                        if (!mySerial.isBlockSend)
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    mySerial.block_falg = true;
                }
                
                Thread.Sleep(sleep_time);
            }
        }

    }


}
