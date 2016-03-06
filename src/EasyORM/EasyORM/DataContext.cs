using EasyORM.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EasyORM.Configuration;
using EasyORM.Utils;
using EasyORM.ObjectMapper;
using EasyORM.DynamicObject;

namespace EasyORM
{
    /// <summary>
    /// Data Context
    /// </summary>
    public class DataContext
    {
        internal static ConfigSection GlobalConfig { get; private set; }

        Lazy<EntityConfigurationManager> _entityCfgManager;

        public Lazy<EntityConfigurationManager> EntityCfgManager
        {
            get { return _entityCfgManager; }
        }
        /// <summary>
        /// Database provider related to current instance
        /// </summary>
        public ProviderBase Provider { get; private set; }
        /// <summary>
        /// Database configuration related to current instance
        /// </summary>
        public DatabaseConfig DatabaseConfig { get; private set; }
        Type _dataContextType;
        static Type _dbSetType = typeof(DbSet<>);
        static Dictionary<Type, DatabaseConfig> _databaseConfigs = new Dictionary<Type, DatabaseConfig>();
        /// <summary>
        /// DbSet cache,the key is the type of DataContext
        /// </summary>
        Dictionary<Type, Dictionary<string, object>> _dbSets = new Dictionary<Type, Dictionary<string, object>>();

        /// <summary>
        /// DbSet property cache,the key is the type of DataContext
        /// </summary>
        static Dictionary<Type, Dictionary<string, PropertyInfo>> _dbSetProperties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        /// <summary>
        /// The cache that maps entity type to DbSet
        /// </summary>
        Dictionary<Type, object> _entitieDbSets = new Dictionary<Type, object>();
        static HashSet<Type> _entityTypes = new HashSet<Type>();

        /// <summary>
        /// Whether the type is entity
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEntity(Type type)
        {
            return _entityTypes.Contains(type);
        }

        /// <summary>
        /// Get the IEntityOperator that maps the entity type correspondingly
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal IEntityOperator GetEntityOperator(Type entityType)
        {
            return (IEntityOperator)_entitieDbSets.GetOrDefault(entityType);
        }

        /// <summary>
        /// Execute sql text 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IList<T> SqlQuery<T>(string sql, Dictionary<string, object> parameters)
        {
            var sqlExecutor =Provider.CreateSqlExecutor();
            using (sqlExecutor)
            {
                var reader = sqlExecutor.ExecuteReader(sql, parameters);
                using (reader)
                    return (IList<T>)EntityMapper.Map(typeof(T), reader);
            }
        }

        EntityConfigurationManager GetEntityCfgManager()
        {
            return new EntityConfigurationManager(this);
        }

        /// <summary>
        /// Get the DbSet that maps the generic type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DbSet<T> Set<T>()
        {
            Dictionary<string, object> dbSets;
            if (!_dbSets.TryGetValue(_dataContextType, out dbSets))
            {
                throw new Exception("Init Fail");
            }
            object property = null;
            var entityType = typeof(T);
            var typeName = entityType.Name;
            var propertyName = StringHelper.ToPlural(typeName);
            if (!dbSets.TryGetValue(propertyName, out property))
            {
                property = AddEntityType(entityType, null);
                dbSets.Add(propertyName, property);
                CreateTableIfAllow(entityType);
            }
            return (DbSet<T>)property;
        }

        /// <summary>
        /// The database provider supported 
        /// </summary>
        public readonly static Dictionary<DatabaseTypes, string> SupportProviders = new Dictionary<DatabaseTypes, string>(){
            { DatabaseTypes.SQLServer,"System.Data.SqlClient"},
            {DatabaseTypes.SQLite,"System.Data.SQLite"},
            {DatabaseTypes.MySql,"MySql.Data.MySqlClient"}
        };
        //private string _connectionStringName;

        static DataContext()
        {
            #region Init Config
            GlobalConfig = ConfigurationManager.GetSection("easyORM") as ConfigSection;
            if (GlobalConfig != null)
            {
                Config.IsEnableLog = GlobalConfig.IsEnableLog;
                Config.SequenceTable = GlobalConfig.SequenceTable;
                Config.SqlBuilder = GlobalConfig.SqlBuilder;
                Config.IsEnableAutoCreateTables = string.IsNullOrWhiteSpace(GlobalConfig.IsAutoCreateTables) ? false : Convert.ToBoolean(GlobalConfig.IsAutoCreateTables);
                Config.IsEnableAllwayAutoCreateTables = string.IsNullOrWhiteSpace(GlobalConfig.IsEnableAllwayAutoCreateTables) ? false : Convert.ToBoolean(GlobalConfig.IsEnableAllwayAutoCreateTables);
            }
            if (string.IsNullOrWhiteSpace(Config.SequenceTable))
            {
                Config.SequenceTable = "Sequences";
            }
            #endregion
        }

