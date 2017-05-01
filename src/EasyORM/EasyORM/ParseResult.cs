using System.Collections.Generic;

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
