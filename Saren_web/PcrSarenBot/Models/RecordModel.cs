using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcrSarenBot.Models
{
    public class RecordModel : BaseModel<RecordModel.Model>
    {
        public RecordModel() { }

        public RecordModel(IQueryCollection query)
        {
            var context = new DatabaseModels.Saren_BotContext();
            var eventId = Convert.ToInt32(query["eventId"].ToString());
            baseQuery = (from r in context.Battles
                         join m in context.Member on
                         r.MemberId equals m.MemberId
                         join b in context.Boss on
                         r.BossId equals b.BossId
                         where r.EventId == eventId && (r.Status == "Finished" ||
                         r.Status == "Last_Hit" ||
                         r.Status == "OffTree")
                         select new Model
                         {
                             battleId = r.BattleId,
                             nickname = m.Nickname,
                             bossName = b.BossName,
                             cycleNumber = r.CycleNumber,
                             damage = r.Damage,
                             status = r.Status,
                             recordTime = r.RecordTime
                         });

        }

        public class Model
        {
            public int battleId { get; set; }
            public string nickname { get; set; }
            public string bossName { get; set; }
            public int? cycleNumber { get; set; }
            public int? damage { get; set; }
            public string status { get; set; }
            public DateTime? recordTime { get; set; }
        }
    }

    public class InfoModel
    {
        public InfoModel() { }
        public void Init(int eventId)
        {
            var context = new DatabaseModels.Saren_BotContext();
            var activeEventId = context.Parameters.Where(p => p.Status == "Active").FirstOrDefault().EventId;
            var bossList = (from b in context.Boss
                            where b.EventId == eventId
                            select b.BossName).ToList();
            bossNames = GenerateSearchOptionValue(bossList);
            var nicknameList = (from b in context.Battles
                                join p in context.Parameters on
                                b.EventId equals p.EventId
                                join m in context.Member on
                                b.MemberId equals m.MemberId
                                where b.EventId == eventId
                                select m.Nickname).Distinct().ToList();
            nicknames = GenerateSearchOptionValue(nicknameList);

        }
        private string GenerateSearchOptionValue(List<string> list)
        {
            var value = ":All;";
            foreach (var item in list)
            {
                value += item + ":" + item + ";";
            }

            return value.TrimEnd(';');
        }
        public string nicknames { get; set; }
        public string bossNames { get; set; }
    }

    public class EventModel
    {
        public int eventId { get; set; }
        public string eventName { get; set; }
        public string status { get; set; }
    }

    public class SummaryModel
    {
        public string nickname { get; set; }
        public int? damageSum { get; set; }
    }

    public class SummaryModelByDay
    {
        public DateTime date { get; set; }
        public int? damageSum { get; set; }
    }
}
