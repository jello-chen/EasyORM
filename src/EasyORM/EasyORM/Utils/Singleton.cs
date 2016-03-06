using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.Utils
{
    public class Singleton<T>
    {
        static Dictionary<Type, object> _lockers = new Dictionary<Type, object>();
        static T _instance;
        public static T GetInstance(params object[] parameters)
        {
            if (_instance == null)
            {
                var type = typeof(T);
                var locker = _lockers.GetOrDefault(type);
                if (locker == null)
                {
                    lock (_lockers)
                    {
                        locker = _lockers.Get(type, x => new object());
                    }
                }
                lock (locker)
                {
                    if (_instance == null)
                    {
                        var cons = type.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).FirstOrDefault();
                        _instance = (T)cons.Invoke(parameters);
                    }
                }
            }
            return _instance;
        }
    }
}
