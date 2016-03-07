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
namespace EasyORM.Provider.SQLServer
{
    /// <summary>
    /// None Key Entity Inserter
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
            var count = 0;
            var type = _table.Type;
            if (DynamicProxy.IsProxy(type))
            {
                type = type.BaseType;
            }
            var getters = ExpressionReflector.GetGetters(type);
            if (list.Count <= 10)
            {
                #region Insert by using insert sentense
                var insertStart = "insert into {0}({1}) values{2}";
                var tableName = string.Empty;
                if (!string.IsNullOrWhiteSpace(_table.DataBase))
                {
                    tableName = string.Format("[{0}].dbo.", _table.DataBase);
                }
                tableName = string.Format(tableName + "[{0}]", _table.Name);
                var fields = new List<string>();
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
                    foreach (var key in getters.Keys)
                    {
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
                #region Insert by using SqlBulkCopy
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
            return count;
        }
    }
}
