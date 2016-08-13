using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DlBot.Models
{
    public class SprintScheuleModel
    {
        public int sprintNumber { get; set; }
        public DateTime sprintStartDate { get; set; }
        public DateTime sprintUatDate { get; set; }
        public DateTime sprintProdDate { get; set; }
    }

    public class SprintDateScheuleModel
    {
        public DateTime targetDate { get; set; }
        public int qaSprintNum { get; set; }
        public int uatSprintNum { get; set; }
        public int prodSprintNum { get; set; }
        public DateTime qaSprintSince { get; set; }
        public DateTime uatSprintSince { get; set; }
        public DateTime prodSprintSince { get; set; }
    }
}
