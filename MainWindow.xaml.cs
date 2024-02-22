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
        private List<string> logFileName = new List<string> { "log1", "log2", "log3", "log4", "log5", "log6", "log7", "log8" };
        private List<string> selectedPorts = new List<string> { "Port1", "Port2", "Port3", "Port4", "Port5", "Port6", "Port7", "Port8" };
        private List<bool> isRunning_start = Enumerable.Repeat(false, 8).ToList();
        private List<bool> isRunning_loopsend = Enumerable.Repeat(true, 8).ToList();
        private List<bool> isRunning_looptimer = Enumerable.Repeat(true, 8).ToList();
  
        private List<GridDataTemp> DataTemp;
        private SerialPortClass []mySerial = new SerialPortClass[8];
        private Thread loopSendThread;
        private LoopSendPara parameters;
        private MyWindow NoteWindow;
        private Thread loopTimeSendThread;
        private Thread startSendThread;
        private Thread autoAckSendThread;
        private Thread mulitSendThread;

        private int portNum = 0;
        string NowTabName;
        int NextIndex;
        int NextIndexChild;
        private bool isRunning_ack = true; //自动应答标志位
        private bool isRunning_mulit = true; //批量发送标志位
        private bool test_falg = false;
        public MainWindow()
        {
            InitializeComponent();
            test_falg = true;
            TextBox[] recvDataTextBox = { recvDataRichTextBox1, recvDataRichTextBox2, recvDataRichTextBox3, recvDataRichTextBox4, recvDataRichTextBox5, recvDataRichTextBox6, recvDataRichTextBox7, recvDataRichTextBox8 };
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
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };

            Send_At_Edit[portNum].Clear();
        }

        private void Button_Display_Start_Windows_Click(object sender, RoutedEventArgs e)
        {
            int port_num = PortTab.SelectedIndex;
            Button[] Start_Button = { Start_Button1, Start_Button2, Start_Button3, Start_Button4,
            Start_Button5,Start_Button6,Start_Button7,Start_Button8};
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
                Start_Window.Left = this.Left + this.Width - 10;

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
                Start_Window.Left = this.Left + this.Width - 10; // 改变左侧位置为主窗口的右侧
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
            CheckBox[] savefileCheckBox = { savefileCheckBox1 , savefileCheckBox2 , savefileCheckBox3 , savefileCheckBox4 , savefileCheckBox5
            , savefileCheckBox6 , savefileCheckBox7 , savefileCheckBox8 };

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
            ComboBox[] portsComboBox = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"Serial_Name{num}"];
                portsComboBox[num].SelectedItem = readValue;
            }
          

            //波特率
            ComboBox[] baudRateComboBox = { baudRateComboBox1, baudRateComboBox2, baudRateComboBox3, baudRateComboBox4, baudRateComboBox5, baudRateComboBox6, baudRateComboBox7, baudRateComboBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"Serial_Rate{num}"];
                baudRateComboBox[num].Text = readValue;
            }

            //AT指令
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"Serial_AtCmd{num}"];
                Send_At_Edit[num].Text = readValue;
            }

            //阻塞发送
            CheckBox[] blockSendCheck = { blockSendCheck1, blockSendCheck2, blockSendCheck3, blockSendCheck4, blockSendCheck5, blockSendCheck6, blockSendCheck7, blockSendCheck8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"block_Checked{num}"];
                blockSendCheck[num].IsChecked = RetCheckStatus(readValue);
            }

            //保存接收
            CheckBox[] savefileCheckBox = { savefileCheckBox1, savefileCheckBox2, savefileCheckBox3, savefileCheckBox4, savefileCheckBox5, savefileCheckBox6, savefileCheckBox7, savefileCheckBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"savefile_Checked{num}"];
                savefileCheckBox[num].IsChecked = RetCheckStatus(readValue);
            }

            //时间戳
            CheckBox[] stampCheckBox = { stampCheckBox1, stampCheckBox2, stampCheckBox3, stampCheckBox4, stampCheckBox5, stampCheckBox6, stampCheckBox7, stampCheckBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"stamp_Checked{num}"];
                stampCheckBox[num].IsChecked = RetCheckStatus(readValue);
            }

            //回车换行
            CheckBox[] enterCheckBox = { enterCheckBox1, enterCheckBox2, enterCheckBox3, enterCheckBox4, enterCheckBox5, enterCheckBox6, enterCheckBox7, enterCheckBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"enter_Checked{num}"];
                enterCheckBox[num].IsChecked = RetCheckStatus(readValue);
            }

            //定时发送时间
            TextBox[] timerSendTextBox = { timerSendTextBox1, timerSendTextBox2, timerSendTextBox3, timerSendTextBox4,
                timerSendTextBox5, timerSendTextBox6, timerSendTextBox7, timerSendTextBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"send_Timer{num}"];
                timerSendTextBox[num].Text = readValue;
            }


            //循环发送次数
            TextBox[] loopSendNumTextBox = { loopSendNumTextBox1, loopSendNumTextBox2, loopSendNumTextBox3, loopSendNumTextBox4,
                loopSendNumTextBox5, loopSendNumTextBox6, loopSendNumTextBox7, loopSendNumTextBox8 };
            for (int num = 0; num < 8; num++)
            {
                readValue = ConfigurationManager.AppSettings[$"loopSend_Num{num}"];
                loopSendNumTextBox[num].Text = readValue;
            }

            //循环发送时间
            TextBox[] loopSendTimeTextBox = { loopSendTimeTextBox1, loopSendTimeTextBox2, loopSendTimeTextBox3, loopSendTimeTextBox4,
                loopSendTimeTextBox5, loopSendTimeTextBox6, loopSendTimeTextBox7, loopSendTimeTextBox8 };
            for (int num = 0; num < 8; num++)
            {
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
            ComboBox[] portsComboBox = { portsComboBox1, portsComboBox2, portsComboBox3, portsComboBox4, portsComboBox5, portsComboBox6, portsComboBox7, portsComboBox8 };
            for (int num = 0; num < 8; num++)
            {
                selectedValue = portsComboBox[num].SelectedValue.ToString();
                config.AppSettings.Settings[$"Serial_Name{num}"].Value = selectedValue;
            }

            //波特率
            ComboBox[] baudRateComboBox = { baudRateComboBox1, baudRateComboBox2, baudRateComboBox3, baudRateComboBox4, baudRateComboBox5, baudRateComboBox6, baudRateComboBox7, baudRateComboBox8 };
            for (int num = 0; num < 8; num++)
            {
                selectedValue = baudRateComboBox[num].Text;//由于可编辑所以要这样获取值
                config.AppSettings.Settings[$"Serial_Rate{num}"].Value = selectedValue;
            }

            //发送AT编辑框
            TextBox[] Send_At_Edit = { Send_At_Edit1, Send_At_Edit2, Send_At_Edit3, Send_At_Edit4, Send_At_Edit5, Send_At_Edit6, Send_At_Edit7, Send_At_Edit8 };
            for (int num = 0; num < 8; num++)
            {
                selectedValue = Send_At_Edit[num].Text;
                config.AppSettings.Settings[$"Serial_AtCmd{num}"].Value = selectedValue;
            }

            //阻塞发送
            CheckBox[] blockSendCheck = { blockSendCheck1, blockSendCheck2, blockSendCheck3, blockSendCheck4, blockSendCheck5, blockSendCheck6, blockSendCheck7, blockSendCheck8 };
            for (int num = 0; num < 8; num++)
            {
                bselectedValue = blockSendCheck[num].IsChecked ?? false;
                config.AppSettings.Settings[$"block_Checked{num}"].Value = bselectedValue.ToString();
            }

            //保存接收
            CheckBox[] savefileCheckBox = { savefileCheckBox1, savefileCheckBox2, savefileCheckBox3, savefileCheckBox4, savefileCheckBox5, savefileCheckBox6, savefileCheckBox7, savefileCheckBox8 };
            for (int num = 0; num < 8; num++)
            {
                bselectedValue = savefileCheckBox[num].IsChecked ?? false;
                config.AppSettings.Settings[$"savefile_Checked{num}"].Value = bselectedValue.ToString();
            }

            //时间戳
            CheckBox[] stampCheckBox = { stampCheckBox1, stampCheckBox2, stampCheckBox3, stampCheckBox4, stampCheckBox5, stampCheckBox6, stampCheckBox7, stampCheckBox8 };
            for (int num = 0; num < 8; num++)
            {
                bselectedValue = stampCheckBox[num].IsChecked ?? false;
                config.AppSettings.Settings[$"stamp_Checked{num}"].Value = bselectedValue.ToString();
            }


            //回车换行
            CheckBox[] enterCheckBox = { enterCheckBox1, enterCheckBox2, enterCheckBox3, enterCheckBox4, enterCheckBox5, enterCheckBox6, enterCheckBox7, enterCheckBox8 };
            for (int num = 0; num < 8; num++)
            {
                bselectedValue = enterCheckBox[num].IsChecked ?? false;
                config.AppSettings.Settings[$"enter_Checked{num}"].Value = bselectedValue.ToString();
            }

            //定时发送时间
            TextBox[] timerSendTextBox = { timerSendTextBox1, timerSendTextBox2, timerSendTextBox3, timerSendTextBox4,
                timerSendTextBox5, timerSendTextBox6, timerSendTextBox7, timerSendTextBox8 };
            for (int num = 0; num < 8; num++)
            {
                selectedValue = timerSendTextBox[num].Text;
                config.AppSettings.Settings[$"send_Timer{num}"].Value = selectedValue;
            }


            //循环发送次数
            TextBox[] loopSendNumTextBox = { loopSendNumTextBox1, loopSendNumTextBox2, loopSendNumTextBox3, loopSendNumTextBox4,
                loopSendNumTextBox5, loopSendNumTextBox6, loopSendNumTextBox7, loopSendNumTextBox8 };
            for (int num = 0; num < 8; num++)
            {
                selectedValue = loopSendNumTextBox[num].Text;
                config.AppSettings.Settings[$"loopSend_Num{num}"].Value = selectedValue;
            }

            //循环发送时间
            TextBox[] loopSendTimeTextBox = { loopSendTimeTextBox1, loopSendTimeTextBox2, loopSendTimeTextBox3, loopSendTimeTextBox4,
                loopSendTimeTextBox5, loopSendTimeTextBox6, loopSendTimeTextBox7, loopSendTimeTextBox8 };
            for (int num = 0; num < 8; num++)
            {
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
            StartSendPara startPara;
            if (OpenSerial_Clicked_Send())
            {
                if (!isRunning_start[port_num])
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
                _ = mySerial.SendPortString(At_cmd);
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
                        _ = mySerial.SendPortString(At_cmd);
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
                        
                        _ = mySerial.SendPortString(At_cmd);
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














