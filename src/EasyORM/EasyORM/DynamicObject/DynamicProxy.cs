using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EasyORM.Utils;

namespace EasyORM.DynamicObject
{
    public class DynamicProxy
    {
        private const string DynamicAssemblyName = "DynamicAssembly"; 
        private const string DynamicModuleName = "DynamicAssemblyModule";
        private const string ProxyClassNameFormater = "{0}_Proxy";
        private static IDictionary<Type, IDictionary<string, Action<object, object[]>>> _propertyDirectSetCache = new Dictionary<Type, IDictionary<string, Action<object, object[]>>>();
        private static readonly Type ModifiedPropertyNamesType = typeof(Dictionary<string, object>);
        private const string ModifiedPropertyNamesFieldName = "ModifiedPropertyNames";
        private static ConstructorInfo modifiedPropertyTypeConstructor;
        private static Dictionary<Type, Type> dynmicProxyList = new Dictionary<Type, Type>();
        private static IDictionary<Type, IList<PropertyInfo>> proxyProperties = new Dictionary<Type, IList<PropertyInfo>>();
        private static MethodInfo addMethod;
        private static MethodInfo removeMethod;
        static DynamicProxy()
        {
            modifiedPropertyTypeConstructor = ModifiedPropertyNamesType.GetConstructor(new Type[0]);
            addMethod = ModifiedPropertyNamesType.GetMethod("Add", new Type[] { typeof(string), typeof(object) });
            removeMethod = ModifiedPropertyNamesType.GetMethod("Remove", new Type[] { typeof(string) });
        }
        private DynamicProxy()
        {
        }

        public const MethodAttributes GetSetMethodAttributes =
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.CheckAccessOnOverride | MethodAttributes.HideBySig | MethodAttributes.Virtual;

        /// <summary>
        /// Creates dynamic assembly,returns AssemblyBuilder
        /// </summary>
        /// <param name="isSavaDll"></param>
        /// <returns></returns>
        private static AssemblyBuilder DefineDynamicAssembly(bool isSavaDll = false)
        {
            AssemblyName DemoName = new AssemblyName(DynamicAssemblyName);
            AssemblyBuilderAccess assemblyBuilderAccess = isSavaDll
                ? AssemblyBuilderAccess.RunAndSave
                : AssemblyBuilderAccess.Run;
            AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(DemoName,
                assemblyBuilderAccess);
            return dynamicAssembly;
        }

        /// <summary>
        /// Creates dynamic module,returns ModuleBuilder
        /// </summary>
        /// <returns>ModuleBuilder</returns>
        private static ModuleBuilder DefineDynamicModule(AssemblyBuilder dynamicAssembly, bool save)
        {
            if (save)
            {
                return dynamicAssembly.DefineDynamicModule(DynamicModuleName, "test.dll");
            }
            return dynamicAssembly.DefineDynamicModule(DynamicModuleName);
        }

        public static Type CreateDynamicProxyType(Type type)
        {
            var proxyType = dynmicProxyList.GetOrDefault(type);
            if (proxyType != null)
            {
                return proxyType;
            }
            bool save = false;
#if DEBUG
            save=true;
#endif
            AssemblyBuilder assemblyBuilder = DefineDynamicAssembly(save);

            ModuleBuilder moduleBuilder = DefineDynamicModule(assemblyBuilder, save);
            string proxyClassName = string.Format(ProxyClassNameFormater + type.GetHashCode().ToString(), type.Name);

            TypeBuilder typeBuilderProxy = moduleBuilder.DefineType(proxyClassName, TypeAttributes.Public, type);
            CustomAttributeBuilder cab =
                new CustomAttributeBuilder(typeof(SerializableAttribute).GetConstructor(new Type[0]), new object[0]);
            typeBuilderProxy.SetCustomAttribute(cab);
            typeBuilderProxy.AddInterfaceImplementation(typeof(IGetUpdatedValues));
            
            //Defines a variable to store the modified property name
            FieldBuilder fbModifiedPropertyNames = typeBuilderProxy.DefineField(ModifiedPropertyNamesFieldName,
                ModifiedPropertyNamesType, FieldAttributes.Public);
            ConstructorBuilder constructorBuilder = typeBuilderProxy.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, null);
            ILGenerator ilgCtor = constructorBuilder.GetILGenerator();
            ilgCtor.Emit(OpCodes.Ldarg_0); //load the current class
            ilgCtor.Emit(OpCodes.Newobj, modifiedPropertyTypeConstructor); //push stack
            ilgCtor.Emit(OpCodes.Stfld, fbModifiedPropertyNames); //assign fbModifiedPropertyNames value
            ilgCtor.Emit(OpCodes.Ret); //return

            //get all properties of proxy and overwrite the get/set method of property
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                Type typePepropertyInfo = propertyInfo.PropertyType;

                PropertyBuilder propertyBuilder = typeBuilderProxy.DefineProperty(propertyName,
                    PropertyAttributes.SpecialName, typePepropertyInfo, null);

