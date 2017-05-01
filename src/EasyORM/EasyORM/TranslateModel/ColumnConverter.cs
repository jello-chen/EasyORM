using System.Collections.Generic;
using System.Reflection;

namespace EasyORM.TranslateModel
{
    public class ColumnConverter
    {
        public ColumnConverter(MemberInfo memberInfo, List<object> parameters, bool isLeftColumn)
        {
            MemberInfo = memberInfo;
            Parameters = parameters;
            IsInstanceColumn = isLeftColumn;
        }
        public ColumnConverter(MemberInfo memberInfo, List<object> parameters)
            : this(memberInfo, parameters, false)
        {
        }
        public MemberInfo MemberInfo { get; set; }
        public List<object> Parameters { get; set; }

        public bool IsInstanceColumn { get; set; }
    }
}
