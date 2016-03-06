using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.SchemaModel;
using EasyORM.DbUtils;

namespace EasyORM.Configuration
{
    /// <summary>
    /// 提供对实体进行配置的功能，该功能优先于通过特性配置
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
        /// 指定该实体的主键
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public EntityConfiguration<T> Key<TKey>(Expression<Func<T, TKey>> selector, ColumnType dataSourceType)
        {
            var col = Parse(selector);
            col.ColumnType = dataSourceType;
            col.Table.Key = col;
            col.IsKey = true;
            return this;
        }

        /// <summary>
        /// 将表达式树转换成Column
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
        /// 返回可配置属性的对象
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
        /// 指定表名
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
        /// 指定数据库名
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
