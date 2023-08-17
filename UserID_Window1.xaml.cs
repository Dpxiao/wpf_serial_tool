
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace WIoTa_Serial_Tool
{
    /// <summary>
    /// UserID_Window1.xaml 的交互逻辑
    /// </summary>
    public partial class UserID_Window1 : Window
    {
        private const int TXT_5120_GROUP_NUM = 8;
        private const int TXT_5120_BURST_NUM = 8;
        private const int TXT_5120_SLOT_NUM = 8;
        private const int TXT_5120_SINGLEPOS_NUM = 10;
        private int group_idx = 0;
        private int burst_idx = 0;
        private int slot_idx = 0;
        private int single_idx = 0;
        private string fileScrambleIdTxt = "userid/scramble_id_set_5120.txt";

        public event EventHandler<string> ValueSelected;

        private void OnValueSelected(string value)
        {
            ValueSelected?.Invoke(this, value);
        }

        public UserID_Window1()
        {
            InitializeComponent();
        }


        private void readIdx()
        {
            if (Userid_TextBox == null)
                return;
            Userid_TextBox.Text = GetUsridToFile(group_idx, burst_idx, slot_idx, single_idx);
        }

        private void groupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            group_idx = group_ComboBox.SelectedIndex;
            readIdx();
        }

        private void burstComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            burst_idx = burst_ComboBox.SelectedIndex;
            readIdx();
        }

        private void slotComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            slot_idx = slot_ComboBox.SelectedIndex;
            readIdx();
        }

        private void singleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            single_idx = single_ComboBox.SelectedIndex;
            readIdx();
        }

        private string GetUsridToFile(int groupidx, int burstidx, int slotidx, int singleidx = 0)
        {
            if (groupidx >= TXT_5120_GROUP_NUM || burstidx >= TXT_5120_BURST_NUM ||
                slotidx >= TXT_5120_SLOT_NUM || singleidx >= TXT_5120_SINGLEPOS_NUM)
            {
                MessageBox.Show("Invalid indices");
                return null;
            }

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

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {

            string selectedValue = Userid_TextBox.Text; // 从子窗口获取的值
            OnValueSelected(selectedValue);
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
