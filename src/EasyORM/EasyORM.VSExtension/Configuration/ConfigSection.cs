using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyORM.VSExtension.Configuration
{
    public class ConfigSection
    {
        XElement _element;
        public ConfigSection(XElement element)
        {
            _element = element;
        }
        public string Name
        {
            get
            {
                return _element.GetAttributeValue("name");
            }
            set
            {
                _element.SetAttributeValue("name", value);
            }
        }
        public string Type
        {
            get
            {
                return _element.GetAttributeValue("type");
            }
            set
            {
                _element.SetAttributeValue("type", value);
            }
        }
    }
}
