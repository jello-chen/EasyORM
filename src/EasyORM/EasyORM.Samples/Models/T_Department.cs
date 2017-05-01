using EasyORM.DbUtils.DataAnnotations;

namespace EasyORM.Samples.Models
{
    public class T_Department
    {
        [DataSource(DataSource = DbUtils.KeyColumnType.AutoIncreament)]
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
