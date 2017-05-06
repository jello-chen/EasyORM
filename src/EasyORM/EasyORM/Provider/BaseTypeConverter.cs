using EasyORM.DynamicObject;
using System;

namespace EasyORM.Provider
{
    public class BaseTypeConverter : BaseTypeSwitcher
    {
        object _obj;
        public BaseTypeConverter(object obj, Type targetType)
            : base(targetType)
        {
            _obj = obj;
        }
        protected override void ProcessByte()
        {
            Result = Convert.ToByte(_obj);
        }

        protected override void ProcessByteNullable()
        {
            if (_obj != null)
            {
                ProcessByte();
            }
        }

        protected override void ProcessInt16()
        {
            Result = Convert.ToInt16(_obj);
        }

        protected override void ProcessInt16Nullable()
        {
            if (_obj != null)
            {
                ProcessInt16();
            }
        }

        protected override void ProcessInt32()
        {
            Result = Convert.ToInt32(_obj);
        }

        protected override void ProcessInt32Nullable()
        {
            if (_obj != null)
            {
                ProcessInt32();
            }
        }

        protected override void ProcessInt64()
        {
            Result = Convert.ToInt64(_obj);
        }

        protected override void ProcessInt64Nullable()
        {
            if (_obj != null)
            {
                ProcessInt64();
            }
        }

        protected override void ProcessEnumNullable()
        {
            if (_obj != null)
            {
                ProcessEnum();
            }
        }

        protected override void ProcessDouble()
        {
            Result = Convert.ToDouble(_obj);
        }

        protected override void ProcessDoubleNullable()
        {
            if (_obj != null)
            {
                ProcessDouble();
            }
        }

        protected override void ProcessString()
        {
            Result = Convert.ToString(_obj);
        }

        protected override void ProcessBooleanNullable()
        {
            ProcessBoolean();
        }

        protected override void ProcessBoolean()
        {
            if (_obj != null)
            {
                Result = Convert.ToBoolean(_obj);
            }
        }

        protected override void ProcessFloat()
        {
            Result = Convert.ToSingle(_obj);
        }

        protected override void ProcessFloatNullable()
        {
            if (_obj != null)
            {
                ProcessFloat();
            }
        }

        protected override void ProcessDecimal()
        {
            Result = Convert.ToDecimal(_obj);
        }

        protected override void ProcessDecimalNullable()
        {
            if (_obj != null)
            {
                ProcessDecimal();
            }
        }

        protected override void ProcessDateTime()
        {
            Result = Convert.ToDateTime(_obj);
        }

        protected override void ProcessDateTimeNullable()
        {
            if (_obj != null)
            {
                ProcessDateTime();
            }
        }

        protected override void ProcessGuid()
        {
            Result = (Guid)_obj;
        }

        protected override void ProcessGuidNullable()
        {
            if(_obj != null)
            {
                ProcessGuid();
            }
        }
    }
}
