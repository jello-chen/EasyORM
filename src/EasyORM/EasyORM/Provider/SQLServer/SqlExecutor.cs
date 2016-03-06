using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Provider.SQLServer
{
    /// <summary>
    /// SQLServer SQLHelper
    /// </summary>
    public class SqlExecutor:SqlExecutorBase
    {
        public SqlExecutor(DataContext context)
            : base(context)
        {

        }
    }
}
