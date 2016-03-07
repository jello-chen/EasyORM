using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.DbUtils
{
    /// <summary>
    /// column type
    /// </summary>
    public enum KeyColumnType
    {
        /// <summary>
        /// manually specify
        /// </summary>
        None = 0,
        /// <summary>
        /// come from sequent table
        /// </summary>
        Sequence = 1,
        /// <summary>
        /// automatically increase
        /// </summary>
        AutoIncreament = 2
    }
}
