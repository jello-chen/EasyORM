using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using EasyORM.SchemaModel;
using EasyORM.Configuration;

namespace EasyORM.Provider.SQLServer
{
    /// <summary>
    /// 当实体主键来源为序列表时，使用该类进行持久化到数据库
    /// </summary>
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
            var type = _table.Type;
            if (DynamicProxy.IsProxy(type))
            {
                type = type.BaseType;
            }
            var keyColumn = _table.Key;
            var count = 0;
            var maxIndex = 0;
            var getters = ExpressionReflector.GetGetters(type);
            var setters = ExpressionReflector.GetSetters(type);
            Action<object, object> keySetter = null;
            if (keyColumn != null)
            {
                keySetter = setters.GetOrDefault(keyColumn.PropertyInfo.Name);
            }
            var obj = sqlExecutor.ExecuteScalar(string.Format("select max(Count) from {0} where Name='{1}'", Config.SequenceTable, _table.Name), new Dictionary<string, object>());
            if (obj == DBNull.Value)
            {
                sqlExecutor.ExecuteNonQuery(string.Format("insert into {0}(Name,Count) values('{1}',{2})", Config.SequenceTable, _table.Name, 0), new Dictionary<string, object>());
            }
            else
            {
                maxIndex = Convert.ToInt32(obj);
            }
            if (list.Count <= 10)
            {
                #region 使用Insert语句插入
                var insertStart = "insert into {0}({1}) values{2}";
                var tableName = string.Empty;
                if (!string.IsNullOrWhiteSpace(_table.DataBase))
                {
                    tableName = string.Format("[{0}].dbo.", _table.DataBase);
                }
                tableName = string.Format(tableName + "[{0}]", _table.Name);
                var fields = new List<string>();
                var seqColumn = string.Empty;
                foreach (var item in _table.Columns.Values)
                {
                    fields.Add(string.Format("[{0}]", item.Name));
                }
                var fieldString = string.Join(",", fields);
                var values = new List<string>();
                var index = 0;
                var sqlParameters = new Dictionary<string, object>();
                foreach (var entity in list)
                {
                    var value = new List<string>();
                    keySetter(entity, ++maxIndex);
                    foreach (var key in getters.Keys)
                    {
                        if (seqColumn == key)
                        {
                            continue;
                        }
                        value.Add(string.Format("@{0}{1}", key, index));
                        sqlParameters.Add(key + index, getters.GetOrDefault(key)(entity));
                    }
                    index++;
                    values.Add(string.Format("({0})", string.Join(",", value)));
                }
                var sql = string.Format(insertStart, tableName, fieldString, string.Join(",", values));
                count = sqlExecutor.ExecuteNonQuery(sql, sqlParameters);
                #endregion
            }
            else
            {
                #region 使用SqlBulkCopy插入
                var sqlBulkCopy = new SqlBulkCopy(_dataContext.DatabaseConfig.ConnectionString);
                sqlBulkCopy.DestinationTableName = "dbo.[" + _table.Name + "]";
                sqlBulkCopy.BulkCopyTimeout = 300;
                if (list.Count > 500000)
                    sqlBulkCopy.BatchSize = list.Count / 10;
                var dataTable = new DataTable();
                foreach (var column in _table.Columns.Values)
                {
                    var dataColumn = new DataColumn();
                    dataColumn.ColumnName = column.Name;
                    dataColumn.DataType = TypeHelper.GetUnderlyingType(column.PropertyInfo.PropertyType);
                    dataTable.Columns.Add(dataColumn);
                    sqlBulkCopy.ColumnMappings.Add(column.Name, column.Name);
                }
                foreach (var item in list)
                {
                    var row = dataTable.NewRow();
                    keySetter(item, ++maxIndex);
                    foreach (var key in getters.Keys)
                    {
                        var value = getters.GetOrDefault(key)(item);
                        if (value == null)
                        {
                            value = DBNull.Value;
                        }
                        row[_table.Columns.GetOrDefault(key).Name] = value;
                    }
                    dataTable.Rows.Add(row);
                }
                sqlBulkCopy.WriteToServer(dataTable);
                sqlBulkCopy.Close();
                #endregion
                count = list.Count;
            }
            sqlExecutor.ExecuteNonQuery(string.Format("update {0} set [Count]={1} where Name='{2}'", Config.SequenceTable, maxIndex, _table.Name), new Dictionary<string, object>());
            return count;
        }
    }
}
