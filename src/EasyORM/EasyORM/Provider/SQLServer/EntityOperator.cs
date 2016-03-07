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
using EasyORM.Configuration;
using EasyORM.Provider;
using EasyORM.DbUtils;

namespace EasyORM.Provider.SQLServer
{
    internal class EntityOperator : EntityOperatorBase,IEntityOperator
    {
        public EntityOperator(DataContext context):base(context)
        {
        }
        #region Bulk insert
        int IEntityOperator.InsertEntities(ArrayList list)
        {
            if (list.Count <= 0)
            {
                return 0;
            }
            var type = EntityOperatorUtils.GetNonProxyType(list[0].GetType());
            var table = _entityCfgManager.GetTable(type);
            var dataSourceType = EntityOperatorUtils.GetKeyColumnType(table);
            EntityInserterBase inserter = null;
            switch (dataSourceType)
            {
                case KeyColumnType.AutoIncreament:
                    inserter = new EntityIdentityInserter(_context, table);
                    break;
                case KeyColumnType.None:
                    inserter = new EntityNoneInserter(_context, table);
                    break;
                case KeyColumnType.Sequence:
                    inserter = new EntitySeqenceInserter(_context, table);
                    break;
            }
            return inserter.Insert(list);
        }
        #endregion

        int IEntityOperator.UpdateValues(SchemaModel.Column keyColumn, SchemaModel.Table table, Dictionary<string, object> values)
        {
            var keyValue = values.GetOrDefault(keyColumn.Name);
            if (keyValue == null)
            {
                throw new InvalidOperationException("Not found the key");
            }
            var updateSql = "UPDATE {0} SET {1} WHERE {2}";
            var tableName = _sqlBuilder.GetTableName(table);
            var sqlParameters = new Dictionary<string, object>();
            var setts = new List<string>();
            var alias = string.Empty;
            foreach (var key in values.Keys)
            {
                if (key == keyColumn.Name)
                {
                    continue;
                }
                alias = ParserUtils.GenerateAlias(key);
                var set = string.Format("[{0}] = @{1}", key, alias);
                sqlParameters.Add(alias, values.GetOrDefault(key));
                setts.Add(set);
            }
            alias = ParserUtils.GenerateAlias(keyColumn.Name);
            var condition = string.Format("[{0}] = @{1}", keyColumn.Name, alias);
            sqlParameters.Add(alias, keyValue);
            updateSql = string.Format(updateSql, tableName, string.Join(",", setts), condition);
            return _sqlExecutor.ExecuteNonQuery(updateSql, sqlParameters);
        }

        int IEntityOperator.Delete(SchemaModel.Column keyColumn, SchemaModel.Table table, params int[] ids)
        {
            if (ids.Length <= 0) return 0;
            var deleteSql = "DELETE FROM {0} WHERE [{1}] IN ({2})";
            var tableName = _sqlBuilder.GetTableName(table);
            deleteSql = string.Format(deleteSql, tableName, keyColumn.Name, string.Join(",", ids));
            return _sqlExecutor.ExecuteNonQuery(deleteSql, new Dictionary<string, object>());
        }
    }
}
