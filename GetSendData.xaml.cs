using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace WIoTa_Serial_Tool
{
    /// <summary>
    /// GetSendData.xaml 的交互逻辑
    /// </summary>
    public partial class GetSendData : Window
    {
        private bool init_flag = false;
        private int[ ,] subframe_sbc = new int[4,8] { { 6, 8, 51, 65, 79, 0, 0, 0}, { 6, 14, 21, 51, 107, 156, 191, 0 }, { 6, 14, 30, 41, 72, 135, 254, 296 }, { 6, 14, 30, 62, 107, 219, 450, 618 } };
        private int[ ,] subframe_bc = new int[4, 8] { { 7, 9, 52, 66, 80, 0, 0, 0 }, { 7, 15, 22, 52, 108, 157, 192, 0 }, { 7, 15, 31, 42, 73, 136, 255, 297 }, { 7, 15, 31, 63, 108, 220, 451, 619 } };
        private double[, ] time_sbc = new double[4, 1] { { 0.073216 }, { 0.146432 }, { 0.292864 }, { 0.585728 } };
        private double[,] time_bc = new double[4, 1] { { 0.073216 }, { 0.146432 }, { 0.292864 }, { 0.585728 } };

        public GetSendData()
        {
            InitializeComponent();
            ReadConfig();
            init_flag = true;

        }

        public void RadioButton_click(object sender, RoutedEventArgs e)
        {
            Query_data_len();
            Create_At_Cmd();
        }

        public void Button_Create_At_Cmd(object sender, RoutedEventArgs e)
        {
            Create_At_Cmd();
        }

        public void Button_Query_data_len(object sender, SelectionChangedEventArgs e)
        {
            Query_data_len();
            Create_At_Cmd();
        }

        public void Button_Create_At_Cmd_Sync(object sender, RoutedEventArgs e)
        {
            Create_At_Cmd_Sync();
        }

        public void Count_str_len(object sender, RoutedEventArgs e)
        {
            String str_data = textBox_str_data.Text;
            string str_len = string.Format("当前文本编辑框种的字符串长度为:{0} Byte", str_data.Length);
            label_data_len.Content = str_len;
        }

        private void Count_str_len_s(object sender, RoutedEventArgs e)
        {
            String str_data = textBox_str_data_s.Text;
            string str_len = string.Format("当前文本编辑框种的字符串长度为:{0} Byte", str_data.Length);
            label_data_len_s.Content = str_len;
        }

        public void GetSendDate_Close(object sender, CancelEventArgs e)
        {
            WriteConfig();
        }

        public String Create_random_str(String text)
        {
            //string text = text_len.Text;
            int tlen = int.Parse(text);
            bool is_punctuation = checkbox_punctuation.IsChecked ?? false;
            bool is_number = checkbox_number.IsChecked ?? false;
            bool is_upper = checkbox_upper.IsChecked ?? false;
            bool is_lower = checkbox_lower.IsChecked ?? false;

            Random randomA = new Random();
            int round = 0;
            int intA = 0;
            String random_str = "";
        
            while (round < tlen)
            {
                //1=特殊符号，2=数字，3=大写字母，4=小写字母
                intA = randomA.Next(1, 5);
                //1、特殊符号
                if (intA == 1 && is_punctuation)
                {
                    //1：33-47值域，2：58-64值域，3：91-96值域，4：123-126值域
                    intA = randomA.Next(1, 5);
                    
                    if (intA == 1)
                    {
                        intA = randomA.Next(33, 48);
                        random_str = ((char)intA).ToString() + random_str;
                        round++;
                        continue;
                    }

                    if (intA == 2)
                    {
                        intA = randomA.Next(58, 65);
                        random_str = ((char)intA).ToString() + random_str;
                        round++;
                        continue;
                    }

                    if (intA == 3)
                    {
                        intA = randomA.Next(91, 97);
                        random_str = ((char)intA).ToString() + random_str;
                        round++;
                        continue;
                    }

                    if (intA == 4)
                    {
                        intA = randomA.Next(123, 127);
                        random_str = ((char)intA).ToString() + random_str;
                        round++;
                        continue;
                    }
                }
                //2、数字
                if (intA == 2 && is_number)
                {
                    intA = randomA.Next(0, 10);
                    random_str = intA.ToString() + random_str;
                    round++;
                    continue;
                }
                //3、大写字母
                if (intA == 3 && is_upper)
                {
                    intA = randomA.Next(65, 91);
                    random_str = ((char)intA).ToString() + random_str;
                    round++;
                    continue;
                }
                //4、小写字母
                if (intA == 4 && is_lower)
                {
                    intA = randomA.Next(97, 123);
                    random_str = ((char)intA).ToString() + random_str;
                    round++;
                    continue;
                }
                if (!is_punctuation && !is_number && !is_upper && !is_lower)
                {
                    break;
                }
            }
            return random_str;
        }

        public void Create_At_Cmd()
        {
            if (init_flag == false)
            {
                return;
            }
            string text = text_len.Text;
            string str_data = Create_random_str(text);
            bool is_bc = radioButton_bc.IsChecked ?? false;
            bool is_sbc = radioButton_sbc.IsChecked ?? false;
            string at_cmd = "";
            if (is_sbc)
            {
                at_cmd = string.Format("AT+WIOTASEND=60000,{0},{1}\\r\\n{2}", str_data.Length + 2, "接收端userid", str_data);
            }
            if ( is_bc)
            {
                at_cmd = string.Format("AT+WIOTASEND=60000,{0},{1}\\r\\n{2}", str_data.Length + 2, "0", str_data);
            }
            textBox_str_data.Text = str_data;
            textBox_at_cmd.Text = at_cmd;
            string str_len = string.Format("当前文本编辑框种的字符串长度为:{0} Byte", str_data.Length);
            label_data_len.Content = str_len;
        }

        public void Create_At_Cmd_Sync()
        {
            if (init_flag == false)
            {
                return;
            }
            string text = text_len_s.Text;
            string str_data = Create_random_str(text);
            bool is_norbc = radioButton_sync_norbc.IsChecked ?? false;
            bool is_otabc = radioButton_sync_otabc.IsChecked ?? false;
            bool is_unicast = radioButton_sync_unicast.IsChecked ?? false;
            bool is_mc = radioButton_sync_mc.IsChecked ?? false;
            bool is_iote = radioButton_sync_iote.IsChecked ?? false;
            bool is_block = radioButton_blocked.IsChecked ?? false;

            String data_id = textBox_dataid.Text;
            String user_id = textBox_userid.Text;
            String mc_id = textBox_mcid.Text;
            int symbol_len = comboBox_sync_symbol.SelectedIndex;
            int mcs = comboBox_sync__mcs.SelectedIndex;

            String at_cmd = "";
            Double time = 0;
            int data_len = 0, time_c = 0, len = str_data.Length;
            int frame_num = 0;

            time = time_bc[symbol_len, 0];
            data_len = subframe_bc[symbol_len, mcs];
            frame_num = (int)Math.Ceiling((double)len / data_len);
            time = (time * frame_num) + 100;
            time_c = 60000;
            if (is_norbc)
            {
                
                if (is_block)
                {
                    at_cmd = string.Format("AT+WIOTABC={0},{1},0,{2},1\\r\\n{3}", data_id, len + 2, time_c, str_data);
                }
                else
                {
                    at_cmd = string.Format("AT+WIOTABC={0},{1},0,{2},0\\r\\n{3}", data_id, len + 2, time_c, str_data);
                }
            }
            if (is_otabc)
            {

                if (is_block)
                {
                    at_cmd = string.Format("AT+WIOTABC={0},{1},1,{2},1\\r\\n{3}", data_id, len + 2, time_c, str_data);
                }
                else
                {
                    at_cmd = string.Format("AT+WIOTABC={0},{1},1,{2},0\\r\\n{3}", data_id, len + 2, time_c, str_data);
                }
            }
            if (is_unicast)
            {
             
                if (is_block)
                {
                    at_cmd = string.Format("AT+WIOTASEND={0},{1},{2},{3},1\\r\\n{4}", data_id, len + 2, user_id, time_c, str_data);
                }
                else
                {
                    at_cmd = string.Format("AT+WIOTASEND={0},{1},{2},{3},0\\r\\n{4}", data_id, len + 2, user_id, time_c, str_data);
                }
            }
            if (is_mc)
            {
            
                if (is_block)
                {
                    at_cmd = string.Format("AT+WIOTAMC={0},{1},{2},{3},1\\r\\n{4}", data_id, len + 2, mc_id, time_c, str_data);
                }
                else
                {
                    at_cmd = string.Format("AT+WIOTAMC={0},{1},{2},{3},0\\r\\n{4}", data_id, len + 2, mc_id, time_c, str_data);
                }
            }
            if (is_iote)
            {
                at_cmd = string.Format("AT+WIOTASEND={0},{1}\\r\\n{2}", time_c, len + 2, str_data);
            }
            textBox_str_data_s.Text = str_data;
            textBox_at_cmd_s.Text = at_cmd;
            string str_len = string.Format("当前文本编辑框种的字符串长度为:{0} Byte", len);
            label_data_len_s.Content = str_len;
            string str0 = string.Format("当前配置，数据长度发送时间为:{0: 0.000} s", time - 100);
            string str1 = string.Format("当前配置，帧数为:{0} 包", frame_num);
            label_data_t_s.Content = str0;
            label_frame_num.Content = str1;
        }

        public void Query_data_len()
        {
           
            if(init_flag == false)
            {
                return;
            }

            string subframe_num_c = comboBox_sub_num.SelectedValue.ToString();
            subframe_num_c = System.Text.RegularExpressions.Regex.Replace(subframe_num_c, @"[^0-9]+", "");
            int subframe_num = int.Parse(subframe_num_c);
            int symbol_len = comboBox_symbol.SelectedIndex;
            int mcs = comboBox_mcs.SelectedIndex;

            bool is_bc = radioButton_bc.IsChecked ?? false;
            bool is_sbc = radioButton_sbc.IsChecked ?? false;

            int send_len = 0, data_len = 0;
            Double frame_t = 0;

            if (is_bc)
            {
                data_len = subframe_bc[symbol_len, mcs];
                send_len = data_len * (subframe_num - 1);
                frame_t = time_bc[symbol_len, 0];
                if (send_len > 1021)
                {
                    send_len = 1021;
                }
            }
            if (is_sbc)
            {
                data_len = subframe_sbc[symbol_len, mcs];
                send_len = data_len * (subframe_num - 1) + 3;
                frame_t = time_sbc[symbol_len, 0];
                if (send_len > 310)
                {
                    send_len = 310;
                }
            }
            Double subframe_t = frame_t / subframe_num;
            string str0 = string.Format("当前配置，一个子帧可承载应用数据长度:{0} Byte", data_len);
            string str1 = string.Format("当前配置，一帧可承载的最大数据长度:{0} Byte", send_len);
            string str2 = string.Format("当前配置，一个子帧数据长度发送时间为:{0: 0.000} s", subframe_t);
            string str3 = string.Format("当前配置，一帧数据长度发送时间为:{0: 0.000} s", frame_t);
            label_data.Content = str0;
            label_send.Content = str1;
            label_subframe_t.Content = str2;
            label_frame_t.Content = str3;
            text_len.Text = (send_len - 2).ToString();
        }

        private string ReadXml(XmlDocument xmlDoc, XmlNode root, string KeyValue)
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

        public void ReadConfig()
        {
            string configFilePath = "datalen_config.xml"; // 指定配置文件的路径
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
            
            //异步广播
            /*readValue = ConfigurationManager.AppSettings[$"is_bc"];
            radioButton_bc.IsChecked = readValue == "true" ? true : false;*/
            readValue = ReadXml(xmlDoc, root, "is_bc");
            radioButton_bc.IsChecked = readValue == "True" ? true : false;

            //异步单播
            /*readValue = ConfigurationManager.AppSettings[$"is_sbc"];
            radioButton_sbc.IsChecked = readValue == "true" ? true : false;*/
            readValue = ReadXml(xmlDoc, root, "is_sbc");
            radioButton_sbc.IsChecked = readValue == "True" ? true : false;

            //子帧数量
            /*readValue = ConfigurationManager.AppSettings[$"subframe_num"];
            comboBox_sub_num.Text = readValue;*/
            readValue = ReadXml(xmlDoc, root, "subframe_num");
            comboBox_sub_num.Text = readValue;

            //symbol长度
            /*readValue = ConfigurationManager.AppSettings[$"symbol_len"];
            comboBox_symbol.Text = readValue;*/
            readValue = ReadXml(xmlDoc, root, "symbol_len");
            comboBox_symbol.Text = readValue;

            //mcs
            /*readValue = ConfigurationManager.AppSettings[$"mcs"];
            comboBox_mcs.Text = readValue;*/
            readValue = ReadXml(xmlDoc, root, "mcs");
            comboBox_mcs.Text = readValue;

            //数据值
            /*readValue = ConfigurationManager.AppSettings[$"str_data"];
            textBox_str_data.Text = readValue;*/
            readValue = ReadXml(xmlDoc, root, "str_data");
            textBox_str_data.Text = readValue;

            //发送命令
            /*readValue = ConfigurationManager.AppSettings[$"at_cmd"];
            textBox_at_cmd.Text = readValue;*/
            readValue = ReadXml(xmlDoc, root, "at_cmd");
            textBox_at_cmd.Text = readValue;

            //特殊符号
            /*readValue = ConfigurationManager.AppSettings[$"is_punctuation"];
            checkbox_punctuation.IsChecked = readValue == "true" ? true : false;*/
            readValue = ReadXml(xmlDoc, root, "is_punctuation");
            checkbox_punctuation.IsChecked = readValue == "True" ? true : false;

            //数字
            /*readValue = ConfigurationManager.AppSettings[$"is_number"];
            checkbox_number.IsChecked = readValue == "true" ? true : false;*/
            readValue = ReadXml(xmlDoc, root, "is_number");
            checkbox_number.IsChecked = readValue == "True" ? true : false;

            //大写字母
            /*readValue = ConfigurationManager.AppSettings[$"is_upper"];
            checkbox_upper.IsChecked = readValue == "true" ? true : false;*/
            readValue = ReadXml(xmlDoc, root, "is_upper");
            checkbox_upper.IsChecked = readValue == "True" ? true : false;

            //小写字母
            /*readValue = ConfigurationManager.AppSettings[$"is_lower"];
            checkbox_lower.IsChecked = readValue == "true" ? true : false;*/
            readValue = ReadXml(xmlDoc, root, "is_lower");
            checkbox_lower.IsChecked = readValue == "True" ? true : false;

            //生成字符串长度
            /*readValue = ConfigurationManager.AppSettings[$"cmd_len"];
            text_len.Text = readValue;*/
            readValue = ReadXml(xmlDoc, root, "cmd_len");
            text_len.Text = readValue;

            //数据字段
            readValue = ReadXml(xmlDoc, root, "label_data");
            label_data.Content = readValue;

            //最大数据长度
            readValue = ReadXml(xmlDoc, root, "label_send");
            label_send.Content = readValue;

            //字符串长度
            readValue = ReadXml(xmlDoc, root, "label_data_len");
            label_data_len.Content = readValue;

            //一子帧所需时间
            readValue = ReadXml(xmlDoc, root, "label_subframe_t");
            label_subframe_t.Content = readValue;

            //一帧所需时间
            readValue = ReadXml(xmlDoc, root, "label_frame_t");
            label_frame_t.Content = readValue;

            //同步
            readValue = ReadXml(xmlDoc, root, "is_norbc");
            radioButton_sync_norbc.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_otabc");
            radioButton_sync_otabc.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_unicast");
            radioButton_sync_unicast.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_mc");
            radioButton_sync_mc.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_iote");
            radioButton_sync_iote.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_blocked");
            radioButton_blocked.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_noblocked");
            radioButton_noblocked.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "data_id");
            textBox_dataid.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "mc_id");
            textBox_mcid.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "user_id");
            textBox_userid.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "symbol_len_s");
            comboBox_sync_symbol.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "mcs_s");
            comboBox_sync__mcs.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "label_data_t_s");
            label_data_t_s.Content = readValue;

            readValue = ReadXml(xmlDoc, root, "label_frame_num");
            label_frame_num.Content = readValue;

            readValue = ReadXml(xmlDoc, root, "textBox_str_data_s");
            textBox_str_data_s.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "label_data_len_s");
            label_data_len_s.Content = readValue;

            readValue = ReadXml(xmlDoc, root, "is_punctuation_s");
            checkbox_punctuation_s.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_number_s");
            checkbox_number_s.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_upper_s");
            checkbox_upper_s.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "is_lower_s");
            checkbox_lower_s.IsChecked = readValue == "True" ? true : false;

            readValue = ReadXml(xmlDoc, root, "text_len_s");
            text_len_s.Text = readValue;

            readValue = ReadXml(xmlDoc, root, "at_cmd_s");
            textBox_at_cmd_s.Text = readValue;
        }

        private void WriteXml(XmlDocument xmlDoc, XmlNode root, string Value, string KeyValue)
        {
            XmlNode exitNode = xmlDoc.CreateElement(KeyValue);
            exitNode.InnerText = Value;
            root.AppendChild(exitNode);
        }

        public void WriteConfig()
        {
            string selectedValue;
            bool bselectedValue;
            string configFilePath = "datalen_config.xml"; // 指定子窗口配置文件的路径

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            /*bselectedValue = radioButton_bc.IsChecked ?? false;
            config.AppSettings.Settings[$"is_bc"].Value = bselectedValue.ToString();*/
            bselectedValue = radioButton_bc.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_bc");

            /*bselectedValue = radioButton_sbc.IsChecked ?? false;
            config.AppSettings.Settings[$"is_sbc"].Value = bselectedValue.ToString();*/
            bselectedValue = radioButton_sbc.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_sbc");

            /*selectedValue = comboBox_sub_num.Text;
            config.AppSettings.Settings[$"subframe_num"].Value = selectedValue;*/
            selectedValue = comboBox_sub_num.Text;
            WriteXml(xmlDoc, root, selectedValue, "subframe_num");

            /*selectedValue = comboBox_symbol.Text;
            config.AppSettings.Settings[$"symbol_len"].Value = selectedValue;*/
            selectedValue = comboBox_symbol.Text;
            WriteXml(xmlDoc, root, selectedValue, "symbol_len");

            /*selectedValue = comboBox_mcs.Text;
            config.AppSettings.Settings[$"mcs"].Value = selectedValue;*/
            selectedValue = comboBox_mcs.Text;
            WriteXml(xmlDoc, root, selectedValue, "mcs");

            /*selectedValue = textBox_str_data.Text;
            config.AppSettings.Settings[$"str_data"].Value = selectedValue;*/
            selectedValue = textBox_str_data.Text;
            WriteXml(xmlDoc, root, selectedValue, "str_data");

            /*selectedValue = textBox_at_cmd.Text;
            config.AppSettings.Settings[$"at_cmd"].Value = selectedValue;*/
            selectedValue = textBox_at_cmd.Text;
            WriteXml(xmlDoc, root, selectedValue, "at_cmd");

            /*bselectedValue = checkbox_punctuation.IsChecked ?? false;
            config.AppSettings.Settings[$"is_punctuation"].Value = bselectedValue.ToString();*/
            bselectedValue = checkbox_punctuation.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_punctuation");

            /*bselectedValue = checkbox_number.IsChecked ?? false;
            config.AppSettings.Settings[$"is_number"].Value = bselectedValue.ToString();*/
            bselectedValue = checkbox_number.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_number");

            /*bselectedValue = checkbox_upper.IsChecked ?? false;
            config.AppSettings.Settings[$"is_upper"].Value = bselectedValue.ToString();*/
            bselectedValue = checkbox_upper.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_upper");

            /*bselectedValue = checkbox_lower.IsChecked ?? false;
            config.AppSettings.Settings[$"is_lower"].Value = bselectedValue.ToString();*/
            bselectedValue = checkbox_lower.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_lower");

            /*selectedValue = text_len.Text;
            config.AppSettings.Settings[$"cmd_len"].Value = selectedValue;*/
            selectedValue = text_len.Text;
            WriteXml(xmlDoc, root, selectedValue, "cmd_len");

            selectedValue = label_data.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_data");

            selectedValue = label_send.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_send");

            selectedValue = label_data_len.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_data_len");

            selectedValue = label_subframe_t.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_subframe_t");

            selectedValue = label_frame_t.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_frame_t");

            //config.Save(ConfigurationSaveMode.Modified);
            /*xmlDoc.Save(configFilePath);*/

            //同步
            bselectedValue = radioButton_sync_norbc.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_norbc");

            bselectedValue = radioButton_sync_otabc.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_otabc");

            bselectedValue = radioButton_sync_unicast.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_unicast");

            bselectedValue = radioButton_sync_mc.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_mc");

            bselectedValue = radioButton_sync_iote.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_iote");

            bselectedValue = radioButton_blocked.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_blocked");

            bselectedValue = radioButton_noblocked.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_noblocked");

            selectedValue = textBox_dataid.Text;
            WriteXml(xmlDoc, root, selectedValue, "data_id");

            selectedValue = textBox_mcid.Text;
            WriteXml(xmlDoc, root, selectedValue, "mc_id");

            selectedValue = textBox_userid.Text;
            WriteXml(xmlDoc, root, selectedValue, "user_id");

            selectedValue = comboBox_sync_symbol.Text;
            WriteXml(xmlDoc, root, selectedValue, "symbol_len_s");

            selectedValue = comboBox_sync__mcs.Text;
            WriteXml(xmlDoc, root, selectedValue, "mcs_s");

            selectedValue = label_data_t_s.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_data_t_s");

            selectedValue = label_frame_num.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_frame_num");

            selectedValue = textBox_str_data_s.Text;
            WriteXml(xmlDoc, root, selectedValue, "textBox_str_data_s");

            selectedValue = label_data_len_s.Content.ToString();
            WriteXml(xmlDoc, root, selectedValue, "label_data_len_s");

            bselectedValue = checkbox_punctuation_s.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_punctuation_s");

            bselectedValue = checkbox_number_s.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_number_s");

            bselectedValue = checkbox_upper_s.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_upper_s");

            bselectedValue = checkbox_lower_s.IsChecked ?? false;
            WriteXml(xmlDoc, root, bselectedValue.ToString(), "is_lower_s");

            selectedValue = text_len_s.Text;
            WriteXml(xmlDoc, root, selectedValue, "text_len_s");

            selectedValue = textBox_at_cmd_s.Text;
            WriteXml(xmlDoc, root, selectedValue, "at_cmd_s");

            xmlDoc.Save(configFilePath);
        }

    }
}
