using CoreSignalR3.Request;
using Infrastructure.Model.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CoreSignalR3.Controllers
{
    public class NoAuthorizeMessageController:ControllerBase
    {
        private readonly ILogger<NoAuthorizeMessageController> _logger;
        private readonly IHubContext<NoAuthorizeHub> _hubcontext;

        public NoAuthorizeMessageController(ILogger<NoAuthorizeMessageController> logger, IHubContext<NoAuthorizeHub> hubcontext)
        {
            _logger = logger;
            _hubcontext = hubcontext;
        }

        [HttpPost]
        public async Task<BaseResponse>  PushMessage([FromBody]PushMessageRequest request)
        {
            _logger.LogInformation("PushMessage:" + JsonConvert.SerializeObject(request));
          
            await _hubcontext.Clients.Group(request.UserId.ToString()).SendAsync("PushMessage",JsonConvert.SerializeObject(request));
            return BaseResponse.Ok();
        }

    }
}
