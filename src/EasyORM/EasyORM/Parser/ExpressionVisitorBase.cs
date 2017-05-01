using EasyORM.TranslateModel;
using System.Linq.Expressions;

namespace EasyORM.Provider.Parser
{
    public abstract class ExpressionVisitorBase:ExpressionVisitor
    {
        TranslateContext _context;
        DataContext _dataContext;

        protected DataContext DataContext
        {
            get { return _context.DataContext; }
        }

        protected TranslateContext Context
        {
            get { return _context; }
        }
        public ExpressionVisitorBase(TranslateContext context)
        {
            _context = context;
        }
        public Token Token { get; protected set; }
        public object ExtraObject { get; protected set; }
    }
}
