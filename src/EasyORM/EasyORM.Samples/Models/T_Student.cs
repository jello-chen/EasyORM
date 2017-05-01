using EasyORM.DbUtils.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace EasyORM.Samples.Models
{
    public class T_Student
    {
        [Key, DataSource(DataSource = DbUtils.KeyColumnType.AutoIncreament)]
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual int Age { get; set; }
        public int DepartmentID { get; set; }
    }
}
