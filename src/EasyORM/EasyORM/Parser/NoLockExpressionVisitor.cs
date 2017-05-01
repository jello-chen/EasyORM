namespace EasyORM.Provider.Parser
{
    public class NoLockExpressionVisitor:ExpressionVisitorBase
    {
        public NoLockExpressionVisitor(TranslateContext context):base(context)
        {

        }
        protected override System.Linq.Expressions.Expression VisitConstant(System.Linq.Expressions.ConstantExpression node)
        {
            var table = DataContext.EntityCfgManager.Value.GetTable(node.Type.GetGenericArguments()[0]);
            ExtraObject = table.Name;
            return base.VisitConstant(node);
        }
    }
}
