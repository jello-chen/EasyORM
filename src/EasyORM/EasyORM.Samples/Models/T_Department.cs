using EasyORM.DbUtils.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Samples.Models
{
    class T_Department
    {
        [DataSource(DataSource = DbUtils.KeyColumnType.AutoIncreament)]
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
