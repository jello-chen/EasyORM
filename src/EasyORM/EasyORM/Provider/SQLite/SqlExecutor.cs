﻿using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
namespace EasyORM.Provider.SQLite
{
    /// <summary>
    /// Sql Executor
    /// </summary>
    public class SqlExecutor : SqlExecutorBase
    {
        public SqlExecutor(DataContext context):base(context)
        {

        }
        protected override DbConnection CreateConnection()
        {
            if (DbConnection != null)
            {
                switch (DbConnection.State)
                {
                    case ConnectionState.Closed:
                        DbConnection.Open();
                        break;
                    case ConnectionState.Open:
                        break;
                    default:
                        throw new Exception();
                }
                return DbConnection;
            }
            return base.CreateConnection();
        }

        public override void Close()
        {
            if (Transaction.Current != null)
            {
                Transaction.Current.TransactionCompleted -= Current_TransactionCompleted;
                Transaction.Current.TransactionCompleted += Current_TransactionCompleted;
            }
            else
            {
                DbConnection.Dispose();
            }
        }

        void Current_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            DbConnection.Dispose();
        }

        /// <summary>
        /// Set Paramters
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        protected override void SetParameter(DbParameter parameter, string parameterName, object value, DbType dbType)
        {
            parameter.ParameterName = parameterName;
            if (value != null)
            {
                parameter.DbType = dbType;
                var dateValue = value as DateTime?;
                if (dateValue != null)
                {
                    if (dateValue.Value.Date == dateValue.Value)
                    {
                        value = dateValue.Value.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        value = dateValue.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    parameter.DbType = DbType.String;
                }
                parameter.Value = value;
            }
            else
            {
                parameter.Value = DBNull.Value;
            }
        }
    }
}
