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
    /// 用于进行实体属性配置的工具
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
        /// 指定最大长度
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
                throw new Exception("未找到" + _propertyName + "属性");
            }
            return column;
        }

        /// <summary>
        /// 指定类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public PropertyConfiguration<T> Type(DbType dbType)
        {
            GetColumn().DbType = dbType;
            return this;
        }

        /// <summary>
        /// Deicmal类型专用
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
        /// 指定列名
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
