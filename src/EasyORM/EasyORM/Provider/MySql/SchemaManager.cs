﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using EasyORM.SchemaModel;
using EasyORM.Utils;
using EasyORM.DbUtils;

namespace EasyORM.Provider.MySql
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
            var sqlBuilder = new StringBuilder("CREATE TABLE " + builder.GetTableName(table.Name));
            var columns = new List<string>();
            foreach (var columnName in table.Columns.Keys)
            {
                var column = table.Columns[columnName];
                var columnBuilder = new StringBuilder("`" + column.Name + "`");
                columnBuilder.Append(" ");
                var typeString = typeMapper.Db2SQLMapper.GetOrDefault(column.DbType);
                if (string.IsNullOrWhiteSpace(typeString))
                {
                    throw new Exception("Not support " + column.DbType.ToString() + " data type");
                }
                columnBuilder.Append(typeString);
                switch (column.DbType)
                {
                    case DbType.Currency:
                    case DbType.Decimal:
                        if (column.Precision <= 0)
                        {
                            column.Precision = 19;
                        }
                        if (column.Scale <= 0)
                        {
                            column.Scale = 4;
                        }
                        columnBuilder.Append(string.Format("({0},{1})", column.Precision, column.Scale));
                        break;
                    case DbType.String:
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.StringFixedLength:
                        if (column.MaxLength <= 0)
                        {
                            column.MaxLength = 50;
                        }
                        columnBuilder.Append(string.Format("({0})", column.MaxLength));
                        break;
                    default:
                        if (column.MaxLength <= 0)
                        {
                            column.MaxLength = 4;
                        }
                        columnBuilder.AppendFormat("({0})", column.MaxLength);
                        break;
                }
                if (column.ColumnType == KeyColumnType.AutoIncreament)
                {
                    columnBuilder.Append(" ");
                    columnBuilder.Append("AUTO_INCREMENT");
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
                keyBuilder.Append("PRIMARY KEY (`" + table.Key.Name + "`)");
                columns.Add(keyBuilder.ToString());
            }
            sqlBuilder.Append("(");
            sqlBuilder.Append(string.Join("," + Environment.NewLine, columns));
            sqlBuilder.Append(")");
            ExecuteNonQuery(sqlBuilder.ToString(), new Dictionary<string, object>());
        }

        List<Table> GetTables(string tableName = null)
        {
            var typeMapper = _provider.CreateTypeMapper();
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
                var schemaDt = ExecuteSchema("SELECT * FROM " + builder.GetTableName(table.Name) + " LIMIT 1");
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
                    column.DbType = typeMapper.Net2DbMapper.Get(Type.GetType(schemaRow["DataType"].ToString()));
                    column.NotNull = Convert.ToBoolean(schemaRow["AllowDBNull"]);
                    table.Columns.Add(column.Name, column);
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
