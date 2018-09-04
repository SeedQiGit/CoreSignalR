using FeiCheSignalR.Models.EnumModel;
using FeiCheSignalR.Models.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiCheSignalR.Middlewares
{
    /// <summary>
    /// 异常捕获中间件
    /// </summary>
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            var body = string.Empty; //读取body是记录请求，post方式参数在body，要加到这里
            try
            {
                if (context.Request.Method.ToUpper() == "POST")
                {
                    context.Request.EnableRewind();//允许更改请求头位置
                    using (var mem = new MemoryStream())
                    {
                        context.Request.Body.CopyTo(mem);
                        mem.Position = 0;
                        using (StreamReader reader = new StreamReader(mem, Encoding.UTF8))
                        {
                            body = reader.ReadToEnd();
                            context.Request.Body.Position = 0;
                        }
                    }
                }
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, body);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex, string body)
        {
            string errorMsg = ex.Source + "\n" + ex.StackTrace + "\n" + ex.Message + "\n" + ex.InnerException;
            WriteErrorLog(context, ex, body);
            PathString path = context.Request.Path;
            var data = BaseResponse.GetBaseResponse(BusinessStatusType.Error);
            var result = JsonConvert.SerializeObject(data);
            context.Response.ContentType = "application/json;charset=utf-8";
            return context.Response.WriteAsync(result);
        }

        private void WriteErrorLog(HttpContext httpContent, Exception ex, string body)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.Append("请求API地址:");
            logBuilder.Append(httpContent.Request.Host.ToString() + httpContent.Request.Path.ToString() + httpContent.Request.QueryString.Value + Environment.NewLine);

            if (!string.IsNullOrWhiteSpace(body) && httpContent.Request.ContentType.ToLower().Contains("json"))
            {
                logBuilder.Append($"请求Body:");
                logBuilder.Append(body + Environment.NewLine);
            }
            logBuilder.Append("异常信息：" + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine);
            _logger.LogError(logBuilder.ToString());
        }
    }

    public static class ExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
