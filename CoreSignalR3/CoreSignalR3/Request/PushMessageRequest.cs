namespace CoreSignalR3.Request
{
    public class PushMessageRequest
    {
        /// <summary>
        /// 消息实体
        /// </summary>
        public object Content { get; set; }
        /// <summary>
        /// 推送到的用户id 
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 消息类型，接口和前端统一
        /// </summary>
        public int MessageType { get; set; }
    }
}
