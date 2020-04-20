using CoreSignalR3.Mq;
using CoreSignalR3.Mq.Event;
using Infrastructure.Model.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CoreSignalR3.Controllers
{
    public class EventController: BaseController
    {
        private readonly ILogger<EventController> _logger;
        private readonly RabbitMqClient _rabbitMqClient;

        public EventController(ILogger<EventController> logger, RabbitMqClient rabbitMqClient )
        {
            _logger = logger;
            _rabbitMqClient = rabbitMqClient;
        }

        [HttpGet("NormalEvent")]
        [ProducesResponseType(typeof(BaseResponse), 1)]
        public async Task<BaseResponse> NormalEvent()
        {
            NormalEvent nomalEvent = new NormalEvent { Content = " hello " };
            _rabbitMqClient.SendMessage(nomalEvent);
            return BaseResponse.Ok();
        }
    }
}
