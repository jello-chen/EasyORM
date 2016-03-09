using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;
using EasyORM.Provider;
using EasyORM.SchemaModel;

namespace EasyORM.VSExtension.CodeGenerator
{
    /// <summary>
    /// Code Generator Utils
    /// </summary>
    public class GeneratorUtils
    {
        /// <summary>
        /// Generates DataContext Class
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="dataContextName"></param>
        /// <param name="tables"></param>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        public static string GenerateDataContext(string nameSpace, string connectionStringName, string dataContextName, List<Table> tables)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using EasyORM;");
            builder.AppendFormat("namespace {0}", nameSpace);
            builder.AppendLine();
            builder.AppendFormat("{{");
            builder.AppendLine();
            builder.AppendFormat("\tpublic class {0} : DataContext", dataContextName);
            builder.AppendLine();
            builder.AppendFormat("\t{{");
            builder.AppendLine();
            builder.AppendFormat("\t\tpublic {0}() : base(\"{1}\")", dataContextName, connectionStringName);
            builder.AppendLine();
            builder.AppendFormat("\t\t{{");
            builder.AppendLine();
            builder.AppendFormat("\t\t}}");
            builder.AppendLine();
            builder.AppendFormat("\t\tprotected override void ConfigurationModel(EntityConfigurationManager entityConfiguration)");
            builder.AppendLine();
            builder.AppendFormat("\t\t{{");
            builder.AppendLine();
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    builder.AppendFormat("\t\t\t#region {0}", table.Name);
                    builder.AppendLine();
                    var goodName = ParserUtils.GetStandardTableName(table.Name);
                    var className = StringHelper.ToSingular(goodName);
                    var getEntityCode = string.Format("\t\t\tvar entity{0} = entityConfiguration.Entity<{0}>();", className);
                    if (!goodName.Equals(table.Name, StringComparison.OrdinalIgnoreCase) || className.Equals(goodName, StringComparison.OrdinalIgnoreCase))
                    {
                        builder.AppendLine(getEntityCode);
                        getEntityCode = null;
                        builder.AppendFormat("\t\t\tentity{0}.TableName(\"{1}\");", className, table.Name);
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendLine(getEntityCode);
                        getEntityCode = null;
                        builder.AppendFormat("\t\t\tentity{0}.Database(\"{1}\");", className, table.DataBase);
                        builder.AppendLine();
                    }
                    var keyColumn = table.Key;
                    if (keyColumn != null)
                    {
                        if (!keyColumn.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrWhiteSpace(getEntityCode))
                            {
                                builder.AppendLine(getEntityCode);
                                getEntityCode = null;
                            }
                            var dataSource = string.Empty;
                            switch (keyColumn.ColumnType)
                            {
                                case DbUtils.KeyColumnType.AutoIncreament:
                                    dataSource = "DataSourceTypes.AutoIncreament";
                                    break;
                                case DbUtils.KeyColumnType.None:
                                    dataSource = "DataSourceTypes.None";
                                    break;
                                case DbUtils.KeyColumnType.Sequence:
                                    dataSource = "DataSourceTypes.Sequence";
                                    break;
                                default:
                                    throw new Exception("Not supported key type");
                            }
                            builder.AppendFormat("\t\t\tentity{0}.Key(x => x.{1}, {2});", className, ParserUtils.GetGoodColumnName(keyColumn.Name), dataSource);
                            builder.AppendLine();
                        }
                    }
                    foreach (var column in table.Columns)
                    {
                        var columnName = ParserUtils.GetGoodColumnName(column.Key);
                        var getPropertyCode = string.Format("\t\t\tvar property{1}{0} = entity{1}.Property(x => x.{2});", columnName, className, columnName);
                        if (columnName != column.Key)
                        {
                            if (!string.IsNullOrWhiteSpace(getEntityCode))
                            {
                                builder.AppendLine(getEntityCode);
                                getEntityCode = null;
                            }
                            if (!string.IsNullOrWhiteSpace(getPropertyCode))
                            {
                                builder.AppendLine(getPropertyCode);
                                getPropertyCode = null;
                            }
                            builder.AppendFormat("\t\t\tproperty{2}{0}.Name(\"{1}\");", columnName, column.Key, className);
                            builder.AppendLine();
                        }
                    }
                    builder.AppendFormat("\t\t\t#endregion");
                    builder.AppendLine();
                    builder.AppendLine();
                }
            }
            builder.AppendFormat("\t\t}}");
            builder.AppendLine();
            builder.AppendFormat("\t}}");
            builder.AppendLine();
            builder.AppendFormat("}}");
            return builder.ToString();
        }
        /// <summary>
        /// Generates model class
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GenerateModels(string nameSpace,ITypeMapper typeMapper, params Table[] tables)
        {
            var codes = new Dictionary<string, string>();
            foreach (var item in tables)
            {
                var builder = new StringBuilder();
                builder.AppendLine("using System;");
                var name = ParserUtils.GetStandardTableName(item.Name);
                name = StringHelper.ToSingular(name);
                builder.AppendFormat("namespace {0}", nameSpace);
                builder.AppendLine();
                builder.AppendFormat("{{");
                builder.AppendLine();
                builder.AppendFormat("\tpublic class {0}", name);
                builder.AppendLine();
                builder.AppendFormat("\t{{");
                builder.AppendLine();
                foreach (var column in item.Columns)
                {
                    var type = typeMapper.Db2NetMapper.GetOrDefault(column.Value.DbType);
                    if (type == null)
                    {
                        throw new Exception("Not support the database");
                    }
                    var typeString = type.Name;
                    if (column.Value.NotNull && type.IsValueType)
                    {
                        typeString += "?";
                    }
                    builder.AppendFormat("\t\tpublic virtual {0} {1} {{ get;set; }}", typeString, ParserUtils.GetGoodColumnName(column.Key));
                    builder.AppendLine();
                }
                builder.AppendFormat("\t}}");
                builder.AppendLine();
                builder.AppendFormat("}}");
                codes.Add(name, builder.ToString());
            }
            return codes;
        }
    }
}
