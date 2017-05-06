using System;
using System.Linq.Expressions;
using EasyORM.Utils;

namespace EasyORM.DynamicObject
{
    public class DataReaderGetMethodSwitcher : BaseTypeSwitcher
    {
        Expression _indexExp;
        Expression _readerExp;
        public DataReaderGetMethodSwitcher(Type type, Expression index, Expression reader)
            : base(type)
        {
            _indexExp = index;
            _readerExp = reader;
        }
        protected override void ProcessByte()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetByteOfIDataReader, _indexExp);
        }

        protected override void ProcessByteNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessInt16()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetInt16OfIDataReader, _indexExp);
        }

        protected override void ProcessInt16Nullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessInt32()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetInt32OfIDataReader, _indexExp);
        }

        protected override void ProcessInt32Nullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessInt64()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetInt64OfIDataReader, _indexExp);
        }

        protected override void ProcessInt64Nullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessDouble()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetDoubleOfIDataReader, _indexExp);
        }

        protected override void ProcessDoubleNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessEnumNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessString()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetStringOfIDataReader, _indexExp);
        }

        protected override void ProcessBooleanNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessBoolean()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetBooleanOfIDataReader, _indexExp);
        }

        protected override void ProcessFloat()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetFloatOfIDataReader, _indexExp);
        }

        protected override void ProcessFloatNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessDecimal()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetDecimalOfIDataReader, _indexExp);
        }

        protected override void ProcessDecimalNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessDateTime()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetDateTimeOfIDataReader, _indexExp);
        }

        protected override void ProcessEnum()
        {
            base.ProcessEnum();
            Result = Expression.Convert((Expression)Result, RawType);
        }

        protected override void ProcessDateTimeNullable()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessGuid()
        {
            Result = Expression.Call(_readerExp, ReflectorConsts.GetGuidOfIDataReader, _indexExp);
        }

        protected override void ProcessGuidNullable()
        {
            throw new NotImplementedException();
        }
    }
}
