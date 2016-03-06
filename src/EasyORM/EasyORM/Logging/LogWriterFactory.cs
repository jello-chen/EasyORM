using EasyORM.Configuration;
using EasyORM.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Logging
{
    /// <summary>
    /// Log Factory
    /// </summary>
    public class LogWriterFactory
    {
        /// <summary>
        /// 根据配置文件创建日志类
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
                        throw new Exception("EasyORM配置文件错误，日志类类型名不能为空");
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
                        throw new Exception("未找到日志类：" + item.Type);
                    }
                    return (ILogWriter)Activator.CreateInstance(type);
                }));
            }
            return loggers;
        }
    }
}
