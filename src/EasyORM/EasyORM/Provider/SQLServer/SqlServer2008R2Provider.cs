using EasyORM.Provider.SQLServer;
using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;

namespace EasyORM.Provider.SQLServer
{
    /// <summary>
    /// SqlServer2008R2 Provider
    /// </summary>
    public class SQLServerProvider : ProviderBase
    {

        public SQLServerProvider(DataContext context)
            : base(context)
        {

        }

        /// <summary>
        ///  Gets a IEntityOperator object
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
