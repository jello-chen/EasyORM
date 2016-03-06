using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
