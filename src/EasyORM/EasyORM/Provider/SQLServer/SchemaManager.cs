using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using EasyORM.SchemaModel;
using EasyORM.Utils;
using EasyORM.DbUtils;

namespace EasyORM.Provider.SQLServer
{
    public class SchemaManager : SchemaManagerBase
    {
        static SchemaManager()
        {
        }
        public SchemaManager(DataContext context)
            : base(context)
        {

        }

        public override SchemaModel.Table GetTable(string tableName)
        {
            return GetTables(tableName).FirstOrDefault();
        }

        internal override void CreateTable(Table table)
        {
            var typeMapper = _provider.CreateTypeMapper();
            var builder = _provider.CreateSqlBuilderFactory().CreateSqlBuilder();
            var sqlBuilder = new StringBuilder("CREATE TABLE [DBO]." + builder.GetTableName(table.Name));
            var columns = new List<string>();
            foreach (var columnName in table.Columns.Keys)
            {
                var column = table.Columns[columnName];
                var columnBuilder = new StringBuilder("[" + column.Name + "]");
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
                if (column.ColumnType == KeyColumnType.AutoIncreament)
                {
                    columnBuilder.Append(" ");
                    columnBuilder.Append("IDENTITY(1,1)");
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
            if (table.Key != null)
            {
                var keyBuilder = new StringBuilder();
                keyBuilder.Append("CONSTRAINT [PK_" + table.Name + "_1] PRIMARY KEY CLUSTERED ([" + table.Key.Name + "] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                columns.Add(keyBuilder.ToString());
            }
            sqlBuilder.Append("(");
            sqlBuilder.Append(string.Join("," + Environment.NewLine, columns));
            sqlBuilder.Append(")ON [PRIMARY]");
            ExecuteNonQuery(sqlBuilder.ToString(), new Dictionary<string, object>());
        }

        public List<Table> GetTables(string tableName = null)
        {
            var typeMapper = _provider.CreateTypeMapper();
            var builder = _provider.CreateSqlBuilderFactory().CreateSqlBuilder();
            List<Table> tables = new List<Table>();
            CreateConnection();
            DataTable tableDt = null;
            try
            {
                tableDt = DbConnection.GetSchema("tables", new string[] { null, null, tableName, "BASE TABLE" });
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
                var schemaDt = ExecuteSchema("SELECT TOP 1 * FROM " + builder.GetTableName(table.Name));
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
                    column.MaxLength = Convert.ToInt32(schemaRow["ColumnSize"]);
                    column.DbType = typeMapper.Net2DbMapper.GetOrDefault(Type.GetType(schemaRow["DataType"].ToString()));
                    column.NotNull = Convert.ToBoolean(schemaRow["AllowDBNull"]);
                    try
                    {
                        table.Columns.Add(column.Name, column);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Get all tables failed，table name：" + table.Name + "，column name：" + column.Name);
                    }
                }
                tables.Add(table);
            }
            return tables;
        }

        public override List<Table> GetTables()
        {
            return GetTables(null);
        }
    }
}
