using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EasyORM
{
    public class DataQuery<T> : IQueryable<T>, IOrderedQueryable<T>
    {
        private QueryProvider provider;
        private Expression _expression;
        Type _elementType;
        public DataQuery(QueryProvider provider)
        {
            this.provider = provider;
            _expression = Expression.Constant(this);
            _elementType = typeof(T);
        }
        public DataQuery(QueryProvider provider, Expression expression)
        {
            this.provider = provider;
            _expression = expression;
            _elementType = typeof(T);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType
        {
            get { return _elementType; }
        }
        public string CommandText
        {
            get
            {
                return ((QueryProvider)Provider).GetCommandText(Expression);
            }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return _expression; }
        }

        public IQueryProvider Provider
        {
            get { return provider; }
        }
    }
}
