using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcrSarenBot.Models
{
    public class DailyRecord
    {
        public DailyRecord() { }
        public DailyRecord(String date)
        {
            var todayChina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
            if (todayChina < DateTime.ParseExact(todayChina.ToString("MM/dd/yyyy") + " 05:00", "MM/dd/yyyy HH:mm", null))
                todayChina = todayChina.AddDays(-1);
            
            var context = new DatabaseModels.Saren_BotContext();
            var activeEvent = context.Parameters.Where(p => p.Status == "active").FirstOrDefault();
            if (activeEvent == null)
                return;
            var startDate = activeEvent.EventStartTime;
            //var dayEnd = startDate.AddDays(1);
            dateRange = new List<string>();
            while (startDate < todayChina && startDate < activeEvent.EventEndTime)
            {
                dateRange.Add(startDate.ToString("MM/dd/yyyy"));
                startDate = startDate.AddDays(1);
            }
            if (date == null || date == "")
                selectedDate = dateRange[dateRange.Count - 1];
            else
                selectedDate = date;
            var startTime = DateTime.ParseExact(selectedDate + " 05:00", "MM/dd/yyyy HH:mm", null);
            var endTime = startTime.AddDays(1);

            var parameter = context.Parameters.Where(p => p.EventStartTime <= startTime && p.EventEndTime > startTime).FirstOrDefault();
            //if (parameter == null)
            //{
            //    currentBoss = null;
            //    return;
            //}
            //currentBoss = context.Boss.Where(b => b.BossId == parameter.CurrentBossId).FirstOrDefault().BossName;
            //currentHealth = parameter.CurrentBossHealth;
            //currentRound = parameter.CurrentCycle;

            var recordList = (from b in context.Battles
                        join o in context.Boss on
                        b.BossId equals o.BossId
                        join m in context.Member on
                        b.MemberId equals m.MemberId
                        join pl in context.Member on
                        b.PlayerId equals pl.MemberId into plView
                        from p in plView.DefaultIfEmpty()
                        where b.RecordTime >= startTime &&
                        b.RecordTime < endTime &&
                        b.Status != "Abandoned"
                        select new IndividualRecord.Battle
                        {
                            battleId = b.BattleId,
                            bossId = (o.BossId%5==0)?5: o.BossId % 5,
                            bossName = o.BossName,
                            roundNumber = b.CycleNumber,
                            memberId = m.MemberId,
                            memberName = m.Nickname,
                            playerName = p.Nickname,
                            damage = b.Damage,
                            type = b.Status,
                            recordTime = b.RecordTime
                        }).ToList();
            var memberList = context.Member.Where(m => m.Role != "inactive").Select(m => m).ToList();
            var slRecords = context.SlRecord.Where(s => s.RecordTime >= startTime && s.RecordTime < endTime).Select(m => m.MemberId).Distinct().ToHashSet();
            recordSet = new Dictionary<long, IndividualRecord>();
            foreach (var member in memberList)
            {
                IndividualRecord record = new IndividualRecord();
                record.name = member.Nickname;
                record.memberId = member.MemberId;
                record.battles = new IndividualRecord.Battle[6];
                record.pointer = 0;
                //True for used sl
                if (slRecords.Contains(member.MemberId))
                    record.slUsed = true;
                else
                    record.slUsed = false;
                recordSet.Add(member.MemberId, record);
            }
            foreach (var record in recordList)
            {
                var list = recordSet[record.memberId];
                
                if (record.type == "Finished")
                {
                    list.battles[list.pointer] = record;
                    list.pointer += 1;
                    if (list.pointer % 2 == 1)
                        list.pointer += 1;
                }
                else if (record.type == "Last_Hit")
                {
                    list.battles[list.pointer] = record;
                    list.pointer += 1;
                }
                
            }

            var groupSets = recordList.GroupBy(g => (g.bossId, g.roundNumber)).Select(s => s.OrderBy(o => (o.roundNumber, o.bossId)).ToList()).ToList();
            var roundGroup = new Dictionary<int?, RoundRecord>();

            foreach (var boss in groupSets)
            {
                if (boss.Count <= 0)
                    continue;
                var bossRec = new RoundRecord.BossRecord();
                bossRec.bossId = boss[0].bossId;
                bossRec.bossName = boss[0].bossName;
                bossRec.trySpend = boss.Where(b => b.type == "Finished").Count();
                bossRec.totalDamage = boss.Sum(b => b.damage);
                RoundRecord rec;
                if (roundGroup.ContainsKey(boss[0].roundNumber))
                {
                    rec = roundGroup[boss[0].roundNumber];
                }
                else
                {
                    rec = new RoundRecord();
                    rec.boss = new RoundRecord.BossRecord[5];
                    rec.roundNumber = boss[0].roundNumber;
                    roundGroup.Add(rec.roundNumber, rec);
                }
              
                rec.boss[bossRec.bossId-1] = bossRec;
            }
            roundList = roundGroup.OrderBy(r => r.Key).Select(s => s.Value).ToList();
            foreach (var r in roundList)
            {
                r.totalTrySpend = 0;
                foreach (var b in r.boss)
                {
                    if (b != null)
                        r.totalTrySpend += b.trySpend;
                }
            }

        }
        public List<string> dateRange { get; set; }
        public string selectedDate { get; set; }
        //public string currentBoss { get; set; }
        //public int? currentRound { get; set; }
        //public int? currentHealth { get; set; }
        //public string startBoss { get; set; }
        //public int? startRound { get; set; }
        //public int? startHealth { get; set; }
        //public int startRound { get; set; }
        public Dictionary<long, IndividualRecord> recordSet { get; set; }
        public List<RoundRecord> roundList { get; set; }
        public class IndividualRecord
        {
            public string name { get; set; }
            public long memberId { get; set; }
            public bool slUsed { get; set; }
            public int pointer { get; set; }
            public Battle[] battles { get; set; }
            public class Battle
            {
                public int battleId { get; set; }
                public int bossId { get; set; }
                public string bossName { get; set; }
                public int? roundNumber { get; set; }
                public long memberId { get; set; }
                public string memberName { get; set; }
                public string playerName { get; set; }
                public int? damage { get; set; }
                public string type { get; set; }
                public DateTime? recordTime { get; set; }
            }
        }
        public class RoundRecord
        {
            public int? roundNumber { get; set; }
            public BossRecord[] boss { get; set; }
            public int totalTrySpend { get; set; }
            public class BossRecord
            {
                public int bossId { get; set; }
                public string bossName { get; set; }
                public int trySpend { get; set; }
                public int? totalDamage { get; set; }
            }
        }
        
    }
}
