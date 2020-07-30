using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace PcrSarenBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        //出刀记录
        public IActionResult ReportPage()
        {
            var context = new DatabaseModels.Saren_BotContext();
            var viewModel = context.Parameters.Select(p => new Models.EventModel { eventId = p.EventId, eventName = p.EventName, status = p.Status }).ToList();
            return View(viewModel);
        }
        //每日出刀汇总
        public IActionResult ArrangementPage(string date)
        {
            var viewModel = new Models.DailyRecord(date);
            return View(viewModel);
        }
        //成员伤害汇总
        public IActionResult SummaryPage()
        {
            var context = new DatabaseModels.Saren_BotContext();
            var viewModel = context.Parameters.Select(p => new Models.EventModel { eventId = p.EventId, eventName = p.EventName, status = p.Status }).ToList();
            return View(viewModel);
        }
        //在线轴共享
        public IActionResult GuidePage()
        {
            var context = new DatabaseModels.Saren_BotContext();
            var viewModel = context.Parameters.Select(p => new Models.EventModel { eventId = p.EventId, eventName = p.EventName, status = p.Status }).ToList();
            return View(viewModel);
        }

        //编辑出刀记录
        [HttpGet]
        public JsonResult EditRecord(int battleId, string nickname, string bossName, int? damage)
        {
            var context = new DatabaseModels.Saren_BotContext();
            try
            {
                var battle = context.Battles.Where(b => b.BattleId == battleId).FirstOrDefault();
                battle.MemberId = context.Member.Where(m => m.Nickname == nickname).FirstOrDefault().MemberId;
                battle.BossId = context.Boss.Where(b => b.BossName == bossName).FirstOrDefault().BossId;
                battle.Damage = damage;
                context.SaveChanges();

            }
            catch (Exception e)
            {
                return Json(new
                {
                    message = "Error: " + e.Message
                });
            }
            return Json(new
            {
                message = "Success"
            });
        }

        //获取基本信息
        [HttpGet]
        public JsonResult GetModel(int eventId)
        {
            var model = new Models.InfoModel();
            model.Init(eventId);
            return Json(new
            {
                message = "Success",
                model = JsonSerializer.Serialize(model)
            });
        }

        //获取指定会战的团员伤害资料
        [HttpGet]
        public JsonResult GetSummary(int eventId)
        {
            var context = new DatabaseModels.Saren_BotContext();
            var eve = context.Parameters.Where(p => p.EventId == eventId).FirstOrDefault();
            var startDate = eve.EventStartTime;
            var endDate = eve.EventEndTime;
            var nextDay = startDate.AddDays(1);
            var summaryByMember = new Dictionary<string, List<Models.SummaryModelByDay>>();
            var nicknameList = (from b in context.Battles
                                join p in context.Parameters on
                                b.EventId equals p.EventId
                                join m in context.Member on
                                b.MemberId equals m.MemberId
                                where b.EventId == eventId && b.Status != "Abandoned"
                                select m.Nickname).Distinct().ToList();
            foreach (var nickname in nicknameList)
            {
                summaryByMember.Add(nickname, new List<Models.SummaryModelByDay>());
            }
            var dateList = new List<DateTime>();
            while (startDate < endDate)
            {
                var damageList = (from b in context.Battles
                                  join m in context.Member on
                                  b.MemberId equals m.MemberId
                                  where b.RecordTime > startDate &&
                                  b.RecordTime < nextDay &&
                                  b.Status != "Abandoned"
                                  group b by m.Nickname into g
                                  select new Models.SummaryModel
                                  {
                                      nickname = g.Key,
                                      damageSum = g.Sum(b => b.Damage)
                                  }).ToList();

                foreach (var day in damageList)
                {
                    var newModel = new Models.SummaryModelByDay();
                    newModel.date = startDate;
                    newModel.damageSum = day.damageSum;
                    summaryByMember[day.nickname].Add(newModel);
                }
                dateList.Add(startDate);
                startDate = nextDay;
                nextDay = nextDay.AddDays(1);
            }

            return Json(new
            {
                message = "Success",
                damageList = JsonSerializer.Serialize(summaryByMember),
                nicknameList = JsonSerializer.Serialize(nicknameList),
                dateList = JsonSerializer.Serialize(dateList)
            });         
        }

        //获取指定会战的boss信息
        [HttpGet]
        public JsonResult SetupTabs(int eventId)
        {
            var context = new DatabaseModels.Saren_BotContext();
            var list = context.Boss.Where(b => b.EventId == eventId).Select(b => new Models.BossTabModel { bossId = b.BossId, bossName = b.BossName }).OrderBy(o => o.bossId).ToList();
            return Json(new
            {
                message = "Success",
                list = JsonSerializer.Serialize(list)
            });
        }


        //核对管理员密码，此功能将在近期被登录验证取代
        [HttpGet]
        public JsonResult VerifyPassword(string password)
        {
            if (password == "DMCCW6EX72")
            {
                return Json(new
                {
                    message = "Success"
                });
            }
            return Json(new
            {
                message = "Fail"
            });

        }

        //录入新轴
        [HttpPost]
        public JsonResult CreateNewGuide(string title, string comment, int eventId, int bossId)
        {
            try
            {
                var file = Request.Form.Files["image"];
                var folderPath = Path.Combine("uploads", "img");
                var pathToSave = Path.Combine(_env.WebRootPath, folderPath);
                string dbPath = null;
                if (file != null && file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName);
                    if (ext != ".jpg" && ext != ".png" && ext != ".gif" && ext != ".jpeg")
                    {
                        return Json(new
                        {
                            message = "Error: 图像格式无法识别"
                        });
                    }
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    //var fullPath = Path.Combine(pathToSave, fileName);
                    string renameFile = Convert.ToString(Guid.NewGuid()) + "." + fileName.Split('.').Last();
                    var fullPath = Path.Combine(pathToSave, renameFile);
                    dbPath = Path.Combine(folderPath, renameFile);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
                var context = new DatabaseModels.Saren_BotContext();
                var newGuide = new DatabaseModels.Guides();
                if(bossId == 0)
                {
                    newGuide.EventId = null;
                    newGuide.BossId = null;
                }
                else
                {
                    newGuide.EventId = eventId;
                    newGuide.BossId = bossId;
                }                
                newGuide.Title = title;
                newGuide.Comment = comment;
                newGuide.ImageUrl = dbPath;
                context.Guides.Add(newGuide);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = "Error: " + ex.Message
                });
            }
            return Json(new
            {
                message = "Success"
            });
        }

        //编辑轴
        [HttpPost]
        public JsonResult EditGuide(string title, string comment, int guideId)
        {
            try
            {
                var context = new DatabaseModels.Saren_BotContext();
                var guide = context.Guides.Where(g => g.GuideId == guideId).FirstOrDefault();
                guide.Title = title;
                guide.Comment = comment;
                var file = Request.Form.Files["image"];
                var folderPath = Path.Combine("uploads", "img");
                var pathToSave = Path.Combine(_env.WebRootPath, folderPath);
                string dbPath = null;
                if (file != null && file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName);
                    if (ext != ".jpg" && ext != ".png" && ext != ".gif" && ext != ".jpeg")
                    {
                        return Json(new
                        {
                            message = "Error: 图像格式无法识别"
                        });
                    }
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    //var fullPath = Path.Combine(pathToSave, fileName);
                    string renameFile = Convert.ToString(Guid.NewGuid()) + "." + fileName.Split('.').Last();
                    var fullPath = Path.Combine(pathToSave, renameFile);
                    dbPath = Path.Combine(folderPath, renameFile);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    guide.ImageUrl = dbPath;
                }                               
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = "Error: " + ex.Message
                });
            }
            return Json(new
            {
                message = "Success"
            });
        }

        //获取指定boss对应轴
        [HttpPost]
        public JsonResult GetGuides(int? bossId)
        {
            var context = new DatabaseModels.Saren_BotContext();
            if (bossId == 0)
                bossId = null;
            var list = (from g in context.Guides
                        where g.Status == "Active" &&
                        g.BossId == bossId
                        orderby g.CreatedAt descending
                        select new Models.GuideModel
                        {
                            guideId = g.GuideId,
                            title = g.Title,
                            comment = g.Comment,
                            imageUrl = g.ImageUrl,
                            createdAt = g.CreatedAt
                        }).ToList();
            return Json(new
            {
                message = "Success",
                list = JsonSerializer.Serialize(list)
            });
        }

        //删除指定轴
        [HttpPost]
        public JsonResult RemoveGuides(int guideId)
        {
            var context = new DatabaseModels.Saren_BotContext();
            try
            {
                var guide = context.Guides.Where(g => g.GuideId == guideId).FirstOrDefault().Status = "Inactive";
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = "Error: " + ex.Message
                });
            }
            return Json(new
            {
                message = "Success"
            });
        }
    }

    public class RecordController : GridHelperController<Models.RecordModel, Models.RecordModel.Model> { }
}
