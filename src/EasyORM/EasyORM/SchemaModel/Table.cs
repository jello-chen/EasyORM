using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.SchemaModel
{
    /// <summary>
    /// 从实体类型分析出来的数据库表信息
    /// </summary>
    public class Table
    {
        public Table()
        {
            Columns = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 数据库
        /// </summary>
        public string DataBase { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public Column Key { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 所有列
        /// </summary>
        public Dictionary<string,Column> Columns { get; set; }
    }
}
