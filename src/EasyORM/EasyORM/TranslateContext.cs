using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM
{
    public class TranslateContext
    {
        public TranslateContext()
        {
            Joins = new Dictionary<string, TranslateModel.Join>();
            Columns = new Dictionary<string, TranslateModel.Column>();
        }
        public Dictionary<string, TranslateModel.Join> Joins { get;private set; }

        public Dictionary<string, TranslateModel.Column> Columns { get; private set; }

        public Type EntityType { get; set; }
        public DataContext DataContext { get; set; }
    }
}
