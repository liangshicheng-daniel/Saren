using System;
using Microsoft.AspNetCore.Hosting;


[assembly: HostingStartup(typeof(PcrSarenBot.Areas.Identity.IdentityHostingStartup))]
namespace PcrSarenBot.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}