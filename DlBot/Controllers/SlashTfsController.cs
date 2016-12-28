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
            var inResponseUrl = Request.Form["response_url"][0];
            var inUserId = Request.Form["user_id"][0];
            var inUserName = Request.Form["user_name"][0];
            Task.Run(() => HandleWork(inCommand, inText, inUserName, inUserId, inResponseUrl)).Forget();
            return Ok();
        }

        private async Task HandleWork(string inCommand, string inText, string inUserName, string inUserId, string inResponseUrl)
        {
            Serilog.Log.Information($"{inCommand} {inText} requested by {inUserName}");


            var wiNumberMatches = Regex.Matches(inText, @"(\d{4,5})");
            var workItems = new List<TfsWorkItemModel>();
            foreach (Match wiNumberMatch in wiNumberMatches)
            {
                var number = int.Parse(wiNumberMatch.Value);
                workItems.Add(await _tfsService.GetWorkItem(number));
            }

            string slackPostJson = GetSlackPost(workItems, inText, inUserId);
            await _slackService.PostToSlack(inResponseUrl, slackPostJson);
        }

        private string GetSlackPost(List<TfsWorkItemModel> workItems, string inText, string inUserId)
        {
            var message = "";
            foreach (var workItem in workItems)
            {
                var workItemType = workItem.Fields.WorkItemType;
                if (workItemType == "Product Backlog Item")
                {
                    workItemType = "PBI";
                }
                message += $"<{workItem.Links.Html.Href}|{workItemType} {workItem.Id}: {workItem.Fields.Title.Replace("<", "&lt;").Replace(">", "&gt;")}>\n";
            }
            string note = Regex.Replace(inText, @"^([0-9 ,])*", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!String.IsNullOrWhiteSpace(note))
            {
                message += $"{note}\n";
            }

            var footer = $"/tfs triggered by <@{inUserId}>";
            // /tfs 27654 30535
            var slackPost = new
            {
                response_type = "in_channel",
                attachments = new[]
                {
                    new
                    {
                        fallback = message,
                        //color = "#0AA6C4",
                        text = message,
                        footer = footer,
                        mrkdwn_in = new[] {"pretext", "text"}
                    }
                }
            };

            var slackPostJson = JsonConvert.SerializeObject(slackPost);
            return slackPostJson;
        }

    }
}
