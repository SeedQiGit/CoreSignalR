using FeiCheSignalR.Infrastructure.Helper;
using Microsoft.AspNetCore.Builder;
using System;
using System.IO;

namespace FeiCheSignalR.Middlewares
{
    public static class BufferedResponseBodyExtensions
    {
        public static IApplicationBuilder UseBufferedResponseBody(this IApplicationBuilder builder)
        {
            return builder.Use(async (context, next) =>
            {
                using (var ms = new MemoryStream())
                {
                    var orgBodyStream = context.Response.Body;
                    context.Response.Body = ms;

                    await next();//there is running MVC

                    using (var sr = new StreamReader(ms))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        LogHelper.Info(context.Items["LogString"].ToString() + Environment.NewLine + "请求返回值：" + sr.ReadToEnd());
                        //将原有的返回值复制给基类stream
                        ms.Seek(0, SeekOrigin.Begin);
                        await ms.CopyToAsync(orgBodyStream);
                        context.Response.Body = orgBodyStream;
                    }
                }
            });
        }
    }
}
