using CoreSignalR3.Mq.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreSignalR3.Mq.EventHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class NormalEventHandler : BaseRabbitListener<NormalEvent>
    {
        private readonly ILogger<NormalEventHandler> _logger;
        

        public NormalEventHandler(RabbitMqClient mqClient, ILogger<NormalEventHandler> logger) : base(mqClient)
        {
            _logger = logger;
        }

        public override async Task Handle(NormalEvent message)
        {
            try
            {
                _logger.LogInformation("收到信息：" + JsonSerializer.Serialize(message) + "时间：" + DateTime.Now);
                //处理完成，手动确认 这个我封装在RabbitMqClient 的_doAsync 里面了
                //channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"NormalEvent异常：" + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException);
            }
        }
    }
}
