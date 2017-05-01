namespace EasyORM.Provider.SQLite
{
    /// <summary>
    /// SQLite Provider
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
