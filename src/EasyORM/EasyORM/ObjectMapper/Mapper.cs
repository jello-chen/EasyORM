using System;
using System.Collections.Generic;
using System.Data;

namespace EasyORM.ObjectMapper
{
    public class Mapper
    {
        public static T MapSingle<T>(DataSet ds)
        {
            if (ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                return default(T);
            }
            return new EntityMapper<T>().Map(ds.Tables[0].Rows[0]);
        }

        public static IList<T> MapList<T> (DataSet ds)
        {
            return new EntityMapper<T>().Map(ds);
        }

        public static IDictionary<TKey,TEntity> Map<TKey,TEntity>(DataSet ds,Func<TEntity,TKey> keySelector)
        {
            return new EntityMapper<TEntity>().Map(ds, keySelector);
        }

        public static TResult Map<TSource, TResult>(TSource source)
        {
            return EntityMapper.Map<TSource, TResult>(source);
        }

        //public static T Map<T>(DataSet ds)
        //{
        //    //EntityMapper.Map<
        //    AutoMapper.Mapper.Reset();
        //    AutoMapper.Mapper.CreateMap<IDataReader, T>();
        //    return AutoMapper.Mapper.Map<T>(ds.CreateDataReader());
        //}

        //public static T Map<T>(IDataReader reader)
        //{
        //    AutoMapper.Mapper.Reset();
        //    AutoMapper.Mapper.CreateMap<IDataReader, T>();
        //    return AutoMapper.Mapper.Map<T>(reader);
        //}
    }
}
