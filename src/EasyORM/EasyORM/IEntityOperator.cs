using System;
using System.Collections;

namespace EasyORM
{
    interface IEntityOperator
    {
        ArrayList GetAdding();
        void ClearAdding();

        void AddEditing(IList list);
        IList GetEditing();
        void ClearEditing();
        IList GetRemoving();
        void ClearRemoving();
        Type GetEntityType();
    }
}
