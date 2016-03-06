using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Provider
{
    public abstract class ParserBase
    {
        protected DataContext _dataContext;
        public ParserBase(DataContext context)
        {
            _dataContext = context;
        }
        public Type ElementType { get; set; }
        public ParseResult Result { get; protected set; }
        public abstract void Parse(Expression expression);
    }
}
