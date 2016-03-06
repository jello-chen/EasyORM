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
    /// 提供者抽象基类
    /// </summary>
    public abstract class ProviderBase
    {
        DatabaseTypes _databaseType;
        private Type _sqlBuilderFactoryType;
        protected DataContext _context;

        /// <summary>
        /// 当前数据库类型
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
        /// 获取一个用于创建数据库表的对象
        /// </summary>
        /// <returns></returns>
        public abstract SchemaManagerBase CreateSchemaManager();

        /// <summary>
        /// 构造方法
        /// </summary>
        public ProviderBase(DataContext context)
        {
            _context = context;
            _databaseType = context.DatabaseConfig.DatabaseType;
        }

        /// <summary>
        /// 创建一个新的分析器
        /// </summary>
        /// <returns></returns>
        public ParserBase CreateParser()
        {
            return new Parser.Parser(_context);
        }

        /// <summary>
        /// 创建一个新的SQL语句执行器
        /// </summary>
        /// <returns></returns>
        public virtual SqlExecutorBase CreateSqlExecutor()
        {
            return new SqlExecutorBase(_context);
        }

        /// <summary>
        /// 创建一个插入、删除、更新实体的对象
        /// </summary>
        /// <returns></returns>
        public abstract IEntityOperator CreateEntityOperator();

        /// <summary>
        /// 创建一个SqlBuilderFactory，即SQL语句生成对象的工厂
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
