using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;

namespace EasyORM.Provider.MySql
{
    /// <summary>
    /// MySql提供者
    /// </summary>
    public class MySqlProvider : ProviderBase
    {
        /// <summary>
        /// 实例化
        /// </summary>
        public MySqlProvider(DataContext context)
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
