using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiCheSignalR.Filters
{
    /// <summary>
    /// 全局日志记录
    /// </summary>
    public class LogAttribute : ActionFilterAttribute
    {
        #region 构造函数及属性

        //定时器的固定key
        private const string timeKey = "__action_duration__";

        #endregion

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {

            var stopWatch = new Stopwatch();
            actionContext.HttpContext.Items[timeKey] = stopWatch;
            stopWatch.Start();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            StringBuilder msgSb = new StringBuilder();

            // 请求标识 请求地址  请求参数
            var requestUri = context.HttpContext.Request.Host.ToString() + context.HttpContext.Request.Path.ToString() + context.HttpContext.Request.QueryString.Value;
            var requestArguments = string.Empty;
            msgSb.Append("请求地址：" + requestUri + Environment.NewLine);

            if (context.HttpContext.Request.Method.ToUpper() == "Post")
            {
                var contentLength = context.HttpContext.Request.ContentLength;
                if (contentLength.HasValue && contentLength > 0)
                {
                    context.HttpContext.Request.EnableRewind();
                    using (var mem = new MemoryStream())
                    {
                        context.HttpContext.Request.Body.Position = 0;
                        context.HttpContext.Request.Body.CopyTo(mem);
                        mem.Position = 0;
                        using (StreamReader reader = new StreamReader(mem, Encoding.UTF8))
                        {
                            msgSb.Append("请求参数：" + reader.ReadToEnd() + Environment.NewLine);
                        }
                    }
                }
            }

            var stopWatch = context.HttpContext.Items[timeKey] as Stopwatch;
            if (stopWatch != null)
            {
                stopWatch.Stop();
                msgSb.Append(string.Format("[接口执行时间(ms):{0}]", stopWatch.Elapsed.TotalMilliseconds));
            }
            context.HttpContext.Items["LogString"] = msgSb.ToString();
        }

    }
}
