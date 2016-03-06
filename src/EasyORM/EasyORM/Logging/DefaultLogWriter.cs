using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyORM.Logging
{
    /// <summary>
    /// 默认日志类
    /// </summary>
    internal class DefaultLogWriter : ILogWriter
    {
        static string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        static object _locker = new object();
        class LogInfo
        {
            public string Expression { get; set; }
            public string Sql { get; set; }
            public long TranslateTime { get; set; }
            public Dictionary<string, object> Parameters { get; set; }
            public long ExecuteTime { get; set; }
        }

        public void WriteLog(string expression, string sql, long tranlsateTime, Dictionary<string, object> parameters, long executeTime)
        {
            lock (_locker)
            {
                var fileName = GetLogFileName() + ".log";
                if (!Directory.Exists(baseFolder))
                {
                    Directory.CreateDirectory(baseFolder);
                }
                var filePath = Path.Combine(baseFolder, fileName);
                if (!File.Exists(filePath))
                {
                    File.CreateText(filePath).Dispose();
                }
                using (var sr = File.AppendText(filePath))
                {
                    sr.WriteLine(DateTime.Now.ToString());
                    sr.WriteLine("表达式：");
                    sr.WriteLine("\t" + expression);
                    sr.WriteLine("SQL：");
                    sr.WriteLine("\t" + sql);
                    sr.WriteLine("翻译用时：");
                    sr.WriteLine("\t" + tranlsateTime.ToString() + "毫秒");
                    if (parameters != null && parameters.Any())
                    {
                        sr.WriteLine("参数：");
                        foreach (var item in parameters.Keys)
                        {
                            sr.WriteLine("\t" + string.Format("{0}:{1}", item, Convert.ToString(parameters[item])));
                        }
                    }
                    sr.WriteLine("执行用时：");
                    sr.WriteLine("\t" + executeTime + "毫秒");
                    sr.WriteLine();
                    sr.Flush();
                }
            }
        }

        static string GetLogFileName()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH");
            var now = DateTime.Now;
            var minute = now.Minute % 10;
            if (minute >= 5 && minute <= 9)
            {
                now = new DateTime(now.Year, now.Month, now.Day, now.Hour, (now.Minute - minute) + 5, now.Second);
            }
            else if (minute < 5 && minute >= 0)
            {
                now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute - minute, now.Second);
            }
            return now.ToString("yyyy-MM-dd HHmm");
        }
    }
}
