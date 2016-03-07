using EasyORM.Provider.SQLServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using System.Data.Common;
using EasyORM.Configuration;

namespace EasyORM.Provider
{
    /// <summary>
    /// Base Provider
    /// </summary>
    public abstract class ProviderBase
    {
        DatabaseTypes _databaseType;
        private Type _sqlBuilderFactoryType;
        protected DataContext _context;

        /// <summary>
        /// Get the current database type
        /// </summary>
        public DatabaseTypes DatabaseType
        {
            get { return _databaseType; }
        }

        public DbProviderFactory CreateDbProviderFactory()
        {
            return ObjectDictionaryCache.GetObject(_context.DatabaseConfig.ProviderName, () => DbProviderFactories.GetFactory(_context.DatabaseConfig.ProviderName));
        }

        /// <summary>
        /// Get a schema manager that is able to create table
        /// </summary>
        /// <returns></returns>
        public abstract SchemaManagerBase CreateSchemaManager();

        public ProviderBase(DataContext context)
        {
            _context = context;
            _databaseType = context.DatabaseConfig.DatabaseType;
        }

        /// <summary>
        /// Create a parser
        /// </summary>
        /// <returns></returns>
        public ParserBase CreateParser()
        {
            return new Parser.Parser(_context);
        }

        /// <summary>
        /// Create a sql executor
        /// </summary>
        /// <returns></returns>
        public virtual SqlExecutorBase CreateSqlExecutor()
        {
            return new SqlExecutorBase(_context);
        }

        /// <summary>
        /// Create a entity operator that is able to insert,update and delete
        /// </summary>
        /// <returns></returns>
        public abstract IEntityOperator CreateEntityOperator();

        /// <summary>
        /// Create a sql builder factory that is able to build sql 
        /// </summary>
        /// <returns></returns>
        public BuilderFactory CreateSqlBuilderFactory()
        {
            if (!string.IsNullOrWhiteSpace(Config.SqlBuilder))
            {
                if (_sqlBuilderFactoryType == null)
                {
                    var assInfos = Config.SqlBuilder.Split(',');
                    _sqlBuilderFactoryType = Assembly.Load(assInfos[1]).GetTypes().FirstOrDefault(x => x.FullName == assInfos[0]);
                }
                return (BuilderFactory)ExpressionReflector.CreateInstance(_sqlBuilderFactoryType, ObjectPropertyConvertType.Cast, this);
            }
            return new BuilderFactory(_context);
        }

        public abstract ITypeMapper CreateTypeMapper();
    }
}
