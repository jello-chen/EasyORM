using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.TranslateModel
{
    /// <summary>
    /// 会将表达式树翻译成此对象
    /// </summary>
    public class Table
    {
        public Table()
        {

        }
        public string DataBase { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 调用者指定的别名，可能为空，如果调用的是Join方法则一定不会为空
        /// </summary>
        public string Alias { get; set; }
        public Type Type { get; set; }
    }
}