                var methodGet = typeBuilderProxy.DefineMethod("get_" + propertyName, GetSetMethodAttributes,
                    typePepropertyInfo, Type.EmptyTypes);
                var methodSet = typeBuilderProxy.DefineMethod("set_" + propertyName, GetSetMethodAttributes, null,
                    new Type[] { typePepropertyInfo });
                var methodDirectSet = typeBuilderProxy.DefineMethod("directSet_" + propertyName, GetSetMethodAttributes, null,
                    new Type[] { typePepropertyInfo });
                //il of get method
                #region Get Method
                var ilGetMethod = methodGet.GetILGenerator();
                ilGetMethod.DeclareLocal(propertyInfo.PropertyType);
                ilGetMethod.Emit(OpCodes.Ldarg_0);
                ilGetMethod.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                ilGetMethod.Emit(OpCodes.Stloc_0);
                ilGetMethod.Emit(OpCodes.Ldloc_0);
                ilGetMethod.Emit(OpCodes.Ret);
                #endregion
                //il of set method
                ILGenerator ilSetMethod = methodSet.GetILGenerator();
                ilSetMethod.Emit(OpCodes.Nop);
                ilSetMethod.Emit(OpCodes.Ldarg_0);
                ilSetMethod.Emit(OpCodes.Ldarg_1);
                ilSetMethod.Emit(OpCodes.Call, propertyInfo.GetSetMethod());


                ilSetMethod.Emit(OpCodes.Ldarg_0);
                ilSetMethod.Emit(OpCodes.Ldfld, fbModifiedPropertyNames);
                ilSetMethod.Emit(OpCodes.Ldstr, propertyInfo.Name);
                ilSetMethod.Emit(OpCodes.Callvirt, removeMethod);
                ilSetMethod.Emit(OpCodes.Pop);


                ilSetMethod.Emit(OpCodes.Ldarg_0);
                ilSetMethod.Emit(OpCodes.Ldfld, fbModifiedPropertyNames);
                ilSetMethod.Emit(OpCodes.Ldstr, propertyInfo.Name);
                ilSetMethod.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsValueType)
                {
                    ilSetMethod.Emit(OpCodes.Box, propertyInfo.PropertyType);
                }
                ilSetMethod.Emit(OpCodes.Callvirt, addMethod);
                ilSetMethod.Emit(OpCodes.Nop);
                ilSetMethod.Emit(OpCodes.Ret);

