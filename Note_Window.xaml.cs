using System.Windows;
using System.Windows.Input;

namespace NoteWindow
{
    public partial class MyWindow : Window
    {
        public MyWindow(string sendButton_Content)
        {
            InitializeComponent();
            MyTextBox.Text = sendButton_Content;
            MyTextBox.Focus();
            MyTextBox.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            //在单击“确定”按钮时执行的操作
            DialogResult = true;
            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //在单击“取消”按钮时执行的操作
            DialogResult = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 处理回车键事件
                DialogResult = true;
            }
        }

    }
}
