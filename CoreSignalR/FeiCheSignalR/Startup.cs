using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeiCheSignalR.Filters;
using FeiCheSignalR.Hubs;
using FeiCheSignalR.Infrastructure.Configuration;
using FeiCheSignalR.Infrastructure.Helper;
using FeiCheSignalR.Middlewares;
using FeiCheSignalR.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;

namespace FeiCheSignalR
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(env.ContentRootPath)
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
             .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //跨域
            services.AddCors(options =>
            {
                options.AddPolicy("SignalRCors",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials());
            });

            services.AddSignalR();

            services.AddMvc(opt =>
            {
                // 跨域
                opt.Filters.Add(new CorsAuthorizationFilterFactory("SignalRCors"));
                //模型验证过滤器，order:数字越小的越先执行
                opt.Filters.Add(typeof(ModelVerifyFilterAttribute), 1);
                //日志记录，全局使用
                opt.Filters.Add(new LogAttribute());

            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });


            services.AddScoped(typeof(MessageCenterService));


            #region 配置

            //获取api地址
            services.Configure<UrlModel>(Configuration.GetSection("UrlModel"));

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // 配置跨域
            app.UseCors("SignalRCors");

            //在管道中使用组建（也不知道我这个理解对不对）
            app.UseSignalR(routes =>
            {
                routes.MapHub<MessageHub>("/messageHub");
            });
            //异常处理中间件
            app.UseExceptionHandling();
            app.UseBufferedResponseBody();
            app.UseMvc();
 
            HttpClientHelper.WarmUpClient();
        }
    }
}
