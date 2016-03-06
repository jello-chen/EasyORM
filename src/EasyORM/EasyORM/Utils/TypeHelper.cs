using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Utils
{
    public class TypeHelper
    {
        /// <summary>
        /// 如果是可空类型，则获取真实类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(Type type)
        {
            if (!type.IsGenericType)
            {
                return type;
            }
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// 判断是否可空类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsGenericType)
            {
                return false;
            }
            if (type.GetGenericTypeDefinition() == ReflectorConsts.NullableType)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否编译器生成的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCompilerGenerated(Type type)
        {
            return type.GetCustomAttributes(ReflectorConsts.CompilerGeneratedAttributeType, false).Any();
        }

        /// <summary>
        /// 是否基元类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(Type type)
        {
            type = GetUnderlyingType(type);
            if (type.IsPrimitive)
            {
                return true;
            }
            if (type == ReflectorConsts.DateTimeType || type == ReflectorConsts.StringType)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否枚举类型
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public static bool IsEnum(Type propertyType)
        {
            propertyType = GetUnderlyingType(propertyType);
            return propertyType.IsEnum;
        }
    }
}
