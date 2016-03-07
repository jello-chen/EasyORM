using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EasyORM.SchemaModel;
using EasyORM.DbUtils;

namespace EasyORM.Configuration
{
    /// <summary>
    /// Entity Configuration,it takes the precedence over the attribute configuration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityConfiguration<T>
    {
        DataContext _context;
        public EntityConfiguration(DataContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Specify the primary key of entity
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public EntityConfiguration<T> Key<TKey>(Expression<Func<T, TKey>> selector, KeyColumnType dataSourceType)
        {
            var col = Parse(selector);
            col.ColumnType = dataSourceType;
            col.Table.Key = col;
            col.IsKey = true;
            return this;
        }

        /// <summary>
        /// Parse expression to column
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        Column Parse<TKey>(Expression<Func<T, TKey>> selector)
        {
            var memberExp = (MemberExpression)selector.Body;
            var constExp = (ParameterExpression)memberExp.Expression;
            var type = constExp.Type;
            var table = _context.EntityCfgManager.Value.GetTable(type);
            var propertyInfo = (PropertyInfo)memberExp.Member;
            var propertyName = propertyInfo.Name;
            var col = table.Columns.FirstOrDefault(x => x.Value.PropertyInfo.Name == propertyName).Value;
            return col;
        }

        /// <summary>
        /// Get the PropertyConfiguration instance
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public PropertyConfiguration<TProperty> Property<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            var table = _context.EntityCfgManager.Value.GetTable(typeof(T));
            var propertyName = ((MemberExpression)selector.Body).Member.Name;
            return new PropertyConfiguration<TProperty>(table, propertyName);
        }

        /// <summary>
        /// Specify the table name
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public EntityConfiguration<T> TableName(string tableName)
        {
            var table = _context.EntityCfgManager.Value.GetTable(typeof(T));
            table.Name = tableName;
            return this;
        }

        /// <summary>
        /// Specify the database
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public EntityConfiguration<T> Database(string db)
        {
            var table = _context.EntityCfgManager.Value.GetTable(typeof(T));
            table.DataBase = db;
            return this;
        }
    }
}
