using System;
using System.Collections.Generic;
using System.Windows;
using NPOI.SS.UserModel;
using System.Configuration;
using SerialPortExample;
using System.ComponentModel;
using System.Threading;
using NoteWindow;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace WIoTa_Serial_Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
 
    public partial class MainWindow : Window
    {
        bool isChecked_at_grid_flag = false;
        double gridWidth;
        private Start_Type_Window Start_Window; // 声明窗口实例变量
        private string logFileName;
        private List<GridDataTemp> DataTemp;

       // private IWorkbook workbook;
        private ISheet[] sheets;
        private SerialPortManager[] mySerial = new SerialPortManager[8];
        private Thread loopSendThread;
        private LoopSendPara parameters;
        private StartSendPara startPara;
        private StartSendPara AutoAckPara;
        private StartSendPara MulitPara;
        private MyWindow NoteWindow;
        private Thread loopTimeSendThread;
        private Thread startSendThread;
        private Thread autoAckSendThread;
        private Thread mulitSendThread;
        private List<string> selectedPorts = new List<string>()
{
    "Port1",
    "Port2",
    "Port3",
    "Port4",
    "Port5",
    "Port6",
    "Port7",
    "Port8"
};
        private int portNum = 0;
        string NowTabName;
        int NextIndex;
        private List<bool> isRunning_start = new List<bool> { true, true, true, true, true, true, true, true};
        private bool isRunning_ack = true; //自动应答标志位
        private bool isRunning_mulit = true; //批量发送标志位
        private List<bool> isRunning_loopsend = new List<bool> { true, true, true, true, true, true, true, true };
        private List<bool> isRunning_looptimer = new List<bool> { true, true, true, true, true, true, true, true };
        private DispatcherTimer clockTimer = new DispatcherTimer();

        //[DllImport("MFCLibrarySerial.dll", EntryPoint = "Opne_serial", CharSet = CharSet.Ansi)]
        // public static extern bool Opne_serial(string portName,int baud);

        public MainWindow()
        {
            InitializeComponent();
            //clockTimer.Tick += ClockTimer_Tick;
            //clockTimer.Start();//CPU消耗10%
            TextBox[] recvDataTextBox = { recvDataRichTextBox1, recvDataRichTextBox2, recvDataRichTextBox3, recvDataRichTextBox4, recvDataRichTextBox5, recvDataRichTextBox6, recvDataRichTextBox7, recvDataRichTextBox8 };
            for (int k = 0; k < 8; k++)
            {
                recvDataTextBox[k].UndoLimit = 0;
            }
    
            DisplayPort();
            //这个位置有错误风险
            int itemCount = ATCmdComBox1.Items.Count;
            sheets = new ISheet[itemCount];

            DataTemp = new List<GridDataTemp>();
            if (!ReadDataGrid())
            {
                for (int i = 1; i <= 60; i++)
                {
                    DataTemp.Add(new GridDataTemp { Index = i, Hex = false, 应答 = "OK", 延时 = "1000", AT指令 = "AT", 发送 = $"发送按钮{i}" });
                }
            }
           
            DataContext = DataTemp;
            ReadAppConfig();
            //recvDataRichTextBox.ScrollToEnd();
        }
        private bool ReadDataGrid()
        {
            string readValue = ConfigurationManager.AppSettings["tab_Index"];
            int tab_index = Convert.ToInt32(readValue);
            string tab_Name = ConfigurationManager.AppSettings["tab_Name"];
            string tabComboBox_Name = ConfigurationManager.AppSettings[$"tabComboBox_Name{tab_index + 1}"];
            string filePath = $".//config//{tab_Name}.xlsx";
            NowTabName = tab_Name;
            NextIndex = tab_index;
            if (File.Exists(filePath))
            {
                DataTemp.Clear();
                
                DataTemp = ReadExcel(filePath, tabComboBox_Name, tab_index);
                DataContext = DataTemp;
                return true;
            }
            else
            {
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
            Send_At_Edit1.Clear();
        }

        private void Button_Display_Start_Windows_Click(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
         
            if (Start_Window == null)
            {

                Start_Window = new Start_Type_Window();

                Start_Window.ButtonClicked += ChildWindow_ButtonClicked;
                Start_Window.serialPort = mySerial[port_num];
                Start_Window.Closed += (s, args) => Start_Window = null; // 在窗口关闭时将实例变量重置为null
                                                                            //Start_Window.ReadAppConfig();
                Start_Window.Show();
            }
            else
            {
                Start_Window.Activate(); // 如果窗口已经创建，则将其激活
            }
           
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            if (Start_Window != null)
            {
                Start_Window.Close();
            }
            //这里需要判断每一个然后进行关闭
            //mySerial[0].CloseThread();
           // mySerial[0].block_falg = false;
            SaveConfig();
            string TabName = (GridTab.SelectedItem as TabItem)?.Header.ToString();
            int tabIndex = GridTab.SelectedIndex;
            saveDataGrid(TabName, tabIndex);

            for (int i = 0; i < 8; i++)
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
            if (loopSendThread !=null)
            {
                loopSendThread.Abort();
            }
            if (autoAckSendThread!=null)//自动应答线程
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
            CheckBox[] savefileCheckBox = { savefileCheckBox1 , savefileCheckBox2 , savefileCheckBox3 , savefileCheckBox4 , savefileCheckBox5
            , savefileCheckBox6 , savefileCheckBox7 , savefileCheckBox8 };

            if (savefileCheckBox[port_num].IsChecked ?? false)
            {
                DateTime now = DateTime.UtcNow;
                string timestamp = now.ToString("yyyy-MM-dd_HH_mm_ss_fff");
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeFolderPath = Path.GetDirectoryName(exePath);

                logFileName = $"{exeFolderPath}\\log\\{portNumber}_{timestamp}.log";
                if (mySerial[port_num] == null)
                {
                    return;
                }
                else
                {
                    mySerial[port_num].logFileName = logFileName;
                }
               
            }
            else
            {
                logFileName = null;//为空时不保存
                if (mySerial[port_num] == null)
                {
                    return;
                }
                else
                {
                    mySerial[port_num].logFileName = logFileName;
                }
            }
           
        }

        private void StampCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            mySerial[port_num].isStamp = stampCheckBox1.IsChecked ?? false;
        }

        private void BlockSendCheck_Click(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            CheckBox[] blockSendCheck = { blockSendCheck1 , blockSendCheck2 , blockSendCheck3 , blockSendCheck4 , blockSendCheck5 ,
                blockSendCheck6 , blockSendCheck7 , blockSendCheck8 };
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
            readValue = ConfigurationManager.AppSettings["Serial_Name"];
            portsComboBox1.SelectedItem = readValue;

            //波特率
            readValue = ConfigurationManager.AppSettings["Serial_Rate"];
            baudRateComboBox1.Text = readValue;

            //AT指令
            readValue = ConfigurationManager.AppSettings["Serial_AtCmd"];
            Send_At_Edit1.Text = readValue;

            //阻塞发送
            readValue = ConfigurationManager.AppSettings["block_Checked"];
            blockSendCheck1.IsChecked = RetCheckStatus(readValue);


            //保存接收
            readValue = ConfigurationManager.AppSettings["savefile_Checked"];
            savefileCheckBox1.IsChecked = RetCheckStatus(readValue);
            
            //时间戳
            readValue = ConfigurationManager.AppSettings["stamp_Checked"];
            stampCheckBox1.IsChecked = RetCheckStatus(readValue);

            //回车换行
            readValue = ConfigurationManager.AppSettings["enter_Checked"];
            enterCheckBox1.IsChecked = RetCheckStatus(readValue);

            //定时发送时间
            readValue = ConfigurationManager.AppSettings["send_Timer"];
            timerSendTextBox1.Text = readValue;

            //循环发送次数
            readValue = ConfigurationManager.AppSettings["loopSend_Num"];
            loopSendNumTextBox1.Text = readValue;

            //循环发送时间
            readValue = ConfigurationManager.AppSettings["loopSend_Timer"];
            loopSendTimeTextBox1.Text = readValue;

            //指令隐藏按键
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
            Button[] AtCmdList_Button = { AtCmdList_Button1, AtCmdList_Button2 , AtCmdList_Button3 , AtCmdList_Button4 , AtCmdList_Button5 ,
                AtCmdList_Button6 , AtCmdList_Button7 , AtCmdList_Button8 };
            if (isChecked_at_grid_flag)
            {
                for (int i = 0; i < 8; i++)
                {
                    AtCmdList_Button[i].Content = "指令列表扩展>>";
                }
                MainGrid.ColumnDefinitions[2].Width = new GridLength(0);
                InvalidateVisual();
            }
            else
            {
                for (int i = 0; i < 8; i++)
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
            selectedValue = portsComboBox1.SelectedValue.ToString();
            config.AppSettings.Settings["Serial_Name"].Value = selectedValue;

            //波特率
            //var selectedItem = baudRateComboBox.SelectedItem as ComboBoxItem;
            //var sheetName = selectedItem.Content;
            selectedValue = baudRateComboBox1.Text;//由于可编辑所以要这样获取值
            config.AppSettings.Settings["Serial_Rate"].Value = selectedValue;

            //发送AT编辑框
            selectedValue = Send_At_Edit1.Text;
            config.AppSettings.Settings["Serial_AtCmd"].Value = selectedValue;

            //阻塞发送
            bselectedValue = blockSendCheck1.IsChecked ?? false;
            config.AppSettings.Settings["block_Checked"].Value = bselectedValue.ToString();

            //保存接收
            bselectedValue = savefileCheckBox1.IsChecked ?? false;
            config.AppSettings.Settings["savefile_Checked"].Value = bselectedValue.ToString();

            //保存接收
            bselectedValue = stampCheckBox1.IsChecked ?? false;
            config.AppSettings.Settings["stamp_Checked"].Value = bselectedValue.ToString();

            //回车换行
            bselectedValue = enterCheckBox1.IsChecked ?? false;
            config.AppSettings.Settings["enter_Checked"].Value = bselectedValue.ToString();

            //定时发送时间
            selectedValue = timerSendTextBox1.Text;
            config.AppSettings.Settings["send_Timer"].Value = selectedValue;

            //循环发送次数
            selectedValue = loopSendNumTextBox1.Text;
            config.AppSettings.Settings["loopSend_Num"].Value = selectedValue;

            //循环发送时间
            selectedValue = loopSendTimeTextBox1.Text;
            config.AppSettings.Settings["loopSend_Timer"].Value = selectedValue;

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
                config.AppSettings.Settings[$"tabComboBox_Index{i+1}"].Value = index.ToString();

                var selectedItem = ATCmdComBox[i].SelectedItem as ComboBoxItem;
                var sheetName = selectedItem.Content;
                config.AppSettings.Settings[$"tabComboBox_Name{i+1}"].Value = (string)sheetName;
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
            if (OpenSerial_Clicked_Send())
            {
                startSendThread = new Thread(new ParameterizedThreadStart(StartSendThread_func));
                startPara = new StartSendPara();
                startPara.PortNum = port_num;
                startPara.mySerial = mySerial[port_num];
               
                startPara.StringList = e.StringList;
                isRunning_start[port_num] = true;
                mySerial[port_num].isRunWiota = true;

                startSendThread.Start(startPara);
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
           
            SerialPortManager mySerial = parameters.mySerial;
            List<string> AtCmdList = parameters.StringList;
            int port_num = parameters.PortNum;
            string At_cmd = "";
            foreach (string AtCmd in AtCmdList)
            {
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
                _ = mySerial.SendPortString(port_num, At_cmd);
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
               
            }
            mySerial.isRunWiota = false;
        }

        private void MulitSendCheckBox_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = GridTab.SelectedIndex;
            int port_num = PortTab.SelectedIndex;
            if (MulitSendCheckBox.IsChecked ?? false)
            {
                if (OpenSerial_Clicked_Send())
                {
                    mulitSendThread = new Thread(new ParameterizedThreadStart(mulitSendThread_func));
                    MulitPara = new StartSendPara();
                    MulitPara.mySerial = mySerial[port_num];
                    MulitPara.PortNum = port_num;
                    MulitPara.DataTemp = DataTemp;
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
            SerialPortManager mySerial = parameters.mySerial;
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
                        _ = mySerial.SendPortString(port_num, At_cmd);
                    }
                }
            }
        }

        private void AutoAckCheckBox_Click(object sender, RoutedEventArgs e)  
        {
            int tabIndex = GridTab.SelectedIndex;
            int port_num = PortTab.SelectedIndex;
            if (AutoAckCheckBox.IsChecked??false)
            {
               
                if (OpenSerial_Clicked_Send())
                {
                    autoAckSendThread = new Thread(new ParameterizedThreadStart(AutoAckSendThread_func));
                    AutoAckPara = new StartSendPara();
                    AutoAckPara.mySerial = mySerial[port_num];
                    AutoAckPara.PortNum = port_num;
                    AutoAckPara.DataTemp = DataTemp;
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
            SerialPortManager mySerial = parameters.mySerial;
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
                        
                        _ = mySerial.SendPortString(port_num, At_cmd);
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

           
        }
    }

    public class GridDataTemp
    {
        public int Index { get; set; }
        public bool Hex { get; set; }
        public string 延时 { get; set; }
        public string 应答 { get; set; }
        public string AT指令 { get; set; }
        public string 发送 { get; set; }

    }

    public class LoopSendPara
    {
        public SerialPortManager mySerial;
        public int PortNum { get; set; }
    }

    public class StartSendPara
    {
        public SerialPortManager mySerial;
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














