using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.DynamicObject
{
    /// <summary>
    /// 转换对象的属性类型的方法
    /// </summary>
    public enum ObjectPropertyConvertType
    {
        /// <summary>
        /// 调用Convert.ToXXX方法进行转换
        /// </summary>
        ConvertTo,

        /// <summary>
        /// 强制转换
        /// </summary>
        Cast
    }
}
