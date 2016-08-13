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
    [Route("api/slap")]
    public class SlashSlapController : Controller
    {
        private readonly SlackService _slackService;

        public SlashSlapController(SlackService slackService)
        {
            _slackService = slackService;
        }

        public async Task<IActionResult> Post()
        {
            var inCommand = Request.Form["command"][0];
            var inText = Request.Form["text"][0];
            var inResponseUrl = Request.Form["response_url"][0];
            var inUserId = Request.Form["user_id"][0];
            var inUserName = Request.Form["user_name"][0];
            Serilog.Log.Information($"{inCommand} {inText} requested by {inUserName}");
            var validUser = await _slackService.IsValidUsername(inText);

            if (!validUser)
            {
                string msg = "Acceptable parameter is a username";
                msg += "\n example:";
                msg += "\n /slap @travis.collins";
                return Ok(msg);
            }

            string slackPostJson = GetSlackPost(inText, inUserId);
            await _slackService.PostToSlack(inResponseUrl, slackPostJson);
            return Ok();
        }

        private string GetSlackPost(string userToSlap, string inUserId)
        {
            var message = $"<@{inUserId}> slaps <{userToSlap}> around a bit with a large trout";

            var slackPost = new
            {
                response_type = "in_channel",
                text = message
            };

            var slackPostJson = JsonConvert.SerializeObject(slackPost);
            return slackPostJson;
        }

    }
}
