
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WIoTa_Serial_Tool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 关闭 GPU 引擎
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 获取未处理的异常对象
            Exception exception = e.ExceptionObject as Exception;

            // 打印错误信息
            if (exception != null)
            {
                string errorMessage = $"发生未处理的异常：{exception.Message}";
                DateTime now = DateTime.Now;
                string timestamp = now.ToString("yyyy-MM-dd_HH_mm_ss_fff");
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeFolderPath = Path.GetDirectoryName(exePath);
                string  logFileName = $"{exeFolderPath}\\log\\Error_{timestamp}.log";
                SaveLogFile(logFileName,errorMessage);
                // 或者使用其他日志记录方式，如写入日志文件
            }
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
                // }
            }
        }
    }
}

