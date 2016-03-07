using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;
using EasyORM.DynamicObject;

namespace EasyORM.Provider
{
    /// <summary>
    /// SqlBuilder Factory
    /// </summary>
    public class BuilderFactory
    {
        ProviderBase _provider;
        DataContext _context;
        public BuilderFactory(DataContext context)
        {
            _context = context;
            _provider = context.Provider;
        }
        static List<Type> _providerTypes;

        /// <summary>
        /// Creates a SqlBuilderBase instance
        /// </summary>
        /// <returns></returns>
        public SqlBuilderBase CreateSqlBuilder()
        {
            if (_providerTypes == null)
            {
                _providerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.BaseType == typeof(SqlBuilderBase)).ToList();
            }
            var typeName = _provider.DatabaseType.ToString() + ".SqlBuilder";
            var types = _providerTypes.Where(x => x.FullName.EndsWith(typeName));
            if (types.Count() > 1)
            {
                throw new InvalidOperationException("may be a uncertain provider");
            }
            var type = types.FirstOrDefault();
            if (type == null)
            {
                throw new InvalidOperationException("Not found the provider：" + typeName);
            }
            return (SqlBuilderBase)ExpressionReflector.CreateInstance(type, ObjectPropertyConvertType.Cast, _context);
        }
    }
}
