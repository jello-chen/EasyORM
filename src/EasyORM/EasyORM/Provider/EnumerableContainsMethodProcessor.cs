using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyORM.DynamicObject;

namespace EasyORM.Provider
{
    public class EnumerableContainsMethodProcessor : BaseTypeSwitcher
    {
        IEnumerable _set;
        string _converter;
        public EnumerableContainsMethodProcessor(Type type, IEnumerable collectionObject, string converter)
            : base(type)
        {
            _set = collectionObject;
            _converter = converter;
        }


        protected override void ProcessInt16()
        {
            ProcessType<short>();
        }

        protected override void ProcessInt16Nullable()
        {
            var list = new List<short>();
            foreach (var item in _set)
            {
                var value = (short?)item;
                if (value.HasValue)
                {
                    list.Add(value.Value);
                }
            }
            FillConverter(list);
        }


        protected override void ProcessInt32()
        {
            ProcessType<int>();
        }

        protected override void ProcessInt32Nullable()
        {
            var list = new List<int>();
            foreach (var item in _set)
            {
                var value = (int?)item;
                if (value.HasValue)
                {
                    list.Add(value.Value);
                }
            }
            FillConverter(list);
        }

        protected override void ProcessInt64()
        {
            ProcessType<long>();
        }


        protected override void ProcessInt64Nullable()
        {
            var list = new List<long>();
            foreach (var item in _set)
            {
                var value = (long?)item;
                if (value.HasValue)
                {
                    list.Add(value.Value);
                }
            }
            FillConverter(list);
        }
        void FillConverter<T>(IEnumerable<T> list)
        {
            if (list.Count() <= 0)
            {
                Result = string.Empty;
                return;
            }
            _converter = string.Format(_converter, "{0} IN (" + string.Join(",", list) + ")");
            Result = _converter;
        }

        protected override void ProcessByte()
        {
            ProcessType<byte>();
        }

        protected override void ProcessByteNullable()
        {
            var list = new List<byte>();
            foreach (var item in _set)
            {
                var value = (byte?)item;
                if (value.HasValue)
                {
                    list.Add(value.Value);
                }
            }
            FillConverter(list);
        }

        void ProcessNullableType<T>()
            where T : struct
        {
            var list = new List<T>();
            foreach (var item in _set)
            {
                var value = (T?)item;
                if (value.HasValue)
                {
                    list.Add(value.Value);
                }
            }
            FillConverter(list);
        }

        void ProcessType<T>()
        {
            var list = new List<T>();
            foreach (var item in _set)
            {
                list.Add((T)item);
            }
            FillConverter(list);
        }

        protected override void ProcessEnumNullable()
        {
            Type = Enum.GetUnderlyingType(Type);
            IsNullable = false;
            ArrayList list = new ArrayList();
            foreach (var item in _set)
            {
                if (item == null) continue;
                list.Add(item);
            }
            _set = list;
            SwitchBaseType();
        }

        protected override void ProcessDouble()
        {
            ProcessType<double>();
        }

        protected override void ProcessDoubleNullable()
        {
            var list = new List<double>();
            foreach (var item in _set)
            {
                var value = (double?)item;
                if (value.HasValue)
                {
                    list.Add(value.Value);
                }
            }
            FillConverter(list);
        }

        protected override void ProcessString()
        {
            ProcessType<string>();
        }

        protected override void ProcessBooleanNullable()
        {
            ProcessNullableType<bool>();
        }

        protected override void ProcessBoolean()
        {
            ProcessType<bool>();
        }

        protected override void ProcessFloat()
        {
            ProcessType<float>();
        }

        protected override void ProcessFloatNullable()
        {
            ProcessNullableType<float>();
        }

        protected override void ProcessDecimal()
        {
            ProcessType<decimal>();
        }

        protected override void ProcessDecimalNullable()
        {
            ProcessNullableType<decimal>();
        }

        protected override void ProcessDateTime()
        {
            ProcessType<DateTime>();
        }

        protected override void ProcessDateTimeNullable()
        {
            ProcessNullableType<DateTime>();
        }

        protected override void ProcessGuid()
        {
            ProcessType<Guid>();
        }

        protected override void ProcessGuidNullable()
        {
            ProcessNullableType<Guid>();
        }
    }
}
