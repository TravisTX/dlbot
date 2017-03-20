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

    }
}
