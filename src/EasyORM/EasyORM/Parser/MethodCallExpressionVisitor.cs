using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EasyORM.Utils;

namespace EasyORM.Provider.Parser
{
    /// <summary>
    /// Expression visitor for calling method
    /// </summary>
    public class MethodCallExpressionVisitor : ExpressionVisitorBase
    {
        Dictionary<string, Join> _joins;
        bool _isColumn = false;

        public MethodCallExpressionVisitor(TranslateContext context)
            : base(context)
        {
            // TODO: Complete member initialization
            this._joins = context.Joins;
        }

        Token ParseArgument(Expression argExp)
        {
            while (argExp.NodeType == ExpressionType.Convert || argExp.NodeType == ExpressionType.ConvertChecked)
            {
                argExp = ((UnaryExpression)argExp).Operand;
            }
            if (argExp.NodeType == ExpressionType.MemberAccess || argExp.NodeType == ExpressionType.Call)
            {
                var visitor = new MemberExpressionVisitor(Context);
                visitor.Visit(argExp);
                if (visitor.Token.Type == TokenType.Column)
                {
                    _isColumn = true;
                }
                return visitor.Token;
                //if (visitor.Token.Type == TokenType.Object)
                //{
                //}
                //else if (visitor.Token.Type == TokenType.Column)
                //{
                //    Type = MemberExpressionType.Column;
                //}
                //else
                //{
                //    throw new Exception();
                //}
                //return visitor.Result;
            }
            else if (argExp.NodeType == ExpressionType.Constant)
            {
                return Token.Create(((ConstantExpression)argExp).Value);
            }
            else
            {
                throw new Exception();
            }
        }



        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var argsExp = node.Arguments;
            var args = new List<Token>();
            foreach (var argExp in argsExp)
            {
                args.Add(ParseArgument(argExp));
            }
            if (node.Object == null)
            {
                //Invoke static method:var col = (Token)body.Result;
                var method = node.Method;
                if (_isColumn)
                {
                    var parameters = new List<object>();
                    switch (method.Name)
                    {
                        case "ToDateTime":
                            Token = (Token)args[0];
                            parameters.AddRange(args.Skip(1));
                            break;
                        case "Contains":
                            Token = (Token)args[1];
                            parameters.Add(args[0].Object);
                            parameters.AddRange(args.Skip(2).Where(x => x.Type == TokenType.Object).Select(x => x.Object));
                            break;
                        default:
                            throw new Exception();
                    }
                    var converter = new ColumnConverter(method, parameters);
                    Token.Column.Converters.Push(converter);
                }
                else
                {
                    var methodArgs = new List<object>();
                    var targetMethodArgs = node.Method.GetParameters().ToArray();
                    BaseTypeConverter converter = null;
                    for (int i = 0; i < targetMethodArgs.Length; i++)
                    {
                        var targetParameter = targetMethodArgs[i];
                        var obj = args[i].Object;
                        if (TypeHelper.IsValueType(targetParameter.ParameterType))
                        {
                            converter = new BaseTypeConverter(obj, targetParameter.ParameterType);
                            converter.Process();
                            methodArgs.Add(converter.Result);
                        }
                        else
                        {
                            methodArgs.Add(obj);
                        }
                    }
                    Token = Token.Create(node.Method.Invoke(null, methodArgs.ToArray()));
                }
            }
            else
            {
                var body = new MemberExpressionVisitor(Context);
                body.Visit(node.Object);
                if (body.Token.Type == TokenType.Column && !_isColumn)
                {
                    Token = body.Token;
                    var method = node.Method;
                    var argObjects = args.Select(x => x.Object);
                    Token.Column.Converters.Push(new ColumnConverter(method, argObjects.ToList(), true));
                }
                else if (body.Token.Type == TokenType.Column && _isColumn)
                {
                    Token = body.Token;
                    var method = node.Method;
                    var parameters = new List<object>();
                    parameters.AddRange(args);
                    Token.Column.Converters.Push(new ColumnConverter(method, parameters, true));
                }
                else if (body.Token.Type == TokenType.Object && !_isColumn)
                {
                    Token = Token.Create(node.Method.Invoke(body.Token.Object, args.Select(x => x.Object).ToArray()));
                }
                else if (body.Token.Type == TokenType.Object && _isColumn)
                {
                    Token = args[0];
                    var parameters = new List<object>();
                    parameters.Add(body.Token.Object);
                    parameters.AddRange(args.Skip(1));
                    Token.Column.Converters.Push(new ColumnConverter(node.Method, parameters, false));
                }
                else
                {
                    throw new Exception();
                }
            }
            return node;
        }
    }
}
