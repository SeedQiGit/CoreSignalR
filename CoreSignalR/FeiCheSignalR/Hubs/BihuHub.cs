using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FeiCheSignalR.Hubs
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
            _logger.LogInformation("已经连接:"+sub+"连接id："+Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, sub);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var sub =Context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            _logger.LogInformation("断开连接:"+sub+"连接id："+Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sub);
            await base.OnDisconnectedAsync(ex);
        }

        //老的join方法，改为认证授权时候建立连接了。
        //public async Task Join(int userId)
        //{
        //    _logger.LogInformation("用户建立了连接"+userId);
        //    await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        //}
    }
}
