using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ExpireAlert
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                if (!EventLog.SourceExists(MainVM.Name))
                {
                    EventLog.CreateEventSource(MainVM.Name, null);
                }

                EventLog.WriteEntry(MainVM.Name, "程序启动", EventLogEntryType.Information);
            }
            catch (Exception) {
                // 忽略任何错误
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            EventLog.WriteEntry(MainVM.Name, "程序退出", EventLogEntryType.Information);
        }
    }
}
