using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.SchemaModel;

namespace EasyORM.Configuration
{
    /// <summary>
    /// Property Configuration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyConfiguration<T>
    {
        Type _propertyType = typeof(T);
        string _propertyName;
        Table _table;
        public PropertyConfiguration(Table table, string propertyName)
        {
            _table = table;
            _propertyName = propertyName;
        }
        /// <summary>
        /// Set the max length on column
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public PropertyConfiguration<T> MaxLength(int length)
        {
            GetColumn().MaxLength = length;
            return this;
        }

        Column GetColumn()
        {
            var column = _table.Columns.FirstOrDefault(x => x.Value.PropertyInfo.Name == _propertyName).Value;
            if (column == null)
            {
                throw new Exception("Not supported " + _propertyName + " property");
            }
            return column;
        }

        /// <summary>
        /// Set the db type on column
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public PropertyConfiguration<T> Type(DbType dbType)
        {
            GetColumn().DbType = dbType;
            return this;
        }

        /// <summary>
        /// Set the precision on column when decimal
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public PropertyConfiguration<T> Precision(int precision, int scale)
        {
            var column = GetColumn();
            column.Precision = precision;
            column.Scale = scale;
            return this;
        }

        /// <summary>
        /// Set the name on column
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PropertyConfiguration<T> Name(string name)
        {
            GetColumn().Name = name;
            return this;
        }
    }
}
