using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DlBot.Models
{
    public class SlackUserListModel
    {
        public List<SlackUserModel> members { get; set; }
    }

    public class SlackUserModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string real_name { get; set; }
        public string image_original { get; set; }
        public SlackUserProfile profile { get; set; }
    }

    public class SlackUserProfile
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string status_text { get; set; }
        public string status_emoji { get; set; }
    }

    public class SlackUserInfoModel
    {
        public bool ok { get; set; }
        public SlackUserModel user { get; set; }
    }
}
