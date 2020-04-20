using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreSignalR3.Mq.Event
{
    /// <summary>
    /// 普通事件
    /// </summary>
    [RabbitMqQueue(MessageKind = RabbitMsgKind.Normal)]
    public class NormalEvent
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
