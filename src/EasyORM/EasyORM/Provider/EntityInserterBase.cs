using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Provider.SQLite;

namespace EasyORM.Provider
{
    /// <summary>
    /// 提供插入实体集的功能
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
        /// 插入指定实体集到数据库
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public abstract int Insert(ArrayList list);
    }
}
