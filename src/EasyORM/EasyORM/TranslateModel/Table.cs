using System;

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
