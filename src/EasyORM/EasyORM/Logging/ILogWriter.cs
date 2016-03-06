using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Logging
{
    public interface ILogWriter
    {
        void WriteLog(string expression, string sql, long tranlsateTime, Dictionary<string, object> parameters, long executeTime);
    }
}
