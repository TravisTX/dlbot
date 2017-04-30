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
    [Route("api/status")]
    public class SlashStatusController : Controller
    {
        private readonly SlackService _slackService;

        public SlashStatusController(SlackService slackService)
        {
            _slackService = slackService;
        }

        [HttpPost("{status}")]
        public async Task<IActionResult> Post(string status)
        {
            status = status.ToLower();
            
            var inText = Request.Form["text"][0];
            var inUserId = Request.Form["user_id"][0];
            
            if (status == "back")
            {
                _slackService.SetUserStatus(inUserId, "", "");
                return Ok(":white_check_mark: You are now marked as *back*");
            }

            string emoji;
            switch (status)
            {
                case "afk":
                    emoji = ":afk:";
                    break;
                case "lunch":
                    emoji = ":fork_and_knife:";
                    break;
                case "meeting":
                    emoji = ":spiral_calendar_pad:";
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(string.IsNullOrWhiteSpace(inText))
            {
                var user = await _slackService.GetUserInfo(inUserId);
                if (user.user.profile.status_emoji == emoji)
                {
                    _slackService.SetUserStatus(inUserId, "", "");
                    return Ok(":white_check_mark: You are now marked as *back*");
                }
            }

            var statusMessage = string.IsNullOrWhiteSpace(inText) ? DateTime.Now.ToString("h:mm tt") : $"{inText} - {DateTime.Now:h:mm tt}";
            _slackService.SetUserStatus(inUserId, emoji, statusMessage);

            return Ok($":white_check_mark: You are now marked as *{emoji} {statusMessage}*");
        }
    }
}
