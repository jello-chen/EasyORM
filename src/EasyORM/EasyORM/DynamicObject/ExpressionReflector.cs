using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasyORM.Utils;

namespace EasyORM.DynamicObject
{
    /// <summary>
    /// 通过表达式树实现类似于反射的功能
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ExpressionReflector<TEntity>
    {
        private static Dictionary<Type, Dictionary<string, Func<TEntity, object>>> _entityGetters;
        private static Dictionary<Type, Dictionary<string, Action<TEntity, object>>> _entitySetters;
        private Type _entityType;
        private static Dictionary<Type, Dictionary<string, Func<object, object>>> _objectGetters;
        private static Dictionary<Type, Dictionary<string, Action<object, object>>> _objectSetters;
        private static Dictionary<Type, Func<IDataReader, object>> _reader2Objects = new Dictionary<Type, Func<IDataReader, object>>();
        private static Type _objectType;
        private PropertyInfo[] _properties;

        static ExpressionReflector()
        {
            ExpressionReflector<TEntity>._entitySetters = new Dictionary<Type, Dictionary<string, Action<TEntity, object>>>();
            ExpressionReflector<TEntity>._entityGetters = new Dictionary<Type, Dictionary<string, Func<TEntity, object>>>();
            ExpressionReflector<TEntity>._objectGetters = new Dictionary<Type, Dictionary<string, Func<object, object>>>();
            ExpressionReflector<TEntity>._objectSetters = new Dictionary<Type, Dictionary<string, Action<object, object>>>();
            ExpressionReflector<TEntity>._objectType = typeof(object);
        }

        public ExpressionReflector()
        {
            this._entityType = typeof(TEntity);
            this._properties = GetProperties(this._entityType);
        }

        private Dictionary<string, Func<TEntity, object>> GetGetters()
        {
            Dictionary<string, Func<TEntity, object>> dictionary = null;
            if (!_entityGetters.TryGetValue(_entityType, out dictionary))
            {
                lock (_entityGetters)
                {
                    if (!_entityGetters.TryGetValue(_entityType, out dictionary))
                    {
                        dictionary = new Dictionary<string, Func<TEntity, object>>();
                        foreach (PropertyInfo info in this._properties)
                        {
                            ParameterExpression expression = Expression.Parameter(this._entityType);
                            Func<TEntity, object> func = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(Expression.Property(expression, info), _objectType), new ParameterExpression[] { expression }).Compile();
                            dictionary.Add(info.Name, func);
                        }
                        _entityGetters.Add(this._entityType, dictionary);
                    }
                }
            }
            return dictionary;
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
            Dictionary<string, Action<TEntity, object>> dictionary = null;
            if (!_entitySetters.TryGetValue(_entityType, out dictionary))
            {
                lock (_entitySetters)
                {
                    if (!_entitySetters.TryGetValue(_entityType, out dictionary))
                    {
                        dictionary = new Dictionary<string, Action<TEntity, object>>();
                        foreach (PropertyInfo info in this._properties)
                        {
                            var setMethod = info.GetSetMethod();
                            if (setMethod == null)
                            {
                                continue;
                            }
                            ParameterExpression instance = Expression.Parameter(this._entityType);
                            ParameterExpression expression = Expression.Parameter(_objectType);
                            UnaryExpression expression3 = Expression.Convert(expression, info.PropertyType);
                            Action<TEntity, object> action = Expression.Lambda<Action<TEntity, object>>(Expression.Call(instance, setMethod, new Expression[] { expression3 }), new ParameterExpression[] { instance, expression }).Compile();
                            dictionary.Add(info.Name, action);
                        }
                        _entitySetters.Add(this._entityType, dictionary);
                    }
                }
            }
            return dictionary;
        }

        public object GetValue(TEntity entity, string propertyName)
        {
            var getters = GetGetters();
            Func<TEntity, object> getter = null;
            if (!getters.TryGetValue(propertyName, out getter))
            {
                throw new Exception("Getter未初始化完整");
            }
            return getter(entity);
        }

