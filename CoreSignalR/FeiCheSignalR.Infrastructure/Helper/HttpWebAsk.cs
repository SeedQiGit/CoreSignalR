using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FeiCheSignalR.Infrastructure.Helper
{
    public class HttpWebAsk
    {
        public static async Task<string> HttpClientPostAsync(string postData, string url)
        {
            var client = HttpClientHelper.GetClient();
            Task<string> result = Task.FromResult(string.Empty);
            HttpContent content = new StringContent(postData);
            MediaTypeHeaderValue typeHeader = new MediaTypeHeaderValue("application/json");
            typeHeader.CharSet = "UTF-8";
            content.Headers.ContentType = typeHeader;

            #region  增加tls设置

            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            #endregion

            var response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync();
            }
            return await result;
        }

        public static async Task<string> HttpClientGetAsync(string url)
        {
            var client = HttpClientHelper.GetClient();
            Task<string> result = Task.FromResult(string.Empty);
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync();
            }
            return await result;
        }
    }
}
