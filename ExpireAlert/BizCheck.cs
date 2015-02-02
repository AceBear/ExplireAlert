using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpireAlert
{
    class BizCheck
    {
        public BizCheck()
        {
            this.PreAlarmDays = 0;
            this.DateExpired = DateTime.Today;
            this.DateAlarm = this.DateExpired;
            this.AlarmedList = new List<Gsp_shouying_qyshb>();
        }

        // 提前预警的天数
        public int PreAlarmDays { get; set; }
        // 过期日期
        public DateTime DateExpired { get; set; }
        // 预警日期
        public DateTime DateAlarm { get; set; }
        // 将要过期的
        public IEnumerable<Gsp_shouying_qyshb> AlarmedList { get; protected set; }

        public void Check()
        {
            // 当前时间
            this.DateExpired = DateTime.Today;

            // 读取配置参数,提前N天预警
            try{
                string strPreAlarmDays = ConfigurationManager.AppSettings["preAlarmDays"];
                this.PreAlarmDays = Int32.Parse(strPreAlarmDays);
            }
            catch(Exception ex){
                EventLog.WriteEntry(MainVM.Name, "读取配置项preAlarmDays失败\r\n" + ex.ToString(), EventLogEntryType.Warning);
            }

            this.DateAlarm = this.DateExpired + TimeSpan.FromDays(this.PreAlarmDays);

            using (var ctx = new sdv7DataContext()) {
                var query = from c in ctx.GetTable<Gsp_shouying_qyshb>()
                            where c.youxiao_rq_xk <= this.DateAlarm
                            orderby c.youxiao_rq_xk
                            select c;
                this.AlarmedList = query.ToList();
            }
        }
    }
}
