using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DlBot.Extensions;
using DlBot.Models;
using DlBot.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog.Events;

namespace DlBot.Controllers
{
    [Route("api/tfs")]
    public class SlashTfsController : Controller
    {
        private readonly SlackService _slackService;
        private readonly TfsService _tfsService;

        public SlashTfsController(SlackService slackService, TfsService tfsService)
        {
            _slackService = slackService;
            _tfsService = tfsService;
        }

        public IActionResult Post()
        {
            var inCommand = Request.Form["command"][0];
            var inText = Request.Form["text"][0];
            var inUserId = Request.Form["user_id"][0];
            var inUserName = Request.Form["user_name"][0];
            var inChannelId = Request.Form["channel_id"][0];
            Task.Run(() => HandleWork(inCommand, inText, inChannelId, inUserName, inUserId)).Forget();
            return Ok("working on it...");
        }

        private async Task HandleWork(string inCommand, string inText, string inChannelId, string inUserName, string inUserId)
        {
            Serilog.Log.Information($"{inCommand} {inText} requested by {inUserName}");
            var slackUser = await _slackService.GetUser(inUserId);


            var wiNumberMatches = Regex.Matches(inText, @"(\d{4,5})");
            var workItems = new List<TfsWorkItemModel>();
            foreach (Match wiNumberMatch in wiNumberMatches)
            {
                var number = int.Parse(wiNumberMatch.Value);
                workItems.Add(await _tfsService.GetWorkItem(number));
            }

            var slackmessage = _tfsService.GetSlackMessage(workItems, inText);
            var slackPost = new SlackWebhookMessageModel
            {
                Channel = inChannelId,
                IconUrl = slackUser.image_original,
                Text = slackmessage,
                Username = slackUser.real_name
            };
            await _slackService.PostWebhookMessage(slackPost);
        }
    }
}
