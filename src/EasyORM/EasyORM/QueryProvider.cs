using EasyORM.Provider;
using EasyORM.Provider.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyORM.DbUtils;
using EasyORM.DynamicObject;
using EasyORM.Utils;
using EasyORM.Configuration;
using EasyORM.Logging;
using System.Diagnostics;
using System.Threading;
using EasyORM.ObjectMapper;

namespace EasyORM
{
    public class QueryProvider : IQueryProvider
    {
        Expression _expression;
        Type _elementType;
        DataContext _context;
        public QueryProvider(DataContext context, Type elementType)
        {
            _context = context;
            _elementType = elementType;
        }
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            _expression = expression;
            return new DataQuery<TElement>(this, expression);
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            Type elementType = expression.Type.GetGenericArguments()[0];
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(DataQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        internal ParseResult ParseExpression(System.Linq.Expressions.Expression expression)
        {
            var provider = _context.Provider;
            var parser = provider.CreateParser();
            parser.ElementType = _elementType;
            parser.Parse(expression);
            return parser.Result;
        }

        public string GetCommandText(System.Linq.Expressions.Expression expression)
        {
            return ParseExpression(expression).CommandText;
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            var provider = _context.Provider;
            Stopwatch wacher = null;
            var expressionStr = expression.ToString();
            if (Config.IsEnableLog)
            {
                wacher = Stopwatch.StartNew();
            }
            var parseResult = ParseExpression(expression);
            var cmdText = parseResult.CommandText;
            long tranlsateTime = 0, executeTime = 0;
            if (Config.IsEnableLog)
            {
                wacher.Stop();
                tranlsateTime = wacher.ElapsedMilliseconds;
            }
            Type type = typeof(TResult);
            var executor = provider.CreateSqlExecutor();
            Action<Action> executeTimer = (action) =>
            {
                if (Config.IsEnableLog)
                {
                    wacher.Reset();
                    wacher.Start();
                    action();
                    wacher.Stop();
                    executeTime = wacher.ElapsedMilliseconds;
                    wacher.Reset();
                }
                else
                {
                    action();
                }
            };
            try
            {
                if (expression.NodeType == ExpressionType.Call && type.IsValueType)
                {
                    var method = ((MethodCallExpression)expression).Method;
                    object r = null;
                    DataSet ds = null;
                    switch (method.Name)
                    {
                        case "Any":
                            executeTimer(() =>
                            {
                                ds = executor.ExecuteDataSet(parseResult.CommandText, parseResult.Parameters);
                            });
                            r = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
                            return (TResult)r;
                        case "Delete":
                        case "Update":
                            executeTimer(() =>
                            {
                                r = executor.ExecuteNonQuery(parseResult.CommandText, parseResult.Parameters);
                            });
                            return (TResult)r;
                        case "Average":
                        case "Sum":
                        case "Count":
                            executeTimer(() =>
                            {
                                r = executor.ExecuteScalar(parseResult.CommandText, parseResult.Parameters);
                            });
                            if (r == DBNull.Value)
                            {
                                return default(TResult);
                            }
                            break;
                        default:
                            throw new Exception();
                    }
                    var baseTypeConverter = new BaseTypeConverter(r, type);
                    baseTypeConverter.Process();
                    return (TResult)baseTypeConverter.Result;
                }
                else
                {
                    bool isValueType = false;
                    if (type.IsGenericType)
                    {
                        IDataReader reader = null;
                        executeTimer(() =>
                        {
                            reader = executor.ExecuteReader(parseResult.CommandText, parseResult.Parameters);
                        });
                        using (reader)
                        {
                            var genericTypeDef = type.GetGenericTypeDefinition();
                            if (genericTypeDef == DbUtils.ReflectorConsts.IEnumerableType)
                            {
                                var elementType = type.GetGenericArguments()[0];
                                IList list = null;
                                list = EntityMapper.Map(elementType, reader);
                                if (list.Count > 10)
                                {
                                    return (TResult)list;
                                }
                                if (EntityConfigurationManager.IsEntity(type))
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        list[i] = DynamicProxy.CreateDynamicProxy(list[i]);
                                    }
                                    var entityOp = _context.GetEntityOperator(type);
                                    entityOp.AddEditing(list);
                                }
                                return (TResult)list;
                            }
                            if (typeof(Nullable<>).IsAssignableFrom(type))
                            {
                                isValueType = true;
                            }
                            if (!isValueType)
                            {
                                throw new Exception();
                            }
                        }
                    }
                    if (type.IsValueType || isValueType)
                    {
                        DataSet ds = null;
                        executeTimer(() =>
                        {
                            ds = executor.ExecuteDataSet(parseResult.CommandText, parseResult.Parameters);
                        });
                        if (ds.Tables[0].Rows.Count <= 0)
                        {
                            return default(TResult);
                        }
                        var result = ds.Tables[0].Rows[0][0];
                        if (result == DBNull.Value)
                        {
                            return default(TResult);
                        }
                        return (TResult)Convert.ChangeType(result, type);
                    }

                    if (EntityConfigurationManager.IsEntity(type))
                    {
                        IDataReader reader = null;
                        executeTimer(() =>
                        {
                            reader = executor.ExecuteReader(parseResult.CommandText, parseResult.Parameters);
                        });
                        IList results = null;
                        try
                        {
                            results = EntityMapper.Map(type, reader);
                        }
                        finally
                        {
                            reader.Dispose();
                        }
                        if (results.Count <= 0)
                        {
                            return default(TResult);
                        }
                        var result = results[0];
                        var entityOp = _context.GetEntityOperator(type);
                        result = DynamicProxy.CreateDynamicProxy(result);
                        entityOp.AddEditing(new ArrayList() { result });
                        return (TResult)result;
                    }
                    throw new Exception();
                }
            }
            finally
            {
                executor.Dispose();
                if (Config.IsEnableLog)
                {
                    var writters = LogWriterFactory.CreateLogWriter();
                    new Thread(() =>
                    {
                        writters.ForEach(writer =>
                        {
                            writer.WriteLog(expressionStr, cmdText, tranlsateTime, parseResult.Parameters, executeTime);
                        });
                    }).Start();
                }
            }


        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            var type = expression.Type;
            if (type.IsGenericType)
            {
                var typeDef = type.GetGenericTypeDefinition();
                if (typeDef == EasyORM.DbUtils.ReflectorConsts.IQueryableType)
                {
                    type = EasyORM.DbUtils.ReflectorConsts.IEnumerableType.MakeGenericType(type.GetGenericArguments());
                }
            }
            return this.GetType().GetMethods().FirstOrDefault(x => x.Name == "Execute" && x.IsGenericMethodDefinition).MakeGenericMethod(type).Invoke(this, new object[] { expression });
        }
    }
}
