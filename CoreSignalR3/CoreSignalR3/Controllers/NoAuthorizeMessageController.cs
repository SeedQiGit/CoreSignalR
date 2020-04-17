using System.Text.Json;
using CoreSignalR3.Request;
using Infrastructure.Model.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CoreSignalR3.Controllers
{
    public class NoAuthorizeMessageController:BaseController
    {
        private readonly ILogger<NoAuthorizeMessageController> _logger;
        private readonly IHubContext<NoAuthorizeHub> _hubcontext;

        public NoAuthorizeMessageController(ILogger<NoAuthorizeMessageController> logger, IHubContext<NoAuthorizeHub> hubcontext)
        {
            _logger = logger;
            _hubcontext = hubcontext;
        }

        [HttpPost("PushMessage")]
        [ProducesResponseType(typeof(BasePageResponse<BaseResponse>), 1)]
        public async Task<BaseResponse>  PushMessage([FromBody]PushMessageRequest request)
        {
            var message = JsonSerializer.Serialize (request);
            _logger.LogInformation("PushMessage:" + message);
           
            await _hubcontext.Clients.Group(request.UserId.ToString()).SendAsync("PushMessage",message);
            return BaseResponse.Ok();
        }
    }
}
