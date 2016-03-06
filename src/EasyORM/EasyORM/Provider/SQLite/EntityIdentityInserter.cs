using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using EasyORM.SchemaModel;
namespace EasyORM.Provider.SQLite
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
        /// 插入实体集
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
            var insertStart = "insert into {0}({1}) values({2});" + Environment.NewLine + "SELECT last_insert_rowid()";
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
                if (item.ColumnType == DbUtils.ColumnType.AutoIncreament)
                {
                    autoincreamentColumn = item.Name;
                    continue;
                }
                fields.Add(string.Format("[{0}]", item.Name));
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
