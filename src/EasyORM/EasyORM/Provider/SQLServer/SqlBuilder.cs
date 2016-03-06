﻿using EasyORM.TranslateModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;
namespace EasyORM.Provider.SQLServer
{
    public class SqlBuilder : SqlBuilderBase
    {
        public SqlBuilder(DataContext context)
            : base(context)
        {

        }
        void BuildPageSql()
        {
            var columns = _context.Columns;
            var conditions = _context.Conditions;
            var joins = _context.Joins;
            var fromBuilder = new StringBuilder("FROM ");
            var selectBuilder = new StringBuilder("SELECT ");
            if (_context.Distinct)
            {
                selectBuilder.Append("DISTINCT ");
            }
            var whereBuilder = new StringBuilder();
            var sortBuilder = new StringBuilder();
            var sqlBuilder = new StringBuilder();
            if (joins != null && joins.Count > 0)
            {
                fromBuilder.Append(BuildJoinSql());

                var selectStr = FormatSelectString(columns);
                if (!_context.Distinct)
                {
                    selectStr = string.Format("{0},{1}", selectStr, string.Format("ROW_NUMBER() OVER({0}) #index", FormatSortColumns()));
                }
                selectBuilder.AppendLine(selectStr);

                if (conditions.Any())
                {
                    whereBuilder.Append(BuildWhere(conditions));
                }

                sqlBuilder.Clear();
                sqlBuilder.AppendFormat("{0} {1} {2} {3}", selectBuilder.ToString(), fromBuilder.ToString(), whereBuilder.ToString(), sortBuilder.ToString());
            }
            else
            {
                fromBuilder = new StringBuilder("FROM ");
                var table = columns.FirstOrDefault().Table;
                fromBuilder.Append(GetTableName(table));
                tableName = ParserUtils.GenerateAlias(table.Name);
                fromBuilder.AppendFormat(" [{0}]", tableName);
                if (_context.NoLockTables.Contains(table.Name))
                {
                    fromBuilder.Append(" WITH (NOLOCK) ");
                }

                var selectStr = FormatSelectString(columns);
                if (!_context.Distinct)
                {
                    selectStr = string.Format("{0},{1}", selectStr, string.Format("ROW_NUMBER() OVER({0}) #index", FormatSortColumns()));
                }
                selectBuilder.Append(selectStr);

                if (conditions.Any())
                {
                    whereBuilder.Append(BuildWhere(conditions));
                }

                sqlBuilder.Clear();
                sqlBuilder.AppendFormat("{0} {1} {2} {3}", selectBuilder.ToString(), fromBuilder.ToString(), whereBuilder.ToString(), sortBuilder.ToString());
            }

            var sql = sqlBuilder.ToString();
            sqlBuilder.Clear();
            selectBuilder.Clear();
            whereBuilder.Clear();
            fromBuilder.Clear();
            if (_context.Distinct)
            {
                //sqlBuilder中在去重复的情况下无法加入分页代码，需要作为子查询加入
                tableName = ParserUtils.GenerateAlias("table");
                fromBuilder.AppendFormat("FROM ({0}) {1}", sql, tableName);

                selectBuilder.Append("SELECT ");
                var fields = new List<string>();
                foreach (var column in columns)
                {
                    fields.Add(string.Format("[{0}].[{1}]", tableName, column.Alias ?? column.MemberInfo.Name));
                }
                fields.Add(string.Format("ROW_NUMBER() OVER({0}) #index", FormatSortColumns()));
                selectBuilder.Append(string.Join(",", fields));
                sqlBuilder.AppendFormat("{0} {1}", selectBuilder.ToString(), fromBuilder.ToString());
                sql = sqlBuilder.ToString();
                sqlBuilder.Clear();
                fromBuilder.Clear();
            }


            if (_context.Pager)
            {
                fromBuilder.AppendFormat("FROM ({0}) [_indexTable]", sql);
                var fields = new List<string>();
                foreach (var item in _context.Columns)
                {
                    fields.Add(string.Format("[_indexTable].[{0}]", item.Alias ?? item.MemberInfo.Name));
                }
                sqlBuilder.Append("SELECT ");
                sqlBuilder.AppendFormat("{0} FROM ({1}) _indexTable where [_indexTable].[#index] BETWEEN {2} AND {3}",
                    string.Join(",", fields), sql, _context.Skip + 1, _context.Skip + _context.Take);
                sql = sqlBuilder.ToString();
            }
            _result.CommandText = sql;
        }

