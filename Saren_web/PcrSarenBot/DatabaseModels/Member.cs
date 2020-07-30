using System;
using System.Collections.Generic;

namespace PcrSarenBot.DatabaseModels
{
    public partial class Member
    {
        public Member()
        {
            BattlesMember = new HashSet<Battles>();
            BattlesPlayer = new HashSet<Battles>();
            SlRecord = new HashSet<SlRecord>();
        }

        public long MemberId { get; set; }
        public string Nickname { get; set; }
        public string Role { get; set; }

        public virtual ICollection<Battles> BattlesMember { get; set; }
        public virtual ICollection<Battles> BattlesPlayer { get; set; }
        public virtual ICollection<SlRecord> SlRecord { get; set; }
    }
}
