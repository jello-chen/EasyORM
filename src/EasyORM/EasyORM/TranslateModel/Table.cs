using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.TranslateModel
{
    public class Table
    {
        public Table()
        {

        }
        public string DataBase { get; set; }
        public string Name { get; set; }

        public string Alias { get; set; }
        public Type Type { get; set; }
    }
}
