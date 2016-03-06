using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Provider
{
    public class MethodProcessor
    {
        public static string ProcessContainsMethod(IQueryable obj,Type elType,string converter)
        {
            QueryableContainsMethodProcessor processor = new QueryableContainsMethodProcessor(obj, elType, converter);
            processor.Process();
            return processor.Result.ToString();
        }
    }
}
