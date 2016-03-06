using EasyORM.SchemaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using EasyORM.Provider;
using System.ComponentModel.DataAnnotations.Schema;
using EasyORM.DbUtils.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using EasyORM.Utils;
using EasyORM.DbUtils;

namespace EasyORM.Configuration
{
    public class EntityConfigurationManager
    {
        DataContext _context;
        public EntityConfigurationManager(DataContext context)
        {
            _context = context;
        }
        static Type _columnAttrType = typeof(ColumnAttribute);
        static Type _tableAttrType = typeof(TableAttribute);
        static Type _dataBaseAttrType = typeof(DataBaseAttribute);
        static Type _keyAttrType = typeof(KeyAttribute);
        static ITypeMapper _typeMapper;
        //static Type _nonSelectAttrType = typeof(NonSelectAttribute);
        static Type _dataBaseGeneratedAttrType = typeof(DatabaseGeneratedAttribute);
        static Dictionary<Type, Table> _tableTypeMap = new Dictionary<Type, Table>();
        /// <summary>
        /// 指定类型是否是一个表的类型
        /// </summary>
        /// <returns></returns>
        public static bool IsEntity(Type type)
        {
            return DataContext.IsEntity(type);
        }

        /// <summary>
        /// 先从缓存找对应实体，若没有则将实体类型转换成Table并加入缓存
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        static Table ToTable(Type entityType)
        {
            var table = _tableTypeMap.GetOrDefault(entityType);
            var tableAttr = (TableAttribute)entityType.GetCustomAttributes(_tableAttrType, true).FirstOrDefault();
            string tableName, dbName = null;
            table = new Table();
            if (tableAttr != null)
            {
                tableName = tableAttr.Name;
            }
            else
            {
                tableName = StringHelper.ToPlural(entityType.Name);
            }
            var dbTableAttr = (DataBaseAttribute)entityType.GetCustomAttributes(_dataBaseAttrType, true).FirstOrDefault();
            if (dbTableAttr != null)
            {
                dbName = dbTableAttr.Name;
            }
            table.DataBase = dbName;
            table.Name = tableName;
            table.Type = entityType;
            return table;
        }

        /// <summary>
        /// 将属性转换为列
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static Column ToColumn(PropertyInfo propertyInfo)
        {
            var attrs = propertyInfo.GetCustomAttributes(false);
            var databaseGeneratedAttribute = (DatabaseGeneratedAttribute)attrs.FirstOrDefault(x => x is DatabaseGeneratedAttribute);
            var dataSourceAttribute = (DataSourceAttribute)attrs.FirstOrDefault(x => x is DataSourceAttribute);
            if (databaseGeneratedAttribute != null && dataSourceAttribute != null)
            {
                throw new Exception(string.Format("实体{0}的列{1}同时标注了DatabaseGenerated与DataSource特性，这两种特性不允许同时使用", propertyInfo.DeclaringType.FullName, propertyInfo.Name));
            }
            var columnAttr = (ColumnAttribute)propertyInfo.GetCustomAttributes(_columnAttrType, false).FirstOrDefault();
            var keyAttr = (KeyAttribute)propertyInfo.GetCustomAttributes(_keyAttrType, true).FirstOrDefault();
            Column column = new Column();
            column.PropertyInfo = propertyInfo;
            column.Name = propertyInfo.Name;
            column.IsKey = keyAttr != null;
            var propertyType = propertyInfo.PropertyType;
            var realType = TypeHelper.GetUnderlyingType(propertyType);
            column.NotNull = realType == propertyType;
            if (realType.IsEnum)
            {
                propertyType = Enum.GetUnderlyingType(propertyType);
            }
            column.DbType = _typeMapper.Net2DbMapper.GetOrDefault(propertyType); //EasyORM.DbUtils.ReflectorConsts.Net2DbTypeMapper[propertyType];
            if (databaseGeneratedAttribute != null)
            {
                switch (databaseGeneratedAttribute.DatabaseGeneratedOption)
                {
                    case DatabaseGeneratedOption.Identity:
                        column.ColumnType = ColumnType.AutoIncreament;
                        break;
                    case DatabaseGeneratedOption.None:
                        column.ColumnType = ColumnType.Sequence;
                        break;
                }
                column.IsKey = true;
            }
            else if (dataSourceAttribute != null)
            {
                column.ColumnType = dataSourceAttribute.DataSource;
                column.IsKey = true;
            }
            else if (column.IsKey)
            {
                column.ColumnType = ColumnType.AutoIncreament;
            }
            if (columnAttr != null)
            {
                column.Name = columnAttr.Name;
            }
            return column;
        }

        /// <summary>
        /// 根据给定的类型分析表名、数据库名
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public Table GetTable(Type entityType)
        {
            var table = _tableTypeMap.GetOrDefault(entityType);
            if (table != null)
            {
                return table;
            }
            lock (_tableTypeMap)
            {
                table = _tableTypeMap.GetOrDefault(entityType);
                if (table != null)
                {
                    return table;
                }
                //if (!IsEntity(entityType))
                //{
                //    throw new Exception("不是实体类型");
                //}
                table = ToTable(entityType);
                GetColumns(table);
                _tableTypeMap.Add(entityType, table);
                return table;
            }
        }

        void GetColumns(Table table)
        {
            _typeMapper = _context.Provider.CreateTypeMapper();
            var type = EntityOperatorUtils.GetNonProxyType(table.Type);
            var pis = type.GetProperties();
            var hasKey = false;
            foreach (var propertyInfo in pis)
            {
                var column = ToColumn(propertyInfo);
                column.Table = table;
                if (column.IsKey)
                {
                    hasKey = true;
                    table.Key = column;
                }
                table.Columns.Add(column.PropertyInfo.Name, column);
            }
            if (!hasKey)
            {
                var column = table.Columns.GetOrDefault("Id");
                if (column != null)
                {
                    column.IsKey = true;
                    column.ColumnType = ColumnType.AutoIncreament;
                    table.Key = column;
                }
            }
        }

        /// <summary>
        /// 获取用于配置指定实体信息的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntityConfiguration<T> Entity<T>()
        {
            return new EntityConfiguration<T>(_context);
        }
    }
}
