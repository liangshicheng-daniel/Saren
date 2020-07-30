using System;
using System.Collections.Generic;

namespace PcrSarenBot.DatabaseModels
{
    public partial class SlRecord
    {
        public int RecordId { get; set; }
        public long? MemberId { get; set; }
        public DateTime? RecordTime { get; set; }

        public virtual Member Member { get; set; }
    }
}
