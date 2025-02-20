﻿using FastGithub.ReverseProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FastGithub
{
    /// <summary>
    /// ApplicationBuilder扩展
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 使用请求日志中间件
        /// </summary>
        /// <param name="app"></param> 
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            var middleware = app.ApplicationServices.GetRequiredService<RequestLoggingMiddleware>();
            return app.Use(next => context => middleware.InvokeAsync(context, next));
        }

        /// <summary>
        /// 使用反向代理中间件
        /// </summary>
        /// <param name="app"></param> 
        /// <returns></returns>
        public static IApplicationBuilder UseReverseProxy(this IApplicationBuilder app)
        {
            var middleware = app.ApplicationServices.GetRequiredService<ReverseProxyMiddleware>();
            return app.Use(next => context => middleware.InvokeAsync(context));
        }
    }
}
