using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.DbUtils.DataAnnotations
{
    /// <summary>
    /// DataSource attribute,only to be effective to primary key column
    /// </summary>
    public class DataSourceAttribute : Attribute
    {
        public KeyColumnType DataSource { get; set; }
    }
}
