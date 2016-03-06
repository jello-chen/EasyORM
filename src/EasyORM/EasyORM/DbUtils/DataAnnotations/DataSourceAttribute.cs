using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.DbUtils.DataAnnotations
{
    /// <summary>
    /// 数据来源方式，只对主键列有效
    /// </summary>
    public class DataSourceAttribute : Attribute
    {
        public ColumnType DataSource { get; set; }
    }
}
