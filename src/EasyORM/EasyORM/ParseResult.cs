using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM
{
    public class ParseResult
    {
        public string CommandText { get;internal set; }
        public ParseResult()
        {
            Parameters = new Dictionary<string, object>();
        }
        public string ParameterizedCommandText { get; internal set; }
        public Dictionary<string, object> Parameters { get; internal set; }
    }
}
