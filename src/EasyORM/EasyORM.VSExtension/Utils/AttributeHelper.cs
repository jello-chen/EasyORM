using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EasyORM.VSExtension.Utils
{
    public class AttributeHelper
    {
        public static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo)
        {
            return (TAttribute)memberInfo.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault();
        }
        public static TAttribute[] GetAttributes<TAttribute>(MemberInfo memberInfo)
        {
            return Array.ConvertAll(memberInfo.GetCustomAttributes(typeof(TAttribute), false), x => (TAttribute)x);
        }
    }
}
