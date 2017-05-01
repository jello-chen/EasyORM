using System;
using System.Collections.Generic;
using System.Linq;
using EasyORM.DynamicObject;

namespace EasyORM.Provider
{
    public class QueryableContainsMethodProcessor : BaseTypeSwitcher
    {
        string _converter = null;
        IQueryable _list;
        public QueryableContainsMethodProcessor(IQueryable list, Type type, string converter)
            : base(type)
        {
            _list = list;
            _converter = converter;
        }
        void FillConverter<T>(IEnumerable<T> list)
        {
            if (!list.Any())
            {
                Result = string.Empty;
                return;
            }
            _converter = string.Format(_converter, "{0} IN (" + string.Join(",", list) + ")");
            Result = _converter;
        }
        protected override void ProcessByte()
        {
            var set = (IEnumerable<byte>)_list;
            FillConverter(set);
        }

        protected override void ProcessByteNullable()
        {
            var set = ((IEnumerable<byte?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessInt16()
        {
            var set = (IEnumerable<short>)_list;
            FillConverter(set);
        }

        protected override void ProcessInt16Nullable()
        {
            var set = ((IEnumerable<short?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessInt32()
        {
            var set = (IEnumerable<int>)_list;
            FillConverter(set);
        }

        protected override void ProcessInt32Nullable()
        {
            var set = ((IEnumerable<int?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessInt64()
        {
            var set = (IEnumerable<long>)_list;
            FillConverter(set);
        }

        protected override void ProcessInt64Nullable()
        {
            var set = ((IEnumerable<long?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessEnumNullable()
        {
            Type = Enum.GetUnderlyingType(Type);
            IsNullable = false;
            SwitchBaseType();
        }

        protected override void ProcessDouble()
        {
            var set = ((IEnumerable<double>)_list);
            FillConverter(set);
        }

        protected override void ProcessDoubleNullable()
        {
            var set = ((IEnumerable<double?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessString()
        {
            var set = ((IEnumerable<string>)_list);
            FillConverter(set);
        }

        protected override void ProcessBooleanNullable()
        {
            var set = ((IEnumerable<bool?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessBoolean()
        {
            var set = ((IEnumerable<bool>)_list);
            FillConverter(set);
        }

        protected override void ProcessFloat()
        {
            var set = ((IEnumerable<float>)_list);
            FillConverter(set);
        }

        protected override void ProcessFloatNullable()
        {
            var set = ((IEnumerable<float?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessDecimal()
        {
            var set = ((IEnumerable<decimal>)_list);
            FillConverter(set);
        }

        protected override void ProcessDecimalNullable()
        {
            var set = ((IEnumerable<decimal?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }

        protected override void ProcessDateTime()
        {
            var set = ((IEnumerable<DateTime>)_list);
            FillConverter(set);
        }

        protected override void ProcessDateTimeNullable()
        {
            var set = ((IEnumerable<DateTime?>)_list).Where(x => x.HasValue).Select(x => x.Value);
            FillConverter(set);
        }
    }
}
