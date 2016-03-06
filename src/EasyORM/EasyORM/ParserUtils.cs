using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EasyORM.Utils;

namespace EasyORM
{
    public class ParserUtils
    {
        static int _tableNum = 0;
        static object _tableLocker = new object();
        static Type _compilerGeneratedAttribute = typeof(CompilerGeneratedAttribute);
        /// <summary>
        /// 根据名称生成随机别名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GenerateAlias(string name)
        {
            lock (_tableLocker)
            {
                _tableNum++;
                return name + _tableNum;
            }
        }

        public static bool IsAnonymousType(Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(_compilerGeneratedAttribute, false).Count() > 0;
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        /// <summary>
        /// 检查给定的字符串是否符合c#变量名标准
        /// </summary>
        /// <returns></returns>
        public static bool IsGoodIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException("identifier");
            }
            return Regex.Match(identifier, @"^[_a-zA-Z][_0-9a-zA-Z]*$").Success;
        }

        /// <summary>
        /// 获取规范化后的标识符
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string GetGoodIdentifier(string identifier)
        {
            if (IsGoodIdentifier(identifier))
            {
                return identifier;
            }
            if (identifier.Contains(" "))
            {
                identifier = identifier.Replace(" ", "");
            }
            if (char.IsNumber(identifier[0]))
            {
                identifier = "_" + identifier;
            }
            return identifier;
        }

        /// <summary>
        /// 获取规范化后的表名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetGoodTableName(string tableName)
        {
            tableName = GetGoodIdentifier(tableName);
            if (tableName.StartsWith("_") && tableName.Length >= 2 && !char.IsNumber(tableName[1]))
            {
                tableName = tableName.Substring(1);
            }
            if (tableName.Contains("_"))
            {
                tableName = tableName.Replace("_", "");
            }
            //tableName = StringHelper.ToSingular(tableName);
            return StringHelper.ToPascal(tableName);
        }

        /// <summary>
        /// 获取规范化后的列名
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetGoodColumnName(string columnName)
        {
            columnName = GetGoodIdentifier(columnName);
            columnName = columnName.Replace("_", "");
            return StringHelper.ToPascal(columnName);
        }
    }
}
