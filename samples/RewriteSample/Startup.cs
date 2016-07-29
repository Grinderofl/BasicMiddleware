﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Rewrite.UrlRewrite;

namespace RewriteSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRewriter(new UrlRewriteOptions()
                  .ImportFromUrlRewrite("UrlRewrite.xml"));
            app.Run(context => context.Response.WriteAsync(context.Request.Path));

        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
