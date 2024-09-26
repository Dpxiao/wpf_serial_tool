using System;
using System.Collections.Generic;
using System.Windows;
using System.Configuration;
using System.ComponentModel;
using System.Threading;
using NoteWindow;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using SerialPort_itas109;

namespace WIoTa_Serial_Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
 
    public partial class MainWindow : Window
    {
        private const int TXT_5120_GROUP_NUM = 8;
        private const int TXT_5120_BURST_NUM = 8;
        private const int TXT_5120_SLOT_NUM = 8;
        private const int TXT_5120_SINGLEPOS_NUM = 10;
        bool isChecked_at_grid_flag = false;
        double gridWidth;
        private Start_Type_Window Start_Window; // 声明窗口实例变量
        private GetSendData GetSendData_Window;
        private List<string> logFileName = new List<string> { "log1", "log2", "log3", "log4", "log5", "log6", "log7", "log8",
                                                              "log9", "log10", "log11", "log12", "log13", "log14", "log15", "log16"            };
        private List<string> selectedPorts = new List<string> { "Port1", "Port2", "Port3", "Port4", "Port5", "Port6", "Port7", "Port8" ,
                                                                "Port9", "Port10", "Port11", "Port12", "Port13", "Port14", "Port15", "Port16"
                                                                };
        private List<bool> isRunning_start = Enumerable.Repeat(false, 16).ToList();
        private List<bool> isRunning_loopsend = Enumerable.Repeat(true, 16).ToList();
        private List<bool> isRunning_looptimer = Enumerable.Repeat(true, 16).ToList();
  
        private List<GridDataTemp> DataTemp;
        private SerialPortClass []mySerial = new SerialPortClass[16];
        private Thread loopSendThread;
        private Thread MulitSendThread;
        private Thread StopMulitWIoTaThread;
        private LoopSendPara parameters;
        private MulitSendPara Mulitpara;
        private MyWindow NoteWindow;
        private Thread loopTimeSendThread;
        private Thread startSendThread;
        private Thread autoAckSendThread;
        private Thread mulitSendThread;
        public List<string> StringAtCmdList;
        public List<SerialPortClass> SerialList;

        private int portNum = 0;
        string NowTabName;
        int NextIndex;
        int NextIndexChild;
        private bool isRunning_ack = true; //自动应答标志位
        private bool isRunning_mulit = true; //批量发送标志位
        private bool isTemptest_flag = false;

        public MainWindow()
        {
            InitializeComponent();
            SerialList = new List<SerialPortClass>(); // 初始化列表
            TextBox[] recvDataTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"recvDataRichTextBox{i}")).ToArray();
            foreach (var textBox in recvDataTextBox)
            {
                textBox.UndoLimit = 0;
            }
            DisplayPort();

            DataTemp = new List<GridDataTemp>();
            if (!ReadDataGrid())
            {
                for (int i = 1; i <= 60; i++)
                {
                    DataTemp.Add(new GridDataTemp { NumCol = i, Hex = false, 应答 = "OK", 延时 = "1000", AT指令 = "AT", 发送 = $"发送按钮{i}" });
                }
            }
            DataContext = DataTemp;
            ReadAppConfig();
            this.SizeChanged += MainWindow_LocationChanged;
        }

        private bool ReadDataGrid()
        {

            string readValue = ConfigurationManager.AppSettings["tab_Index"];
            int tab_index = Convert.ToInt32(readValue);
            string tab_Name = ConfigurationManager.AppSettings["tab_Name"];
            readValue = ConfigurationManager.AppSettings[$"tabComboBox_Index{tab_index + 1}"];
            int sheet_index = Convert.ToInt32(readValue);
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            NowTabName = tab_Name;
            NextIndex = tab_index;
            NextIndexChild = sheet_index;
            string filePath = $".//config//{tab_Name}.db";
            if (ReadSqlData(filePath, sheet_index, tab_index))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ReadSqlData(string filePath, int sheet_index, int tab_index)
        {
            if (File.Exists(filePath))
            {
                DataTemp.Clear();
                DataTemp = ReadDataFromSQLite(filePath, $"sheet{sheet_index}", tab_index);
                if (DataTemp.Count == 0)
                {
                    return false;
                }
                else
                {
                    DataContext = DataTemp;
                    return true;
                }

            }
            else
            {
               // MessageBox.Show($"{filePath}:不存在！");
                return false;
            }
        }

        private void Button_Opinion_Click(object sender, RoutedEventArgs e)
        {
            //string feedback_form_url = "https://forms.office.com/r/sADsGZBFx8";
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = feedback_form_url,
            //    UseShellExecute = true
            //});
            //int tabIndex = GridTab.SelectedIndex;
            //DataGrid[] myDataGrid = { myDataGrid_1, myDataGrid_2, myDataGrid_3 };

            //DataGridCellInfo currentCell = myDataGrid[tabIndex].CurrentCell;
            //if (currentCell != null)
            //{
            //    int insertIndex = myDataGrid[tabIndex].Items.IndexOf(currentCell.Item);
            //    // 处理当前行的数据
            //}
            //读取配置文件，然后将该值传入。
            string readValue;
            // 重新读取配置文件
            ConfigurationManager.RefreshSection("appSettings");
            // 获取新的值
            readValue = ConfigurationManager.AppSettings["expect_read"];

            NoteWindow = new MyWindow(readValue);
            NoteWindow.Title = "阻塞发送接收期望值设置(循环发送有效)";
            // 位置在鼠标下方
            NoteWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // 获取鼠标光标的屏幕位置
            System.Drawing.Point cursorPos = System.Windows.Forms.Cursor.Position;

            // 设置注释窗口的位置为鼠标光标的位置
            NoteWindow.Left = cursorPos.X;
            NoteWindow.Top = cursorPos.Y;

            if (NoteWindow.ShowDialog() == true)
            {
                string expect_str = NoteWindow.MyTextBox.Text;
                //这里写入配置文件，然后再打开监听线程时，将该值传入。
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (mySerial[portNum] != null)
                { 
                    mySerial[portNum].expect_str = expect_str; 
                }

                config.AppSettings.Settings["expect_read"].Value = expect_str;
                config.Save(ConfigurationSaveMode.Modified);
                NoteWindow.Close();
            }
            else
            {
                NoteWindow.Close();
            }
        }


        private void Button_Clear_SendEdit_Click(object sender, RoutedEventArgs e)
        {
            TextBox[] Send_At_Edit = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"Send_At_Edit{i}")).ToArray();

            Console.WriteLine(portNum);
            Send_At_Edit[portNum].Clear();
        }

        private void Button_Display_Start_Windows_Click(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;

            Button[] Start_Button = Enumerable.Range(1, 16).Select(i => (Button)FindName($"Start_Button{i}")).ToArray();


            if (Start_Window == null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Start_Button[i].Content = "一键启动扩展>>";
                }
                Start_Window = new Start_Type_Window();

                Start_Window.ButtonClicked += ChildWindow_ButtonClicked;
                Start_Window.Closed += (s, args) => Start_Window = null; // 在窗口关闭时将实例变量重置为null
                                                                         //Start_Window.ReadAppConfig();

                Start_Window.Owner = this;
                Start_Window.Top = this.Top;
                Start_Window.Left = this.Left + this.Width;

                // 订阅主窗口的 LocationChanged 事件
                this.LocationChanged += MainWindow_LocationChanged;
                Start_Window.Show();
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    Start_Button[i].Content = "一键启动隐藏<<";
                }
                Start_Window.Close(); // 如果窗口已经创建，则将其激活
            }
           
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            // 当主窗口移动时，更新子窗口的位置
            if (Start_Window != null)
            {
                Start_Window.Top = this.Top;
                Start_Window.Left = this.Left + this.Width; // 改变左侧位置为主窗口的右侧
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            if (Start_Window != null)
            {
                Start_Window.Close();
            }

            SaveConfig();
            string TabName = (GridTab.SelectedItem as TabItem)?.Header.ToString();
            int tabIndex = GridTab.SelectedIndex;
          
           saveDataGrid(TabName, tabIndex);
            for (int i = 0; i < 16; i++)
            {
                isRunning_start[i] = false;
                isRunning_ack = false;
                isRunning_loopsend[i] = false;
                isRunning_looptimer[i] = false;
                if (mySerial[i] == null)
                    continue;
                else
                {
                    mySerial[i].ack_falg = false;
                    mySerial[i].CloseThread();
                    mySerial[i].block_falg = false;
                }
            }
            if (loopSendThread != null)
            {
                loopSendThread.Abort();
            }
            if (loopTimeSendThread != null)
            {
                loopTimeSendThread.Abort();
            }
            if (autoAckSendThread != null)//自动应答线程
            {
                isRunning_ack = false;
                autoAckSendThread.Abort();
            }
            if (mulitSendThread != null)
            {
                isRunning_mulit = false;
                mulitSendThread.Abort();
            }
        }
        private void SavefileCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string portNumber = SelectComPort();
            int port_num = PortTab.SelectedIndex;
            CheckBox[] savefileCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"savefileCheckBox{i}")).ToArray();
            if (savefileCheckBox[port_num].IsChecked ?? false)
            {
                DateTime now = DateTime.UtcNow;
                string timestamp = now.ToString("yyyy-MM-dd_HH_mm_ss_fff");
                string timefloder = now.ToString("yyyy-MM-dd");
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeFolderPath = Path.GetDirectoryName(exePath);
                string logpath = $"{exeFolderPath}\\log\\{timefloder}";
                _ = CreateDirectory(logpath);
                logFileName[portNum] = $"{logpath}\\{portNumber}_{timestamp}.log";
                if (mySerial[port_num] == null)
                {
                    return;
                }
                else
                {
                    mySerial[port_num].logFileName = logFileName[port_num];
                }
               
            }
            else
            {
                logFileName[port_num] = null;//为空时不保存
                if (mySerial[port_num] == null)
                {
                    return;
                }
                else
                {
                    mySerial[port_num].logFileName = logFileName[port_num];
                }
            }
           
        }

        private void StampCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            CheckBox[] stampCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"stampCheckBox{i}")).ToArray();
            mySerial[port_num].isStamp = stampCheckBox[port_num].IsChecked ?? false;
        }

        private void BlockSendCheck_Click(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            CheckBox[] blockSendCheck = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"blockSendCheck{i}")).ToArray();
            if (mySerial[port_num] == null)
            {
                return;
            }
            mySerial[port_num].isBlockSend = blockSendCheck[port_num].IsChecked ?? false;
        }

        private bool RetCheckStatus(string value)
        {
            if (value == "False")
                return false;
            else
                return true;
        }

        private void ReadAppConfig()
        {
            string readValue;
            //串口名称
            ComboBox[] portsComboBox = Enumerable.Range(1, 16).Select(i => (ComboBox)FindName($"portsComboBox{i}")).ToArray();
            //波特率
            ComboBox[] baudRateComboBox = Enumerable.Range(1, 16).Select(i => (ComboBox)FindName($"baudRateComboBox{i}")).ToArray();
            //AT指令
            TextBox[] Send_At_Edit = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"Send_At_Edit{i}")).ToArray();
            //阻塞发送
            CheckBox[] blockSendCheck = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"blockSendCheck{i}")).ToArray();
            //保存接收
            CheckBox[] savefileCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"savefileCheckBox{i}")).ToArray();
            //时间戳
            CheckBox[] stampCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"stampCheckBox{i}")).ToArray();
            //回车换行
            CheckBox[] enterCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"enterCheckBox{i}")).ToArray();
            //定时发送时间
            TextBox[] timerSendTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"timerSendTextBox{i}")).ToArray();
            //循环发送次数

            TextBox[] loopSendNumTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"loopSendNumTextBox{i}")).ToArray();
            //循环发送时间
            TextBox[] loopSendTimeTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"loopSendTimeTextBox{i}")).ToArray();
            for (int num = 0; num < 16; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"Serial_Name{num}"];
                portsComboBox[num].SelectedItem = readValue;
                readValue = ConfigurationManager.AppSettings[$"Serial_Rate{num}"];
                baudRateComboBox[num].Text = readValue;
                readValue = ConfigurationManager.AppSettings[$"Serial_AtCmd{num}"];
                Send_At_Edit[num].Text = readValue;
                readValue = ConfigurationManager.AppSettings[$"block_Checked{num}"];
                blockSendCheck[num].IsChecked = RetCheckStatus(readValue);
                readValue = ConfigurationManager.AppSettings[$"savefile_Checked{num}"];
                savefileCheckBox[num].IsChecked = RetCheckStatus(readValue);
                readValue = ConfigurationManager.AppSettings[$"stamp_Checked{num}"];
                stampCheckBox[num].IsChecked = RetCheckStatus(readValue);
                readValue = ConfigurationManager.AppSettings[$"enter_Checked{num}"];
                enterCheckBox[num].IsChecked = RetCheckStatus(readValue);
                readValue = ConfigurationManager.AppSettings[$"send_Timer{num}"];
                timerSendTextBox[num].Text = readValue;
                readValue = ConfigurationManager.AppSettings[$"loopSend_Num{num}"];
                loopSendNumTextBox[num].Text = readValue;
                readValue = ConfigurationManager.AppSettings[$"loopSend_Timer{num}"];
                loopSendTimeTextBox[num].Text = readValue;
            }

            readValue = ConfigurationManager.AppSettings["listButton_Status"];
            isChecked_at_grid_flag = RetCheckStatus(readValue);

            //Tab
            readValue = ConfigurationManager.AppSettings["tab_Index"];
            int index = Convert.ToInt32(readValue);
            GridTab.SelectedIndex = index;


            //tab combobox
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ConfigurationManager.AppSettings[$"tabComboBox_Index{i + 1}"];
                index = Convert.ToInt32(readValue);
                ATCmdComBox[i].SelectedIndex = index;
            }
  

            //指令隐藏
            readValue = ConfigurationManager.AppSettings["at_grid_flag"];
            isChecked_at_grid_flag = RetCheckStatus(readValue);

            //指令列表宽度
            readValue = ConfigurationManager.AppSettings["gridWidth"];
            gridWidth = Convert.ToDouble(readValue);

            readValue = ConfigurationManager.AppSettings["MainHeight"];
            this.Height = Convert.ToDouble(readValue);

            readValue = ConfigurationManager.AppSettings["MainWidth"];
            this.Width = Convert.ToDouble(readValue);

            Gridhide();


        }

        private void Gridhide()
        {
            Button[] AtCmdList_Button = Enumerable.Range(1, 16).Select(i => (Button)FindName($"AtCmdList_Button{i}")).ToArray();
            if (isChecked_at_grid_flag)
            {
                for (int i = 0; i < 16; i++)
                {
                    AtCmdList_Button[i].Content = "指令列表扩展>>";
                }
                MainGrid.ColumnDefinitions[2].Width = new GridLength(0);
                InvalidateVisual();
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    AtCmdList_Button[i].Content = "指令列表隐藏<<";
                }
                MainGrid.ColumnDefinitions[2].Width = new GridLength(gridWidth);
            }
        }

        private void SaveConfig()
        {
            string selectedValue;
            bool bselectedValue;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //串口名称
           ComboBox[] portsComboBox = Enumerable.Range(1, 16).Select(i => (ComboBox)FindName($"portsComboBox{i}")).ToArray();
            //波特率
            ComboBox[] baudRateComboBox = Enumerable.Range(1, 16).Select(i => (ComboBox)FindName($"baudRateComboBox{i}")).ToArray();
            //发送AT编辑框
            TextBox[] Send_At_Edit = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"Send_At_Edit{i}")).ToArray();
            //阻塞发送
            CheckBox[] blockSendCheck = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"blockSendCheck{i}")).ToArray();
            //保存接收
            CheckBox[] savefileCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"savefileCheckBox{i}")).ToArray();
            //时间戳
            CheckBox[] stampCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"stampCheckBox{i}")).ToArray();
            //回车换行
            CheckBox[] enterCheckBox = Enumerable.Range(1, 16).Select(i => (CheckBox)FindName($"enterCheckBox{i}")).ToArray();
            //定时发送时间
            TextBox[] timerSendTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"timerSendTextBox{i}")).ToArray();
            ////循环发送次数
            TextBox[] loopSendNumTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"loopSendNumTextBox{i}")).ToArray();
            //循环发送时间
            TextBox[] loopSendTimeTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"loopSendTimeTextBox{i}")).ToArray();
            for (int num = 0; num < 16; num++)
            {
                selectedValue = portsComboBox[num].SelectedValue.ToString();
                config.AppSettings.Settings[$"Serial_Name{num}"].Value = selectedValue;
                selectedValue = baudRateComboBox[num].Text;//由于可编辑所以要这样获取值
                config.AppSettings.Settings[$"Serial_Rate{num}"].Value = selectedValue;
                selectedValue = Send_At_Edit[num].Text;
                config.AppSettings.Settings[$"Serial_AtCmd{num}"].Value = selectedValue;
                bselectedValue = blockSendCheck[num].IsChecked ?? false;
                config.AppSettings.Settings[$"block_Checked{num}"].Value = bselectedValue.ToString();
                bselectedValue = savefileCheckBox[num].IsChecked ?? false;
                config.AppSettings.Settings[$"savefile_Checked{num}"].Value = bselectedValue.ToString();
                bselectedValue = stampCheckBox[num].IsChecked ?? false;
                config.AppSettings.Settings[$"stamp_Checked{num}"].Value = bselectedValue.ToString();
                bselectedValue = enterCheckBox[num].IsChecked ?? false;
                config.AppSettings.Settings[$"enter_Checked{num}"].Value = bselectedValue.ToString();
                selectedValue = timerSendTextBox[num].Text;
                config.AppSettings.Settings[$"send_Timer{num}"].Value = selectedValue;
                selectedValue = loopSendNumTextBox[num].Text;
                config.AppSettings.Settings[$"loopSend_Num{num}"].Value = selectedValue;
                selectedValue = loopSendTimeTextBox[num].Text;
                config.AppSettings.Settings[$"loopSend_Timer{num}"].Value = selectedValue;
            }

            //指令隐藏按键
            config.AppSettings.Settings["listButton_Status"].Value = isChecked_at_grid_flag.ToString();

            //Tab
            int index = GridTab.SelectedIndex;
            config.AppSettings.Settings["tab_Index"].Value = index.ToString();

            TabItem selectedTab = (TabItem)GridTab.ItemContainerGenerator.ContainerFromIndex(index);
            var title = selectedTab.Header.ToString();
            config.AppSettings.Settings["tab_Name"].Value = (string)title;

            //tab combobox
            ComboBox[] ATCmdComBox = { ATCmdComBox1, ATCmdComBox2, ATCmdComBox3 };
            for (int i = 0; i < 3; i++)
            {
                index = ATCmdComBox[i].SelectedIndex;
                config.AppSettings.Settings[$"tabComboBox_Index{i + 1}"].Value = index.ToString();

                var selectedItem = ATCmdComBox[i].SelectedItem as ComboBoxItem;
                var sheetName = selectedItem.Content;
                config.AppSettings.Settings[$"tabComboBox_Name{i + 1}"].Value = (string)sheetName;
            }


            selectedValue = isChecked_at_grid_flag.ToString();
            config.AppSettings.Settings["at_grid_flag"].Value = selectedValue;


            config.AppSettings.Settings["MainWidth"].Value = this.Width.ToString();

            config.AppSettings.Settings["MainHeight"].Value = this.Height.ToString();

            config.Save(ConfigurationSaveMode.Modified);
        }

        // 子窗口事件处理程序
        private void ChildWindow_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            // 处理事件的逻辑放在这里，可以使用接收到的字符串列表：e.StringList
            // port_num
            int port_num = PortTab.SelectedIndex;
            StartSendPara startPara;
            if (startSendThread != null)
            {
                isRunning_start[port_num] = false;
                if (startSendThread != null)
                {
                    startSendThread.Abort();
                }
            }

            if (OpenSerial_Clicked_Send())
            {
                if (!isRunning_start[port_num])
                {
                    startSendThread = new Thread(new ParameterizedThreadStart(StartSendThread_func));
                    startPara = new StartSendPara();
                    startPara.PortNum = port_num;
                    startPara.mySerial = mySerial[port_num];

                    startPara.StringList = e.StringList;
                    StringAtCmdList = e.StringList;
                    isRunning_start[port_num] = true;
                    mySerial[port_num].isRunWiota = true;

                    startSendThread.Start(startPara);
                }
                startSendThread = null;
                startPara = null;
            }
            else
            {
                isRunning_start[port_num] = false;
                if(startSendThread!=null)
                {
                   startSendThread.Abort();
                }        
            }
        }

        private void StartSendThread_func(object obj)//可以简化
        {
            StartSendPara parameters = (StartSendPara)obj;
           
            SerialPortClass mySerial = parameters.mySerial;
            List<string> AtCmdList = parameters.StringList;
            int port_num = parameters.PortNum;
            string At_cmd = "";
            foreach (string AtCmd in AtCmdList)
            {
                //Console.WriteLine(AtCmd);
                if (!isRunning_start[port_num])
                {
                    break;
                }
                // 向串口写入指令并添加回车换行符
                // 可选：等待一段时间以确保设备处理完指令
                this.Dispatcher.Invoke(new Action(() =>
                {

                    At_cmd = AtCmd + "\r\n";
                    Add_TimeStamp(At_cmd,port_num);
                }));
                mySerial.SendPortString(port_num, At_cmd);
                if (mySerial.isRunWiota)
                {               
                    while (mySerial.block_falg)
                    {
                       
                        if (!isRunning_start[port_num])
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    mySerial.block_falg = true;
                }
                Thread.Sleep(100);
            }
            mySerial.isRunWiota = false;
            isRunning_start[port_num] = false;
        }

        private void MulitSendCheckBox_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int port_num = PortTab.SelectedIndex;
            StartSendPara MulitPara;
            if (MulitSendCheckBox.IsChecked ?? false)
            {
                if (OpenSerial_Clicked_Send())
                {
                    mulitSendThread = new Thread(new ParameterizedThreadStart(mulitSendThread_func));
                    MulitPara = new StartSendPara();
                    MulitPara.mySerial = mySerial[port_num];
                    MulitPara.PortNum = port_num;
                    MulitPara.DataTemp = new List<GridDataTemp>(DataTemp); // 创建副本; 
                    mySerial[port_num].ack_falg = true;
                    //开启自动批量发送
                    isRunning_mulit = true;
                    mulitSendThread.Start(MulitPara);
                }
                else
                {
                    return;
                }
            }
            else
            {
                mySerial[port_num].ack_falg = false;
                isRunning_mulit = false;
                if (mulitSendThread != null)
                {
                    mulitSendThread.Abort();
                }
            }
        }

        private void mulitSendThread_func(object obj)
        {
            StartSendPara parameters = (StartSendPara)obj;
            SerialPortClass mySerial = parameters.mySerial;
            List<GridDataTemp> DataTemp = parameters.DataTemp;

            int port_num = parameters.PortNum;
            string At_cmd = "";
            while (isRunning_mulit)
            {
                for (int i = 0; i < DataTemp.Count; i++)
                {
                    if (!isRunning_mulit)
                    {
                        break;
                    }
                    if (DataTemp[i].Hex)
                    {
                        mySerial.ack_falg = true;
                        string sleep = DataTemp[i].延时;
                        // 向串口写入指令并添加回车换行符
                        // 可选：等待一段时间以确保设备处理完指令
                        string AtCmd = DataTemp[i].AT指令;
                        string output = AtCmd.Replace("\\r", "\r");
                        AtCmd = output.Replace("\\n", "\n");
                        int time = Convert.ToInt32(sleep);
                        if (time > 0)
                        {
                            Thread.Sleep(time);
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (enterCheckBox1.IsChecked ?? false)
                            {
                                At_cmd = AtCmd + "\r\n";
                            }
                            else
                            {
                                At_cmd = AtCmd;
                            }
                           
                            Add_TimeStamp(At_cmd, port_num);
                        }));
                        mySerial.SendPortString(port_num, At_cmd);
                    }
                }
            }
        }

        private void AutoAckCheckBox_Click(object sender, RoutedEventArgs e)  
        {
            int tabIndex = GridTab.SelectedIndex;
            int port_num = PortTab.SelectedIndex;
            StartSendPara AutoAckPara;
            if (AutoAckCheckBox.IsChecked??false)
            {
               
                if (OpenSerial_Clicked_Send())
                {
                    autoAckSendThread = new Thread(new ParameterizedThreadStart(AutoAckSendThread_func));
                    AutoAckPara = new StartSendPara();
                    AutoAckPara.mySerial = mySerial[port_num];
                    AutoAckPara.PortNum = port_num;
                    AutoAckPara.DataTemp = new List<GridDataTemp>(DataTemp); // 创建副本; ;
                    isRunning_ack = true;
                    mySerial[port_num].isAutokSend = true;
                    mySerial[port_num].ack_falg = true;
                    //开启自动应答的线程
                    autoAckSendThread.Start(AutoAckPara);
                }
                else
                {
                    return;
                }
            }
            else
            {
                mySerial[port_num].ack_falg = false;
                isRunning_ack = false;
                if (autoAckSendThread != null)
                {
                    autoAckSendThread.Abort();
                }
            }
        }
        private void AutoAckSendThread_func(object obj)
        {
            StartSendPara parameters = (StartSendPara)obj;
            SerialPortClass mySerial = parameters.mySerial;
            List<GridDataTemp> DataTemp = parameters.DataTemp;
            int port_num = parameters.PortNum;
            string At_cmd = "";
            while (isRunning_ack)
            {

                for (int i = 0; i < DataTemp.Count; i++)
                {
                    if (!isRunning_ack)
                    {
                        break;
                    }
                    if (DataTemp[i].Hex)
                    {
                        mySerial.expect_str = DataTemp[i].应答;
                        while (mySerial.ack_falg)
                        {
                            if (!isRunning_ack)
                            {
                                break;
                            }
                            Thread.Sleep(200);
                        }

                        mySerial.ack_falg = true;
                        string sleep = DataTemp[i].延时;
                        // 向串口写入指令并添加回车换行符
                        // 可选：等待一段时间以确保设备处理完指令
                        string AtCmd = DataTemp[i].AT指令;
                        string output = AtCmd.Replace("\\r", "\r");
                        AtCmd = output.Replace("\\n", "\n");
                        int time = Convert.ToInt32(sleep);
                        if (time > 0)
                        {
                            Thread.Sleep(time);
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (enterCheckBox1.IsChecked ?? false)
                            {
                                At_cmd = AtCmd + "\r\n";
                            }
                            else
                            {
                                At_cmd = AtCmd;
                            }
                            Add_TimeStamp(At_cmd, port_num);
                        }));
                        
                        mySerial.SendPortString(port_num, At_cmd);
                    }
                }
            }
        }
        private void GridCheckStatus_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            GridDataTemp dataObject = (GridDataTemp)checkBox.DataContext;
            dataObject.Hex = checkBox.IsChecked ?? false;

            var bindingExpression = checkBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource(); // 更新数据源
            }
        }

        private void Button_SaveLog_Click(object sender, RoutedEventArgs e)
        {
           string data =  RetTextBoxObject(portNum).Text;
           string result = data.Replace("\r", "").Replace("\n", "");
           int count = StringHelper.CountKeywords(result, "WIOTARECV");
            RetTextBoxObject(portNum).Text = $"{count}";
            mySerial[portNum].lenCount = 0;
        }

        private void OpenFileLocation(object sender, RoutedEventArgs e)
        {
            string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            //Process.Start("explorer.exe", appFolderPath);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = appFolderPath;
            startInfo.UseShellExecute = true;
            startInfo.Verb = "open";
            Process.Start(startInfo);
        }

        private void Button_GetLen_Click(object sender, RoutedEventArgs e)
        {
            GetSendData_Window = new GetSendData();
            GetSendData_Window.Show();
        }

        private void Button_Auto_Test_Click(object sender, RoutedEventArgs e)
        {
            if (MulitSendThread != null)
            {

                if (MulitSendThread.IsAlive)
                {
                    // 线程已经结束
                    TextBox[] recvDataRichTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"recvDataRichTextBox{i}")).ToArray();
                    DateTime now = DateTime.Now;
                    string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    recvDataRichTextBox[portNum].AppendText($"[{timestamp}] 测试线程已经开启，请勿重复点击\r");
                    recvDataRichTextBox[portNum].ScrollToEnd();
                    return;
                }
            }

            //第一步，获取一键启动的指令,需要进行一次一键启动
            if (StringAtCmdList == null)
            {
                TextBox[] recvDataRichTextBox = Enumerable.Range(1, 16).Select(i => (TextBox)FindName($"recvDataRichTextBox{i}")).ToArray();
                DateTime now = DateTime.Now;
                string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                recvDataRichTextBox[portNum].AppendText($"[{timestamp}] 指令列表为空，请先进行一次一键启动\r");
                recvDataRichTextBox[portNum].ScrollToEnd();
                return;
            }
            // 第二步判断所有已经打开的串口
            if (SerialList != null)
            {
                SerialList.Clear();
            }
              
            for (int i = 0; i < 16; i++)
            {
                SerialList.Add(mySerial[i]);
                if (mySerial[i] == null)
                    continue;
                isRunning_start[i] = true;
                mySerial[i].isRunWiota = true;
            }
            //第三步，遍历发送数据,在线程里开启这个测试，

           RunMultSendThread();

            //第三步，统计数据，然后又进行第二步，直至端口遍历完成
        }

        public void RunMultSendThread()
        {
            MulitSendThread = new Thread(new ParameterizedThreadStart(MulitSendThread_func));
            Mulitpara = new MulitSendPara
            {
                mySerialList = SerialList,
                StringList = StringAtCmdList,
                isTemptest_flag = isTemptest_flag,
            };
            MulitSendThread.Start(Mulitpara);
        }


        public void startMulitSend(List<SerialPortClass> mySerialList, List<string> AtCmdList)
        {
            int portIndex = -1;
            string At_cmd = "";
            foreach (SerialPortClass mySerial in mySerialList)
            {
                portIndex += 1;
                if (mySerial == null)
                {
                    continue;
                }
                foreach (string AtCmd in AtCmdList)
                {
                    //Console.WriteLine(AtCmd);
                    if (!isRunning_start[portIndex])
                    {
                        break;
                    }
                    // 向串口写入指令并添加回车换行符
                    // 可选：等待一段时间以确保设备处理完指令
                    this.Dispatcher.Invoke(new Action(() =>
                    {

                        At_cmd = AtCmd + "\r\n";
                        Add_TimeStamp(At_cmd, portIndex);
                    }));
                    mySerial.SendPortString(portIndex, At_cmd);
                    if (mySerial.isRunWiota)
                    {
                        while (mySerial.block_falg)
                        {

                            if (!isRunning_start[portIndex])
                            {
                                break;
                            }
                            Thread.Sleep(100);
                        }
                        mySerial.block_falg = true;
                    }
                    Thread.Sleep(50);
                }
                mySerial.isRunWiota = false;
                isRunning_start[portIndex] = false;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TextBox[] recvDataTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)this.FindName($"recvDataRichTextBox{k}")).ToArray();
                    recvDataTextBox[0].AppendText($"=====端口{portIndex}结束一键启动=====\r\n");
                    recvDataTextBox[0].ScrollToEnd();
                }));
            }

        }

        public void SendloopMulit(List<SerialPortClass> mySerialList)
        {
            int portIndex = -1;
            string loopcount = string.Empty;
            string sleep = string.Empty;
            foreach (SerialPortClass mySerial in mySerialList)
            {
                portIndex += 1;
                if (mySerial == null)
                {
                    continue;
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    TextBox[] loopSendNumTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)FindName($"loopSendNumTextBox{k}")).ToArray();
                    TextBox[] loopSendTimeTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)FindName($"loopSendTimeTextBox{k}")).ToArray();
                    loopcount = loopSendNumTextBox[portIndex].Text;
                    sleep = loopSendTimeTextBox[portIndex].Text;
                }));
                try
                {
                    int num = Convert.ToInt32(loopcount);
                    int sleep_time = Convert.ToInt32(sleep);
                    for (int i = 0; i < num; i++)
                    {
                        if (!isRunning_loopsend[portIndex])
                        {
                            break;
                        }
                        // 在UI线程上下文中更新UI控件
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TextBox[] recvDataTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)this.FindName($"recvDataRichTextBox{k}")).ToArray();
                            recvDataTextBox[portIndex].AppendText($"=====开始第{i + 1}次发送=====\r\n");
                            recvDataTextBox[portIndex].ScrollToEnd();
                            if (logFileName[portIndex] != null)
                            {
                                using (StreamWriter writer = new StreamWriter(logFileName[portIndex], true))
                                {
                                    writer.WriteLine($" ===== 开始第{ i + 1}次发送 =====\r\n");
                                }
                            }
                        }));

                        string AtCmd = string.Empty;

                        // 在UI线程上下文中访问和获取UI控件的值
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TextBox[] Send_At_Edit = Enumerable.Range(1, 16).Select(k => (TextBox)FindName($"Send_At_Edit{k}")).ToArray();
                            CheckBox[] enterCheckBox = Enumerable.Range(1, 16).Select(k => (CheckBox)FindName($"enterCheckBox{k}")).ToArray();
                            CheckBox[] loopSendCheck = Enumerable.Range(1, 16).Select(k => (CheckBox)FindName($"loopSendCheck{k}")).ToArray();
                            AtCmd = Send_At_Edit[portIndex].Text;
                            string output = AtCmd.Replace("\\r", "\r");
                            AtCmd = output.Replace("\\n", "\n");
                            if (enterCheckBox[portIndex].IsChecked ?? false)
                            {
                                AtCmd += "\r\n";
                            }
                            Add_TimeStamp(AtCmd, portIndex);
                        }));
                        mySerialWrite(portIndex, AtCmd);

                        if (mySerial.isBlockSend)
                        {
                            mySerial.block_falg = true;
                            while (mySerial.block_falg)
                            {

                                if (!isRunning_loopsend[portIndex])
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

                    //统计并且写入文件
                    CountRecv_WriteCSV(mySerialList);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TextBox[] recvDataTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)this.FindName($"recvDataRichTextBox{k}")).ToArray();
                        recvDataTextBox[0].AppendText($"=====端口{portIndex}结束循环发送=====\r\n");
                        recvDataTextBox[0].ScrollToEnd();
                    }));

                }
                catch (OverflowException)
                {
                    MessageBox.Show("输出数值超过最大值：2147483647", "警告");
                }
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TextBox[] recvDataTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)FindName($"recvDataRichTextBox{k}")).ToArray();
                        CheckBox[] loopSendCheck = Enumerable.Range(1, 16).Select(k => (CheckBox)FindName($"loopSendCheck{k}")).ToArray();
                        loopSendCheck[portIndex].IsChecked = false;
                        recvDataTextBox[portIndex].ScrollToEnd();
                    }));
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    return;
                }
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                TextBox[] recvDataTextBox = Enumerable.Range(1, 16).Select(k => (TextBox)this.FindName($"recvDataRichTextBox{k}")).ToArray();
                recvDataTextBox[0].AppendText($"=====自动化测试结束=====\r\n");
                recvDataTextBox[0].ScrollToEnd();
            }));
        }

        public void CountRecv_WriteCSV(List<SerialPortClass> mySerialList)
        {
            int portIndex = -1;
            List<string> recvCount = new List<string>();

            foreach (SerialPortClass mySerial in mySerialList)
            {
                portIndex += 1;
                if (mySerial == null)
                {
                    continue;
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    var textBox = RetTextBoxObject(portIndex);
                    if (textBox != null) // 确保 textBox 不为 null
                    {
                        string data = textBox.Text;
                        string result = data.Replace("\r", "").Replace("\n", "");
                        int count = StringHelper.CountKeywords(result, "WIOTARECV");
                        recvCount.Add($"{count}");
                        textBox.Text = $"{count}";
                        mySerial.lenCount = 0; // 确保 mySerial 已初始化
                    }
                }));
            }
            // 确保 recvCount 不为空
            if (recvCount.Count > 0)
            {
                WriteCsv(".\\自动测试报告.csv",recvCount);
            }
        }


        private void MulitSendThread_func(object obj)//可以简化
        {
            MulitSendPara Mulitpara = (MulitSendPara)obj;
            List <SerialPortClass> mySerialList = Mulitpara.mySerialList;
            List<string> AtCmdList = Mulitpara.StringList;
            bool isTemptest_flag = Mulitpara.isTemptest_flag;
            //一键启动
            startMulitSend(mySerialList, AtCmdList);
            // 循环发送
            if (!isTemptest_flag)
            {
                SendloopMulit(mySerialList);
            }
                
        }

        private void stopMulitWIoTattTread_fuc(object obj)
        {
            MulitSendPara Mulitpara = (MulitSendPara)obj;
            List<SerialPortClass> mySerialList = Mulitpara.mySerialList;
            //一键关闭协议栈
            stopMulitWIoTa(mySerialList);
        }

        public void stopMulitWIoTa(List<SerialPortClass> mySerialList)
        {
            int portIndex = -1;
            string At_cmd = "";
            foreach (SerialPortClass mySerial in mySerialList)
            {
                portIndex += 1;
                if (mySerial == null)
                {
                    continue;
                }
                //Console.WriteLine(AtCmd);
                if (!isRunning_start[portIndex])
                {
                    break;
                }
                // 向串口写入指令并添加回车换行符
                // 可选：等待一段时间以确保设备处理完指令
                this.Dispatcher.Invoke(new Action(() =>
                {
                    At_cmd = "AT+WIOTARUN=0" + "\r\n";
                    Add_TimeStamp(At_cmd, portIndex);
                }));
                mySerial.SendPortString(portIndex, At_cmd);
                while (mySerial.block_falg)
                {
                    Thread.Sleep(100);
                }
                mySerial.block_falg = true;
                Thread.Sleep(50);
            }
        }

        //关闭协议栈
        private void StopWiotaRun(object sender, RoutedEventArgs e)
        {
            StopMulitWIoTaThread = new Thread(new ParameterizedThreadStart(stopMulitWIoTattTread_fuc));
            if (SerialList != null)
            {
                SerialList.Clear();
            }

            for (int i = 0; i < 16; i++)
            {
                SerialList.Add(mySerial[i]);
                if (mySerial[i] == null)
                    continue;
                isRunning_start[i] = true;
                mySerial[i].isRunWiota = true;
            }
            MulitSendPara stopMulitpara = new MulitSendPara
            {
                mySerialList = SerialList,
            };
            StopMulitWIoTaThread.Start(stopMulitpara);
        }

        private void WriteCsv(string filePath, List<string> data)
        {
            using (var writer = new StreamWriter(filePath, append: true)) // 设置 append 为 true
            {
                writer.WriteLine(string.Join(",", data)); // 使用逗号分隔
            }
        }

        private void TemperatureCurveTest_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                // 切换复选框状态
                menuItem.IsChecked = !menuItem.IsChecked;

                // 在这里添加你想要执行的操作
                if (menuItem.IsChecked == true)
                {
                    // 选中状态的操作
                    isTemptest_flag = true;             
                }
                else
                {
                    isTemptest_flag = false;
                    // 取消选中状态的操作                  
                }
            }
        }

    }

    public class GridDataTemp
    {
        public int NumCol { get; set; }
        public bool Hex { get; set; }
        public string 延时 { get; set; }
        public string 应答 { get; set; }
        public string AT指令 { get; set; }
        public string 发送 { get; set; }
    }

    public class LoopSendPara
    {
        public SerialPortClass mySerial;
        public int PortNum { get; set; }
    }

    public class MulitSendPara
    {
        public List<SerialPortClass> mySerialList { get; set; }
        public List<string> StringList { get; set; }
        public bool isTemptest_flag { get; set; }
        // 构造函数
        public MulitSendPara()
        {
            mySerialList = new List<SerialPortClass>(); // 初始化列表
        }
    }

    public class StartSendPara
    {
        public SerialPortClass mySerial;
        public List<string> StringList { get; set; }
        public int PortNum { get; set; }
        public List<GridDataTemp> DataTemp;
    }

    public class StringHelper
    {
        public static int CountKeywords(string input, string keyword)
        {
            // 使用正则表达式匹配关键字，并计数匹配的次数
            Regex regex = new Regex(keyword, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(input);
            return matches.Count;
        }
    }
}














