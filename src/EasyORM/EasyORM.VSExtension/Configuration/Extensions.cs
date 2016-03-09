using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyORM.VSExtension.Configuration
{
    public static class Extensions
    {
        public static string GetAttributeValue(this XElement element, string name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
            {
                return null;
            }
            return attr.Value;
        }
    }
}