                ILGenerator ilDirectSetMehotd = methodDirectSet.GetILGenerator();
                ilDirectSetMehotd.Emit(OpCodes.Ldarg_0);
                ilDirectSetMehotd.Emit(OpCodes.Ldarg_1);
                ilDirectSetMehotd.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                ilDirectSetMehotd.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(methodGet);
                propertyBuilder.SetSetMethod(methodSet);
            }
            //generates GetUpdatedValues method，clear original value after returning the updated value 
            var getValueMethodBuilder = typeBuilderProxy.DefineMethod("GetUpdatedValues", MethodAttributes.Assembly | MethodAttributes.Virtual, typeof(Dictionary<string, object>), Type.EmptyTypes);
            var getValueIlGen = getValueMethodBuilder.GetILGenerator();
            getValueIlGen.DeclareLocal(typeof(Dictionary<string, object>));
            getValueIlGen.DeclareLocal(typeof(Dictionary<string, object>));
            //getValueIlGen.Emit(OpCodes.Nop);
            getValueIlGen.Emit(OpCodes.Ldarg_0);
            getValueIlGen.Emit(OpCodes.Ldfld, fbModifiedPropertyNames);
            getValueIlGen.Emit(OpCodes.Newobj, typeof(Dictionary<string, object>)
                .GetConstructor(new Type[] { 
            typeof(IDictionary<string,object>)
            }));
            getValueIlGen.Emit(OpCodes.Stloc_0);
            getValueIlGen.Emit(OpCodes.Ldarg_0);
            getValueIlGen.Emit(OpCodes.Ldfld, fbModifiedPropertyNames);
            getValueIlGen.Emit(OpCodes.Callvirt, typeof(Dictionary<string, object>).GetMethod("Clear"));
            getValueIlGen.Emit(OpCodes.Nop);
            getValueIlGen.Emit(OpCodes.Ldloc_0);
            getValueIlGen.Emit(OpCodes.Stloc_1);
            getValueIlGen.Emit(OpCodes.Ldloc_1);
            getValueIlGen.Emit(OpCodes.Ret);
            typeBuilderProxy.DefineMethodOverride(getValueMethodBuilder, typeof(IGetUpdatedValues).GetMethods().FirstOrDefault());
            
            //build type by using dynamic proxy
            Type proxyClassType = typeBuilderProxy.CreateType();
            dynmicProxyList.Add(type, proxyClassType);
            if (save)
                assemblyBuilder.Save("test.dll");
            return proxyClassType;
        }

        /// <summary>
        /// Build dynamic proxy type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Type CreateDynamicProxyType<T>()
        {
            //overrite the Get/Set method of property and listen the Set method,
            //add the changed property value to list,
            //and if the proerty want to be listened,it should be virtual
            return CreateDynamicProxyType(typeof(T));
        }

        /// <summary>
        /// Gets the changed value of property.
        /// Notes:
        ///     The method only check whether the Set Method of property is invoked,
        ///     not check whether the property value is changed really
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetModifiedProperties(object obj)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(ModifiedPropertyNamesFieldName);
            if (fieldInfo == null) return new Dictionary<string, object>();
            object value = fieldInfo.GetValue(obj);
            return value as Dictionary<string, object>;
        }

        public static object CreateDynamicProxy(object entity)
        {
            var type = entity.GetType();
            Type proxyType = CreateDynamicProxyType(type);
            return CreateDynamicProxy(entity, type, proxyType);
        }

        static object CreateDynamicProxy(object entity, Type rawType, Type proxyType)
        {
            IList<PropertyInfo> properties;

            if (proxyProperties.ContainsKey(proxyType))
            {
                properties = proxyProperties[proxyType];
            }
            else
            {
                properties = ExpressionReflector.GetProperties(proxyType).Values.ToList();
                proxyProperties.Add(proxyType, properties);
            }

            var sourceGetters = ExpressionReflector.GetGetters(rawType);// new ExpressionReflector<T>().GetPropertyGetters();
            var proxyObject = ExpressionReflector.CreateInstance(proxyType, ObjectPropertyConvertType.Cast);
            IDictionary<string, Action<object, object[]>> proxyPropertyDirectSets = null;
            if (!_propertyDirectSetCache.TryGetValue(proxyType, out proxyPropertyDirectSets))
            {
                proxyPropertyDirectSets = new Dictionary<string, Action<object, object[]>>();
                foreach (var propertyInfo in properties)
                {
                    string name = propertyInfo.Name;
                    if (!sourceGetters.ContainsKey(name))
                    {
                        continue;
                    }
                    proxyPropertyDirectSets.Add(name, ExpressionReflector.GetMethodDelegate(proxyObject, "directSet_" + name, propertyInfo.PropertyType));

                }
            }
            foreach (var propertyInfo in properties)
            {
                string name = propertyInfo.Name;
                if (!sourceGetters.ContainsKey(name))
                {
                    continue;
                }
                var value = sourceGetters[name](entity);
                Action<object, object[]> directSetter = null;
                if (!proxyPropertyDirectSets.TryGetValue(name, out directSetter))
                {
                    throw new Exception("Not generate the direct set method successfully");
                }
                directSetter(proxyObject, new object[] { value });

            }
            return proxyObject;
        }

        public static T CreateDynamicProxy<T>(T entity)
        {
            var rawType = typeof(T);
            Type proxyType = CreateDynamicProxyType(rawType);
            return (T)CreateDynamicProxy(entity, rawType, proxyType);
            //IList<PropertyInfo> properties;

            //if (proxyProperties.ContainsKey(proxyType))
            //{
            //    properties = proxyProperties[proxyType];
            //}
            //else
            //{
            //    properties = ExpressionReflector.GetProperties(proxyType).Values.ToList();
            //    proxyProperties.Add(proxyType, properties);
            //}
            //Type type = typeof(T);

            //var sourceGetters = new ExpressionReflector<T>().GetPropertyGetters();
            ////var targetSetters = new ExpressionReflector<T>().GetPropertyGetters();
            //var proxyObject = ExpressionReflector.CreateInstance(proxyType);
            //IDictionary<string, Action<object, object[]>> proxyPropertyDirectSets = null;
            //if (!_propertyDirectSetCache.TryGetValue(proxyType, out proxyPropertyDirectSets))
            //{
            //    proxyPropertyDirectSets = new Dictionary<string, Action<object, object[]>>();
            //    foreach (var propertyInfo in properties)
            //    {
            //        string name = propertyInfo.Name;
            //        if (!sourceGetters.ContainsKey(name))
            //        {
            //            continue;
            //        }
            //        proxyPropertyDirectSets.Add(name, ExpressionReflector.GetMethodDelegate(proxyObject, "directSet_" + name, propertyInfo.PropertyType));

            //    }
            //}
            //foreach (var propertyInfo in properties)
            //{
            //    string name = propertyInfo.Name;
            //    if (!sourceGetters.ContainsKey(name))
            //    {
            //        continue;
            //    }
            //    var value = sourceGetters[name](entity);
            //    Action<object, object[]> directSetter = null;
            //    if (!proxyPropertyDirectSets.TryGetValue(name, out directSetter))
            //    {
            //        throw new Exception("Not generate the direct set method successfully");
            //    }
            //    directSetter(proxyObject, new object[] { value });

            //}
            //return (T)proxyObject;
        }

        public static T CreateDynamicProxy<T>()
        {
            return (T)ExpressionReflector.CreateInstance(CreateDynamicProxyType<T>(), ObjectPropertyConvertType.Cast);
        }

        public static bool IsProxy(Type type)
        {
            return type.Name.EndsWith(type.BaseType.Name + "_Proxy" + type.BaseType.GetHashCode().ToString());
        }
    }
}
