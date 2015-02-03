using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExpireAlert
{
    class MainVM : BaseVM
    {
        public MainVM()
        {
            this.m_iconBlue = BitmapFrame.Create(new Uri("pack://application:,,,/ShieldBlue.ico"));
            this.m_iconYellow = BitmapFrame.Create(new Uri("pack://application:,,,/ShieldYellow.ico"));
            this.m_iconRed = BitmapFrame.Create(new Uri("pack://application:,,,/ShieldRed.ico"));
            this.m_iconNow = this.m_iconBlue;
            this.m_logEA = new EventLog() { Source = Name };
            this.AlarmList = new ObservableCollection<Gsp_shouying_qyshb>();
            this.m_tmSupress = DateTime.Parse("2000-01-01");

            // check interval
            int nInterval = 3600; // default value
            try
            {
                string strCheckIntervalSeconds = ConfigurationManager.AppSettings["checkIntervalSeconds"];
                nInterval = Int32.Parse(strCheckIntervalSeconds);
            }
            catch (Exception ex)
            {
                this.m_logEA.WriteEntry("读取配置项checkIntervalSeconds失败\r\n" + ex.ToString(), EventLogEntryType.Warning);
            }

            this.m_bizCheck = new BizCheck();
            this.m_timerCheck = new Timer(this.Check, null, 0, 1000*nInterval);
            this.m_logEA.WriteEntry(String.Format("每{0}秒检查一次", nInterval), EventLogEntryType.Information);
        }

        public const string Name = "许可证有效期";
        public string Title
        {
            get
            {
                if (m_nAlarmed + m_nExpired == 0)
                    return MainVM.Name;
                else
                    return String.Format("{0}(过期:{1}个 即将过期:{2}个)", MainVM.Name, m_nExpired, m_nAlarmed);
            }
        }

        public ImageSource Icon
        {
            get
            {
                return m_iconNow;      
            }
        }

        public ObservableCollection<Gsp_shouying_qyshb> AlarmList { get; protected set; }

        public void Check(object state)
        {
            try
            {
                this.m_logEA.WriteEntry("检查许可证开始", EventLogEntryType.Information);
                this.m_bizCheck.Check();
                this.JudgeExpired();
                this.m_logEA.WriteEntry("检查许可证完成", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                this.m_logEA.WriteEntry("检查许可证失败\r\n" + ex.ToString(), EventLogEntryType.Error);
            }
        }

        public void Clear()
        {
            this.m_nAlarmed = 0;
            this.m_nExpired = 0;
            this.m_iconNow = m_iconBlue;
            this.AlarmList.Clear();
            this.IsFlashIcon = false;

            OnPropertyChanged("Icon");
            OnPropertyChanged("IsFlashIcon");
            OnPropertyChanged("Title");
        }

        protected void JudgeExpired()
        {
            m_syncCtx.Post((state) => {

                this.AlarmList.Clear();
                this.m_nAlarmed = 0;
                this.m_nExpired = 0;
                foreach (var x in this.m_bizCheck.AlarmedList)
                {
                    x.IsExpired = (x.youxiao_rq_xk <= this.m_bizCheck.DateExpired);
                    x.IsAlarmed = (x.youxiao_rq_xk <= this.m_bizCheck.DateAlarm);
                    if (x.youxiao_rq_xk <= this.m_bizCheck.DateExpired)
                        this.m_nExpired++;
                    else if (x.youxiao_rq_xk <= this.m_bizCheck.DateAlarm)
                        this.m_nAlarmed++;
                    this.AlarmList.Add(x);
                }

                // Tray icon
                if (this.m_nExpired > 0) m_iconNow = m_iconRed;
                else if (this.m_nAlarmed > 0) m_iconNow = m_iconYellow;
                else m_iconNow = m_iconBlue;

                this.IsFlashIcon = (this.m_nExpired > 0);
                OnPropertyChanged("IsFlashIcon");

                OnPropertyChanged("Icon");

                // event log
                var msg = String.Format("检查结果: 过期{0}个, 即将过期{1}个.", this.m_nExpired, this.m_nAlarmed);
                this.m_logEA.WriteEntry(msg, EventLogEntryType.Information);
                OnPropertyChanged("Title");

                if (this.m_nExpired > 0 || this.m_nAlarmed > 0) {
                    var wechat = new WeChat();
                    wechat.Notify(this.m_bizCheck.AlarmedList);
                }

            }, null);
        }

        public bool IsFlashIcon { get; set; }

        private ImageSource m_iconBlue;
        private ImageSource m_iconYellow;
        private ImageSource m_iconRed;
        private ImageSource m_iconNow;

        private Timer m_timerCheck;
        
        private BizCheck m_bizCheck;
        private EventLog m_logEA;

        private DateTime m_tmSupress;

        private int m_nExpired = 0;
        private int m_nAlarmed = 0;
    }
}
