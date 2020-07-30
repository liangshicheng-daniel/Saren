using System;
using System.Collections.Generic;

namespace PcrSarenBot.DatabaseModels
{
    public partial class Parameters
    {
        public Parameters()
        {
            Guides = new HashSet<Guides>();
        }

        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Status { get; set; }
        public int? CurrentBossId { get; set; }
        public int? CurrentCycle { get; set; }
        public int? CurrentBossHealth { get; set; }
        public int? MaxRunningMemberAllowed { get; set; }
        public int? FirstBossId { get; set; }
        public int? LastBossId { get; set; }
        public DateTime EventStartTime { get; set; }
        public DateTime EventEndTime { get; set; }

        public virtual Boss FirstBoss { get; set; }
        public virtual Boss LastBoss { get; set; }
        public virtual ICollection<Guides> Guides { get; set; }
    }
}
