using EasyORM.DbUtils.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.DbUtils
{
    public class ReflectorConsts : Utils.ReflectorConsts
    {
        public static readonly Type NonSelectAttributeType = typeof(NonSelectAttribute);
    }
}
