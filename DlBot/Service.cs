﻿using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using Topshelf;

namespace DlBot
{
    internal class Service
    {
        private IWebHost _webHost;
        private BotUser _botUser;

        public Service(BotUser botUser)
        {
            _botUser = botUser;
        }

        public bool Start(HostControl hostControl)
        {
            Serilog.Log.Information($"DlBot Service starting on {Environment.MachineName}");
            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://*:8463")
                .UseStartup<Startup>()
                .Build();

            _webHost.Start();

            Serilog.Log.Information($"Starting Bot User");
            _botUser.Init();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Serilog.Log.Information($"DlBot Service stopping on {Environment.MachineName}");
            _webHost?.Dispose();
            return true;
        }

    }
}
