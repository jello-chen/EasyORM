using EasyORM.Configuration;
using EasyORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyORM.Logging
{
    /// <summary>
    /// Log Factory
    /// </summary>
    public class LogWriterFactory
    {
        /// <summary>
        /// Create log instance by configuration
        /// </summary>
        /// <returns></returns>
        public static IList<ILogWriter> CreateLogWriter()
        {
            if (DataContext.GlobalConfig == null || DataContext.GlobalConfig.Loggers.Count <= 0)
            {
                return new List<ILogWriter>() { ObjectCache<DefaultLogWriter>.GetObject() };
            }

            var loggers = new List<ILogWriter>();
            foreach (Logger item in DataContext.GlobalConfig.Loggers)
            {
                loggers.Add(ObjectDictionaryCache.GetObject(item.Type, () =>
                {
                    if (string.IsNullOrWhiteSpace(item.Type))
                    {
                        throw new Exception("EasyORM log configurates failed");
                    }
                    var types = item.Type.Split(',');
                    Type type = null;
                    if (types.Length == 2)
                    {
                        type = Assembly.Load(types[1].Trim()).GetExportedTypes().FirstOrDefault(x => x.FullName == types[0]);
                    }
                    else
                    {
                        type = Type.GetType(types[0].Trim());
                    }
                    if (type == null)
                    {
                        throw new Exception("Not found the log class：" + item.Type);
                    }
                    return (ILogWriter)Activator.CreateInstance(type);
                }));
            }
            return loggers;
        }
    }
}
