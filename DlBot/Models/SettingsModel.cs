using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace DlBot.Models
{
    public class SettingsModel
    {
        public string SlackApiKey { get; set; }
        public string TfsUrl { get; set; }
        public string TfsUsername { get; set; }
        public string TfsPassword { get; set; }
    }
}
