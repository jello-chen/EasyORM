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
    /// <summary>
    /// 当实体没有任何主键时，使用该类进行持久化到数据库
    /// </summary>
    public class EntityNoneInserter : EntityInserterBase
    {
        Table _table;
        public EntityNoneInserter(DataContext context, Table table)
            : base(context)
        {
            _table = table;
        }
        public override int Insert(ArrayList list)
        {
            var type = _table.Type;
            if (DynamicProxy.IsProxy(type))
            {
                type = type.BaseType;
            }
            int page, limit = 10;
            page = (int)Math.Ceiling(list.Count / (double)limit);
            int pageIndex = 1;
            var insertStart = "insert into {0}({1}) values({2})";
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
            var fieldString = string.Join(",", fields);
            var getters = ExpressionReflector.GetGetters(type);
            var count = 0;
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
            }

            return count;
        }
    }
}
