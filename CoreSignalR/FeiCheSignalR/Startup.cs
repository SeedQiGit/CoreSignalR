using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using FeiCheSignalR.Filters;
using FeiCheSignalR.Hubs;
using FeiCheSignalR.Infrastructure.Configuration;
using FeiCheSignalR.Infrastructure.Helper;
using FeiCheSignalR.Middlewares;
using FeiCheSignalR.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace FeiCheSignalR
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //跨域
            services.AddCors(options =>
            {
                options.AddPolicy("SignalRCors",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials());
            });

            //增加redis负载
            services.AddSignalR().AddStackExchangeRedis(ConfigurationManager.GetValue("SignalrStoreConnectionString"));

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

            #region 认证授权

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnChallenge = async context =>
                    {
                        //var data = new BaseResponse(BusinessStatusType.Unauthorized);
                        //var result = JsonConvert.SerializeObject(data);
                        var result = " {\"code\":401,\"message\":\"授权失败\",\"data\":null}";
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json;charset=utf-8";
                        await context.Response.WriteAsync(result);
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/bihuHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.Authority = ConfigurationManager.GetValue("Authority");
                options.RequireHttpsMetadata = false;
                options.Audience = "employee_center";
                options.TokenValidationParameters.ValidIssuer = "null";
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromMinutes(30)
                };
            });

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
            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<BihuHub>("/bihuHub", options =>
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransports.All);
                routes.MapHub<BihuHub>("/messageHub");
            });

            //异常处理中间件
            app.UseExceptionHandling();
            app.UseBufferedResponseBody();
            app.UseMvc();
 
            HttpClientHelper.WarmUpClient();
        }
    }
}
