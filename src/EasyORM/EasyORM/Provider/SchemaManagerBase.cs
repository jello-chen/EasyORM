using System.Collections.Generic;
using System.Data;
using EasyORM.SchemaModel;

namespace EasyORM.Provider
{
    /// <summary>
    /// Table Schema Manager Base
    /// </summary>
    public abstract class SchemaManagerBase : SqlExecutorBase
    {
        protected ProviderBase _provider;
        public SchemaManagerBase(DataContext context):base(context)
        {
            _provider = context.Provider;
        }

        /// <summary>
        /// Indicates whether the database has table
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
        /// Gets a Table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract Table GetTable(string tableName);

        internal abstract void CreateTable(Table table);
    }
}
