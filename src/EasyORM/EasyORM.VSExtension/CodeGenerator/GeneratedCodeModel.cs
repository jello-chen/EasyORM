using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Provider;
using EasyORM.SchemaModel;

namespace EasyORM.VSExtension.CodeGenerator
{
    /// <summary>
    /// Generated Code Model
    /// </summary>
    public class GeneratedCodeModel
    {
        public List<Table> Tables { get; set; }
        public string ConnectionStringName { get; set; }
        public DatabaseTypes DatabaseType { get; set; }

        public string DbFactoryName { get; set; }

        public bool GenerateAll { get; set; }
        public string Namespace { get; set; }
    }
}
