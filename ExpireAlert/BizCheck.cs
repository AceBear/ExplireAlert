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
                // index
                this.AlterDatabase(ctx);

                // query
                var query = from c in ctx.GetTable<Gsp_shouying_qyshb>()
                            where c.youxiao_rq_xk <= this.DateAlarm
                            orderby c.youxiao_rq_xk
                            select c;
                this.AlarmedList = query.ToList();
            }
        }

        protected void AlterDatabase(sdv7DataContext ctx)
        {
            string sqlCmd = "IF NOT EXISTS(SELECT * FROM sys.sysindexes WHERE name = 'idx_Gsp_shouying_qyshb__youxiao_rq_xk')\n" +
                "\tCREATE INDEX idx_Gsp_shouying_qyshb__youxiao_rq_xk ON Gsp_shouying_qyshb(youxiao_rq_xk)";
            ctx.ExecuteCommand(sqlCmd);

            sqlCmd = "IF(SCHEMA_ID(N'winphone') IS NULL) EXEC sp_executesql N'CREATE SCHEMA winphone'";
            ctx.ExecuteCommand(sqlCmd);

            sqlCmd = "IF OBJECT_ID(N'winphone.wx_notify', N'U') IS NULL\n" +
                "CREATE TABLE winphone.wx_notify(\n" +
                "md5 CHAR(32) NOT NULL,\n" +
                "openid CHAR(28) NOT NULL,\n" +
                "tm SMALLDATETIME DEFAULT GetDate(),\n" +
                "CONSTRAINT PK_WX_NOTIFY primary key (md5, openid))";
            ctx.ExecuteCommand(sqlCmd);
        }
    }
}
