using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DlBot.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog.Events;

namespace DlBot.Controllers
{
    [Route("api/sprint")]
    public class SlashSprintController : Controller
    {
        private readonly SlackService _slackService;
        private readonly SprintCalculatingService _sprintCalculatingService;

        public SlashSprintController(SlackService slackService, SprintCalculatingService sprintCalculatingService)
        {
            _slackService = slackService;
            _sprintCalculatingService = sprintCalculatingService;
        }

        public async Task<IActionResult> Post()
        {
            var inCommand = Request.Form["command"][0];
            var inText = Request.Form["text"][0];
            var inResponseUrl = Request.Form["response_url"][0];
            var inUserId = Request.Form["user_id"][0];
            var inUserName = Request.Form["user_name"][0];
            Serilog.Log.Information($"{inCommand} {inText} requested by {inUserName}");
            int sprintNum;
            DateTime targetDate;
            string slackPostJson;
            bool isSprintNum = int.TryParse(inText, out sprintNum);
            bool isSprintDate = DateTime.TryParse(inText, out targetDate);
            if (inText.Trim().Length == 0)
            {
                targetDate = DateTime.Today;
                isSprintDate = true;
            }

            if (isSprintNum)
            {
                // sprint number
                slackPostJson = GetSlackPostFromSprintNum(sprintNum, $"{inCommand} {inText}", inUserId);
            }
            else if (isSprintDate)
            {
                // date
                slackPostJson = GetSlackPostFromDate(targetDate, $"{inCommand} {inText}", inUserId);
            }
            else
            {
                // error
                string msg = "Acceptable parameters are a sprint number, or a date";
                msg += "\nexamples:";
                msg += "\n /sprint 3";
                msg += "\n /sprint 6/1/2016";
                return Ok(msg);
            }

            await _slackService.PostToSlack(inResponseUrl, slackPostJson);
            return Ok();
        }

        private string GetSlackPostFromDate(DateTime targetDate, string inCommand, string inUserId)
        {
            var dto = _sprintCalculatingService.CalculateSprintDateSchedule(targetDate);

            var message = $"*On {dto.targetDate:MM/dd/yyyy}*\n" +
                          $"*QA*       Sprint {dto.qaSprintNum} (since {dto.qaSprintSince:MM/dd})\n" +
                          $"*UAT*     Sprint {dto.uatSprintNum} (since {dto.uatSprintSince:MM/dd})\n" +
                          $"*Prod*     Sprint {dto.prodSprintNum} (since {dto.prodSprintSince:MM/dd})";

            var footer = $"{inCommand} triggered by <@{inUserId}>";

            var slackPost = new
            {
                response_type = "in_channel",
                attachments = new[]
                {
                    new
                    {
                        fallback = message.Replace("*", ""),
                        color = "#0AA6C4",
                        text = message,
                        footer = footer,
                        mrkdwn_in = new[] {"pretext", "text"}
                    }
                }
            };

            var slackPostJson = JsonConvert.SerializeObject(slackPost);
            return slackPostJson;
        }

        private string GetSlackPostFromSprintNum(int sprintNum, string inCommand, string inUserId)
        {
            var dto = _sprintCalculatingService.CalculateSprintSchedule(sprintNum);

            var message = $"*Sprint {dto.sprintNumber}*\n" +
                          $"*Start*    {dto.sprintStartDate:MM/dd/yyyy}\n" +
                          $"*UAT*      {dto.sprintUatDate:MM/dd/yyyy}\n" +
                          $"*Prod*     {dto.sprintProdDate:MM/dd/yyyy}\n";

            var footer = $"{inCommand} triggered by <@{inUserId}>";

            var slackPost = new
            {
                response_type = "in_channel",
                attachments = new[]
                {
                    new
                    {
                        fallback = message.Replace("*", ""),
                        color = "#0AA6C4",
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
