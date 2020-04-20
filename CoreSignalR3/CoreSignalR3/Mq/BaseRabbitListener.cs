
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreSignalR3.Mq
{
    /// <summary>
    ///     RabbitMQ消息监听基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseRabbitListener<T> : IHostedService where T : class
    {
        #region 构造函数及字段

        protected RabbitMqClient MqClient;
        protected RabbitMsgKind MessageKind;
        protected IModel RabbitModel;


        public BaseRabbitListener(RabbitMqClient mqClient)
        {
            this.MqClient = mqClient;
            GetMessageKind();
        }

        private void GetMessageKind()
        {
            var result1 = Attribute.GetCustomAttribute(typeof(T), typeof(RabbitMqQueueAttribute));
            var result = result1 != null ? result1 as RabbitMqQueueAttribute : new RabbitMqQueueAttribute();
            MessageKind = result.MessageKind;
        }

        #endregion

        #region 服务启动或停止

        /// <summary>
        /// 实现IHostedService的启动事件
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Register(cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 实现IHostedService的停止事件
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            RabbitModel.Close();
            return Task.CompletedTask;
        }

        #endregion

        #region 消息监听接口

        /// <summary>
        ///     消费者处理事件
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual Task Handle(T message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     注册消费者监听
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected void Register(CancellationToken cancellationToken)
        {
            Func<T, Task> handler = Handle;
            switch (MessageKind)
            {
                case RabbitMsgKind.Normal:
                    RabbitModel = MqClient.ReceiveMessage(handler);
                    break;
                case RabbitMsgKind.Delay:
                    RabbitModel = MqClient.ReceiveMessageDelay(handler);
                    break;
                case RabbitMsgKind.Dead:
                    RabbitModel = MqClient.ReceiveMessageDead(handler);
                    break;
            }
        }

        #endregion
    }
}
