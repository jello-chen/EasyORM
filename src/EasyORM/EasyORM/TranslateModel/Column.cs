using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyORM.TranslateModel
{
    public class Column
    {
        public Table Table { get; set; }
        public string Name { get; set; }
        public MemberInfo MemberInfo { get; set; }

        public string Alias { get; set; }
        public Type DataType { get; set; }

        public string Converter { get; set; }
        public Stack<ColumnConverter> Converters { get; private set; }
        public Column()
        {
            Converters = new Stack<ColumnConverter>();
            ConverterParameters = new List<object>();
        }
        public List<object> ConverterParameters { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}.{1} AS {2}", Table.Name, Name, Alias);
        }

    }
}
