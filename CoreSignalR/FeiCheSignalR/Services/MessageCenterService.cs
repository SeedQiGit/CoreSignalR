using FeiCheSignalR.Hubs;
using FeiCheSignalR.Infrastructure.Configuration;
using FeiCheSignalR.Infrastructure.Helper;
using FeiCheSignalR.Models.EnumModel;
using FeiCheSignalR.Models.Request;
using FeiCheSignalR.Models.Response;
using FeiCheSignalR.Models.ViewModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeiCheSignalR.Services
{
    public class MessageCenterService
    {
        private readonly ILogger<MessageCenterService> _logger;
        private UrlModel _urlModel;
        private readonly IHubContext<MessageHub> _hubcontext;

        public MessageCenterService(ILogger<MessageCenterService> logger, IOptions<UrlModel> option, IHubContext<MessageHub> hubcontext)
        {
            _logger = logger;
            _urlModel = option.Value;
            _hubcontext = hubcontext;
        }

        /// <summary>
        /// 获取待发送信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> MessageShorById(string userId)
        {
            string request = "{ 'UserId':" + userId + "}";
            string url = $"{_urlModel.BihuApi}/api/Message/MessageExistById";
            string result = await HttpWebAsk.HttpClientPostAsync(request, url);
            JObject resultResponse = JsonConvert.DeserializeObject(result) as JObject;
            if (resultResponse["Code"].ToString() != "1")
            {
                _logger.LogError("请求获取人员推送信息出错，用户id：" + userId + Environment.NewLine + result);
                return "";
            }
            string value = resultResponse["Data"]["Value"].ToString(); 
            if (value=="[]")
            {
                return "";
            }
            value = value.Replace("\r\n","");
            return value;
        }

        /// <summary>
        /// 进行消息推送
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse> SendMessageById(PushMessageRequest request)
        {
            MessageViewModel thisMessage = JsonConvert.DeserializeObject<MessageViewModel>(JsonConvert.SerializeObject(request));
            List<MessageViewModel> messageList = new List<MessageViewModel>
                    {
                        thisMessage
                    };
            string content = JsonConvert.SerializeObject(messageList);
            _logger.LogInformation("系统消息已发送ID:" + request.ReceivedUserId + "内容:" + content);
            await _hubcontext.Clients.Client(MessageHub.connections[request.ReceivedUserId.ToString()]).SendAsync("sendMessage", content);
            return BaseResponse.GetBaseResponse(BusinessStatusType.OK);
        }
    }
}
