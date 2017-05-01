using System;
using System.Linq;

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
