using DlBot.Models;
using DlBot.Services;
using Microsoft.Extensions.Options;
using SlackAPI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace DlBot
{
    public class BotUser
    {
        private readonly SettingsModel _settings;
        private readonly SlackService _slackService;
        private readonly TfsService _tfsService;
        public BotUser(IOptions<SettingsModel> settings, SlackService slackService, TfsService tfsService)
        {
            _settings = settings.Value;
            _slackService = slackService;
            _tfsService = tfsService;
        }

        public void Init()
        {
            ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
            SlackSocketClient client = new SlackSocketClient(_settings.SlackBotUserToken);
            client.Connect((connected) =>
            {
                // This is called once the client has emitted the RTM start command
                clientReady.Set();
            }, () =>
            {
                // This is called once the RTM client has connected to the end point
                Serilog.Log.Information("Bot user connected");
            });

            client.OnMessageReceived += (message) =>
            {
                if (message.user == null || message.subtype == "bot_message")
                    return;
                HandleTfsWorkitem(message, client);
                HandleCheerUpTrigger(message, client);
            };
            clientReady.Wait();
        }

        private async void HandleTfsWorkitem(SlackAPI.WebSocketMessages.NewMessage message, SlackSocketClient client)
        {
            var containsTfsTrigger = Regex.IsMatch(message.text, @"(tfs|bug|pbi|task)", RegexOptions.IgnoreCase);
            if (!containsTfsTrigger)
                return;

            Serilog.Log.Information("TFS work item detected");
            var wiNumberMatches = Regex.Matches(message.text, @"(\d{4,5})");

            var workItems = new List<TfsWorkItemModel>();
            foreach (Match wiNumberMatch in wiNumberMatches)
            {
                var number = int.Parse(wiNumberMatch.Value);
                workItems.Add(await _tfsService.GetWorkItem(number));
            }

            var slackmessage = _tfsService.GetSlackMessage(workItems);

            client.PostMessage((mr) =>
            {
                Serilog.Log.Information("Message Posted");
            }, message.channel, slackmessage, "dlbot");
        }

        private void HandleCheerUpTrigger(SlackAPI.WebSocketMessages.NewMessage message, SlackSocketClient client)
        {
            var sadRegex = @"(:disappointed:|:feelsbadman:|:angry:|:anguished:|:unamused:|:worried:|:angry:|:rage:|:slightly_frowning_face:|:white_frowning_face:|:scream:|:fearful:|:frowning:|:cry:|:disappointed_relieved:|:sob:|:face_with_head_bandage:|:scream_cat:|:crying_cat_face:|:pouting_cat:|:middle_finger:)";
            var containsSadTrigger = Regex.IsMatch(message.text, sadRegex, RegexOptions.IgnoreCase);
            if (!containsSadTrigger)
                return;

            Serilog.Log.Information("Sadness detected");
            var ts = (message.ts.ToProperTimeStamp());

            client.AddReaction((rar) =>
            {
                if (rar.error == "ok")
                {
                    Serilog.Log.Information("Cheerup posted");
                }
                else
                {
                    Serilog.Log.Error($"Error posting Cheerup {rar.ok} {rar.error}");
                }
            }, "hugging_face", message.channel, ts.ToString());
        }
    }
}
