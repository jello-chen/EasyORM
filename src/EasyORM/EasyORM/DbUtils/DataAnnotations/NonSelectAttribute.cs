using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.DbUtils.DataAnnotations
{
    /// <summary>
    /// 指示该字段不要加入到select语句中
    /// </summary>
    public class NonSelectAttribute : Attribute
    {
    }
}
