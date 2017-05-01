using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using EasyORM.Configuration;
using EasyORM.Utils;

namespace EasyORM.Provider.Parser
{
    /// <summary>
    /// Exprssion visitor for property or field
    /// </summary>
    public class PropertyFieldExpressionVisitor : ExpressionVisitorBase
    {
        Dictionary<string, Join> _joins;

        public PropertyFieldExpressionVisitor(TranslateContext context)
            : base(context)
        {
            // TODO: Complete member initialization
            this._joins = context.Joins;
            this._columns = context.Columns;
        }

        public override Expression Visit(Expression node)
        {
            var sepcNode = node;
            if (sepcNode.NodeType == ExpressionType.Quote)
            {
                sepcNode = ((UnaryExpression)sepcNode).Operand;
            }
            if (sepcNode.NodeType == ExpressionType.Lambda)
            {
                return base.Visit(((LambdaExpression)sepcNode).Body);
            }
            return base.Visit(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var results = new Dictionary<string, object>();
            foreach (MemberAssignment binding in node.Bindings)
            {
                var visitor = new MemberExpressionVisitor(Context);
                visitor.Visit(binding.Expression);
                if (visitor.Token.Type != TokenType.Object)
                {
                    throw new NotSupportedException("Not supported the type");
                }
                results.Add(binding.Member.Name, visitor.Token.Object);
            }
            Token = Token.Create(results);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            MethodCallExpressionVisitor visitor = new MethodCallExpressionVisitor(Context);
            visitor.Visit(node);
            var type = visitor.Token.Type;
            if (type == TokenType.Column)
            {
                Token = visitor.Token;
                var column = Token.Column;
                while (_memberInfos.Count > 0)
                {
                    var exp = _memberInfos.Pop();
                    switch (exp.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            var memberInfo = ((MemberExpression)exp).Member;
                            column.Converters.Push(new ColumnConverter(memberInfo, new List<object>()));
                            break;
                        default:
                            throw new Exception();
                    }
                }
                //column.Converter = GetConverter(column.Converter);
            }
            else if (type == TokenType.Condition)
            {
                throw new Exception();
            }
            else
            {
                var obj = visitor.Token.Object;
                while (_memberInfos.Count > 0)
                {
                    obj = GetValue((MemberExpression)_memberInfos.Pop(), obj);
                }
                Token = Token.Create(obj);
            }
            return node;
        }

        public object GetValue(MemberExpression memberExpression, object obj)
        {
            switch (memberExpression.NodeType)
            {
                case ExpressionType.MemberAccess:

                    if (memberExpression.Member.MemberType == MemberTypes.Field)
                    {
                        return ((FieldInfo)memberExpression.Member).GetValue(obj);
                    }
                    return ((PropertyInfo)memberExpression.Member).GetValue(obj);
            }
            throw new Exception();
        }

        Stack<Expression> _memberInfos = new Stack<Expression>();
        private MemberInfo _tableMember;
        private Dictionary<string, Column> _columns;
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == null)
            {
                var value = GetValue(node, null);
                while (_memberInfos.Count > 0)
                {
                    value = GetValue((MemberExpression)_memberInfos.Pop(), value);
                }
                Token = Token.Create(value);
                return node;
            }
            if (node.Member.DeclaringType == typeof(TimeSpan))
            {
                if (node.Expression.NodeType == ExpressionType.Subtract)
                {
                    var binaryExp = (BinaryExpression)node.Expression;
                    var left = binaryExp.Left;
                    var right = binaryExp.Right;
                    var leftVisitor = new MemberExpressionVisitor(Context);
                    leftVisitor.Visit(left);
                    var rightVisitor = new MemberExpressionVisitor(Context);
                    rightVisitor.Visit(right);
                    if (leftVisitor.Token.Type == TokenType.Column && rightVisitor.Token.Type == TokenType.Object)
                    {
                        var rightObject = (DateTime)rightVisitor.Token.Object;
                        Token = leftVisitor.Token;
                        Token.Column.Converters.Push(new ColumnConverter(node.Member, new List<object>() { rightObject }, true));
                    }
                    else if (leftVisitor.Token.Type == TokenType.Object && rightVisitor.Token.Type == TokenType.Column)
                    {
                        var leftObject = (DateTime)leftVisitor.Token.Object;
                        Token = rightVisitor.Token;
                        Token.Column.Converters.Push(new ColumnConverter(node.Member, new List<object>() { leftObject }, false));
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
                return node;
            }
            _memberInfos.Push(node);
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            while (_memberInfos.Count > 0)
            {
                value = GetValue((MemberExpression)_memberInfos.Pop(), value);
            }
            Token = Token.Create(value);
            return base.VisitConstant(node);
        }

        SchemaModel.Table GetTable()
        {
            while (true)
            {
                var memExp = (MemberExpression)_memberInfos.Peek();
                _tableMember = memExp.Member;
                Type type = null;
                if (_tableMember.MemberType == MemberTypes.Field)
                {
                    type = ((FieldInfo)_tableMember).FieldType;
                }
                else
                {
                    type = ((PropertyInfo)_tableMember).PropertyType;
                }
                if (TypeHelper.IsCompilerGenerated(type))
                {
                    _memberInfos.Pop();
                    continue;
                }
                if (EntityConfigurationManager.IsEntity(type))
                {
                    _memberInfos.Pop();
                    return GetTable(type);
                }
                return null;
            }
        }

        SchemaModel.Table GetTable(Type tableType)
        {
            if (ParserUtils.IsAnonymousType(tableType))
            {
                _tableMember = ((MemberExpression)_memberInfos.Pop()).Member;
                tableType = ((PropertyInfo)_tableMember).PropertyType;
                return GetTable(tableType);
            }
            return DataContext.EntityCfgManager.Value.GetTable(tableType);
        }


        Table CreateTable(string alias, string db, string name, Type type)
        {
            return new Table()
            {
                Alias = alias,
                DataBase = db,
                Name = name,
                Type = type
            };
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var column = new Column();
            if (EntityConfigurationManager.IsEntity(node.Type))
            {
                //Pop the first parameter, it is usually column
                var table = GetTable(node.Type);
                var _memberInfo = ((MemberExpression)_memberInfos.Pop()).Member;
                column.DataType = ((PropertyInfo)_memberInfo).PropertyType;
                column.Name = table.Columns.GetOrDefault(_memberInfo.Name).Name;
                column.MemberInfo = _memberInfo;
                var tableAlias = node.Name;
                if (_joins != null)
                {
                    if (_joins.ContainsKey(tableAlias))
                    {
                        tableAlias = _joins[tableAlias].Right.Table.Alias;
                    }
                }
                else
                {
                    tableAlias = table.Name;
                }
                column.Table = CreateTable(tableAlias, table.DataBase, table.Name, table.Type);
                while (_memberInfos.Count > 0)
                {
                    var exp = _memberInfos.Pop();
                    switch (exp.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            var memberInfo = ((MemberExpression)exp).Member;
                            column.Converters.Push(new ColumnConverter(memberInfo, new List<object>()));
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
            else
            {
                SchemaModel.Table tableInfo = GetTable();
                MemberInfo columnMember = null;
                var tableAlias = string.Empty;
                Table table = null;
                if (tableInfo != null)
                {
                    tableAlias = _tableMember.Name;
                    columnMember = ((MemberExpression)_memberInfos.Pop()).Member;
                    if (_joins != null)
                    {
                        if (_joins.ContainsKey(tableAlias))
                        {
                            tableAlias = _joins[tableAlias].Right.Table.Alias;
                        }
                    }
                    else
                    {
                        tableAlias = tableInfo.Name;
                    }
                    table = CreateTable(tableAlias, tableInfo.DataBase, tableInfo.Name, tableInfo.Type);
                }
                else
                {
                    columnMember = ((MemberExpression)_memberInfos.Pop()).Member;
                    table = _columns.GetOrDefault(columnMember.Name).Table;
                    tableAlias = table.Alias;
                    tableInfo = GetTable(table.Type);
                }
                var columnType = ((PropertyInfo)columnMember).PropertyType;
                var columnName = string.Empty;
                var columnSechma = tableInfo.Columns.GetOrDefault(columnMember.Name);
                if (columnSechma != null)
                {
                    columnName = columnSechma.Name;
                }
                else
                {
                    columnName = Context.Columns.GetOrDefault(columnMember.Name).Name;
                }
                column = new Column()
                {
                    DataType = columnType,
                    Name = columnName,
                    Table = table,
                    MemberInfo = columnMember
                };
                while (_memberInfos.Count > 0)
                {
                    var exp = _memberInfos.Pop();
                    switch (exp.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            var memberInfo = ((MemberExpression)exp).Member;
                            column.Converters.Push(new ColumnConverter(memberInfo, new List<object>()));
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
            Token = Token.Create(column);
            return node;
        }
    }
}
