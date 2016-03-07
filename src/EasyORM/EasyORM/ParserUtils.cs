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
        /// Create allias by name provided
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
        /// Check whether the identifier is conforming to the C# specification
        /// </summary>
        /// <returns></returns>
        public static bool IsStandardIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException("identifier");
            }
            return Regex.Match(identifier, @"^[_a-zA-Z][_0-9a-zA-Z]*$").Success;
        }

        /// <summary>
        /// Get the standard identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string GetStandardIdentifier(string identifier)
        {
            if (IsStandardIdentifier(identifier))
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
        /// Get a standard table name
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetStandardTableName(string tableName)
        {
            tableName = GetStandardIdentifier(tableName);
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
        /// Get a standard column name
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetGoodColumnName(string columnName)
        {
            columnName = GetStandardIdentifier(columnName);
            columnName = columnName.Replace("_", "");
            return StringHelper.ToPascal(columnName);
        }
    }
}
