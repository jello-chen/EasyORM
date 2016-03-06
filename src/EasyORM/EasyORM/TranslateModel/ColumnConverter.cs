using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// 调用方是否为列
        /// </summary>
        public bool IsInstanceColumn { get; set; }
    }
}
