using EasyORM.Provider;
using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;
using EasyORM.Configuration;

namespace EasyORM
{
    public abstract class SqlBuilderBase
    {
        protected ParseResult _result;
        protected BuilderContext _context;
        protected DataContext _dataContext;
        protected string tableName;
        ProviderBase _provider;

        public ProviderBase Provider
        {
            get { return _provider; }
        }
        public SqlBuilderBase(DataContext context)
        {
            _dataContext = context;
            _provider = _dataContext.Provider;
        }
        protected abstract void BuildSelectSql();
        void BuildDeleteSql()
        {
            var where = string.Empty;
            var table = _dataContext.EntityCfgManager.Value.GetTable(_context.ElementType);
            tableName = table.Name;
            var tableFullName = GetTableName(table);
            if (_context.Conditions.Any())
            {
                where = BuildWhere(_context.Conditions);
            }
            var sql = "DELETE FROM {0} {1}";
            sql = string.Format(sql, tableFullName, where);
            _result.CommandText = sql;
        }

        void BuildUpdateSql()
        {
            var where = string.Empty;
            var table = _dataContext.EntityCfgManager.Value.GetTable(_context.ElementType);
            var keyColumnName = string.Empty;
            var keyColumn = table.Columns.FirstOrDefault(x => x.Value.IsKey).Value;
            if (keyColumn != null)
            {
                keyColumnName = keyColumn.PropertyInfo.Name;
            }
            tableName = table.Name;
            var tableFullName = GetTableName(table);
            if (_context.Conditions.Any())
            {
                where = BuildWhere(_context.Conditions);
            }
            var setts = new List<string>();
            var alias = string.Empty;
            foreach (var key in _context.UpdateResult.Keys)
            {
                if (key == keyColumnName)
                {
                    continue;
                }
                alias = ParserUtils.GenerateAlias(key);
                var set = string.Format("{0} = @{1}", GetTableName(key), alias);
                _result.Parameters.Add(alias, _context.UpdateResult.GetOrDefault(key));
                setts.Add(set);
            }
            var sql = "UPDATE {0} SET {1} {2}";
            sql = string.Format(sql, tableFullName, string.Join(",", setts), where);
            _result.CommandText = sql;
        }

        public ParseResult BuildSql(BuilderContext context)
        {
            _context = context;
            _result = new ParseResult();
            switch (context.SqlType)
            {
                case SqlType.Delete:
                    BuildDeleteSql();
                    break;
                case SqlType.Update:
                    BuildUpdateSql();
                    break;
                case SqlType.Select:
                    BuildSelectSql();
                    break;
                default:
                    throw new Exception();
            }
            return _result;
        }
        protected string FormatColumn(Column column, bool genColumnAlias = true)
        {
            var tblAlias = GetTableAlias(column);
            string col = string.Format("{0}.{1}", GetTableName(tblAlias), GetTableName(column.Name));
            var converter = ParseConverter(column);
            if (!string.IsNullOrWhiteSpace(converter))
            {
                col = string.Format(converter, string.Format("{0}.{1}", GetTableName(tblAlias), GetTableName(column.Name)));
            }
            if (!genColumnAlias)
            {
                return col;
            }
            return string.Format("{0} {1}" + Environment.NewLine, col, GetTableName(column.Alias ?? column.MemberInfo.Name));
        }

        protected virtual string FormatSelectString(List<Column> columns)
        {
            if (_context.AggregationColumns.Count >= 1)
            {
                _context.SortColumns.Clear();
                var aggrColumn = _context.AggregationColumns.FirstOrDefault();
                var columnString = string.Empty;
                if (aggrColumn.Value != null)
                {
                    columnString = FormatColumn(aggrColumn.Value, false);
                }
                switch (aggrColumn.Key)
                {
                    case "Count":
                        if (aggrColumn.Value == null)
                        {
                            return ("Count(1)");
                        }
                        return string.Format("Count({0})", columnString);
                    case "Sum":
                        return string.Format("Sum({0})", columnString);
                    case "Average":
                        return string.Format("AVG({0})", columnString);
                    default:
                        throw new Exception(aggrColumn.Key);
                }
            }
            else
            {
                List<string> fields = new List<string>();
                foreach (var column in columns)
                {
                    string col = FormatColumn(column);
                    fields.Add(col);
                }
                return string.Join(",", fields);
            }
        }

