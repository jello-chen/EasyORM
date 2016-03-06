using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DbUtils.DataAnnotations;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using EasyORM.SchemaModel;
using EasyORM.DbUtils;

namespace EasyORM.Provider.SQLServer
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
            var keyColumn = _table.Key;
            var type = _table.Type;
            if (DynamicProxy.IsProxy(type))
            {
                type = type.BaseType;
            }
            var count = 0;
            var getters = ExpressionReflector.GetGetters(type);
            var setters = ExpressionReflector.GetSetters(type);
            Action<object, object> keySetter = setters.GetOrDefault(keyColumn.PropertyInfo.Name);
            if (list.Count <= 10)
            {
                var insertStart = "INSERT INTO {0}({1}) VALUES({2})" + Environment.NewLine + "SELECT SCOPE_IDENTITY()";
                var tableName = string.Empty;
                if (!string.IsNullOrWhiteSpace(_table.DataBase))
                {
                    tableName = string.Format("[{0}].dbo.", _table.DataBase);
                }
                tableName = string.Format(tableName + "[{0}]", _table.Name);
                var fields = new List<string>();
                var autoincreamentColumn = string.Empty;
                foreach (var item in _table.Columns.Values)
                {
                    if (item.ColumnType == ColumnType.AutoIncreament)
                    {
                        autoincreamentColumn = item.Name;
                        continue;
                    }
                    fields.Add(string.Format("[{0}]", item.Name));
                }
                var fieldString = string.Join(",", fields);
                foreach (var entity in list)
                {
                    var sqlParameters = new Dictionary<string, object>();
                    var value = new List<string>();
                    foreach (var key in getters.Keys)
                    {
                        if (autoincreamentColumn == key)
                        {
                            continue;
                        }
                        value.Add(string.Format("@{0}", key));
                        sqlParameters.Add(key, getters.GetOrDefault(key)(entity));
                    }
                    var sql = string.Format(insertStart, tableName, fieldString, string.Join(",", value));
                    var r = sqlExecutor.ExecuteScalar(sql, sqlParameters);
                    if (r != DBNull.Value)
                    {
                        count += 1;
                        keySetter(entity, Convert.ToInt32(r));
                    }
                }
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
