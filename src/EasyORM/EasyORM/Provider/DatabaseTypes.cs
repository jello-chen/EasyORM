using System.ComponentModel;

namespace EasyORM.Provider
{
    /// <summary>
    /// Database Type
    /// </summary>
    public enum DatabaseTypes
    {
        /// <summary>
        /// SqlServer
        /// </summary>
        [Description("SQLServer")]
        SQLServer,

        /// <summary>
        /// SQLite
        /// </summary>
        [Description("SQLite")]
        SQLite,
        /// <summary>
        /// MySql
        /// </summary>
        [Description("MySql")]
        MySql
    }
}
