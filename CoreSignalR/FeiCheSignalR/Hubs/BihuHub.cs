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

        public async Task Join(int userId)
        {
            _logger.LogInformation("用户建立了连接"+userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }

        /// <summary>
        /// 这个是认证授权之后使用的 目前还是按照老的方式去做 todo 
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Claims.FirstOrDefault(c=>c.Type=="sub")?.Value);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await base.OnDisconnectedAsync(ex);
        }

    }
}
