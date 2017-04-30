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

        public async Task<SlackUserModel> GetUser(string userid)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://slack.com/api/users.info?token={_settings.Value.SlackAccessToken}&user={userid}");
                var response = client.SendAsync(request).Result;
                var responseString = await response.Content.ReadAsStringAsync();
                var logLevel = response.IsSuccessStatusCode
                    ? LogEventLevel.Information
                    : LogEventLevel.Warning;
                Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                Serilog.Log.Write(logLevel, responseString);

                dynamic resultDynamic = JsonConvert.DeserializeObject(responseString);

                var result = new SlackUserModel
                {
                    id = resultDynamic.user.id,
                    name = resultDynamic.user.name,
                    real_name = resultDynamic.user.real_name,
                    image_original = resultDynamic.user.profile.image_original ?? resultDynamic.user.profile.image_512
                };
                return result;
            }
        }

        public async Task<List<SlackUserModel>> GetListOfUsers()
        {
            if (_slackUserList == null)
            {
                // todo: figure out how to lock with async code.
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://slack.com/api/users.list?token={_settings.Value.SlackAccessToken}");
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

        public async Task<SlackUserInfoModel> GetUserInfo(string userId)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://slack.com/api/users.info?token={_settings.Value.SlackAccessToken}&user={userId}");
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var logLevel = response.IsSuccessStatusCode
                    ? LogEventLevel.Information
                    : LogEventLevel.Warning;
                Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                Serilog.Log.Write(logLevel, responseString);

                var result = JsonConvert.DeserializeObject<SlackUserInfoModel>(responseString);
                return result;
            }
        }

        public async void SetUserStatus(string userId, string emoji, string status)
        {
            using (var client = new HttpClient())
            {
                var profileStatus = JsonConvert.SerializeObject(new {
                    status_text = status,
                    status_emoji = emoji
                });

                var request = new HttpRequestMessage(HttpMethod.Post, $"https://slack.com/api/users.profile.set?token={_settings.Value.SlackAccessToken}&user={userId}&profile={System.Net.WebUtility.UrlEncode(profileStatus)}");
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var logLevel = response.IsSuccessStatusCode
                    ? LogEventLevel.Information
                    : LogEventLevel.Warning;
                Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                Serilog.Log.Write(logLevel, responseString);
            }
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

        public async Task PostWebhookMessage(SlackWebhookMessageModel dto)
        {
            var payload = new SlackWebhookPayload
            {
                Channel = dto.Channel,
                IconUrl = dto.IconUrl,
                Username = dto.Username,
                Attachments = new List<SlackWebhookAttachment>
                {
                    new SlackWebhookAttachment
                    {
                        Color = dto.Color,
                        Text = dto.Text
                    }
                }
            };
            string payloadJson = JsonConvert.SerializeObject(payload);

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("payload", payloadJson)
                    }
                );
                var response = client.PostAsync(_settings.Value.SlackWebhookUrl, content).Result;
                var responseString = await response.Content.ReadAsStringAsync();
                var logLevel = response.IsSuccessStatusCode ? LogEventLevel.Information : LogEventLevel.Warning;
                Serilog.Log.Write(logLevel, $"Slack returned code: {(int)response.StatusCode} {response.StatusCode}");
                Serilog.Log.Write(logLevel, responseString);
            }
        }
    }

}
