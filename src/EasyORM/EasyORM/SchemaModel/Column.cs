using EasyORM.DbUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DbUtils.DataAnnotations;

namespace EasyORM.SchemaModel
{
    public class Column
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 该列所关联到的<seealso cref="System.Reflection.PropertyInfo"></seealso>
        /// </summary>
        [JsonIgnore]
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 该列所属的表
        /// </summary>
        [JsonIgnore]
        public Table Table { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// 该列的数据来源类型
        /// </summary>
        public ColumnType ColumnType { get; set; }

        /// <summary>
        /// 是否可空
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int MaxLength { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }
    }
}
