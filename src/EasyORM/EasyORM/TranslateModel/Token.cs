using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.TranslateModel
{
    public class Token
    {
        private Token()
        {

        }

        public static Token Create(Column column)
        {
            return new Token()
            {
                Column = column,
                Type = TokenType.Column
            };
        }
        public static Token Create(object obj)
        {
            if(obj is Token || obj is Column || obj is Condition)
            {
                throw new Exception();
            }
            return new Token()
            {
                Object = obj,
                Type = TokenType.Object
            };
        }
        public static Token Create(Condition obj)
        {
            return new Token()
            {
                Condition = obj,
                Type = TokenType.Condition
            };
        }
        public TokenType Type { get; private set; }
        public Column Column { get; private set; }
        public object Object { get; private set; }
        public Condition Condition { get; private set; }
        public bool IsBool()
        {
            return Type == TokenType.Object && (Object is bool || Object is bool?);
        }

        public bool GetBool()
        {
            return (bool)Object;
        }

        public bool IsNull()
        {
            return Type == TokenType.Object && Object == null;
        }

        internal static Token CreateNull()
        {
            return new Token()
            {
                Object = null,
                Type = TokenType.Object
            };
        }
    }

    public enum TokenType
    {
        Column, Object, Condition
    }
}