        string BuildNoLockSql(string tableName)
        {
            if (_context.NoLockTables.Contains(tableName))
            {
                return " WITH (NOLOCK) ";
            }
            return string.Empty;
        }

        string BuildJoinSql()
        {
            var joins = _context.Joins;
            var firstJoin = joins.Values.FirstOrDefault();
            var leftColumn = firstJoin.Left;
            var leftTable = leftColumn.Table;
            var fromBuilder = new StringBuilder();
            fromBuilder.Append(GetTableName(leftTable));
            fromBuilder.AppendFormat(" [{0}]", leftTable.Alias);
            fromBuilder.Append(BuildNoLockSql(leftTable.Name));
            fromBuilder.Append(GenJoinType(firstJoin.JoinType));
            var rightColumn = firstJoin.Right;
            var rightTable = rightColumn.Table;
            fromBuilder.Append(GetTableName(rightTable));
            fromBuilder.AppendFormat(" [{0}]", rightTable.Alias);
            fromBuilder.Append(BuildNoLockSql(rightTable.Name));

            fromBuilder.AppendLine();
            fromBuilder.Append(" ON ");
            fromBuilder.AppendFormat("[{0}].[{1}]", leftTable.Alias, leftColumn.Name);
            fromBuilder.AppendFormat(" = ");
            fromBuilder.AppendFormat("[{0}].[{1}]" + Environment.NewLine, rightTable.Alias, rightColumn.Name);
            fromBuilder.AppendLine();
            foreach (var join in joins.Values.Skip(1))
            {
                leftColumn = join.Left;
                leftTable = leftColumn.Table;
                fromBuilder.Append(GenJoinType(join.JoinType));
                rightColumn = join.Right;
                rightTable = rightColumn.Table;
                fromBuilder.Append(GetTableName(rightTable));
                fromBuilder.AppendFormat(" [{0}]", rightTable.Alias);
                fromBuilder.Append(BuildNoLockSql(rightTable.Name));

                fromBuilder.AppendLine();
                fromBuilder.Append(" ON ");
                fromBuilder.AppendFormat("[{0}].[{1}]", leftTable.Alias, leftColumn.Name);
                fromBuilder.AppendFormat(" = ");
                fromBuilder.AppendFormat("[{0}].[{1}]" + Environment.NewLine, rightTable.Alias, rightColumn.Name);
                fromBuilder.AppendLine();
            }
            return fromBuilder.ToString();
        }

        protected override void BuildSelectSql()
        {
            if (_context.Pager)
            {
                BuildPageSql();
                return;
            }
            var columns = _context.Columns;
            var conditions = _context.Conditions;
            var joins = _context.Joins;
            var fromBuilder = new StringBuilder("FROM ");
            fromBuilder.AppendLine();
            var selectBuilder = new StringBuilder("SELECT ");
            selectBuilder.AppendLine();
            if (_context.Distinct)
            {
                selectBuilder.Append(" DISTINCT ");
            }
            else if (_context.Take > 0)
            {
                selectBuilder.Append(" TOP " + _context.Take + " ");
            }
            var whereBuilder = new StringBuilder();
            var sortBuilder = new StringBuilder();
            var sqlBuilder = new StringBuilder();
            if (joins != null && joins.Count > 0)
            {
                fromBuilder.Append(BuildJoinSql());
                selectBuilder.Append(FormatSelectString(columns));

                if (conditions.Any())
                {
                    whereBuilder.Append(BuildWhere(conditions));
                }

                if (_context.Skip == -1 || _context.Take == -1)
                {
                    sortBuilder.Append(FormatSortColumns());
                }

                sqlBuilder.Clear();
                sqlBuilder.AppendFormat("{0} {1} {2} {3}", selectBuilder.ToString(), fromBuilder.ToString(), whereBuilder.ToString(), sortBuilder.ToString());
            }
            else
            {
                fromBuilder = new StringBuilder("FROM ");
                var table = columns.FirstOrDefault().Table;
                fromBuilder.Append(GetTableName(table));
                tableName = ParserUtils.GenerateAlias(table.Name);
                fromBuilder.AppendFormat(" [{0}]", tableName);
                if (_context.NoLockTables.Contains(table.Name))
                {
                    fromBuilder.Append(" WITH (NOLOCK) ");
                }

                selectBuilder.Append(FormatSelectString(columns));

                if (conditions.Any())
                {
                    whereBuilder.Append(BuildWhere(conditions));
                }

                if (_context.Take == -1 || _context.Skip == -1)
                {
                    sortBuilder.Append(FormatSortColumns());
                }

                sqlBuilder.Clear();
                sqlBuilder.AppendFormat("{0} {1} {2} {3}", selectBuilder.ToString(), fromBuilder.ToString(), whereBuilder.ToString(), sortBuilder.ToString());
            }

            var sql = sqlBuilder.ToString();
            _result.CommandText = sql;
        }

