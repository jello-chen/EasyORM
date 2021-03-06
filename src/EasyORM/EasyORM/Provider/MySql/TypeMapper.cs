﻿using System;
using System.Collections.Generic;
using System.Data;
using EasyORM.Utils;

namespace EasyORM.Provider.MySql
{
    public class TypeMapper : Singleton<TypeMapper>, ITypeMapper
    {
        Dictionary<Type, DbType> _net2DbMapper = new Dictionary<Type, DbType>();
        Dictionary<DbType, Type> _db2NetMapper = new Dictionary<DbType, Type>();
        Dictionary<DbType, string> _db2SQLMapper = new Dictionary<DbType, string>();
        public TypeMapper()
        {
            _net2DbMapper.Add(ReflectorConsts.DateTimeType, DbType.DateTime2);
            _net2DbMapper.Add(ReflectorConsts.Int32Type, DbType.Int32);
            _net2DbMapper.Add(typeof(short), DbType.Int16);
            _net2DbMapper.Add(typeof(long), DbType.Int64);
            _net2DbMapper.Add(typeof(string), DbType.String);
            _net2DbMapper.Add(typeof(DateTime?), DbType.DateTime2);
            _net2DbMapper.Add(typeof(int?), DbType.Int32);
            _net2DbMapper.Add(typeof(short?), DbType.Int16);
            _net2DbMapper.Add(typeof(long?), DbType.Int64);
            _net2DbMapper.Add(typeof(decimal), DbType.Decimal);
            _net2DbMapper.Add(typeof(decimal?), DbType.Decimal);
            _net2DbMapper.Add(typeof(double), DbType.Double);
            _net2DbMapper.Add(typeof(double?), DbType.Double);
            _net2DbMapper.Add(typeof(float), DbType.Single);
            _net2DbMapper.Add(typeof(float?), DbType.Single);
            _net2DbMapper.Add(typeof(bool), DbType.Boolean);
            _net2DbMapper.Add(typeof(byte), DbType.Byte);
            _net2DbMapper.Add(typeof(byte?), DbType.Byte);
            _net2DbMapper.Add(typeof(byte[]), DbType.Binary);
            _net2DbMapper.Add(typeof(sbyte), DbType.SByte);
            _net2DbMapper.Add(typeof(uint), DbType.UInt32);
            _net2DbMapper.Add(typeof(uint?), DbType.UInt32);
            _net2DbMapper.Add(typeof(UInt16), DbType.UInt16);
            _net2DbMapper.Add(typeof(UInt16?), DbType.UInt16);
            _net2DbMapper.Add(typeof(UInt64), DbType.UInt64);
            _net2DbMapper.Add(typeof(UInt64?), DbType.UInt64);

            _db2NetMapper.Add(DbType.DateTime2, ReflectorConsts.DateTimeType);
            _db2NetMapper.Add(DbType.Int32, ReflectorConsts.Int32Type);
            _db2NetMapper.Add(DbType.Int16, typeof(short));
            _db2NetMapper.Add(DbType.Int64, typeof(long));
            _db2NetMapper.Add(DbType.String, typeof(string));
            _db2NetMapper.Add(DbType.Decimal, typeof(decimal));
            _db2NetMapper.Add(DbType.Double, typeof(double));
            _db2NetMapper.Add(DbType.Single, typeof(float));
            _db2NetMapper.Add(DbType.Boolean, typeof(bool));
            _db2NetMapper.Add(DbType.Byte, typeof(byte));
            _db2NetMapper.Add(DbType.SByte, typeof(byte));
            _db2NetMapper.Add(DbType.Binary, typeof(byte[]));
            _db2NetMapper.Add(DbType.UInt64, typeof(ulong));
            _db2NetMapper.Add(DbType.UInt16, typeof(UInt16));
            _db2NetMapper.Add(DbType.UInt32, typeof(UInt32));


            _db2SQLMapper.Add(DbType.DateTime2, "DATETIME");
            _db2SQLMapper.Add(DbType.DateTime, "DATETIME");
            _db2SQLMapper.Add(DbType.Int32, "INT");
            _db2SQLMapper.Add(DbType.Byte, "TINYINT");
            _db2SQLMapper.Add(DbType.SByte, "TINYINT");
            _db2SQLMapper.Add(DbType.Int64, "BIGINT");
            _db2SQLMapper.Add(DbType.UInt64, "BIGINT");
            _db2SQLMapper.Add(DbType.String, "NVARCHAR");
            _db2SQLMapper.Add(DbType.StringFixedLength, "VARCHAR");
            _db2SQLMapper.Add(DbType.Decimal, "DECIMAL");
            _db2SQLMapper.Add(DbType.Currency, "DECIMAL");
            _db2SQLMapper.Add(DbType.Double, "FLOAT");
            _db2SQLMapper.Add(DbType.Single, "FLOAT");
            _db2SQLMapper.Add(DbType.Boolean, "BIT");
            _db2SQLMapper.Add(DbType.Int16, "MEDIUMINT");
            _db2SQLMapper.Add(DbType.UInt16, "MEDIUMINT");
            _db2SQLMapper.Add(DbType.UInt32, "INT");
            _db2SQLMapper.Add(DbType.Binary, "VARBINARY");
        }
        public Dictionary<Type, System.Data.DbType> Net2DbMapper
        {
            get { return _net2DbMapper; }
        }

        public Dictionary<System.Data.DbType, Type> Db2NetMapper
        {
            get { return _db2NetMapper; }
        }


        public Dictionary<DbType, string> Db2SQLMapper
        {
            get { return _db2SQLMapper; }
        }
    }
}
