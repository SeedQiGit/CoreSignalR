using FeiCheSignalR.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FeiCheSignalR.Infrastructure.Helper
{
    public class HttpClientHelper
    {
        //私用的静态变量应该是线程安全的，静态方法也是线程安全的。
        private static HttpClient _client;

        public static void WarmUpClient()
        {
            var urlModel = ConfigurationManager.GetValue<string>("UrlModel:BihuApi");
            var client = GetClient();
            client.SendAsync(new HttpRequestMessage
            {
                //此方法被用来获取请求实体的元信息而不需要传输实体主体（entity-body）。此方法经常被用来测试超文本链接的有效性，可访问性，和最近的改变。
                Method = new HttpMethod("HEAD"),
                RequestUri = new Uri(urlModel + "/api/Message/MessageExistById")
            }).Wait();
        }
        public static HttpClient GetClient()
        {
            try
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.Timeout = TimeSpan.FromSeconds(200);//设置默认200s
                    _client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    // 清除头部信息
                    _client.DefaultRequestHeaders.Connection.Clear();
                    // 
                    _client.DefaultRequestHeaders.ConnectionClose = false;
                    return _client;
                }
                return _client;
            }
            catch (Exception)
            {
                return new HttpClient();
            }
        }
    }
}
