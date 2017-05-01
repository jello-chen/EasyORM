using System;

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
