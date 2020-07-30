using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcrSarenBot.Models
{
    public class BossTabModel
    {
        public int bossId { get; set; }
        public string bossName { get; set; }
    }

    public class GuideModel
    {
        public int guideId { get; set; }
        public string imageUrl { get; set; }
        public string title { get; set; }
        public string comment { get; set; }
        public DateTime? createdAt { get; set; }
    }
}
