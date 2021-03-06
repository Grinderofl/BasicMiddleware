﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace RewriteSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment hostingEnv)
        {
            app.UseRewriter(new UrlRewriteOptions()
                  .ImportFromUrlRewrite(hostingEnv, "UrlRewrite.xml")
                  .ImportFromModRewrite(hostingEnv, "Rewrite.txt"));
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
