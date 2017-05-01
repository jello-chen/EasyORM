using System.Collections.Generic;

namespace EasyORM.DynamicObject
{
    public interface IGetUpdatedValues
    {
        Dictionary<string, object> GetUpdatedValues();
    }
}
