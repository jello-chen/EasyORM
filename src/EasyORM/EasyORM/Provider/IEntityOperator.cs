using EasyORM.SchemaModel;
using System.Collections;
using System.Collections.Generic;

namespace EasyORM.Provider
{
    public interface IEntityOperator
    {
        int InsertEntities(ArrayList list);
        int UpdateValues(Column keyColumn, Table table, Dictionary<string,object> values);

        int Delete(Column keyColumn, Table table, params int[] ids);
    }
}
