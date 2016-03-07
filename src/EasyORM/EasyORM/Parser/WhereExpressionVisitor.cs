using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using EasyORM.Utils;

namespace EasyORM.Provider.Parser
{
    public class WhereExpressionVisitor : ExpressionVisitorBase
    {
        Dictionary<string, Join> _joins;
        public WhereExpressionVisitor(TranslateContext context)
            : base(context)
        {
            this._joins = context.Joins;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            BinaryExpressionVisitor visitor = new BinaryExpressionVisitor(Context);
            visitor.Visit(node);
            Token = visitor.Token;
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            MemberExpressionVisitor visitor = new MemberExpressionVisitor(Context);
            visitor.Visit(node);
            Token = visitor.Token;
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    var oper = node.Operand;
                    ExpressionVisitorBase visitor = null;
                    switch (oper.NodeType)
                    {
                        case ExpressionType.Call:
                            visitor = new MethodCallExpressionVisitor(Context);
                            break;
                        case ExpressionType.MemberAccess:
                            visitor = new MemberExpressionVisitor(Context);
                            break;
                        case ExpressionType.OrElse:
                        case ExpressionType.AndAlso:
                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                        case ExpressionType.Divide:
                            visitor = new BinaryExpressionVisitor(Context);
                            break;
                        default:
                            throw new Exception();
                    }
                    visitor.Visit(oper);
                    Token = visitor.Token;
                    if (Token.IsBool())
                    {
                        Token = Token.Create(!((bool)Token.Object));
                    }
                    else if (Token.Type == TokenType.Column)
                    {
                        Token = Token.Create(new Condition()
                        {
                            CompareType = CompareType.Not,
                            Left = Token
                        });
                    }
                    else if (Token.Type == TokenType.Condition)
                    {
                        Token = Token.Create(new Condition()
                        {
                            CompareType = CompareType.Not,
                            Left = Token
                        });
                    }
                    else if (Token.Type == TokenType.Object)
                    {
                        Token = Token.Create(true);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    break;
                default:
                    throw new Exception();
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitor = new MethodCallExpressionVisitor(Context);
            visitor.Visit(node);
            Token = visitor.Token;
            return node;
        }

        class BinaryExpressionVisitor : ExpressionVisitorBase
        {
            Dictionary<string, Join> _joins;
            public BinaryExpressionVisitor(TranslateContext context)
                : base(context)
            {
                this._joins = context.Joins;
            }
            public override Expression Visit(Expression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Constant:
                        Token = Token.Create((((ConstantExpression)node)).Value);
                        return node;
                    case ExpressionType.OrElse:
                    case ExpressionType.AndAlso:
                        Token = ParseLogicBinary((BinaryExpression)node);
                        return node;
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.NotEqual:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                        Token = ParseMathBinary((BinaryExpression)node);
                        return node;
                    case ExpressionType.Not:
                        Token = ParseNotExpression((UnaryExpression)node);
                        return node;
                    case ExpressionType.Call:
                        Token = ParseMethodCallExpression((MethodCallExpression)node);
                        return node;
                    default:
                        throw new Exception(node.NodeType.ToString());
                }
            }

            Token ParseBinaryExpressionLeftOrRight(BinaryExpression node)
            {
                Token result = null;
                if (node.Left is MemberExpression)
                {
                    MemberExpressionVisitor visitor = new MemberExpressionVisitor(Context);
                    visitor.Visit(node.Left);
                    result = visitor.Token;
                    switch (result.Type)
                    {
                        case TokenType.Column:
                            if (EasyORM.Utils.TypeHelper.GetUnderlyingType(result.Column.DataType) == EasyORM.Utils.ReflectorConsts.BoolType)
                            {
                                //IsEnabled && CreateTime==DateTime.Now
                                var left = result;
                                var right = Token.Create(1);
                                var condition = new Condition();
                                condition.CompareType = CompareType.Equal;
                                condition.Left = left;
                                condition.Right = right;
                                result = Token.Create(condition);
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;
                        default:
                            throw new Exception();
                    }
                }
                else
                {
                    result = VisitBinaryNode(node.Left);
                }
                return result;
            }

            private Token ParseMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                var visitor = new MethodCallExpressionVisitor(Context);
                visitor.Visit(methodCallExpression);
                return visitor.Token;
            }

            Token ParseNotExpression(UnaryExpression unaryExpression)
            {
                ExpressionVisitorBase visitor;
                var operand = unaryExpression.Operand;
                switch (operand.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.MemberInit:
                        visitor = new MemberExpressionVisitor(Context);
                        break;
                    case ExpressionType.Call:
                        visitor = new MethodCallExpressionVisitor(Context);
                        break;
                    //visitor.Visit(operand);
                    //var token = visitor.Token;
                    //switch (token.Type)
                    //{
                    //    case TokenType.Column:
                    //        return Token.Create(new Condition()
                    //        {
                    //            Left = token,
                    //            CompareType = CompareType.Not
                    //        });
                    //    case TokenType.Condition:
                    //        return Token.Create(new Condition()
                    //        {
                    //            Left = Token.Create(token.Condition),
                    //            CompareType = CompareType.Not
                    //        });
                    //    case TokenType.Object:
                    //        return Token.Create(!((bool)token.Object));
                    //    default:
                    //        throw new Exception();
                    //}
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.NotEqual:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.OrElse:
                    case ExpressionType.AndAlso:
                        visitor = new BinaryExpressionVisitor(Context);
                        break;
                    default:
                        throw new Exception();
                }
                visitor.Visit(operand);
                switch (visitor.Token.Type)
                {
                    case TokenType.Object:
                        if (visitor.Token.IsBool())
                        {
                            return Token.Create(!((bool)visitor.Token.Object));
                        }
                        else
                        {
                            throw new Exception("Not support the type");
                        }
                    case TokenType.Condition:
                        return Token.Create(new Condition()
                        {
                            Left = visitor.Token,
                            CompareType = CompareType.Not
                        });
                    case TokenType.Column:
                        if (operand.Type == typeof(bool) || operand.Type == typeof(bool?))
                        {
                            return Token.Create(new Condition()
                            {
                                CompareType = CompareType.Equal,
                                Left = Token.Create(1),
                                Right = Token.Create(1)
                            });
                        }
                        return Token.Create(new Condition()
                        {
                            Left = visitor.Token,
                            CompareType = CompareType.Not
                        });
                    default:
                        throw new Exception();
                }
                throw new Exception();
            }

            Token ParseMathBinary(BinaryExpression node)
            {
                ExpressionVisitorBase leftVisitor, rightVisitor;
                if (node.Left is BinaryExpression)
                {
                    leftVisitor = new BinaryExpressionVisitor(Context);
                }
                else
                {
                    leftVisitor = new MemberExpressionVisitor(Context);
                }
                leftVisitor.Visit(node.Left);
                var leftResult = leftVisitor.Token;
                if (node.Right is BinaryExpression)
                {
                    rightVisitor = new BinaryExpressionVisitor(Context);
                }
                else
                {
                    rightVisitor = new MemberExpressionVisitor(Context);
                }
                rightVisitor.Visit(node.Right);
                var rightResult = rightVisitor.Token;
                if (leftResult.Type == TokenType.Object && rightResult.Type == TokenType.Object)
                {
                    if (leftResult.Object == null && rightResult.Object == null)
                    {
                        return Token.Create(true);
                    }
                    if (leftResult == null || rightResult == null)
                    {
                        return Token.Create(false);
                    }
                    if (leftResult.Type == TokenType.Object)
                    {
                        if (leftResult.Object is string || leftResult.Object is bool || leftResult.Object is bool?)
                        {
                            return Token.Create(leftResult.Equals(rightResult));
                        }
                        #region Comparsion
                        if (leftResult.Object is DateTime || leftResult.Object is DateTime?)
                        {
                            var left = Convert.ToDateTime(leftResult.Object);
                            var right = Convert.ToDateTime(rightResult.Object);
                            switch (node.NodeType)
                            {
                                case ExpressionType.LessThan:
                                    return Token.Create(left < right);
                                case ExpressionType.LessThanOrEqual:
                                    return Token.Create(left <= right);
                                case ExpressionType.Equal:
                                    return Token.Create(left = right);
                                case ExpressionType.GreaterThan:
                                    return Token.Create(left > right);
                                case ExpressionType.GreaterThanOrEqual:
                                    return Token.Create(left >= right);
                                default:
                                    throw new Exception();
                            }
                        }
                        else if (leftResult.Object is int || leftResult.Object is int?)
                        {
                            var left = Convert.ToInt32(leftResult.Object);
                            var right = Convert.ToInt32(rightResult.Object);
                            switch (node.NodeType)
                            {
                                case ExpressionType.LessThan:
                                    return Token.Create(left < right);
                                case ExpressionType.LessThanOrEqual:
                                    return Token.Create(left <= right);
                                case ExpressionType.Equal:
                                    return Token.Create(left = right);
                                case ExpressionType.GreaterThan:
                                    return Token.Create(left > right);
                                case ExpressionType.GreaterThanOrEqual:
                                    return Token.Create(left >= right);
                                default:
                                    throw new Exception();
                            }
                        }
                        else if (leftResult.Object is short || leftResult.Object is short?)
                        {
                            var left = Convert.ToInt16(leftResult.Object);
                            var right = Convert.ToInt16(rightResult.Object);
                            switch (node.NodeType)
                            {
                                case ExpressionType.LessThan:
                                    return Token.Create(left < right);
                                case ExpressionType.LessThanOrEqual:
                                    return Token.Create(left <= right);
                                case ExpressionType.Equal:
                                    return Token.Create(left = right);
                                case ExpressionType.GreaterThan:
                                    return Token.Create(left > right);
                                case ExpressionType.GreaterThanOrEqual:
                                    return Token.Create(left >= right);
                                default:
                                    throw new Exception();
                            }
                        }
                        else if (leftResult.Object is long || leftResult.Object is long?)
                        {
                            var left = Convert.ToInt64(leftResult.Object);
                            var right = Convert.ToInt64(rightResult.Object);
                            switch (node.NodeType)
                            {
                                case ExpressionType.LessThan:
                                    return Token.Create(left < right);
                                case ExpressionType.LessThanOrEqual:
                                    return Token.Create(left <= right);
                                case ExpressionType.Equal:
                                    return Token.Create(left = right);
                                case ExpressionType.GreaterThan:
                                    return Token.Create(left > right);
                                case ExpressionType.GreaterThanOrEqual:
                                    return Token.Create(left >= right);
                                default:
                                    throw new Exception();
                            }
                        }
                        else
                        {
                            return Token.Create(leftResult.Object == rightResult.Object);
                        }
                        #endregion
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if ((leftResult.Type == TokenType.Column && rightResult.Type == TokenType.Object)
                    || (leftResult.Type == TokenType.Object && rightResult.Type == TokenType.Column))
                {
                    var condition = new Condition();
                    if (TypeHelper.GetUnderlyingType(node.Left.Type) == ReflectorConsts.BoolType)
                    {
                        if (rightResult.Object == null)
                        {
                            condition.Left = leftResult;
                            condition.Right = rightResult;
                            switch (node.NodeType)
                            {
                                case ExpressionType.Equal:
                                    condition.CompareType = CompareType.Equal;
                                    break;
                                case ExpressionType.NotEqual:
                                    condition.CompareType = CompareType.NotEqual;
                                    break;
                            }
                            return Token.Create(condition);
                        }
                        var right = (bool)rightResult.Object;
                        switch (node.NodeType)
                        {
                            case ExpressionType.Equal:
                                if (right)
                                {
                                    return leftResult;
                                }
                                return Token.Create(new Condition()
                                {
                                    CompareType = CompareType.Not,
                                    Left = leftResult
                                });
                            case ExpressionType.NotEqual:
                                if (right)
                                {
                                    return Token.Create(new Condition()
                                    {
                                        CompareType = CompareType.Not,
                                        Left = leftResult
                                    });
                                }
                                return leftResult;
                        }
                    }
                    condition.Left = leftResult;
                    condition.CompareType = SelectMathCompareType(node.NodeType);
                    condition.Right = rightResult;
                    return Token.Create(condition);
                }
                else if ((leftResult.Type == TokenType.Condition && rightResult.Type == TokenType.Object)
                    || (leftResult.Type == TokenType.Object && rightResult.Type == TokenType.Condition))
                {
                    var condition = new Condition();
                    condition.Left = leftResult;
                    condition.CompareType = SelectMathCompareType(node.NodeType);
                    condition.Right = rightResult;
                    return Token.Create(condition);
                }
                else
                {
                    throw new Exception();
                }
                throw new Exception();
            }

            private CompareType SelectMathCompareType(ExpressionType expressionType)
            {
                switch (expressionType)
                {
                    case ExpressionType.LessThan:
                        return CompareType.LessThan;
                    case ExpressionType.LessThanOrEqual:
                        return CompareType.LessThanOrEqual;
                    case ExpressionType.Equal:
                        return CompareType.Equal;
                    case ExpressionType.GreaterThan:
                        return CompareType.GreaterThan;
                    case ExpressionType.GreaterThanOrEqual:
                        return CompareType.GreaterThanOrEqual;
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return CompareType.Add;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return CompareType.Substarct;
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Multiply:
                        return CompareType.Multiply;
                    case ExpressionType.Divide:
                        return CompareType.Divide;
                    case ExpressionType.NotEqual:
                        return CompareType.NotEqual;
                    default:
                        throw new Exception();
                }
            }

            /// <summary>
            /// Parse the logical expression
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            Token ParseLogicBinary(BinaryExpression node)
            {
                Token leftResult = null, rightResult = null;
                leftResult = VisitBinaryNode(node.Left);
                if (leftResult.IsNull())
                {
                    return leftResult;
                }
                if (leftResult.IsBool())
                {
                    Token = leftResult;
                    var r = (bool)leftResult.Object;
                    if (r)
                    {
                        if (node.NodeType == ExpressionType.OrElse)
                        {
                            return Token.CreateNull();
                        }
                    }
                    else
                    {
                        if (node.NodeType == ExpressionType.AndAlso)
                        {
                            return Token.CreateNull();
                        }
                    }
                }
                rightResult = VisitBinaryNode(node.Right);
                if (leftResult.IsBool())
                {
                    if (rightResult.IsNull())
                    {
                        return leftResult;
                    }
                    else if (rightResult.IsBool())
                    {
                        switch (node.NodeType)
                        {
                            case ExpressionType.AndAlso:
                                return Token.Create((bool)rightResult.Object && (bool)leftResult.Object);
                            case ExpressionType.OrElse:
                                return Token.Create((bool)rightResult.Object || (bool)leftResult.Object);
                            default:
                                throw new Exception();
                        }
                    }
                    else if (rightResult.Type == TokenType.Condition || rightResult.Type == TokenType.Column)
                    {
                        var lr = (bool)leftResult.Object;
                        switch (node.NodeType)
                        {
                            case ExpressionType.AndAlso:
                                if (lr)
                                {
                                    return rightResult;
                                }
                                break;
                            case ExpressionType.OrElse:
                                if (!lr)
                                {
                                    return rightResult;
                                }
                                break;
                        }
                        return Token.CreateNull();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (leftResult.Type == TokenType.Condition)
                {
                    var left = (Condition)leftResult.Condition;
                    if (rightResult == null)
                    {
                        return Token.Create(left);
                    }
                    else if (rightResult.IsBool())
                    {
                        var rr = (bool)rightResult.Object;
                        switch (node.NodeType)
                        {
                            case ExpressionType.AndAlso:
                                if (rr)
                                {
                                    return Token.Create(left);
                                }
                                break;
                            case ExpressionType.OrElse:
                                if (!rr)
                                {
                                    return Token.Create(left);
                                }
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    else if (rightResult.Type == TokenType.Condition)
                    {
                        var condition = new Condition();
                        condition.Left = Token.Create(left);
                        condition.Right = rightResult;
                        condition.CompareType = SelectLogicCompareType(node.NodeType);
                        return Token.Create(condition);
                    }
                    else if (rightResult.Type == TokenType.Column)
                    {
                        var condition = new Condition();
                        condition.Left = Token.Create(left);
                        condition.Right = rightResult;
                        condition.CompareType = SelectLogicCompareType(node.NodeType);
                        return Token.Create(condition);
                    }
                    else
                    {
                        var leftToken = (Token)leftResult;
                        var rightToken = Token.Create(rightResult);
                        var condition = new Condition();
                        condition.Left = leftToken;
                        condition.Right = rightToken;
                        condition.CompareType = SelectLogicCompareType(node.NodeType);
                        return Token.Create(condition);
                    }
                }
                else if (leftResult.Type == TokenType.Column)
                {
                    if (rightResult == null)
                    {
                        return leftResult;
                    }
                    else if (rightResult.IsBool())
                    {
                        var rr = (bool)rightResult.Object;
                        switch (node.NodeType)
                        {
                            case ExpressionType.AndAlso:
                                if (rr)
                                {
                                    return leftResult;
                                }
                                break;
                            case ExpressionType.OrElse:
                                if (!rr)
                                {
                                    return leftResult;
                                }
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    else if (rightResult.Type == TokenType.Condition)
                    {
                        var condition = new Condition();
                        condition.Left = leftResult;
                        condition.Right = rightResult;
                        condition.CompareType = SelectLogicCompareType(node.NodeType);
                        return Token.Create(condition);
                    }
                    else if (rightResult.Type == TokenType.Column)
                    {
                        var condition = new Condition();
                        condition.Left = leftResult;
                        condition.Right = rightResult;
                        condition.CompareType = SelectLogicCompareType(node.NodeType);
                        return Token.Create(condition);
                    }
                    else
                    {
                        var leftToken = (Token)leftResult;
                        var rightToken = Token.Create(rightResult);
                        var condition = new Condition();
                        condition.Left = leftToken;
                        condition.Right = rightToken;
                        condition.CompareType = SelectLogicCompareType(node.NodeType);
                        return Token.Create(condition);
                    }
                }
                throw new Exception();
            }

            private CompareType SelectLogicCompareType(ExpressionType expressionType)
            {
                switch (expressionType)
                {
                    case ExpressionType.AndAlso:
                        return CompareType.And;
                    case ExpressionType.OrElse:
                        return CompareType.Or;
                    default:
                        throw new Exception();
                }
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                BinaryExpressionVisitor visitor = new BinaryExpressionVisitor(Context);
                visitor.Visit(node.Left);
                Token = visitor.Token;
                return node;
            }
            Token VisitBinaryNode(Expression node)
            {
                ExpressionVisitorBase visitor = null;
                switch (node.NodeType)
                {
                    case ExpressionType.Not:
                        var token = VisitBinaryNode(((UnaryExpression)node).Operand);
                        switch (token.Type)
                        {
                            case TokenType.Condition:
                                token = Token.Create(new Condition()
                                {
                                    Left = token,
                                    CompareType = CompareType.Not
                                });
                                break;
                            case TokenType.Object:
                                token = Token.Create(!((bool)token.Object));
                                break;
                            case TokenType.Column:
                                token = Token.Create(new Condition()
                                {
                                    Left = token,
                                    CompareType = CompareType.Not
                                });
                                break;
                            default:
                                throw new Exception();
                        }
                        return token;
                    case ExpressionType.MemberAccess:
                        visitor = new MemberExpressionVisitor(Context);
                        visitor.Visit(node);
                        var result = visitor.Token;
                        var memExp = node as MemberExpression;
                        switch (memExp.Member.Name)
                        {
                            case "HasValue":
                                if (TypeHelper.IsNullableType(memExp.Member.DeclaringType))
                                {
                                    result.Column.ConverterParameters.Clear();
                                    return Token.Create(new Condition()
                                    {
                                        CompareType = CompareType.Not,
                                        Left = Token.Create(new Condition()
                                        {
                                            Left = result,
                                            Right = Token.CreateNull()
                                        })
                                    });
                                }
                                break;
                            case "Value":
                                if (TypeHelper.IsNullableType(memExp.Member.DeclaringType))
                                {
                                    result.Column.ConverterParameters.Clear();
                                    return result;
                                }
                                break;
                        }
                        switch (result.Type)
                        {
                            case TokenType.Column:
                                if (EasyORM.Utils.TypeHelper.GetUnderlyingType(result.Column.DataType) == EasyORM.Utils.ReflectorConsts.BoolType)
                                {
                                   // return result;
                                    //IsEnabled && CreateTime==DateTime.Now
                                    //var left = result;
                                    //var right = Token.Create(1);
                                    //var condition = new Condition();
                                    //condition.CompareType = CompareType.Equal;
                                    //condition.Left = left;
                                    //condition.Right = right;
                                    //result = Token.Create(condition);
                                }
                                else
                                {
                                    throw new Exception();
                                }
                                break;
                            case TokenType.Object:

                                break;
                            default:
                                throw new Exception();
                        }
                        return result;
                    default:
                        visitor = new BinaryExpressionVisitor(Context);
                        visitor.Visit(node);
                        return visitor.Token;
                }
            }
        }
    }
}
