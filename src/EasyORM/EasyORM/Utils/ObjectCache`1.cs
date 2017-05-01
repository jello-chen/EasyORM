using System;
using System.Runtime.Remoting.Messaging;

namespace EasyORM.Utils
{
    /// <summary>
    /// A thread-unsafe but hign-performance object cache
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public static class ObjectCache<TObject>
    {
        public static TObject GetObject()
        {
            return GetObject(() => (TObject)Activator.CreateInstance(typeof(TObject), false));
        }
        public static TObject GetObject(Func<TObject> createInstanceAction)
        {
            if (Instance == null)
            {
                Instance = createInstanceAction();
            }
            return Instance;
        }

        public static object GetObjectFromCallContext()
        {
            return GetObjectFromCallContext(x => Activator.CreateInstance(x));
        }

        public static object GetObjectFromCallContext(Func<Type, object> createInstance)
        {
            var type = typeof(TObject);
            object instance = CallContext.LogicalGetData(type.FullName);
            if (instance == null)
            {
                instance = createInstance(type);// Activator.CreateInstance(type);
                CallContext.LogicalSetData(type.FullName, instance);
            }
            return instance;
        }
        static TObject Instance { get; set; }
    }
}
