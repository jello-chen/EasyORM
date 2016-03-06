using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Utils;

namespace EasyORM.DynamicObject
{
    public abstract class BaseTypeSwitcher
    {
        public object Result { get; protected set; }
        Type _rawType;

        public Type RawType
        {
            get { return _rawType; }
        }
        Type _type;

        public Type Type
        {
            get { return _type; }
            protected set { _type = value; }
        }
        public bool IsNullable { get; protected set; }
        public BaseTypeSwitcher(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            _rawType = type;
            _type = TypeHelper.GetUnderlyingType(type);
            IsNullable = TypeHelper.IsNullableType(type);
        }

        protected void SwitchBaseType()
        {
            if (_type == ReflectorConsts.Int16Type)
            {
                if (IsNullable)
                {
                    ProcessInt16Nullable();
                }
                else
                {
                    ProcessInt16();
                }
            }
            else if (_type == ReflectorConsts.Int32Type)
            {
                if (IsNullable)
                {
                    ProcessInt32Nullable();
                }
                else
                {
                    ProcessInt32();
                }
            }
            else if (_type == ReflectorConsts.Int64Type)
            {
                if (IsNullable)
                {
                    ProcessInt64Nullable();
                }
                else
                {
                    ProcessInt64();
                }
            }
            else if (_type == ReflectorConsts.ByteType)
            {
                if (IsNullable)
                {
                    ProcessByteNullable();
                }
                else
                {
                    ProcessByte();
                }
            }
            else if (_type == ReflectorConsts.DoubleType)
            {
                if (IsNullable)
                {
                    ProcessDoubleNullable();
                }
                else
                {
                    ProcessDouble();
                }
            }
            else if (_type == ReflectorConsts.StringType)
            {
                ProcessString();
            }
            else if (_type == ReflectorConsts.BoolType)
            {
                if (IsNullable)
                {
                    ProcessBooleanNullable();
                }
                else
                {
                    ProcessBoolean();
                }
            }
            else if (_type == ReflectorConsts.FloatType)
            {
                if (IsNullable)
                {
                    ProcessFloatNullable();
                }
                else
                {
                    ProcessFloat();
                }
            }else if(_type==ReflectorConsts.DecimalType)
            {
                if(IsNullable)
                {
                    ProcessDecimalNullable();
                }
                else
                {
                    ProcessDecimal();
                }
            }else if(_type==ReflectorConsts.DateTimeType)
            {
                if(IsNullable)
                {
                    ProcessDateTimeNullable();
                }
                else
                {
                    ProcessDateTime();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public void Process()
        {
            if (_type.IsEnum)
            {
                if (IsNullable)
                {
                    ProcessEnumNullable();
                }
                else
                {
                    ProcessEnum();
                }
            }
            else
            {
                SwitchBaseType();
            }
        }

        protected abstract void ProcessByte();
        protected abstract void ProcessByteNullable();
        protected abstract void ProcessInt16();
        protected abstract void ProcessInt16Nullable();
        protected abstract void ProcessInt32();
        protected abstract void ProcessInt32Nullable();
        protected abstract void ProcessInt64();
        protected abstract void ProcessInt64Nullable();
        protected abstract void ProcessDouble();
        protected abstract void ProcessDoubleNullable();
        protected abstract void ProcessString();
        protected abstract void ProcessBooleanNullable();
        protected abstract void ProcessBoolean();
        protected abstract void ProcessFloat();
        protected abstract void ProcessFloatNullable();
        protected abstract void ProcessDecimal();
        protected abstract void ProcessDecimalNullable();
        protected abstract void ProcessDateTime();
        protected abstract void ProcessDateTimeNullable();
        protected virtual void ProcessEnum()
        {
            _type = Enum.GetUnderlyingType(Type);
            SwitchBaseType();
        }
        protected abstract void ProcessEnumNullable();
    }
}
