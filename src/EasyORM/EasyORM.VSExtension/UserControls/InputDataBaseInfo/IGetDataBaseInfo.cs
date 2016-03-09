using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyORM.VSExtension.UserControls.InputDataBaseInfo
{
    public interface IGetDataBaseInfo
    {
        string ConnectionStringName { get; }
        string ConnectionString { get; }
    }
}
