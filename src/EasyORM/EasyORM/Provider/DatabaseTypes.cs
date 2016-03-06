using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Provider
{
    /// <summary>
    /// 数据库类型
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
