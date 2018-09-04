using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeiCheSignalR.Filters;
using FeiCheSignalR.Hubs;
using FeiCheSignalR.Models.EnumModel;
using FeiCheSignalR.Models.Request;
using FeiCheSignalR.Models.Response;
using FeiCheSignalR.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FeiCheSignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageCenterController : ControllerBase
    {
        private readonly ILogger<MessageCenterController> _logger;
        private readonly MessageCenterService _messageSer;

        public MessageCenterController(ILogger<MessageCenterController> logger, MessageCenterService messageSer)
        {
            _logger = logger;
            _messageSer = messageSer;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            _logger.LogInformation("123123");
            _logger.LogError("1231231");
            return new string[] { "value1", "value2" };
        }

        [HttpPost("TestPost")]
        [ModelVerifyFilter]
        public async Task<BaseResponse> TestPost()
        {
            return BaseResponse.GetBaseResponse(BusinessStatusType.OK, await _messageSer.MessageShorById("105"));
            //return await Task.Run(() => { return BaseResponse.GetBaseResponse(BusinessStatusType.OK); });
        }

        /// <summary>
        /// 给用户发送信息，并返回发送状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SendMessageById")]
        [ModelVerifyFilter]
        public async Task<BaseResponse>  SendMessageById([FromBody]PushMessageRequest request)
        {
            if (!MessageHub.connections.ContainsKey(request.ReceivedUserId.ToString()))
            {
                return BaseResponse.GetBaseResponse(BusinessStatusType.LoginExpire); 
            }
            return await _messageSer.SendMessageById(request);
        }
    }
}