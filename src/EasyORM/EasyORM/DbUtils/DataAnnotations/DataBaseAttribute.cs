using System;

namespace EasyORM.DbUtils.DataAnnotations
{
    public class DataBaseAttribute : Attribute
    {
        public string Name { get; private set; }
        public DataBaseAttribute(string dataBaseName)
        {
            Name = dataBaseName;
        }
    }
}
