using CoreSignalR3.Mq;
using CoreSignalR3.Mq.EventHandler;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Model.Enums;
using Infrastructure.Model.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace CoreSignalR3
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            #region MVC

            //AddControllersWithViews 是mvc项目使用  api可以直接AddControllers
            services.AddControllersWithViews(options =>
                {
                    // 异常处理  使用中间件的异常处理吧
                    //options.Filters.Add(typeof(HttpGlobalExceptionFilter<Exception>));
                })
                .AddJsonOptions(options =>
                {
                    //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    //options.SuppressModelStateInvalidFilter = true;
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var validationErrors = context.ModelState
                            .Keys
                            .SelectMany(k => context.ModelState[k].Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray();
                        var json = BaseResponse.GetBaseResponse(BusinessStatusType.ParameterError, string.Join(",", validationErrors));

                        return new BadRequestObjectResult(json)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

            services.AddHttpClient();

            services.AddHttpContextAccessor();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder.AllowAnyOrigin()
            //            .WithMethods("GET", "POST", "HEAD", "PUT", "DELETE", "OPTIONS")
            //    );
            //});

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        //.WithMethods("GET", "POST", "HEAD", "PUT", "DELETE", "OPTIONS")
                        //如果把下面的也允许就会报错
                        //.AllowCredentials()
                        );
            });

            #endregion

            #region Authentication

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
                        //会报StatusCode cannot be set because the response has already started.
                        //context.Response.StatusCode = 401;
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
                options.Authority = SettingManager.GetValue("Authority");
                options.RequireHttpsMetadata = false;
                options.Audience = "employee_center";
                //options.TokenValidationParameters.ValidIssuer = "null";
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromMinutes(30)
                };
            });

            #endregion

            #region SignalR

            services.AddSignalR();

            services
                .AddSignalR()
                .AddStackExchangeRedis(SettingManager.GetValue("SignalrStoreConnectionString"), options =>
                {
                    options.Configuration.ChannelPrefix = "bihuHub";
                });

            #endregion

            #region DataProtection密钥共享

            //cookies 密钥共享，可以用redis 也可以是同一个xml文件  这里我用redis 
            //services.AddSingleton<IXmlRepository, CustomFileXmlRepository>();
            //services.AddDataProtection(configure =>
            //{
            //    configure.ApplicationDiscriminator = "bihuHub";
            //});

            //建立Redis 连接
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(SettingManager.GetValue("SignalrStoreConnectionString"));
            services.AddSingleton(redis);

            //添加数据保护服务，设置统一应用程序名称，并指定使用Reids存储私钥
            services.AddDataProtection()
                .SetApplicationName(Assembly.GetExecutingAssembly().FullName)
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

            #endregion

            #region Rabbitmq

            services.AddRabbitmq();
            ConfigureRabbitMqClient(services);

            #endregion

        }

        /// <summary>
        /// 配置RabbitMqClient订阅
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureRabbitMqClient(IServiceCollection services)
        {
            services.AddSingleton<RabbitMqClient>();
            services.AddHostedService<NormalEventHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            // 配置跨域
            app.UseCors("CorsPolicy");

            app.UseRouting();
            // app.UseAuthorization() must appear between app.UseRouting() and app.UseEndpoints(...).

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseExceptionHandling();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NoAuthorizeHub>("/noAuthorizeHub", options =>
                     options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransports.All);
                endpoints.MapHub<BihuHub>("/bihuHub", options =>
                     options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransports.All);
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}");
            });

            ServiceProviderExtension.ServiceProvider = app.ApplicationServices;
        }
    }

    public static class CustomExtensionsMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitmq(this IServiceCollection services)
        {
            var section = SettingManager.GetSection("RabbitMqSettings");
            string connectionString = section.GetSection("RabbitMqConnection").Value;
            string userName = section.GetSection("RabbitMqUserName").Value;
            string password = section.GetSection("RabbitMqPwd").Value;

            var factory = new ConnectionFactory()
            {
                HostName = connectionString,
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(15)
            };

            if (!string.IsNullOrEmpty(userName))
            {
                factory.UserName = userName;
            }

            if (!string.IsNullOrEmpty(password))
            {
                factory.Password = password;
            }
            services.AddSingleton(factory);
            return services;
        }
    }
}
