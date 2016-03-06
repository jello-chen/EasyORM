using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DbUtils.DataAnnotations;
using EasyORM.Utils;
using EasyORM.SchemaModel;

namespace EasyORM.Provider
{
    /// <summary>
    /// 表结构管理器基类
    /// </summary>
    public abstract class SchemaManagerBase : SqlExecutorBase
    {
        protected ProviderBase _provider;
        public SchemaManagerBase(DataContext context):base(context)
        {
            _provider = context.Provider;
        }

        /// <summary>
        /// 数据库是否已有表
        /// </summary>
        /// <returns></returns>
        public bool HasTable()
        {
            DataTable tableDt = null;
            CreateConnection();
            try
            {
                tableDt = DbConnection.GetSchema("tables", new string[] { null, null, null, null });
                return tableDt.Rows.Count > 0;
            }
            finally
            {
                Close();
            }
        }

        public abstract List<Table> GetTables();

        /// <summary>
        /// 获取指定表及列
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract Table GetTable(string tableName);

        internal abstract void CreateTable(Table table);
    }
}
