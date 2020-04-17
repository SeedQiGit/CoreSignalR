using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreSignalR3
{
    [Authorize]
    public class BihuHub : Hub
    {
        private readonly ILogger<BihuHub> _logger;
        
        public BihuHub(ILogger<BihuHub> logger)
        {
            _logger = logger;

        }

        /// <summary>
        /// 这个是认证授权之后使用的 
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            var sub =Context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            _logger.LogInformation("已经连接:"+sub+"。连接id："+Context.ConnectionId);
            if (!string.IsNullOrEmpty(sub))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, sub);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var sub =Context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            _logger.LogInformation("断开连接:"+sub+"。连接id："+Context.ConnectionId);
            if (!string.IsNullOrEmpty(sub))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sub);
            }
            await base.OnDisconnectedAsync(ex);
        }
    }
}
