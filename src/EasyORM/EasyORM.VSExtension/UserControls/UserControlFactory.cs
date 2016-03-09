using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;
using EasyORM.Provider;
using EasyORM.VSExtension.UserControls.InputDataBaseInfo;

namespace EasyORM.VSExtension.UserControls
{
    public class UserControlFactory
    {
        DatabaseTypes _databaseType;
        public UserControlFactory(DatabaseTypes databaseType)
        {
            _databaseType = databaseType;
        }
        public IGetDataBaseInfo CreateGetDataBaseInfoControl(string folder, string projectFolder, ProviderBase provider)
        {
            switch (_databaseType)
            {
                case DatabaseTypes.SQLServer:
                    return new SqlServerUserControl(folder);
                case DatabaseTypes.SQLite:
                    return new SQLiteUserControl(folder, projectFolder);
                case DatabaseTypes.MySql:
                    return new MySqlUserControl(folder, provider);
            }
            return null;
        }
    }
}
