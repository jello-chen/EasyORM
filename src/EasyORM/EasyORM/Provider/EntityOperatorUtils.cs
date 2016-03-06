using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.SchemaModel;
using EasyORM.DbUtils;

namespace EasyORM.Provider
{
    public class EntityOperatorUtils
    {
        /// <summary>
        /// 根据实体类型返回该实体在插入到数据库时应该对主键列采用哪一种数据来源
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ColumnType GetDataSourceType(Table table)
        {
            var keyColumn = table.Key;
            if (keyColumn != null)
            {
                return keyColumn.ColumnType;
            }
            else
            {
                return ColumnType.None;
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
