using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace EasyORM.Provider.Parser
{
    /// <summary>
    /// 对访问成员方法或属性的表达式进行分析
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
