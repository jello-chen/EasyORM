using System;
using System.Collections;
using System.Collections.Generic;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using EasyORM.SchemaModel;
using EasyORM.Configuration;

namespace EasyORM.Provider.SQLite
{
    public class EntitySeqenceInserter : EntityInserterBase
    {
        Table _table;
        public EntitySeqenceInserter(DataContext context, Table table)
            : base(context)
        {
            _table = table;
        }
        public override int Insert(ArrayList list)
        {
            int page, limit = 10;
            page = (int)Math.Ceiling(list.Count / (double)limit);
            int pageIndex = 1;
            var insertStart = "insert into {0}({1}) values{2}";
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
                fields.Add(string.Format("[{0}]", item.Name));
            }
            var type = _table.Type;
            if (DynamicProxy.IsProxy(type))
            {
                type = type.BaseType;
            }
            var getters = ExpressionReflector.GetGetters(type);
            var count = 0;
            long maxIndex = 0;
            var setters = ExpressionReflector.GetSetters(type);
            Action<object, object> keySetter = null;
            var keyColumn = _table.Key;
            keySetter = setters.GetOrDefault(keyColumn.PropertyInfo.Name);
            var obj = sqlExecutor.ExecuteScalar(string.Format("select max(Count) from {0} where Name='{1}'", Config.SequenceTable, _table.Name), new Dictionary<string, object>());
            if (obj == DBNull.Value)
            {
                sqlExecutor.ExecuteNonQuery(string.Format("insert into {0}(Name,Count) values('{1}',{2})", Config.SequenceTable, _table.Name, 0), new Dictionary<string, object>());
            }
            else
            {
                maxIndex = Convert.ToInt32(obj);
            }
            var fieldString = string.Join(",", fields);
            while (pageIndex <= page)
            {
                var start = (pageIndex - 1) * limit;
                ArrayList entities = null;
                if (start + limit > list.Count)
                {
                    entities = list.GetRange(start, list.Count - start);
                }
                else
                {
                    entities = list.GetRange(start, limit);
                }
                var values = new List<string>();
                var index = 0;
                var sqlParameters = new Dictionary<string, object>();
                foreach (var entity in entities)
                {
                    var value = new List<string>();
                    keySetter(entity, ++maxIndex);
                    foreach (var key in getters.Keys)
                    {
                        if (autoincreamentColumn == key)
                        {
                            continue;
                        }
                        value.Add(string.Format("@{0}{1}", key, index));
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
                        sqlParameters.Add(key + index, valueParam);
                    }
                    index++;
                    values.Add(string.Format("({0})", string.Join(",", value)));
                }
                var sql = string.Format(insertStart, tableName, fieldString, string.Join(",", values));
                count += sqlExecutor.ExecuteNonQuery(sql, sqlParameters);
                pageIndex++;
                sqlParameters.Clear();
            }

            sqlExecutor.ExecuteNonQuery(string.Format("update {0} set {3}={1} where Name='{2}'", Config.SequenceTable, maxIndex, _table.Name, _sqlBuilder.GetTableName("Count")), new Dictionary<string, object>());
            return count;
        }
    }
}
