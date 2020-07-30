using System;
using System.Collections.Generic;

namespace PcrSarenBot.DatabaseModels
{
    public partial class Boss
    {
        public Boss()
        {
            Battles = new HashSet<Battles>();
            Guides = new HashSet<Guides>();
            ParametersFirstBoss = new HashSet<Parameters>();
            ParametersLastBoss = new HashSet<Parameters>();
        }

        public int BossId { get; set; }
        public int? EventId { get; set; }
        public string BossCode { get; set; }
        public string BossName { get; set; }
        public int? HealthPool { get; set; }

        public virtual ICollection<Battles> Battles { get; set; }
        public virtual ICollection<Guides> Guides { get; set; }
        public virtual ICollection<Parameters> ParametersFirstBoss { get; set; }
        public virtual ICollection<Parameters> ParametersLastBoss { get; set; }
    }
}