        public void SetValue(TEntity entity, string propertyName, object value)
        {
            var setters = GetSetters();
            Action<TEntity, object> setter = null;
            if (!setters.TryGetValue(propertyName, out setter))
            {
                throw new Exception("Setter未初始化完整");
            }
            setter(entity, value);
        }

        public PropertyInfo[] Properties
        {
            get
            {
                return this._properties;
            }
        }

        public Dictionary<string, Func<TEntity, object>> GetPropertyGetters()
        {
            return GetGetters();
        }
        public Dictionary<string, Action<TEntity, object>> GetPropertySetters()
        {
            return GetSetters();
        }
    }

    /// <summary>
    /// 通过表达式树实现类似于反射的功能
    /// </summary>
    public class ExpressionReflector
    {
        private static IDictionary<Type, Dictionary<string, Func<object, object>>> _objectGetters = new Dictionary<Type, Dictionary<string, Func<object, object>>>();
        private static IDictionary<Type, Dictionary<string, Action<object, object>>> _objectSetters = new Dictionary<Type, Dictionary<string, Action<object, object>>>();
        private static IDictionary<Type, Action<object, object>[]> _objectSettersArray = new Dictionary<Type, Action<object, object>[]>();
        //private static IDictionary<Type, Dictionary<string, Action<object, object[]>>> _objectMethods = new Dictionary<Type, Dictionary<string, Action<object, object[]>>>();
        private static Dictionary<Type, Func<object[], object>> _objectConstructors = new Dictionary<Type, Func<object[], object>>();

        public static Dictionary<string, Func<object, object>> GetGetters(Type entityType)
        {
            Dictionary<string, Func<object, object>> dictionary = null;
            if (!_objectGetters.TryGetValue(entityType, out dictionary))
            {
                lock (_objectGetters)
                {
                    if (!_objectGetters.TryGetValue(entityType, out dictionary))
                    {
                        dictionary = new Dictionary<string, Func<object, object>>();
                        foreach (PropertyInfo info in ExpressionReflectorCore.GetProperties(entityType).Values)
                        {
                            ParameterExpression expression = Expression.Parameter(ExpressionReflectorCore.ObjectType);
                            Expression entityExp = Expression.Convert(expression, entityType);
                            Func<object, object> func = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(entityExp, info), ExpressionReflectorCore.ObjectType), new ParameterExpression[] { expression }).Compile();
                            dictionary.Add(info.Name, func);
                        }
                        _objectGetters.Add(entityType, dictionary);
                    }
                }
            }
            return dictionary;
        }

        public static Dictionary<string, object> GetPropertyValues(object entity)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Dictionary<string, Func<object, object>> getters = GetGetters(entity.GetType());
            foreach (KeyValuePair<string, Func<object, object>> pair in getters)
            {
                dictionary.Add(pair.Key, pair.Value(entity));
            }
            return dictionary;
        }

        public static Func<object[], object> GetConstructor(Type type, ObjectPropertyConvertType convertType)
        {
            Func<object[], object> ctorDelegate = null;
            if (!_objectConstructors.TryGetValue(type, out ctorDelegate))
            {
                lock (_objectConstructors)
                {
                    if (!_objectConstructors.TryGetValue(type, out ctorDelegate))
                    {
                        Expression newExp;
                        var parameterExp = Expression.Parameter(ReflectorConsts.ObjectArrayType);
                        if (TypeHelper.IsValueType(type))
                        {
                            newExp = Expression.New(type);
                        }
                        else
                        {
                            var ctorInfo = type.GetConstructors()[0];
                            List<Expression> expList = new List<Expression>();
                            var parameterList = ctorInfo.GetParameters();
                            for (int i = 0; i < parameterList.Length; i++)
                            {
                                var parameter = parameterList[i];
                                var paramObj = Expression.ArrayIndex(parameterExp, Expression.Constant(i));
                                var nullable = TypeHelper.IsNullableType(parameter.ParameterType);
                                var parameterType = parameter.ParameterType;
                                if (nullable)
                                    parameterType = TypeHelper.GetUnderlyingType(parameterType);
                                Expression expObj = null;
                                switch (convertType)
                                {
                                    case ObjectPropertyConvertType.ConvertTo: MethodInfo method = null;
                                        if (parameterType == ReflectorConsts.DateTimeType)
                                        {
                                            method = ReflectorConsts.ConvertToDateTimeMethod;
                                            expObj = Expression.Call(null, ReflectorConsts.ConvertToDateTimeMethod, paramObj);
                                        }
                                        else if (parameterType == ReflectorConsts.StringType)
                                        {
                                            method = ReflectorConsts.ConvertToStringMethod;
                                        }
                                        else if (parameterType == ReflectorConsts.Int32Type)
                                        {
                                            method = ReflectorConsts.ConvertToInt32Method;
                                        }
                                        else if (parameterType == ReflectorConsts.BoolType)
                                        {
                                            method = ReflectorConsts.ConvertToBoolMethod;
                                        }
                                        else
                                        {
                                            throw new Exception("不支持：" + parameterType);
                                        }
                                        expObj = Expression.Call(null, method, paramObj);
                                        if (nullable)
                                        {
                                            expObj = Expression.Convert(expObj, parameter.ParameterType);
                                        }
                                        break;
                                    case ObjectPropertyConvertType.Cast:
                                        expObj = Expression.Convert(paramObj, parameter.ParameterType);
                                        break;
                                }
                                //if (nullable && convertType != ObjectPropertyConvertType.Cast)
                                //    expObj = Expression.Convert(expObj, parameter.ParameterType);
                                expList.Add(expObj);
                            }
                            newExp = Expression.New(ctorInfo, expList.ToArray());
                        }
                        ctorDelegate = Expression.Lambda<Func<object[], object>>(newExp, parameterExp).Compile();
                        _objectConstructors.Add(type, ctorDelegate);
                    }
                }
            }
            return ctorDelegate;
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="convertType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object CreateInstance(Type type, ObjectPropertyConvertType convertType, params object[] parameters)
        {
            Func<object[], object> ctorDelegate = GetConstructor(type, convertType);
            return ctorDelegate(parameters);
        }

        static Dictionary<Type, Func<IDataReader, IList>> _dataReader2ListCahce = new Dictionary<Type, Func<IDataReader, IList>>();

        /// <summary>
        /// 获取一个IDataReader转List的委托
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Func<IDataReader, IList> GetDataReaderMapeer(Type type,IDataReader reader)
        {
            Func<IDataReader, IList> func = null;
            if (!_dataReader2ListCahce.TryGetValue(type, out func))
            {
                lock (_dataReader2ListCahce)
                {
                    if (!_dataReader2ListCahce.TryGetValue(type, out func))
                    {
                        var readerExp = Expression.Parameter(ReflectorConsts.IDataReaderType, "reader");
                        var properties = ExpressionReflector.GetProperties(type);
                        var fieldCount = reader.FieldCount;
                        var expressions = new List<Expression>();
                        var objVar = Expression.Variable(type, "entity");
                        var fieldCountVar = Expression.Variable(ReflectorConsts.Int32Type, "fieldCount");
                        var readerVar = Expression.Variable(ReflectorConsts.IDataReaderType, "readerVar");
                        var propertyNameArr = Expression.Variable(ReflectorConsts.StringArrayType, "pis");
                        var indexArrVar = Expression.Variable(ReflectorConsts.Int32ArrayType, "indexes");
                        var readIndexVar = Expression.Variable(ReflectorConsts.Int32Type, "readIndex");
                        var indexVar = Expression.Variable(ReflectorConsts.Int32Type, "index");
                        var forBreakLabel = Expression.Label("forBreak");
                        var assignIndexVar = Expression.Assign(indexVar, Expression.Constant(0));
                        var listType = ReflectorConsts.ListType.MakeGenericType(type);
                        var listVar = Expression.Variable(listType, "list");
                        expressions.Add(Expression.Assign(listVar, Expression.New(listType)));
                        expressions.Add(Expression.Assign(readerVar, readerExp));
                        expressions.Add(assignIndexVar);
                        var assignFieldCountVar = Expression.Assign(fieldCountVar,
                                Expression.MakeMemberAccess(readerVar, ReflectorConsts.FieldCountOfIDataReader)
                            );
                        expressions.Add(assignFieldCountVar);
                        var readNameExp = Expression.Call(readerVar, ReflectorConsts.GetOrdinalOfIDataReader, Expression.ArrayIndex(propertyNameArr, indexVar));
                        var initIndexArray = Expression.Assign(indexArrVar, Expression.NewArrayBounds(ReflectorConsts.Int32Type, Expression.Constant(fieldCount)));
                        var initPropertyArrayExpressions = new List<Expression>();
                        for (int i = 0; i < fieldCount; i++)
                        {
                            initPropertyArrayExpressions.Add(Expression.Constant(reader.GetName(i)));
                        }
                        var initPropertyArray = Expression.Assign(propertyNameArr, Expression.NewArrayInit(ReflectorConsts.StringType, initPropertyArrayExpressions));
                        var assignIndexArrayVar = Expression.Assign(Expression.ArrayAccess(indexArrVar, indexVar), readNameExp);
                        expressions.Add(initPropertyArray);
                        expressions.Add(initIndexArray);
                        expressions.Add(Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.LessThan(indexVar, fieldCountVar),
                                    Expression.Block(
                                        assignIndexArrayVar,
                                        Expression.Assign(
                                            indexVar,
                                            Expression.Add(indexVar, Expression.Constant(1))
                                        )
                                    ),
                                    Expression.Break(forBreakLabel)
                                ),
                                forBreakLabel
                            ));
                        Expression body = null;
                        DataReaderGetMethodSwitcher switcher = null;
                        var labelTarget = Expression.Label(type, "return");
                        var paramterExpressions = new List<ParameterExpression>();
                        var setEntityExpressions = new List<Expression>();
                        if (TypeHelper.IsCompilerGenerated(type))
                        {
                            var constructor = type.GetConstructors().FirstOrDefault();
                            if (constructor == null)
                            {
                                throw new ArgumentException("类型" + type.FullName + "未找到构造方法");
                            }
                            var parameters = constructor.GetParameters();
                            var expressionParams = new List<ParameterExpression>();
                            for (int i = 0; i < fieldCount; i++)
                            {
                                var parameter = parameters[i];
                                var parameterVar = Expression.Variable(parameter.ParameterType, parameter.Name);
                                var parameterType = TypeHelper.GetUnderlyingType(parameter.ParameterType);
                                switcher = new DataReaderGetMethodSwitcher(parameterType, readIndexVar, readerVar);
                                switcher.Process();
                                var rightExp = (Expression)switcher.Result;
                                if (TypeHelper.IsNullableType(parameter.ParameterType))
                                {
                                    rightExp = Expression.Convert(rightExp, parameter.ParameterType);
                                }
                                var isNullExp = Expression.Call(readerExp, ReflectorConsts.IsDBNullfIDataReader, readIndexVar);
                                var ifExp = Expression.IfThenElse(isNullExp, Expression.Assign(parameterVar, Expression.Default(parameter.ParameterType)), Expression.Assign(parameterVar, rightExp));
                                var exps = new List<Expression>();
                                setEntityExpressions.Add(
                                    Expression.Assign(
                                        readIndexVar,
                                        Expression.ArrayIndex(indexArrVar, Expression.Constant(i))
                                    )
                                );
                                setEntityExpressions.Add(ifExp);
                                expressionParams.Add(parameterVar);
                            }
                            setEntityExpressions.Add(Expression.Assign(objVar, Expression.New(constructor, expressionParams)));
                            paramterExpressions.AddRange(expressionParams);
                            paramterExpressions.Add(readerVar);
                            paramterExpressions.Add(listVar);
                        }
                        else
                        {
                            var newExp = Expression.New(type);
                            setEntityExpressions.Add(Expression.Assign(objVar, newExp));
                            for (int i = 0; i < fieldCount; i++)
                            {
                                var propertyName = reader.GetName(i);
                                var property = properties.GetOrDefault(propertyName);
                                if (property == null)
                                {
                                    continue;
                                }
                                var propertyAssignExpressions = new List<Expression>();
                                var propertyExp = Expression.Property(objVar, property);
                                var propertyType = TypeHelper.GetUnderlyingType(property.PropertyType);
                                Expression rightExp = null;
                                switcher = new DataReaderGetMethodSwitcher(propertyType, readIndexVar, readerVar);
                                switcher.Process();
                                rightExp = (Expression)switcher.Result;
                                if (TypeHelper.IsNullableType(property.PropertyType))
                                {
                                    rightExp = Expression.Convert(rightExp, property.PropertyType);
                                }
                                setEntityExpressions.Add(
                                    Expression.Assign(
                                        readIndexVar,
                                        Expression.ArrayIndex(indexArrVar, Expression.Constant(i))
                                    )
                                );
                                var ifExp = Expression.IfThen(
                                    Expression.Not(
                                        Expression.Call(readerExp, ReflectorConsts.IsDBNullfIDataReader, readIndexVar)
                                    ),
                                    Expression.Assign(propertyExp, rightExp)
                                );
                                setEntityExpressions.Add(ifExp);
                            }
                            paramterExpressions.Add(listVar);
                            paramterExpressions.Add(readerVar);
                        }
                        paramterExpressions.Add(indexVar);
                        paramterExpressions.Add(propertyNameArr);
                        paramterExpressions.Add(fieldCountVar);
                        paramterExpressions.Add(indexArrVar);
                        paramterExpressions.Add(readIndexVar);
                        //expressions.Add(Expression.Call(
                        //        null,
                        //        typeof(MessageBox).GetMethod("Show", new Type[] { ReflectorConsts.StringType }),
                        //    Expression.Call(
                        //        null,
                        //        ReflectorConsts.ConvertToStringMethod,
                        //        Expression.Convert(
                        //            Expression.ArrayIndex(indexArrVar, Expression.Constant(1)),
                        //            ReflectorConsts.ObjectType)
                        //        )));
                        setEntityExpressions.Add(Expression.Call(listVar, listType.GetMethods().FirstOrDefault(x => x.Name == "Add"), objVar));
                        expressions.Add(
                            Expression.Loop(
                                Expression.Block(
                                    Expression.IfThenElse(
                                        Expression.Call(readerVar, ReflectorConsts.ReadOfIDataReader),
                                        Expression.Block(new[] { objVar }, setEntityExpressions),
                                        Expression.Break(labelTarget, Expression.Default(type))
                                    )
                                ),
                                labelTarget
                            ));
                        expressions.Add(listVar);
                        body = Expression.Block(
                            paramterExpressions,
                            expressions
                        );
                        func = Expression.Lambda<Func<IDataReader, IList>>(body, readerExp).Compile();
                        _dataReader2ListCahce.Add(type, func);
                    }
                }
            }
            return func;
        }
        public static Dictionary<string, Action<object, object>> GetSetters(Type entityType)
        {
            Dictionary<string, Action<object, object>> dictionary = null;
            if (!_objectSetters.TryGetValue(entityType, out dictionary))
            {
                lock (_objectSetters)
                {
                    if (!_objectSetters.TryGetValue(entityType, out dictionary))
                    {
                        dictionary = new Dictionary<string, Action<object, object>>();
                        foreach (PropertyInfo info in ExpressionReflectorCore.GetProperties(entityType).Values)
                        {
                            var setMethod = info.GetSetMethod();
                            if (setMethod == null)
                            {
                                continue;
                            }
                            ParameterExpression instance = Expression.Parameter(ExpressionReflectorCore.ObjectType);
                            Expression instanceObj = Expression.Convert(instance, entityType);
                            ParameterExpression valueExp = Expression.Parameter(ExpressionReflectorCore.ObjectType);
                            UnaryExpression valueObjExp = Expression.Convert(valueExp, info.PropertyType);
                            var propertyExp = Expression.Property(instanceObj, info);
                            var assignExp = Expression.Assign(propertyExp, valueObjExp);
                            //Action<object, object> action = Expression.Lambda<Action<object, object>>(Expression.Call(instanceObj, setMethod, new Expression[] { valueObjExp }), new ParameterExpression[] { instance, valueExp }).Compile();
                            Action<object, object> action = Expression.Lambda<Action<object, object>>(assignExp, new ParameterExpression[] { instance, valueExp }).Compile();
                            dictionary.Add(info.Name, action);
                        }
                        _objectSetters.Add(entityType, dictionary);
                    }
                }
            }
            return dictionary;
        }

        public static object GetValue(object entity, string propertyName)
        {
            var getters = GetGetters(entity.GetType());
            Func<object, object> getter = null;
            if (!getters.TryGetValue(propertyName, out getter))
            {
                throw new Exception("Getter未初始化完整");
            }
            return getter(entity);
        }

        public static void SetValue(object entity, string propertyName, object value)
        {
            var setters = GetSetters(entity.GetType());
            Action<object, object> setter = null;
            if (!setters.TryGetValue(propertyName, out setter))
            {
                throw new Exception("Setter未初始化完整");
            }
            setter(entity, value);
        }

        public static IDictionary<string, PropertyInfo> GetProperties(Type type)
        {
            return ExpressionReflectorCore.GetProperties(type);
        }

        /// <summary>
        /// 只编译出委托，不进行任何缓存
        /// </summary>
        /// <param name="proxyObject"></param>
        /// <param name="methodName"></param>
        /// <param name="argTypes"></param>
        public static Action<object, object[]> GetMethodDelegate(object proxyObject, string methodName, params Type[] argTypes)
        {
            var proxyType = proxyObject.GetType();
            var method = proxyType.GetMethod(methodName, argTypes);
            if (method == null)
            {
                throw new ArgumentException("指定方法未找到");
            }
            ParameterExpression expression = Expression.Parameter(ExpressionReflectorCore.ObjectType);
            UnaryExpression expression2 = Expression.Convert(expression, proxyType);
            ParameterExpression array = Expression.Parameter(typeof(object[]));
            List<Expression> list = new List<Expression>();
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo info2 = parameters[i];
                UnaryExpression item = Expression.Convert(Expression.ArrayIndex(array, Expression.Constant(i)), parameters[i].ParameterType);
                list.Add(item);
            }
            Expression instance = method.IsStatic ? null : Expression.Convert(expression, method.ReflectedType);
            return Expression.Lambda<Action<object, object[]>>(Expression.Call(instance, method, list.ToArray()), new ParameterExpression[] { expression, array }).Compile();
        }

        public static Type GetNullableOrSelfType(Type type)
        {
            Type result = Nullable.GetUnderlyingType(type);
            if (result == null)
            {
                return type;
            }
            return result;
        }

        public static bool IsEntityPropertyType(Type type)
        {
            return ExpressionReflectorCore.EntityPropertyTypes.Contains(type);
        }
    }
}