        /// <summary>
        /// 配置实体的相关信息
        /// </summary>
        protected virtual void ConfigurationModel(EntityConfigurationManager entityConfiguration)
        {

        }

        /// <summary>
        /// 将指定实体初始化并缓存
        /// </summary>
        /// <param name="entityType"></param>
        object AddEntityType(Type entityType, Type dbsetType)
        {
            if (dbsetType == null)
            {
                dbsetType = _dbSetType.MakeGenericType(entityType);
            }
            QueryProvider provider = new QueryProvider(this, entityType);
            var set = Activator.CreateInstance(dbsetType, provider);
            _entitieDbSets.Add(entityType, set);
            _entityTypes.Add(entityType);
            return set;
        }

        void CreateTableIfAllow(Type entityType)
        {
            if (!AllowCreateTable())
            {
                return;
            }
            CreateTable(entityType);
        }

        /// <summary>
        /// 是否允许创建表
        /// </summary>
        /// <returns></returns>
        bool AllowCreateTable()
        {
            if (Config.IsEnableAutoCreateTables)
            {
                var schemaManager = Provider.CreateSchemaManager();
                if (!Config.IsEnableAllwayAutoCreateTables)
                {
                    if (schemaManager.HasTable())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else if (Config.IsEnableAllwayAutoCreateTables)
            {
                throw new Exception("检测到未启用【自动创建数据库表】功能，但启用了【始终自动创建数据库表】功能，请检查配置，除非需要此功能，否则不要启用");
            }
            else
            {
                return false;
            }
        }

        void CreateTable(Type entityType)
        {
            var schemaManager = Provider.CreateSchemaManager();
            var table = new EntityConfigurationManager(this).GetTable(entityType);
            var existsTable = schemaManager.GetTable(table.Name);
            if (existsTable == null)
            {
                //创建该表
                schemaManager.CreateTable(table);
            }
        }

        #region 构造方法

        public DataContext(string connectionString,string providerName)
        {
            Init(connectionString, providerName);
        }

        void Init(string connectionString,string providerName)
        {
            _dataContextType = GetType();
            var databaseConfig = _databaseConfigs.GetOrDefault(_dataContextType);
            if (databaseConfig == null)
            {
                lock (_databaseConfigs)
                {
                    databaseConfig = _databaseConfigs.GetOrDefault(_dataContextType);
                    if (databaseConfig == null)
                    {
                        databaseConfig = new DatabaseConfig();
                        databaseConfig.ConnectionString = connectionString;
                        databaseConfig.ProviderName = providerName;
                        foreach (var item in SupportProviders)
                        {
                            if (item.Value == providerName)
                            {
                                databaseConfig.DatabaseType = item.Key;
                                break;
                            }
                        }
                        _databaseConfigs.Add(_dataContextType, databaseConfig);
                    }
                }
            }
            DatabaseConfig = databaseConfig;
            Provider = ProviderFactory.CreateProvider(this);
            _entityCfgManager = new Lazy<EntityConfigurationManager>(GetEntityCfgManager);
            Dictionary<string, PropertyInfo> dbSetProperties;
            Dictionary<string, object> dbSets;

            #region 获取DbSet<T>属性集
            if (!_dbSetProperties.TryGetValue(_dataContextType, out dbSetProperties))
            {
                lock (_dbSetProperties)
                {
                    if (!_dbSetProperties.TryGetValue(_dataContextType, out dbSetProperties))
                    {
                        dbSetProperties = _dataContextType.GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == _dbSetType).ToDictionary(x => x.Name);
                        _dbSetProperties.Add(_dataContextType, dbSetProperties);
                    }
                }
            }
            #endregion

            #region 从获取到的属性集中获取硬编码的所有实体并更新到缓存
            if (!_dbSets.TryGetValue(_dataContextType, out dbSets))
            {
                lock (_dbSets)
                {
                    if (!_dbSets.TryGetValue(_dataContextType, out dbSets))
                    {
                        dbSets = new Dictionary<string, object>();
                        foreach (var dbSet in dbSetProperties)
                        {
                            var entityType = dbSet.Value.PropertyType.GetGenericArguments()[0];
                            var set = AddEntityType(entityType, dbSet.Value.PropertyType);
                            dbSets.Add(StringHelper.ToPlural(entityType.Name), set);
                        }
                        _dbSets.Add(_dataContextType, dbSets);
                    }
                }
            }
            #endregion

            #region 给DbSet<T>属性赋值
            foreach (var dbSetProperty in dbSetProperties)
            {
                dbSetProperty.Value.SetValue(this, dbSets[StringHelper.ToPlural(dbSetProperty.Value.PropertyType.GetGenericArguments()[0].Name)]);
            }
            #endregion

            var entityConfiguration = EntityCfgManager.Value;

            ConfigurationModel(entityConfiguration);

            if (AllowCreateTable())
            {
                foreach (var entityType in _entityTypes)
                {
                    CreateTable(entityType);
                }
            }
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name">连接字符串名</param>
        public DataContext(string name)
        {
            var conn = ConfigurationManager.ConnectionStrings[name];
            if (conn == null)
            {
                throw new Exception("连接字符串" + name + "未找到");
            }
            Init(conn.ConnectionString, conn.ProviderName);
        }
        #endregion

        #region 保存到数据库
        /// <summary>
        /// 保存更改到数据库，若保存成功则，清空缓存
        /// </summary>
        /// <returns></returns>
        public int SaveChanges()
        {
            Dictionary<string, object> dbSets;
            if (!_dbSets.TryGetValue(_dataContextType, out dbSets))
            {
                throw new Exception("初始有问题");
            }
            var count = 0;
            using (var scope = new TransactionScope())
            {
                var op = Provider.CreateEntityOperator();
                foreach (IEntityOperator dbSet in dbSets.Values)
                {
                    var list = dbSet.GetAdding();
                    var total = op.InsertEntities(list);
                    if (total != list.Count)
                    {
                        throw new Exception("批量插入失败");
                    }
                    var entityType = dbSet.GetEntityType();
                    if (total <= 10)
                    {
                        var entityOp = GetEntityOperator(entityType);
                        entityOp.AddEditing(list);
                    }
                    count += total;
                    var editings = dbSet.GetEditing();
                    var table = EntityCfgManager.Value.GetTable(entityType);
                    var keyColumn = table.Columns.FirstOrDefault(x => x.Value.IsKey).Value;
                    if (keyColumn == null)
                    {
                        throw new InvalidOperationException("实体" + entityType.FullName + "不存在主键，无法更新");
                    }
                    var getters = ExpressionReflector.GetGetters(entityType);
                    var keyGetter = getters.GetOrDefault(keyColumn.PropertyInfo.Name);
                    if (keyGetter == null)
                    {
                        throw new InvalidOperationException("keyGetter为null");
                    }
                    foreach (var editing in editings)
                    {
                        var iGetUpdatedValue = editing as IGetUpdatedValues;
                        if (iGetUpdatedValue == null)
                        {
                            continue;
                        }
                        var values = iGetUpdatedValue.GetUpdatedValues();
                        values.Remove(keyColumn.PropertyInfo.Name);
                        if (values.Count <= 0)
                        {
                            continue;
                        }
                        var keyValue = keyGetter(editing);
                        var value = values.GetOrDefault(keyColumn.Name);
                        if (value == null)
                        {
                            values.Add(keyColumn.Name, keyValue);
                        }
                        else
                        {
                            values[keyColumn.Name] = keyValue;
                        }
                        count += op.UpdateValues(keyColumn, table, values);
                    }

                    var removings = dbSet.GetRemoving();
                    var ids = new List<int>();
                    foreach (var removing in removings)
                    {
                        var kv = keyGetter(removing);
                        if (kv == null)
                        {
                            throw new InvalidOperationException("删除时主键必须有值");
                        }
                        ids.Add(Convert.ToInt32(kv));
                    }
                    if (ids.Any())
                    {
                        count += op.Delete(keyColumn, table, ids.ToArray());
                    }
                }
                scope.Complete();
                foreach (IEntityOperator item in dbSets.Values)
                {
                    item.ClearAdding();
                    item.ClearRemoving();
                }
            }
            return count;
        }
        #endregion

    }
}
