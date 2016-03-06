using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
