using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.SchemaModel;
using EasyORM.Utils;
using EasyORM.DbUtils.DataAnnotations;
using EasyORM.DbUtils;

namespace EasyORM.Provider.SQLite
{
    public class SchemaManager : SchemaManagerBase
    {
        public SchemaManager(DataContext context)
            : base(context)
        {

        }

        public List<Table> GetTables(string tableName)
        {

            var builder = _provider.CreateSqlBuilderFactory().CreateSqlBuilder();
            List<Table> tables = new List<Table>();
            CreateConnection();
            DataTable tableDt = null;
            try
            {
                tableDt = DbConnection.GetSchema("tables", new string[] { null, null, tableName, null });
            }
            finally
            {
                Close();
            }
            foreach (DataRow row in tableDt.Rows)
            {
                var table = new Table();
                table.Name = Convert.ToString(row["TABLE_NAME"]);
                table.DataBase = Convert.ToString(row["TABLE_CATALOG"]);
                var schemaDt = ExecuteSchema("SELECT * FROM " + builder.GetTableName(tableName) + " LIMIT 1");
                foreach (DataRow schemaRow in schemaDt.Rows)
                {
                    var column = new Column();
                    column.IsKey = Convert.ToBoolean(schemaRow["IsKey"]);
                    if (Convert.ToBoolean(schemaRow["IsAutoincrement"]))
                    {
                        column.ColumnType = KeyColumnType.AutoIncreament;
                    }
                    else
                    {
                        column.ColumnType = KeyColumnType.None;
                    }
                    if (column.IsKey)
                    {
                        table.Key = column;
                    }
                    column.Name = Convert.ToString(schemaRow["COLUMNNAME"]);
                    column.Table = table;
                }
                tables.Add(table);
            }
            return tables;
        }

        public override SchemaModel.Table GetTable(string tableName)
        {
            return GetTables(tableName).FirstOrDefault();
        }

        internal override void CreateTable(Table table)
        {
            var typeMapper = _provider.CreateTypeMapper();
            var builder = _provider.CreateSqlBuilderFactory().CreateSqlBuilder();
            var sqlBuilder = new StringBuilder("CREATE TABLE main." + builder.GetTableName(table.Name));
            var columns = new List<string>();
            foreach (var columnName in table.Columns.Keys)
            {
                var column = table.Columns[columnName];
                var columnBuilder = new StringBuilder("\"" + column.Name + "\"");
                columnBuilder.Append(" ");
                var typeString = typeMapper.Db2SQLMapper.GetOrDefault(column.DbType);
                if (string.IsNullOrWhiteSpace(typeString))
                {
                    throw new Exception("Not supported " + column.DbType.ToString() + " data type");
                }
                columnBuilder.Append(typeString);
                if (column.MaxLength > 0)
                {
                    columnBuilder.Append(string.Format("({0})", column.MaxLength));
                }
                else if (column.Precision > 0 && column.Scale > 0)
                {
                    columnBuilder.Append(string.Format("({0},{1})", column.Precision, column.Scale));
                }
                if (column.IsKey)
                {
                    columnBuilder.Append(" ");
                    columnBuilder.Append("PRIMARY KEY");
                }
                if (column.ColumnType == KeyColumnType.AutoIncreament)
                {
                    columnBuilder.Append(" ");
                    columnBuilder.Append("AUTOINCREMENT");
                }
                columnBuilder.Append(" ");
                if (column.NotNull)
                {
                    columnBuilder.Append("NOT NULL");
                }
                else
                {
                    columnBuilder.Append("NULL");
                }
                columns.Add(columnBuilder.ToString());
            }
            sqlBuilder.Append("(");
            sqlBuilder.Append(string.Join("," + Environment.NewLine, columns));
            sqlBuilder.Append(")");
            ExecuteNonQuery(sqlBuilder.ToString(), new Dictionary<string, object>());
        }

        public override List<Table> GetTables()
        {
            return GetTables(null);
        }
    }
}
