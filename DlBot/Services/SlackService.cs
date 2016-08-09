using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Events;

namespace DlBot.Services
{
    public class SlackService
    {
        public async Task PostToSlack(string url, string slackPostJson)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("payload", slackPostJson)
                    }
                );
                var response = client.PostAsync(url, content).Result;
                var responseString = await response.Content.ReadAsStringAsync();
                var logLevel = response.IsSuccessStatusCode ? LogEventLevel.Information : LogEventLevel.Warning;
                Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                Serilog.Log.Write(logLevel, responseString);
            }
        }
    }
}
