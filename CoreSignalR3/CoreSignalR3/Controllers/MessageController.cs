using CoreSignalR3.Request;
using Infrastructure.Model.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreSignalR3.Controllers
{
    public class MessageController:BaseController
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IHubContext<BihuHub> _hubcontext;

        public MessageController(ILogger<MessageController> logger, IHubContext<BihuHub> hubcontext)
        {
            _logger = logger;
            _hubcontext = hubcontext;
        }

        [HttpPost("QuoteResultMessage")]
        [ProducesResponseType(typeof(BasePageResponse<BaseResponse>), 1)]
        public async Task<BaseResponse>  PushMessage([FromBody]QuoteResultMessageRequest request)
        {
            _logger.LogInformation("报价结果推送:" + JsonSerializer.Serialize(request));
            await _hubcontext.Clients.Group(request.EmployeeId.ToString()).SendAsync("QuoteResultMessage",JsonSerializer.Serialize(new QuoteResultMessage() { Buid = request.Buid, Guid = request.guid, Source = request.Source }));
            return BaseResponse.Ok();
        }
    }
}
