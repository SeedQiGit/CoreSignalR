using FeiCheSignalR.Infrastructure.Helper;
using FeiCheSignalR.Models.EnumModel;
using FeiCheSignalR.Models.Response;
using FeiCheSignalR.Models.ViewModel;
using FeiCheSignalR.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeiCheSignalR.Hubs
{
    public class MessageHub: Hub
    {
        private readonly ILogger<MessageHub> _logger;
        private MessageCenterService _messageSer;
        private const string UserIdKey = "UserId";

        /// <summary>
        /// 用户的connectionID与用户名对照表,线程安全的集合
        /// </summary>
        public static ConcurrentDictionary<string, string> connections = new ConcurrentDictionary<string, string>();

        public MessageHub(ILogger<MessageHub> logger, MessageCenterService messageSer)
        {
            _logger = logger;
            _messageSer = messageSer;
        }

        #region 下线方法

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string userId = Context.Items[UserIdKey].ToString();
                if (!string.IsNullOrEmpty(userId))
                {
                    //如果在集合中，就移除掉
                    if (connections[userId] == Context.ConnectionId)
                    {
                        connections.TryRemove(userId);
                    }
                }
            }
            catch (Exception)
            {
            }
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 退出signar 
        /// </summary>
        /// <returns></returns>
        public async Task LogOut(string clientId)
        {
            try
            {
                await Clients.Client(clientId).SendAsync("logOut");
            }
            catch (Exception ex)
            {
                _logger.LogError("退出signar发生异常:" + ex.Source + "\n" + ex.StackTrace + "\n" + ex.Message + "\n" + ex.InnerException);
            }
        }

        #endregion

        #region 用户上线方法

        /// <summary>
        /// 用户上线函数
        /// </summary>
        /// <param name="name"></param>
        public async Task sendLogin(string userId)
        {
            try
            {
                string thisClient = Context.ConnectionId;
                //记录这个链接对应的UserId
                Context.Items.Add(UserIdKey, userId);
                //之前的连接下线
                if (connections.ContainsKey(userId))
                {
                    //记录id，执行下线
                    string userBefor = connections[userId];
                    if (!string.IsNullOrEmpty(userBefor))
                    {
                        LogOut(userBefor).Start();
                        //await LogOut(userBefor);
                    }
                }
                connections.AddOrUpdate(userId, thisClient, (x, y) => { return thisClient; });

                await getMessageAsy(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError("发生异常:" + ex.Source + "\n" + ex.StackTrace + "\n" + ex.Message + "\n" + ex.InnerException);
            }
        }

        /// <summary>
        /// 异步获取信息，并发送给客户端
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task getMessageAsy(string userId)
        {
            try
            {
                string messageList = await _messageSer.MessageShorById(userId);
                //查看是否有数据
                if (string.IsNullOrEmpty(messageList))
                {
                    return;
                }
                //这里是用集合的方式发送给前端，所以造成以后单条的也要集合方式发送给前端。。。
                await sendByClient(userId, messageList);
            }
            catch (Exception ex)
            {
                _logger.LogError("发生异常:" + ex.Source + "\n" + ex.StackTrace + "\n" + ex.Message + "\n" + ex.InnerException);
            }
        }

        #endregion

        /// <summary>
        ///  发送函数，服务器将消息发送给前端，通过前端已经实现的SendMessage方法
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="content">推送内容</param>
        public async Task sendByClient(string userId, string content)
        {
            //sendMessage函数是前端函数
            await Clients.Client(connections[userId]).SendAsync("sendMessage", content);
        }

       
    }
}
