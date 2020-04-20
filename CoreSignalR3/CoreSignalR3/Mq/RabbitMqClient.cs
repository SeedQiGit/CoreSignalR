using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CoreSignalR3.Mq
{
    /// <summary>
    /// RabbitMQ客户端辅助类
    /// </summary>
    public class RabbitMqClient : IDisposable
    {
        #region 构造函数及字段

        private readonly IConnection _conn;

        /// <summary>
        ///  Common AMQP model,usually call it channel
        /// </summary>
        protected readonly ConcurrentDictionary<string, IModel> ModelDic = new ConcurrentDictionary<string, IModel>();

        private readonly ILogger<RabbitMqClient> _logger;
        private readonly IHostingEnvironment _env;

        /// <summary>
        /// 构造函数，单例启动
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionFactory"></param>
        /// <param name="env"></param>
        public RabbitMqClient(ILogger<RabbitMqClient> logger, ConnectionFactory connectionFactory, IHostingEnvironment env)
        {
            _conn = _conn ?? connectionFactory.CreateConnection();
            _logger = logger;
            _env = env;
            //string host = configuration.GetSection("EventBusConnection").Value;
            //string userName = configuration.GetSection("EventBusUserName").Value;
            //string pwd = configuration.GetSection("EventBusPassword").Value;
            //Open(host, userName, pwd);
        }

        public void Dispose()
        {
            foreach (var item in ModelDic) item.Value.Dispose();
            _conn.Dispose();
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 获取特性拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private RabbitMqQueueAttribute GetRabbitMqAttribute<T>()
        {
            var result1 = Attribute.GetCustomAttribute(typeof(T), typeof(RabbitMqQueueAttribute));
            var result = result1 != null ? result1 as RabbitMqQueueAttribute : new RabbitMqQueueAttribute();
            string rolekey = typeof(T).Name + $"_{_env.EnvironmentName}_";
            result.RouteKey = rolekey;
            result.DelayRouteKey = rolekey;
            result.DeadRouteKey = rolekey;
            result.QueueName = string.Concat(result.QueueName, "_", rolekey);
            result.DelayQueueName = string.Concat(result.DelayQueueName, "_", rolekey);
            result.DeadQueueName = string.Concat(result.DeadQueueName, $"_", rolekey);

            return result;
        }

        /// <summary>
        ///  获取或新增Model 也叫channel
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">匹配规则</param>
        /// <param name="exchangeType">交换机类型 direct</param>
        /// <param name="durable">是否持久化</param>
        /// <param name="autoDelete"></param>
        /// <param name="exchangeArgs"></param>
        /// <param name="queueArgs"></param>
        /// <returns></returns>
        private IModel GetOrAddModel(string exchange, string queue, string routingKey, string exchangeType,
            bool durable = true,
            bool autoDelete = false, IDictionary<string, object> exchangeArgs = null,
            IDictionary<string, object> queueArgs = null)
        {
            return ModelDic.GetOrAdd(queue, key =>
            {
                var model = _conn.CreateModel();
                //声明订阅通道
                ExchangeDeclare(model, exchange, exchangeType, durable, autoDelete, exchangeArgs);

                QueueDeclare(model, queue, durable, autoDelete, queueArgs);
                //绑定队列
                model.QueueBind(queue, exchange, routingKey);
                ModelDic[queue] = model;
                return model;
            });
        }

        private void ExchangeDeclare(IModel iModel, string exchange, string type, bool durable = true,
            bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            exchange = string.IsNullOrWhiteSpace(exchange) ? "" : exchange.Trim();
            iModel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }

        private void QueueDeclare(IModel channel, string queue, bool durable = true, bool autoDelete = false,
            IDictionary<string, object> arguments = null, bool exclusive = false)
        {
            queue = string.IsNullOrWhiteSpace(queue) ? "UndefinedQueueName" : queue.Trim();
            channel.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        #region 单例创建连接工厂，已经改为di 单例

        //private static readonly object LockObj = new object();

        ///// <summary>
        ///// 单例创建ConnectionFactory  这里直接用di ConnectionFactory的单例就行
        ///// </summary>
        ///// <param name="host"></param>
        ///// <param name="userName"></param>
        ///// <param name="pwd"></param>
        //private void Open(string host, string userName, string pwd)
        //{
        //    if (_conn != null) return;
        //    lock (LockObj)
        //    {
        //        if (_conn != null) return;
        //        // 不过这个写法也留着吧 这都是不同的方法，无关大局
        //        var factory = new ConnectionFactory
        //        {
        //            HostName = host,
        //            UserName = userName,
        //            Password = pwd,
        //            AutomaticRecoveryEnabled = true,
        //            RequestedHeartbeat = 15
        //        };
        //        _conn = _conn ?? factory.CreateConnection();
        //    }
        //}

        #endregion

        #endregion

        #region 普通收发消息

        public void SendMessage<T>(T messageObj) where T : class
        {
            var queueInfo = GetRabbitMqAttribute<T>();
            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Normal)
                throw new ArgumentException("消息上不具有任何特性");
            var channel = GetOrAddModel(queueInfo.ExchangeName, queueInfo.QueueName, queueInfo.RouteKey,
                queueInfo.ExchangeType, queueInfo.Durable);
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;//数据持久化
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageObj));
            channel.BasicPublish(queueInfo.ExchangeName, queueInfo.RouteKey, properties, body);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObj"></param>
        /// <param name="queueInfo"></param>
        public void SendMessage<T>(T messageObj, RabbitMqQueueModel queueInfo) where T : class
        {
            if (queueInfo == null)
                throw new ArgumentException("消息上不具有任何特性");
            var channel = GetOrAddModel(queueInfo.ExchangeName, queueInfo.QueueName, queueInfo.RouteKey,
                queueInfo.ExchangeType, queueInfo.Durable);
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            channel.ConfirmSelect();
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageObj));
            channel.BasicPublish(queueInfo.ExchangeName, queueInfo.RouteKey, properties, body);
            bool isOk = channel.WaitForConfirms();
            if (!isOk) _logger.LogError("发布消息至对列确认失败。队列内容：{0}", JsonSerializer.Serialize(messageObj));
        }

        #endregion

        #region 延时消息，插件

        /// <summary>
        /// 发送延迟消息 通过插件rabbitmq-delayed-message-exchange实现 (服务器要安装这个插件)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObj"></param>
        /// <param name="publishTime">消息推送时间，即服务接收到消息的时间</param>
        public void SendMessageDelay<T>(T messageObj, DateTime publishTime) where T : class
        {
            long delay = (long)(publishTime - DateTime.Now).TotalSeconds;
            SendMessageDelay(messageObj, delay);
        }

        /// <summary>
        /// 发送延迟消息 通过插件rabbitmq-delayed-message-exchange实现 (服务器要安装这个插件)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObj"></param>
        /// <param name="seconds">延时的秒数</param>
        public void SendMessageDelay<T>(T messageObj, long seconds) where T : class
        {
            var queueInfo = GetRabbitMqAttribute<T>();
            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Delay)
                throw new ArgumentException("消息上不具有任何特性");

            //延迟队列参数，必须
            IDictionary<string, object> args = new Dictionary<string, object>
            {
                {"x-delayed-type", "direct"}
            };
            var channel = GetOrAddModel(queueInfo.DelayExchangeName, queueInfo.DelayQueueName, queueInfo.DelayRouteKey,
                queueInfo.DelayExchangeType, queueInfo.Durable, false, args);
            var properties = channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object>
            {
                {"x-delay", (seconds*1000)}
            };
            properties.DeliveryMode = 2;

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageObj));
            channel.BasicPublish(queueInfo.DelayExchangeName, queueInfo.DelayRouteKey, properties, body);
        }

        #endregion

        #region 延迟消息，死信

        /// <summary>
        ///  发送延迟消息 通过死信实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObj"></param>
        /// <param name="publishTime">消息推送时间，即服务接收到消息的时间</param>
        public void SendMessageDead<T>(T messageObj, DateTime publishTime) where T : class
        {
            long delay = (long)(publishTime - DateTime.Now).TotalMilliseconds;
            SendMessageDead(messageObj, delay);
        }

        /// <summary>
        /// 发送延迟消息 通过死信实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObj"></param>
        /// <param name="milliseconds">延时的秒数</param>
        public void SendMessageDead<T>(T messageObj, long milliseconds) where T : class
        {
            var queueInfo = GetRabbitMqAttribute<T>();
            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Dead)
                throw new ArgumentException("消息上不具有任何特性");

            //延迟队列参数，必须
            IDictionary<string, object> queueArgs = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", queueInfo.DeadExchangeName},
                {"x-dead-letter-routing-key", queueInfo.DeadRouteKey},
                {"x-message-ttl", milliseconds}, // 这个时间要一致 //单位毫秒！！
                {"x-expires", milliseconds+5000},//单位毫秒！！
            };

            //创建一个名叫"wait_dead_queuexxxxxx"的固定等死消息队列
            string queueName = "wait_dead_queue" + Guid.NewGuid().ToString();
            var model = _conn.CreateModel();
            model.QueueDeclare(queueName, false, false, true, queueArgs);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageObj));
            //发送消息到等死队列
            model.BasicPublish("", queueName, null, body);
        }

        #endregion

        #region 消息接收

        public void ReceiveMessage<T>(Func<T, CancellationToken, Task> handler)
        {
            var queueInfo = GetRabbitMqAttribute<T>();

            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Normal)
                throw new ArgumentException("消息上不具有任何特性");
            if (handler == null) throw new NullReferenceException("处理事件为null");
            var channel = GetOrAddModel(queueInfo.ExchangeName, queueInfo.QueueName, queueInfo.RouteKey,
                queueInfo.ExchangeType, queueInfo.Durable);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueInfo.QueueName, false, consumer);
            consumer.Received += async (model, ea) => { await _doAsync(channel, ea, handler); };
        }

        public IModel ReceiveMessage<T>(Func<T, Task> handler)
        {
            var queueInfo = GetRabbitMqAttribute<T>();
            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Normal)
                throw new ArgumentException("消息上不具有任何特性");
            if (handler == null) throw new NullReferenceException("处理事件为null");
            var channel = GetOrAddModel(queueInfo.ExchangeName, queueInfo.QueueName, queueInfo.RouteKey,
                queueInfo.ExchangeType, queueInfo.Durable);
            //同一时间不处理超过一条消息
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            //autoAck =true 自动应答，一旦我们完成任务，消费者会自动发送应答。通知RabbitMQ消息已被处理，可以从内存删除
            channel.BasicConsume(queueInfo.QueueName, false, consumer);
            consumer.Received += async (model, ea) => { await _doAsync(channel, ea, handler); };
            return channel;
        }

        /// <summary>
        ///  接收消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueInfo"></param>
        /// <param name="handler"></param>
        public IModel ReceiveMessage<T>(RabbitMqQueueModel queueInfo, Func<T, CancellationToken, Task> handler)
        {
            if (queueInfo == null)
                throw new ArgumentException("消息上不具有任何特性");
            if (handler == null) throw new NullReferenceException("处理事件为null");
            var channel = GetOrAddModel(queueInfo.ExchangeName, queueInfo.QueueName, queueInfo.RouteKey,
                queueInfo.ExchangeType, queueInfo.Durable);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueInfo.QueueName, false, consumer);
            consumer.Received += async (model, ea) => { await _doAsync(channel, ea, handler); };
            return channel;
        }

        #region 延时消息

        /// <summary>
        ///     接收延迟消息，通过 通过插件rabbitmq-delayed-message-exchange实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public IModel ReceiveMessageDelay<T>(Func<T, Task> handler)
        {
            var queueInfo = GetRabbitMqAttribute<T>();

            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Delay)
                throw new ArgumentException("消息上不具有任何特性");
            if (handler == null) throw new NullReferenceException("处理事件为null");
            //延迟队列参数，必须

            IDictionary<string, object> args = new Dictionary<string, object>
            {
                {"x-delayed-type", "direct"}
            };
            var channel = GetOrAddModel(queueInfo.DelayExchangeName, queueInfo.DelayQueueName, queueInfo.DelayRouteKey,
                queueInfo.DelayExchangeType, queueInfo.Durable, false, args);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueInfo.DelayQueueName, false, consumer);
            consumer.Received += async (model, ea) => { await _doAsync(channel, ea, handler); };
            return channel;
        }

        /// <summary>
        ///  接收延迟消息 通过死信方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public IModel ReceiveMessageDead<T>(Func<T, Task> handler)
        {
            var queueInfo = GetRabbitMqAttribute<T>();
            if (queueInfo == null || queueInfo.MessageKind != RabbitMsgKind.Dead)
                throw new ArgumentException("消息上不具有任何特性");

            if (handler == null) throw new NullReferenceException("处理事件为null");

            var channel = GetOrAddModel(queueInfo.DeadExchangeName, queueInfo.DeadQueueName, queueInfo.DeadRouteKey,
                queueInfo.DeadExchangeType, queueInfo.Durable);
            //同一时间不处理超过20条消息
            channel.BasicQos(0, 20, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueInfo.DeadQueueName, false, consumer);
            consumer.Received += async (model, ea) => { await _doAsync(channel, ea, handler); };
            return channel;
        }

        #endregion


        private async Task _doAsync<T>(IModel channel, BasicDeliverEventArgs ea,
            Func<T, CancellationToken, Task> normalHandler)
        {
            var body = ea.Body;
            try
            {
                var msgBody = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body.ToArray()));
                await normalHandler(msgBody, default(CancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError($"_doAsync处理异常：" + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException);
            }
            finally
            {
                //如果autoAck =true 可以不用再次确认收到   所以这里我再统一应答，各个方法使用自己的写法
                channel.BasicAck(ea.DeliveryTag, false);
            }
        }


        private async Task _doAsync<T>(IModel channel, BasicDeliverEventArgs ea,
            Func<T, Task> normalHandler)
        {
            try
            {                
                //Memory<byte> buffer = Encoding.UTF8.GetBytes(text).AsMemory();
                var msgBody = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                await normalHandler(msgBody);
            }
            catch (Exception ex)
            {
                _logger.LogError($"_doAsync处理异常：" + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException);
            }
            finally
            {
                //如果autoAck =true 可以不用再次确认收到
                channel.BasicAck(ea.DeliveryTag, false);
            }
        }

        #endregion
    }
}
