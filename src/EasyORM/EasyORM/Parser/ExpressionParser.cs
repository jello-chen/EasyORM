using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Provider;
using EasyORM.Provider.Parser;
using EasyORM.TranslateModel;

namespace EasyORM.Parser
{
    public class ExpressionParser
    {
        DatabaseTypes _dataBaseType;
        public ExpressionParser(DatabaseTypes dataBaseType)
        {
            _dataBaseType = dataBaseType;
        }


        public Token ParseCondition(LambdaExpression operand, TranslateContext context)
        {
            var body = operand.Body;
            if (body is ConstantExpression)
            {
                var constExp = body as ConstantExpression;
                if ((bool)constExp.Value == false)
                {
                    return Token.Create(false);
                }
            }
            var visitor = new WhereExpressionVisitor(context);
            visitor.Visit(operand);
            return visitor.Token;
        }

        public string ParserCondition(IList<Token> conditions)
        {
            throw new NotImplementedException();
        }
    }
}
