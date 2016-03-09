using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.VSExtension.Configuration
{
    public class ConfigurationManager
    {
        public static ConfigurationFile Open(string fileName)
        {
            return new ConfigurationFile(fileName);
        }

    }
}
