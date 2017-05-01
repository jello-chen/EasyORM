using System.Configuration;

namespace EasyORM.Configuration
{
    public class Logger : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }
    }
}
