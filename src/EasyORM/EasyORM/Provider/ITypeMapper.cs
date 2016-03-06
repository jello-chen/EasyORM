using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Provider
{
    public interface ITypeMapper
    {
        /// <summary>
        /// 存储从<seealso cref="System.Type"/>到<seealso cref="System.Data.DbType"/>的映射
        /// </summary>
        Dictionary<Type, DbType> Net2DbMapper { get; }
        /// <summary>
        /// 存储从<seealso cref="System.Data.DbType"/>到<seealso cref="System.Type"/>映射
        /// </summary>
        Dictionary<DbType, Type> Db2NetMapper { get; }

        /// <summary>
        /// 存储从<seealso cref="System.Data.DbType"/>到SQL字符串的映射
        /// </summary>
        Dictionary<DbType, string> Db2SQLMapper { get; }
    }
}