        protected string FormatSortColumns()
        {
            var sortBuilder = new StringBuilder();
            if (_context.SortColumns.Any())
            {
                sortBuilder.Append("ORDER BY ");
                var sorts = new List<string>();
                foreach (var sortColumn in _context.SortColumns)
                {
                    var col = string.Format("{0}.{1}", GetTableName(GetTableAlias(sortColumn.Value)), GetTableName(sortColumn.Value.Alias ?? sortColumn.Value.Name));
                    var converter = ParseConverter(sortColumn.Value);
                    if (!string.IsNullOrWhiteSpace(converter))
                    {
                        col = string.Format(converter, col);
                    }
                    sorts.Add(col);
                }
                sortBuilder.Append(string.Join(",", sorts));
            }
            return sortBuilder.ToString();
        }

        protected string BuildWhere(IList<Token> conditions)
        {
            var whereBuilder = new StringBuilder();
            if (conditions.Any())
            {
                var filters = new List<string>();
                foreach (var condition in conditions)
                {
                    var filter = StartBuildCondition(condition);
                    if (condition.Type == TokenType.Column && TypeHelper.GetUnderlyingType(condition.Column.DataType) == ReflectorConsts.BoolType)
                    {
                        filter += " = 1";
                    }
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        continue;
                    }
                    filters.Add(filter);
                }
                if (filters.Any())
                {
                    whereBuilder.Append("WHERE ");
                    whereBuilder.Append(string.Join(" AND ", filters));
                }
            }
            return whereBuilder.ToString();
        }
        protected string BuildCondition(Condition condition)
        {
            var left = condition.Left;
            var result = string.Empty;
            switch (condition.CompareType)
            {
                case CompareType.Not:
                    result = BuildToken(left);
                    if (left.Type == TokenType.Column)
                    {
                        if (TypeHelper.GetUnderlyingType(left.Column.DataType) == ReflectorConsts.BoolType)
                        {
                            result += " = 0";
                            return result;
                        }
                    }
                    return string.Format("NOT ({0})", result);
                default:
                    string leftStr = BuildToken(left);
                    string rightStr = BuildToken(condition.Right);
                    if (leftStr == null)
                    {
                        return string.Format("{0} IS NULL", rightStr);
                    }
                    else if (rightStr == null)
                    {
                        return string.Format("{0} IS NULL", leftStr);
                    }
                    if (left.Type == TokenType.Column && TypeHelper.GetUnderlyingType(left.Column.DataType) == ReflectorConsts.BoolType)
                    {
                        leftStr += " = 1";
                        //var rightObject = Convert.ToBoolean(condition.Right.Object);
                        //var rightBoolString = string.Empty;
                        //if (rightObject)
                        //{
                        //    rightBoolString = "1";
                        //}
                        //else
                        //{
                        //    rightBoolString = "0";
                        //}
                    }
                    if (condition.Right.Type == TokenType.Column && TypeHelper.GetUnderlyingType(condition.Right.Column.DataType) == ReflectorConsts.BoolType)
                    {
                        rightStr += " = 1";
                    }
                    return string.Format("({0} {1} {2})", leftStr, SelectOperation(condition.CompareType), rightStr);

            }
        }

        protected string BuildColumn(Column column)
        {
            var col = string.Format("{0}.{1}", GetTableName(GetTableAlias(column)), GetTableName(column.Name));
            var convert = string.Empty;
            if (column.Converters.Any())
            {
                convert = ParseConverter(column);
                if (string.IsNullOrWhiteSpace(convert))
                {
                    return "1=2";
                }
                col = string.Format(convert, col);
            }
            return col;
        }

