using EasyORM.Provider.SQLServer;
using EasyORM.TranslateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DynamicObject;
using EasyORM.Utils;

namespace EasyORM.Provider.SQLServer
{
    /// <summary>
    /// SqlServer2008R2提供者
    /// </summary>
    public class SQLServerProvider : ProviderBase
    {
        /// <summary>
        /// 实例化
        /// </summary>
        public SQLServerProvider(DataContext context)
            : base(context)
        {

        }

        /// <summary>
        /// 返回一个用于在SaveChanges时操作实体的对象
        /// </summary>
        /// <returns></returns>
        public override IEntityOperator CreateEntityOperator()
        {
            return new EntityOperator(_context);
        }

        /// <summary>
        /// 返回一个获取数据库架构的对象
        /// </summary>
        /// <returns></returns>
        public override SchemaManagerBase CreateSchemaManager()
        {
            return new SchemaManager(_context);
        }

        /// <summary>
        /// 返回一个类型映射工具
        /// </summary>
        /// <returns></returns>
        public override ITypeMapper CreateTypeMapper()
        {
            return TypeMapper.GetInstance();
        }
    }
}
