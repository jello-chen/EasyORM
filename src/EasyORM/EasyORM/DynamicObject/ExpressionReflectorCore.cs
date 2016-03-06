using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EasyORM.DynamicObject
{
    internal class ExpressionReflectorCore
    {
        public static readonly Type ObjectType = typeof(object);
        private static IDictionary<Type, IDictionary<string, PropertyInfo>> _propertyInfos { get; set; }
        public static HashSet<Type> EntityPropertyTypes { get; private set; }
        static ExpressionReflectorCore()
        {
            _propertyInfos = new Dictionary<Type, IDictionary<string, PropertyInfo>>();
            EntityPropertyTypes = new HashSet<Type>();
            EntityPropertyTypes.Add(typeof(string));
            EntityPropertyTypes.Add(typeof(DateTime));
            EntityPropertyTypes.Add(typeof(DateTime?));
            EntityPropertyTypes.Add(typeof(int));
            EntityPropertyTypes.Add(typeof(short));
            EntityPropertyTypes.Add(typeof(long));
            EntityPropertyTypes.Add(typeof(int?));
            EntityPropertyTypes.Add(typeof(short?));
            EntityPropertyTypes.Add(typeof(long?));
            EntityPropertyTypes.Add(typeof(bool));
            EntityPropertyTypes.Add(typeof(bool?));
            EntityPropertyTypes.Add(typeof(decimal));
            EntityPropertyTypes.Add(typeof(decimal?));
            EntityPropertyTypes.Add(typeof(float?));
            EntityPropertyTypes.Add(typeof(float));
            EntityPropertyTypes.Add(typeof(double?));
            EntityPropertyTypes.Add(typeof(double));
            EntityPropertyTypes.Add(typeof(byte));
            EntityPropertyTypes.Add(typeof(byte?));
        }

        public static IDictionary<string, PropertyInfo> GetProperties(Type entityType)
        {
            IDictionary<string, PropertyInfo> properties = null;
            if (!_propertyInfos.TryGetValue(entityType, out properties))
            {
                lock (_propertyInfos)
                {
                    if (!_propertyInfos.TryGetValue(entityType, out properties))
                    {
                        properties = new Dictionary<string, PropertyInfo>();
                        foreach (var property in entityType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty))
                        {
                            var propertyType = property.PropertyType;
                            if (!EntityPropertyTypes.Contains(propertyType) && !propertyType.IsEnum)
                            {
                                continue;
                            }
                            properties.Add(property.Name, property);
                        }
                        _propertyInfos.Add(entityType, properties);
                    }
                }
            }
            return properties;
        }
    }
}
