using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;

namespace EasyORM
{
    public class BuilderContext
    {
        public SqlType SqlType { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool Pager { get; set; }
        public Type ElementType { get; set; }
        public IList<Token> Conditions { get; set; }

        public bool Distinct { get; set; }

        public List<KeyValuePair<string, Column>> SortColumns { get; set; }

        public Dictionary<string, Join> Joins { get; set; }

        public Dictionary<string, object> UpdateResult { get; set; }

        public Dictionary<string, Column> AggregationColumns { get; set; }

        public List<Column> Columns { get; set; }

        public List<string> NoLockTables { get; set; }
    }
}
