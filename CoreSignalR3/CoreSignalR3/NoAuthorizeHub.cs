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
            _logger.LogInformation("用户建立了连接" + userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }
    }
}
