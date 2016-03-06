using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.TranslateModel
{
    public class Condition 
    {
        public Token Left { get; set; }
        public CompareType CompareType { get; set; }
        public Token Right { get; set; }
    }
}
