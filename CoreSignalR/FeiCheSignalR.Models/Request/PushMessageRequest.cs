using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FeiCheSignalR.Models.Request
{
    public class PushMessageRequest
    {
        /// <summary>
        /// 推送消息ID
        /// </summary>
        [Required, Range(1, Int64.MaxValue, ErrorMessage = "MessageId参数超出范围")]
        public Int64 MessageId { get; set; }
        [Required, Range(1, Int64.MaxValue, ErrorMessage = "ReceivedUserId参数超出范围")]
        public Int64 ReceivedUserId { get; set; }
        /// <summary>
        /// 消息类型  1 核心系统状态提醒  2 新任务数提醒
        /// </summary>
        [Required, Range(1, 2, ErrorMessage = "参数类型超出范围")]
        public int MessageType { get; set; }
        /// <summary>
        /// 报案id
        /// </summary>
        public int ReportId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
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
        /// 报案号 从中心返回(消息类型为1时需要赋值)
        /// </summary>
        public string RegistNo { get; set; }
    }
}
