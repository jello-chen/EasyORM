using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DbUtils.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EasyORM.Samples.Models
{
    [Table("T_User")]
    public class T_User
    {
        [DataSource(DataSource = DbUtils.KeyColumnType.None)]
        [Column("ID"), Key]
        public int ID { get; set; }
        public virtual string Name { get; set; }
        public int Age { get; set; }
    }
}
