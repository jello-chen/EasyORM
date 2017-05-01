using System;
using System.Collections.Generic;
using System.Data;

namespace EasyORM.Provider
{
    public interface ITypeMapper
    {
        /// <summary>
        /// Map <seealso cref="System.Type"/> to <seealso cref="System.Data.DbType"/>
        /// </summary>
        Dictionary<Type, DbType> Net2DbMapper { get; }
        /// <summary>
        /// Map <seealso cref="System.Data.DbType"/> to <seealso cref="System.Type"/>
        /// </summary>
        Dictionary<DbType, Type> Db2NetMapper { get; }

        /// <summary>
        /// Map <seealso cref="System.Data.DbType"/> to string
        /// </summary>
        Dictionary<DbType, string> Db2SQLMapper { get; }
    }
}
