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
    /// SqlBuilder工厂类
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
        /// 根据当前数据库创建一个SqlBuilderBase类
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
                throw new NotSupportedException("找到了多个包含" + typeName + "的提供者类");
            }
            var type = types.FirstOrDefault();
            if (type == null)
            {
                throw new NotSupportedException("未找到提供者类：" + typeName);
            }
            return (SqlBuilderBase)ExpressionReflector.CreateInstance(type, ObjectPropertyConvertType.Cast, _context);
        }
    }
}
