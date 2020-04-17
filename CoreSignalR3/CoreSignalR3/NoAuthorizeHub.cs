using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CoreSignalR3
{
    public class NoAuthorizeHub: Hub
    {
        private readonly ILogger<NoAuthorizeHub> _logger;
        
        public NoAuthorizeHub(ILogger<NoAuthorizeHub> logger)
        {
            _logger = logger;

        }

        public async Task Join(long userId)
        {
            try
            {
                _logger.LogInformation("用户建立了连接" + userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogInformation("异常信息：" + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException);
            }
        }
    }
}
