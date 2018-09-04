using System;
using System.Collections.Generic;
using System.Text;

namespace FeiCheSignalR.Models.ViewModel
{
    public class MessageViewModel
    {
        /// <summary>
        /// 推送消息ID
        /// </summary>
        public Int64 MessageId { get; set; }
        /// <summary>
        /// 报案id
        /// </summary>
        public Int64 ReportId { get; set; }
        /// <summary>
        /// 消息内容（当消息类型为2的时候为任务数）
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 保单号
        /// </summary>
        public string PolicyNo { get; set; }
        /// <summary>
        /// 出险人姓名
        /// </summary>
        public string LossName { get; set; }
        /// <summary>
        /// 消息类型  1 核心系统状态提醒  2 新任务数提醒
        /// </summary>
        public int MessageType { get; set; }
        /// <summary>
        /// 报案号 从中心返回(消息类型为1时需要赋值)
        /// </summary>
        public string RegistNo { get; set; }
    }
}
