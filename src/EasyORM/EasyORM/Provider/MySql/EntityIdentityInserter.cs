using System;
using System.Collections;
using System.Collections.Generic;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using EasyORM.SchemaModel;
using EasyORM.DbUtils;

namespace EasyORM.Provider.MySql
{
    public class EntityIdentityInserter : EntityInserterBase
    {
        Table _table;
        public EntityIdentityInserter(DataContext context, Table table)
            : base(context)
        {
            _table = table;
        }

        /// <summary>
        /// Insert a list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public override int Insert(ArrayList list)
        {
            var count = 0;
            var type = _table.Type;
            if (DynamicProxy.IsProxy(type))
            {
                type = type.BaseType;
            }
            var getters = ExpressionReflector.GetGetters(type);
            var setters = ExpressionReflector.GetSetters(type);
            Action<object, object> keySetter = null;
            var keyColumn = _table.Key;
            keySetter = setters.GetOrDefault(keyColumn.PropertyInfo.Name);
            int page, limit = 10;
            page = (int)Math.Ceiling(list.Count / (double)limit);
            var insertStart = "insert into {0}({1}) values({2});" + Environment.NewLine + "SELECT LAST_INSERT_ID();";
            var tableName = string.Empty;
            if (!string.IsNullOrWhiteSpace(_table.DataBase))
            {
                tableName = _sqlBuilder.GetTableName(_table.DataBase) + ".";
            }
            tableName = tableName + _sqlBuilder.GetTableName(_table.Name);
            var fields = new List<string>();
            var autoincreamentColumn = string.Empty;
            foreach (var item in _table.Columns.Values)
            {
                if (item.ColumnType == KeyColumnType.AutoIncreament)
                {
                    autoincreamentColumn = item.Name;
                    continue;
                }
                fields.Add(item.Name);
            }
            var fieldString = string.Join(",", fields);
            var sqlParameters = new Dictionary<string, object>();
            foreach (var entity in list)
            {
                var value = new List<string>();
                foreach (var key in getters.Keys)
                {
                    if (autoincreamentColumn == key)
                    {
                        continue;
                    }
                    value.Add(string.Format("@{0}", key));
                    var valueParam = getters.GetOrDefault(key)(entity);
                    var dateValue = valueParam as DateTime?;
                    if (dateValue != null)
                    {
                        if (dateValue.Value.Date == dateValue.Value)
                        {
                            valueParam = dateValue.Value.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            valueParam = dateValue.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                    sqlParameters.Add(key, valueParam);
                }
                var sql = string.Format(insertStart, tableName, fieldString, string.Join(",", value));
                var r = sqlExecutor.ExecuteScalar(sql, sqlParameters);
                if (r != DBNull.Value)
                {
                    count += 1;
                    keySetter(entity, Convert.ToInt32(r));
                }
                sqlParameters.Clear();
            }
            return count;
        }
    }
}
