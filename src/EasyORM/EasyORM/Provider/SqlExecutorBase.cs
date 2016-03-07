using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;

namespace EasyORM.Provider
{
    /// <summary>
    /// Sql Executor Base
    /// </summary>
    public class SqlExecutorBase : IDisposable
    {
        static DbProviderFactory _factory;
        private DbConnection _dbConnection;
        private ITypeMapper _typeMapper;
        ProviderBase _provider;
        DataContext _context;
        public SqlExecutorBase(DataContext context)
        {
            _context = context;
            _provider = context.Provider;
            _typeMapper = _provider.CreateTypeMapper();
            _factory = _provider.CreateDbProviderFactory();
        }
        public DbConnection DbConnection
        {
            get { return _dbConnection; }
        }

        protected virtual DbConnection CreateConnection()
        {
            _dbConnection = _factory.CreateConnection();
            _dbConnection.ConnectionString = _context.DatabaseConfig.ConnectionString;
            _dbConnection.Open();
            return _dbConnection;
        }

        public virtual void Close()
        {
            if (_dbConnection != null)
            {
                _dbConnection.Dispose();
                _dbConnection = null;
            }
        }

        protected virtual void SetParameter(DbParameter parameter, string parameterName, object value, DbType dbType)
        {
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            parameter.DbType = dbType;
        }

        DbCommand CreateCommand(DbConnection conn, string cmdText, Dictionary<string, object> parameters)
        {
            var dbCommand = _factory.CreateCommand();
            dbCommand.Connection = conn;
            dbCommand.CommandText = cmdText;
            foreach (var parameterName in parameters.Keys)
            {
                var parameter = dbCommand.CreateParameter();
                var value = parameters.GetOrDefault(parameterName);
                if (value != null)
                {
                    var propertyType = value.GetType();
                    DbType dbType;
                    if (propertyType.IsEnum)
                    {
                        dbType = _typeMapper.Net2DbMapper.GetOrDefault(Enum.GetUnderlyingType(propertyType));
                    }
                    else
                    {
                        dbType = _typeMapper.Net2DbMapper.GetOrDefault(propertyType);
                    }
                    SetParameter(parameter, parameterName, value, dbType);
                }
                else
                {
                    parameter.ParameterName = parameterName;
                    parameter.Value = DBNull.Value;
                }
                dbCommand.Parameters.Add(parameter);
            }
            return dbCommand;
        }

        DbDataAdapter CreateDataAdapter(DbCommand cmd)
        {
            var dbDataAdapter = _factory.CreateDataAdapter();
            dbDataAdapter.SelectCommand = cmd;
            return dbDataAdapter;
        }

        public virtual object ExecuteScalar(string sql, Dictionary<string, object> parameters)
        {
            var conn = CreateConnection();
            try
            {
                var cmd = CreateCommand(conn, sql, parameters);
                return cmd.ExecuteScalar();
            }
            finally
            {
                Close();
            }
        }

        public DataSet ExecuteDataSet(string sql, Dictionary<string, object> parameters)
        {
            var conn = CreateConnection();
            try
            {
                var cmd = CreateCommand(conn, sql, parameters);
                using (cmd)
                {
                    var dda = CreateDataAdapter(cmd);
                    using (dda)
                    {
                        DataSet ds = new DataSet();
                        dda.Fill(ds);
                        return ds;
                    }
                }
            }
            finally
            {
                Close();
            }
        }
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            var conn = CreateConnection();
            try
            {
                var cmd = CreateCommand(conn, sql, parameters);
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                Close();
            }
        }
        public IDataReader ExecuteReader(string cmdText, Dictionary<string, object> parameters)
        {
            var conn = CreateConnection();
            var cmd = CreateCommand(conn, cmdText, parameters);
#if DEBUG
            Debug.WriteLine(cmdText);
            foreach (var key in parameters.Keys)
            {
                Debug.WriteLine("{0}:{1}", key, parameters[key]);
            }
#endif
            var reader = cmd.ExecuteReader();
            return reader;
        }
        public DataTable ExecuteSchema(string cmdText)
        {
            var conn = CreateConnection();
            try
            {
                var cmd = CreateCommand(conn, cmdText, new Dictionary<string, object>());
                var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                using (cmd)
                {
                    using (reader)
                    {
                        return reader.GetSchemaTable();
                    }
                }
            }
            finally
            {
                Close();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
