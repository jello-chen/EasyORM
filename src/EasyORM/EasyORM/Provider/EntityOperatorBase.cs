using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM;
using EasyORM.Configuration;
using EasyORM.Provider;

namespace EasyORM.Provider
{
    public class EntityOperatorBase
    {
        protected ProviderBase _provider { get; private set; }
        protected SqlExecutorBase _sqlExecutor { get; private set; }
        protected SqlBuilderBase _sqlBuilder { get; private set; }
        protected DataContext _context { get; private set; }
        protected EntityConfigurationManager _entityCfgManager { get; private set; }
        public EntityOperatorBase(DataContext context)
        {
            _context = context;
            _provider = _context.Provider;
            _sqlExecutor = _provider.CreateSqlExecutor();
            _sqlBuilder = _provider.CreateSqlBuilderFactory().CreateSqlBuilder();
            _entityCfgManager = new EntityConfigurationManager(_context);
        }
    }
}
