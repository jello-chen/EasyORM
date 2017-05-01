using EasyORM.Provider;

namespace EasyORM
{
    /// <summary>
    /// Database Configuration
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// ConnectionString
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// ProviderName
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Database Type
        /// </summary>
        public DatabaseTypes DatabaseType { get; set; }
    }
}
