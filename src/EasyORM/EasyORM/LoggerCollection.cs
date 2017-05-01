using System.Configuration;
using EasyORM.Configuration;

namespace EasyORM
{
    public class LoggerCollection : ConfigurationElementCollection
    {
        public Logger this[int index]
        {
            get { return (Logger)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(Logger serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Logger();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Logger)element).Type;
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}
