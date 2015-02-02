using System;
using System.Collections.Generic;
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

        public void Notify()
        {
            try
            {
                var client = new HttpClient();
                // 1. Get the AccessToken
                var uriToken = new Uri("http://dev.incardata.com.cn/srv/s/1/AccessToken");
                client.GetStringAsync(uriToken).ContinueWith((taskToken) =>
                {
                    var token = JsonConvert.DeserializeObject(taskToken.Result) as JObject;

                    var templateId = "hy9lTGSM7Tu-YN8kDNQXvejJ3hUWXD5OhV1Rq45uGJE";

                    // 2. Send message
                    var uriMsg = String.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", token.GetValue("token"));
                    var dataMsg = JsonConvert.SerializeObject(new
                    {
                        touser = "oAPKMuJssQAohcEgKyKkcRDUDiAw",
                        template_id = templateId,
                        url = "www.baidu.com",
                        topcolor = "#FF0000",
                        data = new
                        {
                            first = new { value = "许可证到期告警", color = "#173177" },
                            content = new { value = "许可证XXX已到期", color = "#173177" },
                            occurtime = new { value = "2015-01-16", color = "#173177" },
                            remark = new { value = "其它未尽事宜", color = "#173177" },
                        }
                    });
                    client.PostAsync(uriMsg, new StringContent(dataMsg)).ContinueWith((taskMsg) =>
                    {
                        taskMsg.Result.Content.ReadAsStringAsync().ContinueWith((taskSendResult) =>
                        {
                            System.Diagnostics.Trace.WriteLine(taskSendResult.Result);
                        });
                    });
                });

            }
            catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
    }
}
