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
        /// Returns the underlying type argument of the specified nullable type
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
        /// Indicates whether the type is nullable
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
        /// Indicates whether the type is generated from compiler
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCompilerGenerated(Type type)
        {
            return type.GetCustomAttributes(ReflectorConsts.CompilerGeneratedAttributeType, false).Any();
        }

        /// <summary>
        /// Indicates whether property type is primitive type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(Type type)
        {
            int? i = 1;
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
        /// Indicates whether property type is enum
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
