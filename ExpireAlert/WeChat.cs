using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                        foreach (WechatUser usr in wechatCfg.Users)
                        {
                            // 2. Send message
                            var dataMsg = JsonConvert.SerializeObject(new
                            {
                                touser = usr.OpenId, // XGH
                                template_id = templateId,
                                url = "",
                                topcolor = "#FF0000",
                                data = new
                                {
                                    first = new { value = strTitle, color = "#FF3333" },
                                    content = new { value = sbContent.ToString(), color = "#FF3333" },
                                    occurtime = new { value = DateTime.Today.ToString("yyyy年M月d日"), color = "#FF3333" },
                                    remark = new { value = sbRemark.ToString(), color = "#FF7700" },
                                }
                            });
                            client.PostAsync(uriMsg, new StringContent(dataMsg)).ContinueWith((taskMsg) =>
                            {
                                taskMsg.Result.Content.ReadAsStringAsync().ContinueWith((taskSendResult) =>
                                {
                                    System.Diagnostics.Trace.WriteLine(taskSendResult.Result);
                                });
                            });
                        }
                    });

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }
        }
    }
}
