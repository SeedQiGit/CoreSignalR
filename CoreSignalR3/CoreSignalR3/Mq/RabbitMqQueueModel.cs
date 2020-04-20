using System;

namespace CoreSignalR3.Mq
{
    /// <summary>
    /// 消息类别
    /// </summary>
    public enum RabbitMsgKind
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal = 1,
        /// <summary>
        /// 延时
        /// </summary>
        Delay,
        /// <summary>
        /// 死信
        /// </summary>
        Dead,
    }

    /// <summary>
    /// MQ配置属性
    /// </summary>
    public class RabbitMqQueueAttribute : Attribute
    {
        /// <summary>
        /// 消息类别
        /// </summary>
        public RabbitMsgKind MessageKind { get; set; } = RabbitMsgKind.Normal;

        #region 普通消息

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; set; } = "bihu_api_exc";

        /// <summary>
        /// 队列名称【必填】
        /// </summary>
        public string QueueName { get; set; } = "bihu_api";

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 路由名称【必填】
        /// </summary>
        public string RouteKey { get; set; }

        /// <summary>
        /// 交换机类型
        /// </summary>
        public string ExchangeType { get; set; } = "direct";

        #endregion

        #region 插件延时使用

        /// <summary>
        /// 插件延迟消息交换机名称
        /// </summary>
        public string DelayExchangeName { get; set; } = "bihu_api_delay_exc";

        /// <summary>
        /// 插件延时消息队列名称  【必填】
        /// </summary>
        public string DelayQueueName { get; set; } = "bihu_api_delay";

        /// <summary>
        /// 插件延时消息路由Key名称 【必填】
        /// </summary>
        public string DelayRouteKey { get; set; } = "bihu_api_delay_routekey";

        /// <summary>
        /// 插件延时交换机类型
        /// </summary>
        public string DelayExchangeType { get; set; } = "x-delayed-message";

        #endregion

        #region 死信延时使用

        /// <summary>
        /// 死信延时消息交换机名称 默认和ExchangeName一样
        /// </summary>
        public string DeadExchangeName { get; set; } = "bihu_api_dead_exc";

        /// <summary>
        /// 死信延时消息队列名称 【必填】
        /// </summary>
        public string DeadQueueName { get; set; } = "bihu_api_dead";

        /// <summary>
        /// 死信延时消息路由Key名称 【必填】
        /// </summary>
        public string DeadRouteKey { get; set; } = "bihu_api_dead_routekey";

        /// <summary>
        /// 死信延时交换机类型
        /// </summary>
        public string DeadExchangeType { get; set; } = "direct";

        #endregion
    }
    public class RabbitMqQueueModel
    {
        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 队列名称【必填】
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// 路由名称【必填】
        /// </summary>
        public string RouteKey { get; set; }

        /// <summary>
        /// 交换机类型
        /// </summary>
        public string ExchangeType { get; set; }
    }
}
