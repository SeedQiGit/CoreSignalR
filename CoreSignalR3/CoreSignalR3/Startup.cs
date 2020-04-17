using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Model.Enums;
using Infrastructure.Model.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

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
            services.AddControllersWithViews()
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

            services.AddSignalR();

            services
                .AddSignalR()
                .AddStackExchangeRedis(SettingManager.GetValue("SignalrStoreConnectionString"), options =>
                {
                    options.Configuration.ChannelPrefix = "bihuHub";
                });

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

            app.UseExceptionHandling();
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NoAuthorizeHub>("/noAuthorizeHub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}");
            });

            ServiceProviderExtension.ServiceProvider = app.ApplicationServices;
        }
    }
}
