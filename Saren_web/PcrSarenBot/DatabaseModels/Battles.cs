using System;
using System.Collections.Generic;

namespace PcrSarenBot.DatabaseModels
{
    public partial class Battles
    {
        public int BattleId { get; set; }
        public long? MemberId { get; set; }
        public int? EventId { get; set; }
        public int? BossId { get; set; }
        public int? CycleNumber { get; set; }
        public int? Damage { get; set; }
        public string Status { get; set; }
        public DateTime? RecordTime { get; set; }
        public long? PlayerId { get; set; }

        public virtual Boss Boss { get; set; }
        public virtual Member Member { get; set; }
        public virtual Member Player { get; set; }
    }
}
