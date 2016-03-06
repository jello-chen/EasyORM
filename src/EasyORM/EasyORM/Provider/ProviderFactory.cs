﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;

namespace EasyORM.Provider
{
    /// <summary>
    /// 数据库提供者工厂
    /// </summary>
    public class ProviderFactory
    {
        static Type _type = null;
        static Dictionary<string, Type> _providerTypes;

        /// <summary>
        /// 根据指定枚举创建提供者实例
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        public static ProviderBase CreateProvider(DataContext context)
        {
            if (_providerTypes == null)
            {
                _providerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.BaseType == typeof(ProviderBase)).ToDictionary(x => x.Name);
            }
            var typeName = string.Format("Provider.{0}.{0}Provider", context.DatabaseConfig.DatabaseType.ToString());
            var types = _providerTypes.Values.Where(x => x.FullName.EndsWith(typeName));
            if (types.Count() > 1)
            {
                throw new NotSupportedException("找到了多个包含" + typeName + "的提供者类");
            }
            _type = types.FirstOrDefault();
            if (_type == null)
            {
                throw new NotSupportedException("未找到提供者类：" + typeName);
            }
            return (ProviderBase)Activator.CreateInstance(_type, context);
        }

        public static DbProviderFactory GetFactory(string providerName)
        {
            return DbProviderFactories.GetFactory(providerName);
        }
    }
}