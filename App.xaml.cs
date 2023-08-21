
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        }

    }
}

