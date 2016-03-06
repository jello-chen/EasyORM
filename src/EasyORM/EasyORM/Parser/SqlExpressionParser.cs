using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Provider.Parser
{
    internal class SqlExpressionParser : ExpressionVisitor
    {
        DataContext _dataContext;
        public SqlExpressionParser(DataContext context)
        {
            _dataContext = context;
        }
        /// <summary>
        /// 将给定的表结构信息转换成完整表名
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string GetTableName(SchemaModel.Table table)
        {
            var tableName = string.Empty;
            tableName = string.Format("{0}[{1}]", tableName, table.Name);
            return tableName;
        }
        /// <summary>
        /// 将给定的表结构信息转换成完整表名
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string GetTableName(TranslateModel.Table table)
        {
            var tableName = string.Empty;
            tableName = string.Format("{0}[{1}]", tableName, table.Name);
            return tableName;
        }
        //SqlServer2008R2Provider provider = null;
        bool _distinct = false, _isCallAny = false, _isDelete, _isUpdate;
        List<KeyValuePair<string, Expression>> _sortExpressions;
        Dictionary<string, Expression> _aggregationExpressions;
        Dictionary<string, Column> _aggregationColumns;
        List<KeyValuePair<string, Column>> _sortColumns;
        public Dictionary<string, object> UpdateResult { get; private set; }

        int _take = -1, _skip = -1;

        public bool IsUpdate
        {
            get
            {
                return _isUpdate;
            }
        }

        public bool IsCallAny
        {
            get
            {
                return _isCallAny;
            }
        }
        public bool IsDelete
        {
            get
            {
                return _isDelete;
            }
        }
        public int Skip
        {
            get
            {
                return _skip;
            }
        }

        public int Take
        {
            get { return _take; }
        }

        public Dictionary<string, Column> AggregationColumns
        {
            get { return _aggregationColumns; }
        }

        public List<KeyValuePair<string, Column>> SortColumns
        {
            get
            {
                return _sortColumns;
            }
        }

        public bool Distinct
        {
            get { return _distinct; }
        }
        public Type ElementType { get; set; }
        public ParseResult Result { get; private set; }
        public List<Column> Columns
        {
            get;
            private set;
        }

        public List<string> NoLockTables { get; private set; }

        public IList<Token> Conditions { get; private set; }
        List<Expression> _nolockExpressions;
        Expression _selectExpression;
        List<KeyValuePair<string, Expression>> _expression = new List<KeyValuePair<string, Expression>>();
        List<Expression> _whereExpressions = new List<Expression>();
        List<Expression> _joinExpressions = new List<Expression>();
        Expression _updateExpression;
        private TranslateContext _context;
        public Dictionary<string, Join> Joins
        {
            get;
            private set;
        }
        public SqlExpressionParser()
        {
        }

        internal void Parse(Expression expression)
        {
            _distinct = false;
            _isCallAny = false;
            _isDelete = false;
            _isUpdate = false;
            UpdateResult = new Dictionary<string, object>();
            _aggregationExpressions = new Dictionary<string, Expression>();
            Conditions = new List<Token>();
            _nolockExpressions = new List<Expression>();
            NoLockTables = new List<string>();
            _aggregationColumns = new Dictionary<string, Column>();
            _sortExpressions = new List<KeyValuePair<string, Expression>>();
            _sortColumns = new List<KeyValuePair<string, Column>>();
            _context = new TranslateContext();
            _context.DataContext = _dataContext;
            _take = -1;
            _skip = -1;
            _context.EntityType = ElementType;
            Visit(expression);
            if (_aggregationColumns.Count > 1)
            {
                throw new Exception();
            }
            if (_skip != -1 && _take != -1 && !_sortExpressions.Any())
            {
                throw new Exception("分页必须进行排序");
            }

            #region 解析Join子句
            foreach (MethodCallExpression node in _joinExpressions)
            {
                VisitJoinExpression(node);
                break;
            }
            #endregion

            #region 解析Select子句
            if (_selectExpression != null)
            {
                var unary = _selectExpression as UnaryExpression;
                var lambdaExp = unary.Operand as LambdaExpression;
                //var newExp = lambdaExp.Body as NewExpression;
                //if (newExp == null)
                //{
                //    throw new NotSupportedException("Select子句中只能使用new表达式");
                //}
                VisitSelectExpression(lambdaExp.Body);
            }
            else
            {
                VisitSelectExpression(null);
            }
            #endregion


            #region 解析Where子句
            foreach (MethodCallExpression node in _whereExpressions)
            {
                var unary = node.Arguments[1] as UnaryExpression;
                var operand = unary.Operand as LambdaExpression;
                var body = operand.Body;
                if (body is ConstantExpression)
                {
                    var constExp = body as ConstantExpression;
                    if ((bool)constExp.Value == false)
                    {
                        Conditions.Add(Token.Create(false));
                    }
                }
                else
                {
                    VisitWhereExpression(body);
                }
            }
            #endregion

            #region 解析Lock子句
            foreach (var nolockExpression in _nolockExpressions)
            {
                VisitNoLockExpression(nolockExpression);
            }
            #endregion

            #region 解析Sum、Avg等子句
            foreach (var aggreationExpression in _aggregationExpressions)
            {
                VisitAggreationExpression(aggreationExpression);
            }
            #endregion

            #region 解析Order By子句
            foreach (var sortExpression in _sortExpressions)
            {
                VisitSortExpression(sortExpression);
            }
            #endregion

            VisitUpdateExpression(_updateExpression);
        }

        private void VisitUpdateExpression(Expression updateExpression)
        {
            if (updateExpression == null)
            {
                return;
            }
            var visitor = new MemberExpressionVisitor(_context);
            visitor.Visit(updateExpression);
            if (visitor.Token.Type != TokenType.Object)
            {
                throw new NotSupportedException("不支持");
            }
            UpdateResult = (Dictionary<string, object>)visitor.Token.Object;
        }

        private void VisitSortExpression(KeyValuePair<string, Expression> sortExpression)
        {
            var visitor = new MemberExpressionVisitor(_context);
            visitor.Visit(sortExpression.Value);
            if (visitor.Token.Type == TokenType.Column)
            {
                _sortColumns.Add(new KeyValuePair<string, Column>(sortExpression.Key, visitor.Token.Column));
                return;
            }
            throw new Exception();
        }

        private void VisitAggreationExpression(KeyValuePair<string, Expression> aggreationExpression)
        {
            if (aggreationExpression.Value != null)
            {
                var visitor = new MemberExpressionVisitor(_context);
                visitor.Visit(aggreationExpression.Value);
                if (visitor.Token.Type != TokenType.Column)
                {
                    throw new Exception("只能针对列进行聚合操作");
                }
                _aggregationColumns.Add(aggreationExpression.Key, visitor.Token.Column);
            }
            else
            {
                _aggregationColumns.Add(aggreationExpression.Key, null);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            while (true)
            {
                switch (node.Method.Name)
                {
                    case "Select":
                        _selectExpression = node.Arguments[1];
                        break;
                    case "Where":
                        _whereExpressions.Add(node);
                        break;
                    case "Join":
                    case "GroupJoin":
                    case "SelectMany":
                        _joinExpressions.Add(node);
                        break;
                    case "FirstOrDefault":
                    case "First":
                        if (node.Arguments.Count > 1)
                        {
                            _whereExpressions.Add(node);
                        }
                        break;
                    case "DefaultIfEmpty":

                        break;
                    case "Distinct":
                        _distinct = true;
                        break;
                    case "NoLock":
                        _nolockExpressions.Add(node);
                        break;
                    case "Count":
                    case "LongCount":
                    case "Sum":
                    case "Average":
                        Expression exp = null;
                        if (node.Arguments.Count >= 2)
                        {
                            if (node.Method.Name == "Count")
                            {
                                _whereExpressions.Add(node);
                            }
                            else
                            {
                                exp = node.Arguments[1];
                            }
                        }
                        _aggregationExpressions.Add(node.Method.Name, exp);
                        break;
                    case "OrderByDescending":
                    case "ThenByDescending":
                        _sortExpressions.Add(new KeyValuePair<string, Expression>("DESC", node.Arguments[1]));
                        break;
                    case "OrderBy":
                    case "ThenBy":
                        _sortExpressions.Add(new KeyValuePair<string, Expression>("ASC", node.Arguments[1]));
                        break;
                    case "Take":
                        var takeExp = node.Arguments[1];
                        if (takeExp.NodeType == ExpressionType.Constant)
                        {
                            _take = Convert.ToInt32(((ConstantExpression)takeExp).Value);
                        }
                        else
                        {
                            var memVisitor = new MemberExpressionVisitor(_context);
                            memVisitor.Visit(takeExp);
                            _take = Convert.ToInt32(memVisitor.Token.Object);
                        }
                        break;
                    case "Skip":
                        var skipExp = node.Arguments[1];
                        if (skipExp.NodeType == ExpressionType.Constant)
                        {
                            _skip = Convert.ToInt32(((ConstantExpression)skipExp).Value);
                        }
                        else
                        {
                            var memVisitor = new MemberExpressionVisitor(_context);
                            memVisitor.Visit(skipExp);
                            _skip = Convert.ToInt32(memVisitor.Token.Object);
                        }
                        break;
                    case "Any":
                        _isCallAny = true;
                        if (node.Arguments.Count > 1)
                        {
                            _whereExpressions.Add(node);
                        }
                        break;
                    case "Delete":
                        _isDelete = true;
                        if (node.Arguments.Count > 1)
                        {
                            _whereExpressions.Add(node);
                        }
                        break;
                    case "Update":
                        _isUpdate = true;
                        _updateExpression = node.Arguments[1];
                        break;
                    default:
                        throw new NotSupportedException("未支持的方法：" + node.Method.Name);
                }
                var call = node.Arguments[0];
                if (call.NodeType == ExpressionType.Call)
                {
                    node = (MethodCallExpression)call;
                    continue;
                }
                break;
            }
            return node;
        }

        private void VisitJoinExpression(MethodCallExpression node)
        {
            var visitor = new JoinExpressionVisitor(_context);
            visitor.Visit(node);
            Joins = visitor.Joins;
            if (_selectExpression == null)
                _selectExpression = visitor.SelectExpression;
        }

        private void VisitWhereExpression(Expression binaryExpression)
        {
            var visitor = new WhereExpressionVisitor(_context);
            visitor.Visit(binaryExpression);
            Conditions.Add(visitor.Token);
        }



        private void VisitSelectExpression(Expression node)
        {
            var visitor = new SelectExpressionVisitor(_context);
            visitor.Visit(node);
            Columns = visitor.Columns;
        }

        private void VisitNoLockExpression(Expression node)
        {
            var visitor = new NoLockExpressionVisitor(_context);
            visitor.Visit(node);
            NoLockTables.Add(visitor.ExtraObject.ToString());
        }


    }
}
