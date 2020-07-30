using System;
using System.Collections.Generic;

namespace PcrSarenBot.DatabaseModels
{
    public partial class Guides
    {
        public int GuideId { get; set; }
        public int? EventId { get; set; }
        public int? BossId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Boss Boss { get; set; }
        public virtual Parameters Event { get; set; }
    }
}
