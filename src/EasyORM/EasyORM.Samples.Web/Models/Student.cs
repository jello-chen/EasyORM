using EasyORM.DbUtils.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EasyORM.Samples.Web.Models
{
    [Table("T_Student")]
    public class Student
    {
        [Key, DataSource(DataSource = DbUtils.KeyColumnType.AutoIncreament)]
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual int Age { get; set; }
        public int DepartmentID { get; set; }

        public Guid? IDKey { get; set; }
    }
}