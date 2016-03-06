using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
