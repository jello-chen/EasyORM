﻿using System;
using System.Collections.Generic;

namespace EasyORM.SchemaModel
{

    public class Table
    {
        public Table()
        {
            Columns = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
        }


        public string DataBase { get; set; }

        public string Name { get; set; }

        public Column Key { get; set; }

        public Type Type { get; set; }

        public Dictionary<string,Column> Columns { get; set; }
    }
}