        protected string StartBuildCondition(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Condition:
                    return BuildCondition(token.Condition);
                case TokenType.Column:
                    var result = BuildColumn(token.Column);
                    //if (TypeHelper.GetUnderlyingType(token.Column.DataType) == ReflectorConsts.BoolType)
                    //{
                    //    result += " = 1";
                    //}
                    return result;
                case TokenType.Object:
                    if (token.Object == null)
                    {
                        return null;
                    }
                    if (token.IsBool())
                    {
                        if (!token.GetBool())
                        {
                            return "1=2";
                        }
                        return null;
                    }
                    var paramName = ParserUtils.GenerateAlias("param");
                    _result.Parameters.Add(paramName, token.Object);
                    return "@" + paramName;
                default:
                    throw new Exception();
            }
            throw new Exception();
        }
        protected string BuildToken(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Condition:
                    return BuildCondition(token.Condition);
                case TokenType.Column:
                    var result = BuildColumn(token.Column);
                    //if (TypeHelper.GetUnderlyingType(token.Column.DataType) == ReflectorConsts.BoolType)
                    //{
                    //    result += " = 1";
                    //}
                    return result;
                case TokenType.Object:
                    if (token.Object == null)
                    {
                        return null;
                    }
                    if (token.IsBool())
                    {
                        if (!token.GetBool())
                        {
                            return "0";
                        }
                        return "1";
                    }
                    var paramName = ParserUtils.GenerateAlias("param");
                    _result.Parameters.Add(paramName, token.Object);
                    return "@" + paramName;
                default:
                    throw new Exception();
            }
            throw new Exception();
        }
        protected string SelectOperation(CompareType compareType)
        {
            switch (compareType)
            {
                case CompareType.And:
                    return "AND";
                case CompareType.Equal:
                    return "=";
                case CompareType.GreaterThan:
                    return ">";
                case CompareType.GreaterThanOrEqual:
                    return ">=";
                case CompareType.LessThan:
                    return "<";
                case CompareType.LessThanOrEqual:
                    return "<=";
                case CompareType.Or:
                    return "OR";
                case CompareType.Add:
                    return "+";
                case CompareType.Substarct:
                    return "-";
                case CompareType.Multiply:
                    return "*";
                case CompareType.Divide:
                    return "/";
                case CompareType.NotEqual:
                    return "<>";
                default:
                    throw new Exception();
            }
        }

        protected string GetTableAlias(Column column)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                return tableName;
            }

            if (!string.IsNullOrWhiteSpace(column.Table.Alias))
            {
                return column.Table.Alias;
            }
            var columnName = column.Name;
            foreach (var join in _context.Joins.Values)
            {
                if (join.Left.Name == columnName)
                {
                    return join.Left.Table.Alias;
                }
                else if (join.Right.Name == columnName)
                {
                    return join.Right.Table.Alias;
                }
                var tableInfo =_dataContext.EntityCfgManager.Value.GetTable(join.Left.Table.Type);
                foreach (var columnItem in tableInfo.Columns.Values)
                {
                    if (columnItem.Name == columnName)
                    {
                        return join.Left.Table.Name == tableInfo.Name ? join.Left.Table.Alias : join.Right.Table.Alias;
                    }
                }
                tableInfo = _dataContext.EntityCfgManager.Value.GetTable(join.Right.Table.Type);
                foreach (var columnItem in tableInfo.Columns.Values)
                {
                    if (columnItem.Name == columnName)
                    {
                        return join.Left.Table.Name == tableInfo.Name ? join.Left.Table.Alias : join.Right.Table.Alias;
                    }
                }
            }
            throw new Exception();
        }

        protected string GenJoinType(JoinType joinType)
        {
            if (joinType == JoinType.Inner)
            {
                return (" INNER JOIN ");
            }
            else if (joinType == JoinType.Left)
            {
                return (" Left JOIN ");
            }
            else
            {
                throw new NotSupportedException("未支持的Join类型：" + joinType);
            }
        }

        public abstract string GetTableName(SchemaModel.Table table);
        public abstract string GetTableName(Table leftTable);
        public virtual string GetTableName(string tableName)
        {
            return string.Format("[{0}]", tableName);
        }
        public abstract string ParseConverter(Column column);
    }
}
