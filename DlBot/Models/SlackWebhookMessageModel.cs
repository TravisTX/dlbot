using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DlBot.Models
{
    public class SlackWebhookMessageModel
    {
        public string Channel { get; set; }
        public string Username { get; set; }
        public string IconUrl { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
    }


    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class SlackWebhookPayload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("attachments")]
        public List<SlackWebhookAttachment> Attachments { get; set; }
    }

    public class SlackWebhookAttachment
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}
