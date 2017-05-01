using EasyORM.DbUtils.DataAnnotations;
using System;

namespace EasyORM.DbUtils
{
    public class ReflectorConsts : Utils.ReflectorConsts
    {
        public static readonly Type NonSelectAttributeType = typeof(NonSelectAttribute);
    }
}
