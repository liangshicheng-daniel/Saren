using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcrSarenBot.Models
{
    public class Helper
    {
    }
    public abstract class BaseModel<T>
        where T : new()
    {
        public IQueryable<T> baseQuery { get; set; }
    }

    public class JqGridFilterModel
    {
        public string groupOper { get; set; }
        public List<Rules> rules { get; set; }
    }

    public class Rules
    {
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }
    }

    public class ColModel
    {
        public string name { get; set; }
        public bool hidden { get; set; }
        public string searchVal { get; set; }
    }
}
