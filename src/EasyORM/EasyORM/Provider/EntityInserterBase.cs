using System.Collections;

namespace EasyORM.Provider
{
    /// <summary>
    /// Entity Inserter Base
    /// </summary>
    public abstract class EntityInserterBase
    {
        protected ProviderBase provider { get; private set; }
        protected SqlExecutorBase sqlExecutor { get; private set; }
        protected SqlBuilderBase _sqlBuilder { get; private set; }
        protected DataContext _dataContext { get; private set; }
        public EntityInserterBase(DataContext dataContext)
        {
            _dataContext = dataContext;
            this.provider = dataContext.Provider;
            sqlExecutor = provider.CreateSqlExecutor();
            _sqlBuilder = provider.CreateSqlBuilderFactory().CreateSqlBuilder();
        }
        /// <summary>
        /// Insert a list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public abstract int Insert(ArrayList list);
    }
}
