namespace EasyORM.ObjectMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using EasyORM.DynamicObject;
    using EasyORM.Utils;

    public class EntityMapper
    {
        //private static Dictionary<Type, Dictionary<string, Func<object, object>>> _objectGetters = new Dictionary<Type, Dictionary<string, Func<object, object>>>();
        //private static Dictionary<Type, Dictionary<string, Action<object, object[]>>> _objectMethods = new Dictionary<Type, Dictionary<string, Action<object, object[]>>>();
        //private static Dictionary<Type, Dictionary<string, PropertyInfo>> _objectProperties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        //private static Dictionary<Type, Dictionary<string, Action<object, object>>> _objectSetters = new Dictionary<Type, Dictionary<string, Action<object, object>>>();
        //private static Type _objectType = typeof(object);


        //public static Dictionary<string, PropertyInfo> GetProperties(Type type)
        //{
        //    if (_objectProperties.ContainsKey(type))
        //    {
        //        return _objectProperties[type];
        //    }
        //    PropertyInfo[] properties = type.GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance);
        //    Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>();
        //    foreach (PropertyInfo info in properties)
        //    {
        //        dictionary.Add(info.Name, info);
        //    }
        //    _objectProperties.Add(type, dictionary);
        //    return dictionary;
        //}

        //public static Dictionary<string, Func<object, object>> GetPropertyGetters(Type objectType)
        //{
        //    Type key = objectType;
        //    if (_objectGetters.ContainsKey(key))
        //    {
        //        return _objectGetters[key];
        //    }
        //    Dictionary<string, Func<object, object>> dictionary = new Dictionary<string, Func<object, object>>();
        //    foreach (PropertyInfo info in GetProperties(key).Values)
        //    {
        //        ParameterExpression expression = Expression.Parameter(_objectType);
        //        dictionary.Add(info.Name, Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.MakeMemberAccess(Expression.Convert(expression, info.DeclaringType), info), _objectType), new ParameterExpression[] { expression }).Compile());
        //    }
        //    _objectGetters.Add(key, dictionary);
        //    return dictionary;
        //}

        //public static Dictionary<string, Action<object, object>> GetPropertySetters(Type objectType)
        //{
        //    if (_objectSetters.ContainsKey(objectType))
        //    {
        //        return _objectSetters[objectType];
        //    }
        //    Dictionary<string, Action<object, object>> dictionary = new Dictionary<string, Action<object, object>>();
        //    foreach (PropertyInfo info in GetProperties(objectType).Values)
        //    {
        //        ParameterExpression expression = Expression.Parameter(_objectType);
        //        Expression instance = Expression.Convert(expression, objectType);
        //        ParameterExpression expression3 = Expression.Parameter(_objectType);
        //        UnaryExpression expression4 = Expression.Convert(expression3, info.PropertyType);
        //        Action<object, object> action = Expression.Lambda<Action<object, object>>(Expression.Call(instance, info.GetSetMethod(), new Expression[] { expression4 }), new ParameterExpression[] { expression, expression3 }).Compile();
        //        dictionary.Add(info.Name, action);
        //    }
        //    lock (_objectSetters)
        //    {
        //        _objectSetters.Add(objectType, dictionary);
        //    }
        //    return dictionary;
        //}

        public static Dictionary<string, object> GetPropertyValues(object entity)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Dictionary<string, Func<object, object>> propertyGetters = ExpressionReflector.GetGetters(entity.GetType());
            foreach (KeyValuePair<string, Func<object, object>> pair in propertyGetters)
            {
                dictionary.Add(pair.Key, pair.Value(entity));
            }
            return dictionary;
        }

        public static Dictionary<string, object> GetPropertyValues<TEntity>(TEntity entity)
        {
            Type objectType = typeof(TEntity);
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Dictionary<string, Func<object, object>> propertyGetters = ExpressionReflector.GetGetters(objectType);
            foreach (KeyValuePair<string, Func<object, object>> pair in propertyGetters)
            {
                dictionary.Add(pair.Key, pair.Value(entity));
            }
            return dictionary;
        }

        //public static void Invoke(object entity, string methodName, params object[] args)
        //{
        //    Type key = entity.GetType();
        //    Action<object, object[]> action = null;
        //    if (_objectMethods.ContainsKey(key))
        //    {
        //        Dictionary<string, Action<object, object[]>> dictionary = _objectMethods[key];
        //        if (dictionary.ContainsKey(methodName))
        //        {
        //            action = dictionary[methodName];
        //        }
        //    }
        //    else
        //    {
        //        Dictionary<string, Action<object, object[]>> dictionary2 = new Dictionary<string, Action<object, object[]>>();
        //        _objectMethods.Add(key, dictionary2);
        //    }
        //    if (action == null)
        //    {
        //        ParameterExpression expression = Expression.Parameter(_objectType);
        //        UnaryExpression expression2 = Expression.Convert(expression, key);
        //        ParameterExpression array = Expression.Parameter(typeof(object[]));
        //        List<Expression> list = new List<Expression>();
        //        MethodInfo method = key.GetMethod(methodName);
        //        ParameterInfo[] parameters = method.GetParameters();
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            ParameterInfo info2 = parameters[i];
        //            UnaryExpression item = Expression.Convert(Expression.ArrayIndex(array, Expression.Constant(i)), parameters[i].ParameterType);
        //            list.Add(item);
        //        }
        //        Expression instance = method.IsStatic ? null : Expression.Convert(expression, method.ReflectedType);
        //        action = Expression.Lambda<Action<object, object[]>>(Expression.Call(instance, method, list.ToArray()), new ParameterExpression[] { expression, array }).Compile();
        //        _objectMethods[key].Add(methodName, action);
        //    }
        //    action(entity, args);
        //}

        public static List<TObject> Map<TObject>(DataSet ds) where TObject : class, new()
        {
            List<TObject> list = new List<TObject>();
            Type objectType = typeof(TObject);
            Dictionary<string, Action<object, object>> propertySetters = ExpressionReflector.GetSetters(objectType);
            var properties = ExpressionReflector.GetProperties(objectType);
            DataTable table = ds.Tables[0];
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                TObject local = Activator.CreateInstance<TObject>();
                foreach (string str in propertySetters.Keys)
                {
                    object obj2 = row[str];
                    if (obj2 != DBNull.Value)
                    {
                        Type propertyType = properties[str].PropertyType;
                        Type underlyingType = Nullable.GetUnderlyingType(propertyType);
                        if (underlyingType == null)
                        {
                            underlyingType = propertyType;
                        }
                        if (underlyingType.IsEnum)
                        {
                            obj2 = Enum.Parse(underlyingType, Convert.ToString(obj2));
                        }
                        else
                        {
                            obj2 = Convert.ChangeType(obj2, underlyingType);
                        }
                        propertySetters[str](local, obj2);
                    }
                }
                list.Add(local);
            }
            return list;
        }


        public static IList Map(Type objectType, IDataReader dataReader)
        {
            if (TypeHelper.IsValueType(objectType))
            {
                var list = (IList)Activator.CreateInstance(ReflectorConsts.ListType.MakeGenericType(objectType));
                while (dataReader.Read())
                {
                    if (dataReader.IsDBNull(0))
                    {
                        continue;
                    }
                    list.Add(dataReader.GetValue(0));
                }
                return list;
            }
            var mapper = ExpressionReflector.GetDataReaderMapeer(objectType, dataReader);
            return mapper(dataReader);
        }

        public static IList Map(Type objectType, DataSet ds, ObjectPropertyConvertType convertType)
        {
            var list = (IList)Activator.CreateInstance(ReflectorConsts.ListObjectType);
            Dictionary<string, Action<object, object>> propertySetters = ExpressionReflector.GetSetters(objectType);
            var properties = ExpressionReflector.GetProperties(objectType);
            DataTable table = ds.Tables[0];
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                object local = null;
                if (objectType.IsSealed)
                {
                    var parameterObjects = new List<object>();
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        var obj = row[column];
                        if (obj == DBNull.Value)
                        {
                            obj = null;
                        }
                        parameterObjects.Add(obj);
                    }
                    if (TypeHelper.IsValueType(objectType))
                    {
                        local = parameterObjects[0];
                    }
                    else
                    {
                        local = ExpressionReflector.CreateInstance(objectType, convertType, parameterObjects.ToArray());
                    }
                }
                else
                {
                    local = ExpressionReflector.CreateInstance(objectType, convertType);
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        object obj2 = row[column];
                        if (obj2 != DBNull.Value)
                        {
                            var property = properties.GetOrDefault(column.ColumnName);
                            if (property == null)
                            {
                                continue;
                            }
                            Type propertyType = property.PropertyType;
                            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
                            if (underlyingType == null)
                            {
                                underlyingType = propertyType;
                            }
                            if (underlyingType.IsEnum)
                            {
                                obj2 = Enum.Parse(underlyingType, Convert.ToString(obj2));
                            }
                            else
                            {
                                obj2 = Convert.ChangeType(obj2, underlyingType);
                            }
                            propertySetters.GetOrDefault(column.ColumnName)(local, obj2);
                        }
                    }
                }
                list.Add(local);
            }
            return list;
        }


        public static TTarget Map<TSource, TTarget>(TSource source)
        {
            TTarget target = (TTarget)ExpressionReflector.CreateInstance(typeof(TTarget), ObjectPropertyConvertType.Cast);
            Map<TSource, TTarget>(source, target);
            return target;
        }

        public static void Map<TSource, TTarget>(TSource source, TTarget target)
        {
            Type objectType = typeof(TSource);
            Type type2 = typeof(TTarget);
            Dictionary<string, Func<object, object>> propertyGetters = ExpressionReflector.GetGetters(objectType);
            Dictionary<string, Action<object, object>> propertySetters = ExpressionReflector.GetSetters(type2);
            foreach (string str in propertyGetters.Keys)
            {
                if (propertySetters.ContainsKey(str))
                {
                    propertySetters[str](target, propertyGetters[str](source));
                }
            }
        }

        //public static void SetValue(object entity, string propertyName, object value)
        //{
        //     GetPropertySetters(entity.GetType())[propertyName](entity, value);
        //}
    }
    public class EntityMapper<TEntity>
    {
        private static Dictionary<Type, Dictionary<string, Func<TEntity, object>>> _entityGetters;
        private static Dictionary<Type, Dictionary<string, Action<TEntity, object>>> _entitySetters;
        private Type _entityType;
        private static Dictionary<Type, Dictionary<string, Func<object, object>>> _objectGetters;
        private static Dictionary<Type, Dictionary<string, Action<object, object>>> _objectSetters;
        private static Type _objectType;
        private PropertyInfo[] _properties;

        static EntityMapper()
        {
            EntityMapper<TEntity>._entitySetters = new Dictionary<Type, Dictionary<string, Action<TEntity, object>>>();
            EntityMapper<TEntity>._entityGetters = new Dictionary<Type, Dictionary<string, Func<TEntity, object>>>();
            EntityMapper<TEntity>._objectGetters = new Dictionary<Type, Dictionary<string, Func<object, object>>>();
            EntityMapper<TEntity>._objectSetters = new Dictionary<Type, Dictionary<string, Action<object, object>>>();
            EntityMapper<TEntity>._objectType = typeof(object);
        }

        public EntityMapper()
        {
            this._entityType = typeof(TEntity);
            this._properties = EntityMapper<TEntity>.GetProperties(this._entityType);
        }

        private Dictionary<string, Func<TEntity, object>> GetGetters()
        {
            if (!EntityMapper<TEntity>._entityGetters.ContainsKey(this._entityType))
            {
                Dictionary<string, Func<TEntity, object>> dictionary = new Dictionary<string, Func<TEntity, object>>();
                foreach (PropertyInfo info in this._properties)
                {
                    ParameterExpression expression = Expression.Parameter(this._entityType);
                    Func<TEntity, object> func = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(Expression.Property(expression, info), EntityMapper<TEntity>._objectType), new ParameterExpression[] { expression }).Compile();
                    dictionary.Add(info.Name, func);
                }
                EntityMapper<TEntity>._entityGetters.Add(this._entityType, dictionary);
            }
            return EntityMapper<TEntity>._entityGetters[this._entityType];
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance);
        }

        public Dictionary<string, object> GetPropertyValues(TEntity entity)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Dictionary<string, Func<TEntity, object>> getters = this.GetGetters();
            foreach (KeyValuePair<string, Func<TEntity, object>> pair in getters)
            {
                dictionary.Add(pair.Key, pair.Value(entity));
            }
            return dictionary;
        }

        private Dictionary<string, Action<TEntity, object>> GetSetters()
        {
            if (EntityMapper<TEntity>._entitySetters.ContainsKey(this._entityType))
            {
                return EntityMapper<TEntity>._entitySetters[this._entityType];
            }
            Dictionary<string, Action<TEntity, object>> dictionary = new Dictionary<string, Action<TEntity, object>>();
            foreach (PropertyInfo info in this.Properties)
            {
                ParameterExpression instance = Expression.Parameter(this._entityType);
                ParameterExpression expression = Expression.Parameter(EntityMapper<TEntity>._objectType);
                UnaryExpression expression3 = Expression.Convert(expression, info.PropertyType);
                var setMethod = info.GetSetMethod();
                if (setMethod == null)
                {
                    continue;
                }
                Action<TEntity, object> action = Expression.Lambda<Action<TEntity, object>>(Expression.Call(instance, setMethod, new Expression[] { expression3 }), new ParameterExpression[] { instance, expression }).Compile();
                dictionary.Add(info.Name, action);
            }
            lock (EntityMapper<TEntity>._entitySetters)
            {
                EntityMapper<TEntity>._entitySetters.Add(this._entityType, dictionary);
            }
            return dictionary;
        }

        public object GetValue(TEntity entity, string propertyName)
        {
            return this.GetGetters()[propertyName](entity);
        }

        public TEntity Map(DataRow row)
        {
            TEntity local = Activator.CreateInstance<TEntity>();
            Dictionary<string, Action<TEntity, object>> setters = this.GetSetters();
            foreach (string str in setters.Keys)
            {
                object obj2 = row[str];
                if (obj2 != DBNull.Value)
                {
                    setters[str](local, obj2);
                }
            }
            return local;
        }
        public IList<TEntity> Map(DataSet ds)
        {
            var list = new List<TEntity>();
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                return list;
            }

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                list.Add(Map(item));
            }

            return list;
        }

        //public List<TModel> Map<TModel>(DataSet ds) where TModel : class, new()
        //{
        //    List<TModel> list = new List<TModel>();
        //    Dictionary<string, Action<object, object>> propertySetters = GetPropertySetters(typeof(TModel));
        //    DataTable table = ds.Tables[0];
        //    foreach (DataRow row in ds.Tables[0].Rows)
        //    {
        //        TModel local = Activator.CreateInstance<TModel>();
        //        foreach (string str in propertySetters.Keys)
        //        {
        //            object obj2 = row[str];
        //            if (obj2 != DBNull.Value)
        //            {
        //                propertySetters[str](local, obj2);
        //            }
        //        }
        //        list.Add(local);
        //    }
        //    return list;
        //}

        //public List<TEntity> Map(DataSet ds, bool createDynamicProxy = false)
        //{
        //    List<TEntity> list = new List<TEntity>();
        //    Dictionary<string, Action<TEntity, object>> setters = this.GetSetters();
        //    DataTable table = ds.Tables[0];
        //    foreach (DataRow row in ds.Tables[0].Rows)
        //    {
        //        TEntity local = Activator.CreateInstance<TEntity>();
        //        foreach (string str in setters.Keys)
        //        {
        //            object obj2 = row[str];
        //            if (obj2 != DBNull.Value)
        //            {
        //                setters[str](local, obj2);
        //            }
        //        }
        //        if (createDynamicProxy)
        //        {
        //            local = DynamicProxy.CreateDynamicProxy<TEntity>(local);
        //        }
        //        list.Add(local);
        //    }
        //    return list;
        //}

        public Dictionary<TKey, TEntity> Map<TKey>(DataSet ds, Func<TEntity, TKey> keySelector)
        {
            Dictionary<TKey, TEntity> dictionary = new Dictionary<TKey, TEntity>();
            Dictionary<string, Action<TEntity, object>> setters = this.GetSetters();
            DataTable table = ds.Tables[0];
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                TEntity local = Activator.CreateInstance<TEntity>();
                foreach (string str in setters.Keys)
                {
                    object obj2 = row[str];
                    if (obj2 != DBNull.Value)
                    {
                        setters[str](local, obj2);
                    }
                }
                dictionary.Add(keySelector(local), local);
            }
            return dictionary;
        }

        //public Dictionary<TKey, TModel> Map<TModel, TKey>(DataSet ds, Func<TModel, TKey> keySelector) where TModel : class, new()
        //{
        //    Dictionary<TKey, TModel> dictionary = new Dictionary<TKey, TModel>();
        //    Dictionary<string, Action<object, object>> propertySetters = EntityMapper.GetPropertySetters(typeof(TModel));
        //    DataTable table = ds.Tables[0];
        //    foreach (DataRow row in ds.Tables[0].Rows)
        //    {
        //        TModel local = Activator.CreateInstance<TModel>();
        //        foreach (string str in propertySetters.Keys)
        //        {
        //            object obj2 = row[str];
        //            if (obj2 != DBNull.Value)
        //            {
        //                propertySetters[str](local, obj2);
        //            }
        //        }
        //        dictionary.Add(keySelector(local), local);
        //    }
        //    return dictionary;
        //}

        public void SetValue(TEntity entity, string propertyName, object value)
        {
            this.GetSetters()[propertyName](entity, value);
        }

        public PropertyInfo[] Properties
        {
            get
            {
                return this._properties;
            }
        }
    }
}

