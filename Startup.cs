using KiraNet.GutsMvc.BBS.Hub;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.Filter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace KiraNet.GutsMvc.BBS
{
    public class Startup : IStartup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("setting.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void Configure(IApplicationBuilder app)
        {
            // 注册WebSocketHub中间件
            app.UserWebSocketHub(hub =>
            {
                hub.AddHub<ChatHub>("/hub/chat/");
            });

            // 注册MVC中间件
            app.UseGutsMvc(route =>
                route.AddRouteMap("default", "/{controller=home}/{action=index}/{id}"))
                .ConfigureNotFoundView("NotFound");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 增加对WebSocket的支持
            services.AddWebSocketHub();

            // 增加对GutsMvc的支持
            services.AddGutsMvc();

            // 增加自定义认证授权的支持
            services.AddSingleton<IClaimSchema, ClaimShema>();

            services.Configure<MapSetting>(Configuration.GetSection("MapSetting"));


            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            #if DEBUG
            loggerFactory.AddDebug();
            #endif
            services.AddSingleton<ILoggerFactory, LoggerFactory>(_ => loggerFactory);

            services.AddDbContext<GutsMvcDbContext>(builder =>
            {
                var dbLink = Configuration.GetSection("MapSetting:DbLink").Value;
                if (String.IsNullOrWhiteSpace(dbLink))
                {
                    throw new Exception("未找到数据库链接。");
                }

                builder.UseSqlServer(dbLink);
            });

            // 改用EF 2.0 新增的API来注入DbContext，同时配置最大数据库连接数
            //services.AddDbContextPool<GutsMvcDbContext>(option =>
            //{
            //    var dbLink = Configuration.GetSection("MapSetting:DbLink").Value;
            //    if (String.IsNullOrWhiteSpace(dbLink))
            //    {
            //        throw new Exception("未找到数据库链接。");
            //    }

            //    option.UseSqlServer(dbLink);
            //}, 200);

            // 添加cache支持
            services.AddDistributedMemoryCache();
            services.AddLogging();
            services.AddScoped<GutsMvcUnitOfWork>();
            services.AddScoped<IGutsMvcLogger, GutsMvcLogger>();
        }
    }
}
