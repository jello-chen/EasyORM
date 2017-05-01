using System.Collections.Generic;

namespace EasyORM.Logging
{
    public interface ILogWriter
    {
        void WriteLog(string expression, string sql, long tranlsateTime, Dictionary<string, object> parameters, long executeTime);
    }
}
