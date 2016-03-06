using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;

namespace EasyORM.Provider.SQLite
{
    /// <summary>
    /// SQLite提供者
    /// </summary>
    public class SQLiteProvider : ProviderBase
    {
        public SQLiteProvider(DataContext context)
            : base(context)
        {

        }
        public override IEntityOperator CreateEntityOperator()
        {
            return new EntityOperator(_context);
        }

        public override SqlExecutorBase CreateSqlExecutor()
        {
            return new SqlExecutor(_context);
        }

        public override SchemaManagerBase CreateSchemaManager()
        {
            return new SchemaManager(_context);
        }

        public override ITypeMapper CreateTypeMapper()
        {
            return TypeMapper.GetInstance();
        }
    }
}
