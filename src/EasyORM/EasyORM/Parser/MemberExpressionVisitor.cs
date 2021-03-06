﻿using EasyORM.TranslateModel;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EasyORM.Provider.Parser
{
    /// <summary>
    /// Expression Visitor for Member
    /// </summary>
    public class MemberExpressionVisitor : ExpressionVisitorBase
    {
        Dictionary<string, Join> _joins;
        public MemberExpressionVisitor(TranslateContext context)
            : base(context)
        {
            this._joins = context.Joins;
        }
        public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression node)
        {
            ExpressionVisitorBase visitor = null;
            if (node.NodeType == ExpressionType.Quote)
            {
                node = ((UnaryExpression)node).Operand;
            }
            if (node.NodeType == ExpressionType.Lambda)
            {
                node = ((LambdaExpression)node).Body;
            }
            if (node.NodeType == System.Linq.Expressions.ExpressionType.Call)
            {
                visitor = new MethodCallExpressionVisitor(Context);
            }
            else
            {
                visitor = new PropertyFieldExpressionVisitor(Context);
            }
            visitor.Visit(node);
            Token = visitor.Token;
            return node;
        }
    }
}
