using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;

namespace EasyORM.Provider.MySql
{
    /// <summary>
    /// MySql Provider
    /// </summary>
    public class MySqlProvider : ProviderBase
    {

        public MySqlProvider(DataContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Gets a IEntityOperator object
        /// </summary>
        /// <returns></returns>
        public override IEntityOperator CreateEntityOperator()
        {
            return new EntityOperator(_context);
        }

        /// <summary>
        /// Gets a Schema Manager object
        /// </summary>
        /// <returns></returns>
        public override SchemaManagerBase CreateSchemaManager()
        {
            return new SchemaManager(_context);
        }

        /// <summary>
        /// Get s type mapper 
        /// </summary>
        /// <returns></returns>
        public override ITypeMapper CreateTypeMapper()
        {
            return TypeMapper.GetInstance();
        }
    }
}
