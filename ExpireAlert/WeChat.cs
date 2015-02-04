using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExpireAlert
{
    class WeChat
    {
        public WeChat()
        {
        }

        public void Notify(IEnumerable<Gsp_shouying_qyshb> listAlarms)
        {
            // 免打扰时间不发送消息
            if (WechatConfigSection.Current.DoNotDisturb()) return;

            if (listAlarms != null && listAlarms.Count() > 0)
            {
                string strTitle = "许可证即将到期";
                var sbContent = new StringBuilder();
                var sbRemark = new StringBuilder();
                foreach(var x in listAlarms)
                {
                    if (x.IsExpired)
                    {
                        sbContent.AppendFormat("{0}[{1:yyyy-MM-dd}]\n", x.mingcheng, x.youxiao_rq_xk);
                        strTitle = "许可证已到期";
                    }
                    else if (x.IsAlarmed)
                    {
                        if (sbRemark.Length == 0) sbRemark.AppendLine("\n以下许可证也即将到期:");
                        sbRemark.AppendFormat("{0}[{1:yyyy-MM-dd}]\n", x.mingcheng, x.youxiao_rq_xk);
                    }
                }

                try
                {
                    var client = new HttpClient();
                    // 1. Get the AccessToken
                    var uriToken = new Uri("http://dev.incardata.com.cn/srv/s/1/AccessToken");
                    client.GetStringAsync(uriToken).ContinueWith((taskToken) =>
                    {
                        var token = JsonConvert.DeserializeObject(taskToken.Result) as JObject;
                        var uriMsg = String.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", token.GetValue("token"));

                        var wechatCfg = WechatConfigSection.Current;
                        var templateId = wechatCfg.NotifyTemplateId;

                        using (var ctx = new sdv7DataContext())
                        {
                            foreach (WechatUser usr in wechatCfg.Users)
                            {
                                // 2. Prepare message
                                var dataMsg = JsonConvert.SerializeObject(new
                                {
                                    touser = usr.OpenId, // XGH
                                    template_id = templateId,
                                    url = "",
                                    topcolor = "#FF7700",
                                    data = new
                                    {
                                        first = new { value = strTitle, color = "#FF3333" },
                                        content = new { value = sbContent.ToString(), color = "#FF3333" },
                                        occurtime = new { value = DateTime.Today.ToString("yyyy年M月d日"), color = "#FF3333" },
                                        remark = new { value = sbRemark.ToString(), color = "#FF7700" },
                                    }
                                });

                                // 3. Check if send already
                                var md5 = MakeMD5(dataMsg);
                                var querySendAlready = from log in ctx.GetTable<wx_notify>()
                                                       where log.md5 == md5 && log.openid == usr.OpenId
                                                       select log;
                                if (querySendAlready.Any()) continue;

                                // 4. Send out
                                client.PostAsync(uriMsg, new StringContent(dataMsg)).ContinueWith((taskMsg) =>
                                {
                                    taskMsg.Result.Content.ReadAsStringAsync().ContinueWith((taskSendResult) =>
                                    {
                                        if (taskSendResult.Result != null)
                                        {
                                            var sent = JsonConvert.DeserializeObject(taskSendResult.Result) as JObject;
                                            if (sent.Value<int>("errcode") == 0)
                                            {
                                                // 5. Log it
                                                using (var ctx2 = new sdv7DataContext())
                                                {
                                                    var log = new wx_notify() {
                                                        md5 = md5,
                                                        openid = usr.OpenId,
                                                        tm = DateTime.Today
                                                    };
                                                    ctx2.GetTable<wx_notify>().InsertOnSubmit(log);
                                                    ctx2.SubmitChanges();
                                                }
                                            }
                                        }
                                        
                                    });
                                });
                            }
                        }
                    });

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }
        }

        private string MakeMD5(string strSource)
        {
            using (var md5 = MD5.Create())
            {
                byte[] md5Data = md5.ComputeHash(Encoding.UTF8.GetBytes(strSource));
                var sb = new StringBuilder();
                foreach (var c in md5Data)
                    sb.Append(c.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
