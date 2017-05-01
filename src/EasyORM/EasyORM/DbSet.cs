using EasyORM.DynamicObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyORM
{
    /// <summary>
    /// Provides a set of db operation
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class DbSet<T> : IQueryable<T>, IOrderedQueryable<T>, IEntityOperator
    {
        DataQuery<T> _dataQuery;
        HashSet<T> _entitiesToAdd = new HashSet<T>();
        HashSet<T> _entitiesToEdit = new HashSet<T>();
        HashSet<T> _entitiesToRemove = new HashSet<T>();
        public DbSet(QueryProvider provider)
        {
            _dataQuery = new DataQuery<T>(provider);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _dataQuery.GetEnumerator();//.Provider.Execute<T>(Expression);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dataQuery.GetEnumerator();
        }


        public Type ElementType
        {
            get { return _dataQuery.ElementType; }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return _dataQuery.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _dataQuery.Provider; }
        }

        public T Add(T obj)
        {
            obj = DynamicProxy.CreateDynamicProxy(obj);
            if (!_entitiesToAdd.Contains(obj))
            {
                _entitiesToAdd.Add(obj);
            }
            return obj;
        }

        ArrayList IEntityOperator.GetAdding()
        {
            var list = new ArrayList();
            foreach (var item in _entitiesToAdd)
            {
                list.Add(item);
            }
            return list;
        }

        void IEntityOperator.ClearAdding()
        {
            lock (_entitiesToAdd)
                _entitiesToAdd.Clear();
        }


        void IEntityOperator.AddEditing(IList list)
        {
            foreach (T item in list)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                _entitiesToEdit.Add(item);
            }
        }


        IList IEntityOperator.GetEditing()
        {
            var list = new ArrayList();
            foreach (var item in _entitiesToEdit)
            {
                list.Add(item);
            }
            return list;
        }

        void IEntityOperator.ClearEditing()
        {
            _entitiesToEdit.Clear();
        }


        Type IEntityOperator.GetEntityType()
        {
            return ElementType;
        }

        public void Remove(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (_entitiesToRemove.Contains(item))
            {
                return;
            }
            _entitiesToRemove.Add(item);
        }


        IList IEntityOperator.GetRemoving()
        {
            var list = new ArrayList();
            foreach (var item in _entitiesToRemove)
            {
                list.Add(item);
            }
            return list;
        }

        void IEntityOperator.ClearRemoving()
        {
            _entitiesToRemove.Clear();
        }
    }
}
