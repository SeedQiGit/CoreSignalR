using System.ComponentModel;

namespace FeiCheSignalR.Models.EnumModel
{
    /// <summary>
    /// 业务状态码
    /// </summary>
    public enum BusinessStatusType
    {
        /// <summary>
        /// 失败
        /// </summary>
        [Description("操作失败")]
        Failed = 0,

        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        OK = 1,

        /// <summary>
        /// 用户未登陆或登陆到期
        /// </summary>
        [Description("用户未登陆或登陆到期")]
        LoginExpire = 9,


        [Description("未找到对应内容")]
        NotFindObject = 404,
        /// <summary>
        /// 请求参数错误
        /// </summary>
        [Description("请求参数错误")]
        ParameterError = -10000,

        /// <summary>
        /// 参数校验错误
        /// </summary>
        [Description("参数校验错误")]
        ModelVerifyFailed = -10001,

        /// <summary>
        /// 请求发生异常
        /// </summary>
        [Description("请求发生异常")]
        Error = -10003
    }
}
