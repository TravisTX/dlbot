using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DlBot.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Events;

namespace DlBot.Services
{
    public class SlackService
    {
        private static SlackUserListModel _slackUserList;
        private readonly IOptions<SettingsModel> _settings;

        public SlackService(IOptions<SettingsModel> settings)
        {
            _settings = settings;
        }


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

        public async Task<List<SlackUserModel>> GetListOfUsers()
        {
            if (_slackUserList == null)
            {
                // todo: figure out how to lock with async code.
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://slack.com/api/users.list?token={_settings.Value.SlackApiKey}");
                    var response = client.SendAsync(request).Result;
                    var responseString = await response.Content.ReadAsStringAsync();
                    var logLevel = response.IsSuccessStatusCode
                        ? LogEventLevel.Information
                        : LogEventLevel.Warning;
                    Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                    Serilog.Log.Write(logLevel, responseString);

                    var result = JsonConvert.DeserializeObject<SlackUserListModel>(responseString);
                    _slackUserList = result;
                }
            }
            return _slackUserList.members;
        }

        public async Task<bool> IsValidUsername(string username)
        {
            username = username.Trim();

            if (username.Length == 0)
                return false;

            if (username[0] != '@')
                return false;

            username = username.Substring(1); // remove the leading @

            var users = await GetListOfUsers();
            return users.Any(x => string.Equals(x.name, username, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
