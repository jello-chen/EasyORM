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
        public string Name { get; set; }

        [JsonIgnore]
        public PropertyInfo PropertyInfo { get; set; }

        [JsonIgnore]
        public Table Table { get; set; }

        public DbType DbType { get; set; }

        public bool IsKey { get; set; }

        public KeyColumnType ColumnType { get; set; }

        public bool NotNull { get; set; }

        public int MaxLength { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }
    }
}