        public override string GetTableName(Table table)
        {
            var tableName = string.Empty;
            if (!string.IsNullOrWhiteSpace(table.DataBase))
            {
                tableName = string.Format("[{0}].DBO.", table.DataBase);
            }
            tableName = string.Format("{0}{1}", tableName, GetTableName(table.Name));
            return tableName;
        }


        public override string GetTableName(SchemaModel.Table table)
        {
            var tableName = string.Empty;
            if (!string.IsNullOrWhiteSpace(table.DataBase))
            {
                tableName = string.Format("[{0}].DBO.", table.DataBase);
            }
            tableName = string.Format("{0}{1}", tableName, GetTableName(table.Name));
            return tableName;
        }

        string FormatConverter(bool isColumnCaller, string rawConverter, string converter, string param)
        {
            if (isColumnCaller)
            {
                converter = string.Format(rawConverter, string.Format(converter, param, "{0}"));
            }
            else
            {
                converter = string.Format(rawConverter, string.Format(converter, "{0}", param));
            }
            return converter;
        }

        public override string ParseConverter(Column column)
        {
            var converter = string.Empty;
            if (column.Converters.Any())
            {
                converter = "{0}";
            }
            while (column.Converters.Count > 0)
            {
                var columnConverter = column.Converters.Pop();
                var memberInfo = columnConverter.MemberInfo;
                var args = columnConverter.Parameters;
                var paramName = "@" + ParserUtils.GenerateAlias("param");
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        if (TypeHelper.IsNullableType(memberInfo.DeclaringType))
                        {
                            continue;
                        }
                        if (memberInfo.DeclaringType == ReflectorConsts.DateTimeNullableType || memberInfo.DeclaringType == ReflectorConsts.DateTimeType)
                        {
                            switch (memberInfo.Name)
                            {
                                case "Date":
                                    converter = string.Format(converter, "CONVERT(DATE,{0},211)");
                                    break;
                                case "Value":

                                    break;
                                default:
                                    throw new Exception("不支持");
                            }
                            continue;
                        }
                        else if (memberInfo.DeclaringType == ReflectorConsts.TimeSpanType)
                        {
                            var unit = string.Empty;
                            switch (memberInfo.Name)
                            {
                                case "TotalDays":
                                    unit = "DAY";
                                    break;
                                case "TotalHours":
                                    unit = "HOUR";
                                    break;
                                case "TotalMilliseconds":
                                    unit = "MILLISECOND";
                                    break;
                                case "TotalMinutes":
                                    unit = "MINUTE";
                                    break;
                                case "TotalSeconds":
                                    unit = "SECOND";
                                    break;
                                default:
                                    throw new Exception("不支持");
                            }
                            converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEDIFF(" + unit + ",{1},{0})", paramName);
                            _result.Parameters.Add(paramName, args[0]);
                            continue;
                        }
                        throw new Exception("不支持");
                    case MemberTypes.Method:
                        if (memberInfo.DeclaringType == ReflectorConsts.StringType)
                        {
                            switch (memberInfo.Name)
                            {
                                case "Contains":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "CHARINDEX({0},{1})>0", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "StartsWith":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "CHARINDEX({0},{1})=1", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "Substring":
                                    if (columnConverter.Parameters.Count == 1)
                                    {
                                        if (columnConverter.IsInstanceColumn)
                                        {
                                            converter = string.Format(converter, "SUBSTRING({0}," + (Convert.ToInt32(columnConverter.Parameters[0]) + 1) + ",LEN({0})+1-" + columnConverter.Parameters[0] + ")");
                                        }
                                        else
                                        {
                                            throw new Exception("不支持");
                                        }
                                    }
                                    else if (columnConverter.Parameters.Count == 2)
                                    {
                                        if (columnConverter.IsInstanceColumn)
                                        {
                                            converter = string.Format(converter, "SUBSTRING({0}," + (Convert.ToInt32(columnConverter.Parameters[0]) + 1) + "," + columnConverter.Parameters[1] + ")");
                                        }
                                        else
                                        {
                                            throw new Exception("不支持");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("不支持");
                                    }
                                    break;
                                default:
                                    throw new Exception("不支持");
                            }
                            continue;
                        }
                        else if (memberInfo.DeclaringType == ReflectorConsts.DateTimeType || memberInfo.DeclaringType == ReflectorConsts.DateTimeNullableType)
                        {
                            switch (memberInfo.Name)
                            {
                                case "AddDays":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(DAY,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "AddHours":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(HOUR,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "AddYears":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(YEAR,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "AddMonths":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(MONTH,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "AddSeconds":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(SECOND,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "AddMilliseconds":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(MILLISECOND,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                case "AddMinutes":
                                    converter = FormatConverter(columnConverter.IsInstanceColumn, converter, "DATEADD(MINUTE,{0},{1})", paramName);
                                    _result.Parameters.Add(paramName, args[0]);
                                    break;
                                default:
                                    throw new Exception("不支持");
                            }
                            continue;
                        }
                        else if (memberInfo.DeclaringType.IsGenericType)
                        {
                            switch (memberInfo.Name)
                            {
                                case "Contains":
                                    if (columnConverter.IsInstanceColumn)
                                    {
                                        throw new Exception("不支持");
                                    }
                                    var arg = args[0];
                                    var type = memberInfo.DeclaringType;
                                    if (type.IsGenericType)
                                    {
                                        var set = (IEnumerable)arg;
                                        var elType = type.GetGenericArguments().Last();
                                        EnumerableContainsMethodProcessor proccessor = new EnumerableContainsMethodProcessor(elType, set, converter);
                                        proccessor.Process();
                                        converter = proccessor.Result.ToString();
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }

                                    break;
                                default:
                                    throw new Exception("不支持");
                            }
                        }
                        else if (memberInfo.DeclaringType == ReflectorConsts.EnumerableType)
                        {
                            switch (memberInfo.Name)
                            {
                                case "Contains":
                                    if (columnConverter.IsInstanceColumn)
                                    {
                                        throw new Exception("不支持");
                                    }
                                    var arg = args[0];
                                    var type = arg.GetType();
                                    Type elType = null;
                                    IEnumerable set = (IEnumerable)arg;
                                    if (type.IsArray)
                                    {
                                        Array array = (Array)arg;
                                        elType = type.GetElementType();
                                    }
                                    else if (type.IsGenericType)
                                    {
                                        elType = type.GetGenericArguments().Last();
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                    EnumerableContainsMethodProcessor proccessor = new EnumerableContainsMethodProcessor(elType, set, converter);
                                    proccessor.Process();
                                    converter = proccessor.Result.ToString();
                                    break;
                                default:
                                    throw new Exception("不支持");
                            }
                            continue;
                        }
                        else if (memberInfo.DeclaringType == ReflectorConsts.QueryableType)
                        {
                            switch (memberInfo.Name)
                            {
                                case "Contains":
                                    if (columnConverter.IsInstanceColumn)
                                    {
                                        throw new Exception("不支持");
                                    }
                                    var arg = (IQueryable)args[0];
                                    var type = arg.GetType();
                                    if (type.IsGenericType)
                                    {
                                        var elType = type.GetGenericArguments().Last();
                                        QueryableContainsMethodProcessor processor = new QueryableContainsMethodProcessor(arg, elType, converter);
                                        processor.Process();
                                        converter = processor.Result.ToString();
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }

                                    break;
                                default:
                                    throw new Exception("暂时对延迟加载支持不好");
                            }
                            break;
                        }
                        else
                        {
                            throw new Exception("不支持");
                        }
                        break;
                    default:
                        throw new Exception();
                }
            }
            return converter;
        }
    }
}