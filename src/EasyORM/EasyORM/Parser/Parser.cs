using System.Linq.Expressions;

namespace EasyORM.Provider.Parser
{
    public class Parser : ParserBase
    {
        public Parser(DataContext dataContext):base(dataContext)
        {

        }
        SqlExpressionParser parser;
        Expression _expression;
        public override void Parse(System.Linq.Expressions.Expression expression)
        {
            parser = new SqlExpressionParser(_dataContext);
            _expression = expression;
            parser.ElementType = ElementType;
            parser.Parse(expression);
            var builderFactory = _dataContext.Provider.CreateSqlBuilderFactory();
            var builder = builderFactory.CreateSqlBuilder();
            BuilderContext context = new BuilderContext();
            var sqlType = SqlType.Select;
            if (parser.IsCallAny)
            {
                context.Take = 1;
            }
            if (parser.IsDelete)
            {
                sqlType = SqlType.Delete;
            }
            else if (parser.IsUpdate)
            {
                sqlType = SqlType.Update;
            }
            context.SqlType = sqlType;
            context.Pager = parser.Skip != -1 && parser.Take != -1;
            context.SortColumns = parser.SortColumns;
            context.Joins = parser.Joins;
            context.UpdateResult = parser.UpdateResult;
            context.Skip = parser.Skip;
            context.Take = parser.Take;
            context.AggregationColumns = parser.AggregationColumns;
            context.Columns = parser.Columns;
            context.Distinct = parser.Distinct;
            context.NoLockTables = parser.NoLockTables;
            context.Conditions = parser.Conditions;
            context.ElementType = parser.ElementType;
            Result = builder.BuildSql(context);
        }

    }
}
