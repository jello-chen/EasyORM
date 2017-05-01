using System;
using EasyORM.SchemaModel;
using EasyORM.DbUtils;

namespace EasyORM.Provider
{
    public class EntityOperatorUtils
    {
        /// <summary>
        /// Get the primary key column type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static KeyColumnType GetKeyColumnType(Table table)
        {
            var keyColumn = table.Key;
            if (keyColumn != null)
            {
                return keyColumn.ColumnType;
            }
            else
            {
                return KeyColumnType.None;
            }
        }

        public static Type GetNonProxyType(Type type)
        {
            if(DynamicObject.DynamicProxy.IsProxy(type))
            {
                return type.BaseType;
            }
            return type;
        }
    }
}
