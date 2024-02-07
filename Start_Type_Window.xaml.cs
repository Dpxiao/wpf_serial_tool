
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.Xml;

namespace WIoTa_Serial_Tool
{
    /// <summary>
    /// Start_Type_Window.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class Start_Type_Window : Window
    {
        public event EventHandler<ButtonClickedEventArgs> ButtonClicked;
        private UserID_Window1 MyUserIDSet;

        public event EventHandler<string> ValuePassed;

        private void OnValuePassed(string value)
        {
            ValuePassed?.Invoke(this, value);
        }

        public Start_Type_Window()
        {
            InitializeComponent();
            ReadAppConfig();
        }

        private void setosc2_Click(object sender, RoutedEventArgs e)
        {
            if (oscCheckBox2.IsChecked ?? false)
            {
                checkBox_dcxo2.IsChecked = false;
            }

            if (oscCheckBox3.IsChecked ?? false)
            {
                checkBox_dcxo3.IsChecked = false;
            }
        }

        private void setdcxo2_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_dcxo2.IsChecked ?? false)
            {
                oscCheckBox2.IsChecked = false;
            }

            if (checkBox_dcxo3.IsChecked ?? false)
            {
                oscCheckBox3.IsChecked = false;
            }
        }

        private void start_Button_Click(object sender, RoutedEventArgs e)
        {
            int type = StartTab.SelectedIndex;
            List<string> AtCmdList = new List<string>();
            switch (type)
            {
                case 0:
                    AtCmdList = Get_Sync_Ap_Cmd();
                    ButtonClicked?.Invoke(this, new ButtonClickedEventArgs(AtCmdList));

                    break;
                case 1:

                    AtCmdList = Get_Sync_Iote_Cmd();
                  
                    ButtonClicked?.Invoke(this, new ButtonClickedEventArgs(AtCmdList));
                    break;
                case 2:
                    AtCmdList = Get_Async_IoTE_Cmd();
                    ButtonClicked?.Invoke(this, new ButtonClickedEventArgs(AtCmdList));
                    break;
                default:

                    break;
            }

        }

        public List<string> Get_Sync_Ap_Cmd()
        {
            List<string> AtCmdList = new List<string>();
            if (exitWIoTaCheckBox1.IsChecked ?? false)
            { AtCmdList.Add("AT+WIOTARUN=0"); }
            if (InitWIoTaCheckBox1.IsChecked ?? false)
            { AtCmdList.Add("AT+WIOTAINIT"); }
            //if (boost0_5CheckBox2.IsChecked ?? false)
            //{; }//这个指令暂时不知道
            if (setFreqCheckBox1.IsChecked ?? false)
            {
                string freq_idx = FreqIdxTextBox1.Text;
                AtCmdList.Add($"AT+WIOTAFREQ={freq_idx}");
            }//这个指令暂时不知道
            if (systemConfigCheckBox1.IsChecked ?? false)
            {
                string ap_power = apPowerTextBox1.Text;//+20
                int power = Convert.ToInt32(ap_power) + 20;
                int idlen = idLenComboBox1.SelectedIndex;
                int symbol_len = symbolComboBox1.SelectedIndex;
                //int bandWidth = bandWidthComboBox.SelectedIndex;
                //int pz = pzComboBox.SelectedIndex;
                int dlul = dlulComboBox1.SelectedIndex;
                int bt = BTComboBox1.SelectedIndex;
                int groupnum = groupnumComboBox1.SelectedIndex;
                int spectrum = spectrumComboBox1.SelectedIndex;
                string subSystemid = subSystemidTextBox1.Text;
                //< ap_max_pow >,< id_len >,< symbol >,< dlul >,< bt >,< group_num >,< spec_idx >,< old_v >,< bitscb >,< subsystemid >
                //22,1,1,0,1,0,3,0,1,21456981
                int bitbcs = 1;
                if (bitscbCheckBox1.IsChecked ?? false)
                { bitbcs = 1; }
                else
                { bitbcs = 0; }
                AtCmdList.Add($"AT+WIOTACONFIG={power},{idlen},{symbol_len},{dlul},{bt},{groupnum},{spectrum},0,{bitbcs},{subSystemid}");

            }
            if (runWIoTaCheckBox1.IsChecked ?? false)
            {
                AtCmdList.Add($"AT+WIOTARUN=1");
            }
            return AtCmdList;
        }
        public List<string> Get_Sync_Iote_Cmd()
        {
            List<string> AtCmdList = new List<string>();
            if (exitWIoTaCheckBox2.IsChecked ?? false)
            { AtCmdList.Add("AT+WIOTARUN=0"); }
            if (InitWIoTaCheckBox2.IsChecked ?? false)
            { AtCmdList.Add("AT+WIOTAINIT"); }
            //if (boost0_5CheckBox2.IsChecked ?? false)
            //{; }//这个指令暂时不知道
            if (setFreqCheckBox2.IsChecked ?? false)
            {
                string freq_idx = FreqIdxTextBox2.Text;
                AtCmdList.Add($"AT+WIOTAFREQ={freq_idx}");
            }
            if (checkBox_dcxo2.IsChecked ?? false)
            {
                string dcxo = textBox_dcxo2.Text;
                AtCmdList.Add($"AT+WIOTAOSC=0");
                AtCmdList.Add($"AT+WIOTADCXO={dcxo}");
            }
            if (useridCheckBox2.IsChecked ?? false)
            {
                string userid = useridTextBox2.Text;
                AtCmdList.Add($"AT+WIOTAUSERID={userid}");
            }
            if (oscCheckBox2.IsChecked ?? false)
            { AtCmdList.Add($"AT+WIOTAOSC=1"); }
            if (systemConfigCheckBox2.IsChecked ?? false)
            {
                string ap_power = apPowerTextBox2.Text;//+20
                int power = Convert.ToInt32(ap_power) + 20;
                int idlen = idLenComboBox2.SelectedIndex;
                int symbol_len = symbolComboBox2.SelectedIndex;
                //int bandWidth = bandWidthComboBox.SelectedIndex;
                //int pz = pzComboBox.SelectedIndex;
                int dlul = dlulComboBox2.SelectedIndex;
                int bt = BTComboBox2.SelectedIndex;
                int groupnum = groupnumComboBox2.SelectedIndex;
                int spectrum = spectrumComboBox2.SelectedIndex;
                string subSystemid = subSystemidTextBox2.Text;
                //< ap_max_pow >,< id_len >,< symbol >,< dlul >,< bt >,< group_num >,< spec_idx >,< old_v >,< bitscb >,< subsystemid >
                //22,1,1,0,1,0,3,0,1,21456981
                int bitbcs = 1;
                if (bitscbCheckBox2.IsChecked ?? false)
                { bitbcs = 1; }
                else
                { bitbcs = 0; }
                AtCmdList.Add($"AT+WIOTACONFIG={power},{idlen},{symbol_len},{dlul},{bt},{groupnum},{spectrum},0,{bitbcs},{subSystemid}");

            }
            ////iote的配置
            //if (txmodeCheckBox2.IsChecked ?? false)
            //{
            //    int index = txmodeComboBox2.SelectedIndex;
            //    int txmodeValue = 3;
            //    if (index == 0)
            //    { txmodeValue = 0; }
            //    if (index == 1)
            //    { txmodeValue = 3; }
            //    AtCmdList.Add($"AT+WIOTATXMODE={txmodeValue}");
            //}
            if (iotepowerCheckBox2.IsChecked ?? false)
            {
                string iotepower = iotepowerTextBox2.Text;
                int power = Convert.ToInt32(iotepower) + 20;
                AtCmdList.Add($"AT+WIOTAPOW=0,{power}");
            }
            if (iotemcsCheckBox2.IsChecked ?? false)
            {
                int mcs = iotemcsComboBox2.SelectedIndex;
                AtCmdList.Add($"AT+WIOTARATE=0,{mcs}");
            }

            if (runWIoTaCheckBox2.IsChecked ?? false)
            {
                AtCmdList.Add($"AT+WIOTARUN=1");
            }
            if (connectCheckBox2.IsChecked ?? false)
            {
                string connect_time = connectTextBox2.Text;
                AtCmdList.Add($"AT+WIOTACONNECT=1,{connect_time}");
            }
            return AtCmdList;
        }
        public List<string> Get_Async_IoTE_Cmd()
        {
            List<string> AtCmdList = new List<string>();
            if (exitWIoTaCheckBox3.IsChecked ?? false)
            { AtCmdList.Add("AT+WIOTARUN=0"); }
            if (InitWIoTaCheckBox3.IsChecked ?? false)
            { AtCmdList.Add("AT+WIOTAINIT"); }
            //if (boost0_5CheckBox3.IsChecked ?? false)
            //{; }//这个指令暂时不知道
            if (setFreqCheckBox3.IsChecked ?? false)
            {
                string freq_idx = FreqIdxTextBox3.Text;
                AtCmdList.Add($"AT+WIOTAFREQ={freq_idx}");
            }
            if (checkBox_dcxo3.IsChecked ?? false)
            {
                string dcxo = textBox_dcxo3.Text;
                AtCmdList.Add($"AT+WIOTAOSC=0");
                AtCmdList.Add($"AT+WIOTADCXO={dcxo}");
            }
            if (useridCheckBox3.IsChecked ?? false)
            {
                string userid = useridTextBox3.Text;
                AtCmdList.Add($"AT+WIOTAUSERID={userid}");
            }
            if (oscCheckBox3.IsChecked ?? false)
            { AtCmdList.Add($"AT+WIOTAOSC=1"); }
            if (systemConfigCheckBox3.IsChecked ?? false)
            {
                int pz = 8;
                int idlen = idLenComboBox3.SelectedIndex;
                int symbol_len = symbolComboBox3.SelectedIndex;
                int bandWidth = bandWidthComboBox3.SelectedIndex;
                int pz_index = pzComboBox3.SelectedIndex;
                if (pz_index == 0)
                {
                    pz = 4;
                }
                else
                {
                    pz = 8;
                }
                int bt = BTComboBox3.SelectedIndex;
                int spectrum = spectrumComboBox3.SelectedIndex;
                string subSystemid = subSystemidTextBox3.Text;
                string systemid = SystemidTextBox3.Text;
                //AT+WIOTACONFIG=<id_len>,<symbol>,<band>,<pz>,<bt>,<spec_idx>,<systemid>,<subsystemid>
                AtCmdList.Add($"AT+WIOTACONFIG={idlen},{symbol_len},{bandWidth},{pz},{bt},{spectrum},{systemid},{subSystemid}");

            }
            //iote的配置
            if (txmodeCheckBox3.IsChecked ?? false)
            {
                int index = txmodeComboBox3.SelectedIndex;
                int txmodeValue = 3;
                if (index == 0)
                { txmodeValue = 0; }
                if (index == 1)
                { txmodeValue = 3; }
                AtCmdList.Add($"AT+WIOTATXMODE={txmodeValue}");
            }
            if (subframeCheckBox3.IsChecked ?? false)
            {
                int Index = subframeComboBox3.SelectedIndex;
                AtCmdList.Add($"AT+WIOTASUBNUM={Index + 3}");
            }
            if (rounbcCheckBox3.IsChecked ?? false)
            {
                int Index = rounbcComboBox3.SelectedIndex;
                AtCmdList.Add($"AT+WIOTABCROUND={Index + 1}");
            }

            if (iotepowerCheckBox3.IsChecked ?? false)
            {
                string iotepower = iotepowerTextBox3.Text;
                int power = Convert.ToInt32(iotepower) + 20;
                AtCmdList.Add($"AT+WIOTAPOW=0,{power}");
            }
            if (iotemcsCheckBox3.IsChecked ?? false)
            {
                int mcs = iotemcsComboBox3.SelectedIndex;
                AtCmdList.Add($"AT+WIOTARATE=0,{mcs}");
            }

            if (runWIoTaCheckBox3.IsChecked ?? false)
            {
                AtCmdList.Add($"AT+WIOTARUN=1");
            }

            return AtCmdList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveConfig();
            if (MyUserIDSet != null)
            {
                MyUserIDSet.Close();
            }
        }
        private bool RetCheckStatus(string value)
        {
            if (value == "False")
                return false;
            else
                return true;
        }

        private string ReadXml(XmlDocument xmlDoc, XmlNode root,string KeyValue)
        {
            XmlNode typeNode = root.SelectSingleNode(KeyValue);
            if (typeNode == null)
            {
                return null;
            }
            else
            {
                string typeValue = typeNode.InnerText;
                return typeValue;
            }
            
        }
        public void ReadAppConfig()
        {
            
            string configFilePath = "start_config.xml"; // 指定配置文件的路径
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(configFilePath); // 加载XML文件
            }
            catch (Exception ex)
            {
                // 捕获其他异常（可选）
                MessageBox.Show(ex.Message);
                return;
            }
            XmlNode root = xmlDoc.SelectSingleNode("root"); // 获取根节点
            
            string readValue;
            //类型
            readValue = ReadXml(xmlDoc, root,"start_Type");
            int index = Convert.ToInt32(readValue);
            StartTab.SelectedIndex = index;
            //退出协议栈
            CheckBox[] exitWIoTaCheckBox = { exitWIoTaCheckBox1, exitWIoTaCheckBox2, exitWIoTaCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"exit_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "False";
                exitWIoTaCheckBox[i].IsChecked = RetCheckStatus(readValue);
            }
            //初始化
            CheckBox[] InitWIoTaCheckBox = { InitWIoTaCheckBox1, InitWIoTaCheckBox2, InitWIoTaCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"Init_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "False";
                InitWIoTaCheckBox[i].IsChecked = RetCheckStatus(readValue);
            }
           
            ////boost0_5
            //readValue = ReadXml(xmlDoc, root, "boost0_5_WIoTa2");
            //if (readValue == null)
            //    readValue = "False";
            //boost0_5CheckBox2.IsChecked = RetCheckStatus(readValue);

            CheckBox[] setFreqCheckBox = {setFreqCheckBox1, setFreqCheckBox2, setFreqCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                //设置频点
                readValue = ReadXml(xmlDoc, root, $"freqidx_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "False";
                setFreqCheckBox[i].IsChecked = RetCheckStatus(readValue);
            }

            TextBox[] FreqIdxTextBox = { FreqIdxTextBox1, FreqIdxTextBox2, FreqIdxTextBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"freqidxValue_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "120";
                FreqIdxTextBox[i].Text = readValue;
            }

            //设置dcxo
            readValue = ReadXml(xmlDoc, root, "dcxo_WIoTa2");
            if (readValue == null)
                readValue = "False";
            checkBox_dcxo2.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "dcxoValue_WIoTa2");
            if (readValue == null)
                readValue = "20000";
            textBox_dcxo2.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "dcxo_WIoTa3");
            if (readValue == null)
                readValue = "False";
            checkBox_dcxo3.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "dcxoValue_WIoTa3");
            if (readValue == null)
                readValue = "20000";
            textBox_dcxo3.Text = readValue;

            //设置userid
            readValue = ReadXml(xmlDoc, root, "userid_WIoTa2");
            if (readValue == null)
                readValue = "False";
            useridCheckBox2.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "useridValue_WIoTa2");
            if (readValue == null)
                readValue = "12345678";
            useridTextBox2.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "userid_WIoTa3");
            if (readValue == null)
                readValue = "False";
            useridCheckBox3.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "useridValue_WIoTa3");
            if (readValue == null)
                readValue = "12345678";
            useridTextBox3.Text = readValue;

            //设置有源晶体
            readValue = ReadXml(xmlDoc, root, "osc_WIoTa2");
            if (readValue == null)
                readValue = "False";
            oscCheckBox2.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "osc_WIoTa3");
            if (readValue == null)
                readValue = "False";
            oscCheckBox3.IsChecked = RetCheckStatus(readValue);

            //系统配置
            CheckBox[] systemConfigCheckBox = { systemConfigCheckBox1, systemConfigCheckBox2, systemConfigCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"system_WIoTa{ i + 1}");
                if (readValue == null)
                    readValue = "False";
                systemConfigCheckBox[i].IsChecked = RetCheckStatus(readValue);
            }
            

            //Ap最大功率
            readValue = ReadXml(xmlDoc, root, "apPower_WIoTa1");
            if (readValue == null)
                readValue = "20";
            apPowerTextBox1.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "apPower_WIoTa2");
            if (readValue == null)
                readValue = "20";
            apPowerTextBox2.Text = readValue;
            //id 长度
            //id 长度
            ComboBox[] idLenComboBox = { idLenComboBox1, idLenComboBox2, idLenComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"idLength_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "0";
                index = Convert.ToInt32(readValue);
                idLenComboBox[i].SelectedIndex = index;
            }

            //符号长度
            ComboBox[] symbolComboBox = { symbolComboBox1, symbolComboBox2, symbolComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"symbolLen_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "1";
                index = Convert.ToInt32(readValue);
                symbolComboBox[i].SelectedIndex = index;
            }
           

            //带宽
            readValue = ReadXml(xmlDoc, root, "bandwidth_WIoTa3");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            bandWidthComboBox3.SelectedIndex = index;

            //pz
            readValue = ReadXml(xmlDoc, root, "pz_WIoTa3");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            pzComboBox3.SelectedIndex = index;

            //帧结构
            readValue = ReadXml(xmlDoc, root, "dlul_WIoTa1");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            dlulComboBox1.SelectedIndex = index;

            readValue = ReadXml(xmlDoc, root, "dlul_WIoTa2");
            index = Convert.ToInt32(readValue);
            dlulComboBox2.SelectedIndex = index;

            //BT
            ComboBox[] BTComboBox = { BTComboBox1, BTComboBox2, BTComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"bt_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "0";
                index = Convert.ToInt32(readValue);
                BTComboBox[i].SelectedIndex = index;
            }

            ComboBox[] groupnumComboBox = { groupnumComboBox1, groupnumComboBox2 };
            for (int i = 0; i < 2; i++)
            {
                //groupnum
                readValue = ReadXml(xmlDoc, root, $"groupnum_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "1";
                index = Convert.ToInt32(readValue);
                groupnumComboBox[i].SelectedIndex = index;
            }

            //频谱
            ComboBox[] spectrumComboBox = { spectrumComboBox1, spectrumComboBox2, spectrumComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"spectrum_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "3";
                index = Convert.ToInt32(readValue);
                spectrumComboBox[i].SelectedIndex = index;
            }

            TextBox[] subSystemidTextBox = { subSystemidTextBox1, subSystemidTextBox2, subSystemidTextBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"subSystemid{i + 1}");
                subSystemidTextBox[i].Text = readValue;
            }

            //bit加扰
            readValue = ReadXml(xmlDoc, root, "bitscb_WIoTa11");
            if (readValue == null)
                readValue = "True";
            bitscbCheckBox1.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "bitscb_WIoTa12");
            if (readValue == null)
                readValue = "True";
            bitscbCheckBox2.IsChecked = RetCheckStatus(readValue);

            //iote设置
            //RF TXmode模式
            readValue = ReadXml(xmlDoc, root, "txmode_WIoTa3");
            if (readValue == null)
                readValue = "True";
            txmodeCheckBox3.IsChecked = RetCheckStatus(readValue);

            //txmode模式
            readValue = ReadXml(xmlDoc, root, "txmodeValue_WIoTa3");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            txmodeComboBox3.SelectedIndex = index;

            //子帧数量
            readValue = ReadXml(xmlDoc, root, "subframe_WIoTa3");
            if (readValue == null)
                readValue = "True";
            subframeCheckBox3.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "subframeValue_WIoTa3");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            subframeComboBox3.SelectedIndex = index;

            //广播轮数
            readValue = ReadXml(xmlDoc, root, "roundbc_WIoTa3");
            if (readValue == null)
                readValue = "True";
            rounbcCheckBox3.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "roundbcValue_WIoTa3");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            rounbcComboBox3.SelectedIndex = index;

            //IotePower 
            readValue = ReadXml(xmlDoc, root, "iotepower_WIoTa2");
            if (readValue == null)
                readValue = "True";
            iotepowerCheckBox2.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "iotepowerValue_WIoTa2");
            if (readValue == null)
                readValue = "20";
            iotepowerTextBox2.Text = readValue;

            //IotePower 
            readValue = ReadXml(xmlDoc, root, "iotepower_WIoTa3");
            if (readValue == null)
                readValue = "True";
            iotepowerCheckBox3.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "iotepowerValue_WIoTa3");
            if (readValue == null)
                readValue = "20";
            iotepowerTextBox3.Text = readValue;

            //iote mcs
            readValue = ReadXml(xmlDoc, root, "iotemcs_WIoTa2");
            if (readValue == null)
                readValue = "True";
            iotemcsCheckBox2.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "iotemcsValue_WIoTa2");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            iotemcsComboBox2.SelectedIndex = index;

            readValue = ReadXml(xmlDoc, root, "iotemcs_WIoTa3");
            if (readValue == null)
                readValue = "True";
            iotemcsCheckBox3.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "iotemcsValue_WIoTa3");
            if (readValue == null)
                readValue = "1";
            index = Convert.ToInt32(readValue);
            iotemcsComboBox3.SelectedIndex = index;

            //启动协议栈
            CheckBox[] runWIoTaCheckBox = { runWIoTaCheckBox1, runWIoTaCheckBox2, runWIoTaCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                readValue = ReadXml(xmlDoc, root, $"run_WIoTa{i + 1}");
                if (readValue == null)
                    readValue = "True";
                runWIoTaCheckBox[i].IsChecked = RetCheckStatus(readValue);
            }
            //connect
            readValue = ReadXml(xmlDoc, root, "connect_WIoTa2");
            if (readValue == null)
                readValue = "True";
            connectCheckBox2.IsChecked = RetCheckStatus(readValue);

            readValue = ReadXml(xmlDoc, root, "activeTime_WIoTa2");
            if (readValue == null)
                readValue = "0";
            connectTextBox2.Text = readValue;
        }

        private void writeXml(XmlDocument xmlDoc, XmlNode root, string Value, string KeyValue)
        {
            XmlNode exitNode = xmlDoc.CreateElement(KeyValue);
            exitNode.InnerText = Value;
            root.AppendChild(exitNode);
        }
        private void SaveConfig()
        {
            string selectedValue;
            bool bselectedValue;
            string configFilePath = "start_config.xml"; // 指定子窗口配置文件的路径

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
    
            //类型
            selectedValue = StartTab.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "start_Type");

            //退出协议栈

            CheckBox[] exitWIoTaCheckBox = { exitWIoTaCheckBox1, exitWIoTaCheckBox2, exitWIoTaCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                bselectedValue = exitWIoTaCheckBox[i].IsChecked ?? false;
                writeXml(xmlDoc, root, bselectedValue.ToString(), $"exit_WIoTa{i + 1}");
            }

            //初始化
            CheckBox[] InitWIoTaCheckBox = { InitWIoTaCheckBox1, InitWIoTaCheckBox2,InitWIoTaCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                bselectedValue = InitWIoTaCheckBox[i].IsChecked ?? false;
                writeXml(xmlDoc, root, bselectedValue.ToString(), $"Init_WIoTa{i+1}");
            }

            ////boost0_5
            //bselectedValue = boost0_5CheckBox2.IsChecked ?? false;
            //writeXml(xmlDoc, root, bselectedValue.ToString(), "boost0_5_WIoTa2");

            //设置频点
            CheckBox[] setFreqCheckBox = { setFreqCheckBox1, setFreqCheckBox2, setFreqCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                bselectedValue = setFreqCheckBox[i].IsChecked ?? false;
                writeXml(xmlDoc, root, bselectedValue.ToString(), $"freqidx_WIoTa{i + 1}");
            }
            TextBox[] FreqIdxTextBox = { FreqIdxTextBox1, FreqIdxTextBox2, FreqIdxTextBox3 };
            for (int i = 0; i < 3; i++)
            {
                selectedValue = FreqIdxTextBox[i].Text;
                writeXml(xmlDoc, root, selectedValue, $"freqidxValue_WIoTa{i + 1}");
            }

            //设置dcxo
            bselectedValue = checkBox_dcxo2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "dcxo_WIoTa2");

            bselectedValue = checkBox_dcxo3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "dcxo_WIoTa3");

            selectedValue = textBox_dcxo2.Text;
            writeXml(xmlDoc, root, selectedValue, "dcxoValue_WIoTa2");

            selectedValue = textBox_dcxo3.Text;
            writeXml(xmlDoc, root, selectedValue, "dcxoValue_WIoTa3");

            //设置userid
            bselectedValue = useridCheckBox2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "userid_WIoTa2");

            bselectedValue = useridCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "userid_WIoTa3");

            selectedValue = useridTextBox2.Text;
            writeXml(xmlDoc, root, selectedValue, "useridValue_WIoTa2");

            selectedValue = useridTextBox3.Text;
            writeXml(xmlDoc, root, selectedValue, "useridValue_WIoTa3");

            //设置有源晶体
            bselectedValue = oscCheckBox2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "osc_WIoTa2");

            bselectedValue = oscCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "osc_WIoTa3");

            //系统配置
            CheckBox[] systemConfigCheckBox = { systemConfigCheckBox1, systemConfigCheckBox2, systemConfigCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                bselectedValue = systemConfigCheckBox[i].IsChecked ?? false;
                writeXml(xmlDoc, root, bselectedValue.ToString(), $"system_WIoTa{i + 1}");
            }

            //Ap最大功率
            selectedValue = apPowerTextBox1.Text;
            writeXml(xmlDoc, root, selectedValue, "apPower_WIoTa1");

            selectedValue = apPowerTextBox2.Text;
            writeXml(xmlDoc, root, selectedValue, "apPower_WIoTa2");

            //id 长度
            ComboBox[] idLenComboBox = { idLenComboBox1, idLenComboBox2, idLenComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                selectedValue = idLenComboBox[i].SelectedIndex.ToString();
                writeXml(xmlDoc, root, selectedValue, $"idLength_WIoTa{i + 1}");
            }

            //符号长度
            ComboBox[] symbolComboBox = { symbolComboBox1, symbolComboBox2, symbolComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                selectedValue = symbolComboBox[i].SelectedIndex.ToString();
                writeXml(xmlDoc, root, selectedValue, $"symbolLen_WIoTa{i + 1}");
            }

            //带宽
            selectedValue = bandWidthComboBox3.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "bandwidth_WIoTa3");
   

            //pz
            selectedValue = pzComboBox3.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "pz_WIoTa3");

            //帧结构
            selectedValue = dlulComboBox1.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "dlul_WIoTa1");

            selectedValue = dlulComboBox2.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "dlul_WIoTa2");

            //BT
            ComboBox[] BTComboBox = { BTComboBox1, BTComboBox2, BTComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                selectedValue = BTComboBox[i].SelectedIndex.ToString();
                writeXml(xmlDoc, root, selectedValue, $"bt_WIoTa{i + 1}");
            }

            //groupnum
            ComboBox[] groupnumComboBox = { groupnumComboBox1, groupnumComboBox2 };
            for (int i = 0; i < 2; i++)
            {
                selectedValue = groupnumComboBox[i].SelectedIndex.ToString();
                writeXml(xmlDoc, root, selectedValue, $"groupnum_WIoTa{i + 1}");
            }

            //频谱
            ComboBox[] spectrumComboBox = { spectrumComboBox1, spectrumComboBox2, spectrumComboBox3 };
            for (int i = 0; i < 3; i++)
            {
                selectedValue = spectrumComboBox[i].SelectedIndex.ToString();
                writeXml(xmlDoc, root, selectedValue, $"spectrum_WIoTa{i + 1}");
            }

            TextBox[] subSystemidTextBox = { subSystemidTextBox1, subSystemidTextBox2, subSystemidTextBox3 };
            for (int i = 0; i < 3; i++)
            {
                selectedValue = subSystemidTextBox[i].Text;
                writeXml(xmlDoc, root, selectedValue, $"subSystemid{i + 1}");
            }

            //bit加扰
            bselectedValue = bitscbCheckBox1.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "bitscb_WIoTa1");

            bselectedValue = bitscbCheckBox2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "bitscb_WIoTa2");

            //iote设置
            //RF TXmode模式
            bselectedValue = txmodeCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "txmode_WIoTa3");

            selectedValue = txmodeComboBox3.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "txmodeValue_WIoTa3");

            //子帧数量
            bselectedValue = subframeCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, selectedValue, "subframe_WIoTa3");

            selectedValue = subframeComboBox3.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "subframeValue_WIoTa3");

            //广播轮数
            bselectedValue = rounbcCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "roundbc_WIoTa3");

            selectedValue = rounbcComboBox3.SelectedIndex.ToString();
            writeXml(xmlDoc, root, selectedValue, "roundbcValue_WIoTa3");

            //IotePower 
            bselectedValue = iotepowerCheckBox2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "iotepower_WIoTa2");

            selectedValue = iotepowerTextBox2.Text;
            writeXml(xmlDoc, root, selectedValue, "iotepowerValue_WIoTa2");

            bselectedValue = iotepowerCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "iotepower_WIoTa3");

            selectedValue = iotepowerTextBox3.Text;
            writeXml(xmlDoc, root, selectedValue, "iotepowerValue_WIoTa3");

            //iote mcs
            bselectedValue = iotemcsCheckBox2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "iotemcs_WIoTa2");

            selectedValue = iotemcsComboBox2.Text;
            writeXml(xmlDoc, root, selectedValue, "iotemcsValue_WIoTa2");

            bselectedValue = iotemcsCheckBox3.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "iotemcs_WIoTa3");

            selectedValue = iotemcsComboBox3.Text;
            writeXml(xmlDoc, root, selectedValue, "iotemcsValue_WIoTa3");

            //启动协议栈
            CheckBox[] runWIoTaCheckBox = { runWIoTaCheckBox1, runWIoTaCheckBox2, runWIoTaCheckBox3 };
            for (int i = 0; i < 3; i++)
            {
                bselectedValue = runWIoTaCheckBox[i].IsChecked ?? false;
                writeXml(xmlDoc, root, bselectedValue.ToString(), $"run_WIoTa{i + 1}");
            }

            //connect
            bselectedValue = connectCheckBox2.IsChecked ?? false;
            writeXml(xmlDoc, root, bselectedValue.ToString(), "connect_WIoTa2");

            selectedValue = connectTextBox2.Text;
            writeXml(xmlDoc, root, selectedValue, "activeTime_WIoTa2");

            xmlDoc.Save(configFilePath);
        }

        private void Button_Userid_Click(object sender, RoutedEventArgs e)
        {
            MyUserIDSet = new UserID_Window1();
            MyUserIDSet.ValueSelected += ChildWindow_ValueSelected;
            MyUserIDSet.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MyUserIDSet.ShowDialog();
        }

        private void ChildWindow_ValueSelected(object sender, string value)
        {
            int type = StartTab.SelectedIndex;
            if (type == 1)
            {
                useridTextBox2.Text = value;
            }
            if (type == 2)
            {
                useridTextBox3.Text = value;
            }
        }
    }
    public class ButtonClickedEventArgs : EventArgs
    {
        public List<string> StringList { get; set; }

        public ButtonClickedEventArgs(List<string> stringList)
        {
            StringList = stringList;
        }
    }
}
