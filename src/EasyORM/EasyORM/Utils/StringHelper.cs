namespace EasyORM.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    public class StringHelper
    {
        private static string _codeSerial = "0123456789abcdefghijklmnopqrstuvwxyzQWERTYUIOPASDFGHJKLZXCVBNM";
        private static string _letter = "abcdefghijklmnopqrstuvwxyz";
        private static Random _random = new Random();
        private static string _symbol = "。，、；：？！…-\x00b7ˉˇ\x00a8‘'“”々～‖∶＂＇｀｜〃〔〕〈〉《》「」『』．〖〗【】（）［］｛｝ ㈠㈡㈢㈣㈤㈥㈦㈧㈨㈩⑴⑵⑶⑷⑸⑹⑺⑻⑼⑽①②③④⑤⑥⑦⑧⑨⑩⑾⑿⒀⒁⒂⒃⒄⒅⒆⒇①②③④⑤⑥⑦⑧⑨⑩№⑴⑵⑶⑷⑸⑹⑺⑻⑼⑽⑾⑿⒀⒁⒂⒃⒄⒅⒆⒇⒈⒉⒊⒋⒌⒍⒎⒏⒐⒑⒒⒓⒔⒕⒖⒗⒘⒙⒚⒛ⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩⅪⅫⅰⅱⅲⅳⅴⅵⅶⅷⅸⅹ≈≡≠＝≤≥＜＞≮≯∷\x00b1＋－\x00d7\x00f7／∫∮∝∞∧∨∑∏∪∩∈∵∴⊥∥∠⌒⊙≌∽√┌┍┎┏┐┑┒┓─┄┈├┝┞┟┠┡┢┣│┆┊┬┭┮┯┰┱┲┳┼┽┾┿╀╁╂╃\x00a7№☆★○●◎◇◆□■△▲※→←↑↓〓＃＆＠＼＾＿＆＠＼＾αβγδεζηθικλμνξοπρστυφχψωΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩабвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯㄅㄉㄓㄚㄞㄢㄦㄆㄊㄍㄐㄔㄗㄧㄛㄟㄣㄇㄋㄎㄑㄕㄘㄨㄜㄠㄤㄈㄏㄒㄖㄙㄩㄝㄡㄥā\x00e1ǎ\x00e0ō\x00f3ǒ\x00f2\x00eaē\x00e9ě\x00e8ī\x00edǐ\x00ecū\x00faǔ\x00f9ǖǘǚǜ\x00fcぁぃぅぇぉかきくけこんさしすせそたちつってとゐなにぬねのはひふへほゑまみむめもゃゅょゎをァィゥヴェォカヵキクケヶコサシスセソタチツッテトヰンナニヌネノハヒフヘホヱマミムメモャュョヮヲ";

        public static object[] CreateRegionCode(int strlength)
        {
            string[] strArray = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
            object[] objArray = new object[strlength];
            for (int i = 0; i < strlength; i++)
            {
                int num3;
                int num5;
                int index = _random.Next(11, 14);
                string str = strArray[index].Trim();
                _random = new Random((index * ((int) DateTime.Now.Ticks)) + i);
                if (index == 13)
                {
                    num3 = _random.Next(0, 7);
                }
                else
                {
                    num3 = _random.Next(0, 0x10);
                }
                string str2 = strArray[num3].Trim();
                _random = new Random((num3 * ((int) DateTime.Now.Ticks)) + i);
                int num4 = _random.Next(10, 0x10);
                string str3 = strArray[num4].Trim();
                _random = new Random((num4 * ((int) DateTime.Now.Ticks)) + i);
                if (num4 == 10)
                {
                    num5 = _random.Next(1, 0x10);
                }
                else if (num4 == 15)
                {
                    num5 = _random.Next(0, 15);
                }
                else
                {
                    num5 = _random.Next(0, 0x10);
                }
                string str4 = strArray[num5].Trim();
                byte num6 = Convert.ToByte(str + str2, 0x10);
                byte num7 = Convert.ToByte(str3 + str4, 0x10);
                byte[] buffer = new byte[] { num6, num7 };
                objArray.SetValue(buffer, i);
            }
            return objArray;
        }

        public static string EncodePassword(string userName, string password)
        {
            return Md5(Md5(userName) + Md5(password));
        }

        public static string GetChineseChar(int strLength)
        {
            StringBuilder builder = new StringBuilder();
            Encoding encoding = Encoding.GetEncoding("gb2312");
            object[] objArray = CreateRegionCode(strLength);
            for (int i = 0; i < strLength; i++)
            {
                string str = encoding.GetString((byte[]) Convert.ChangeType(objArray[i], typeof(byte[])));
                builder.Append(str);
            }
            return builder.ToString();
        }

        public static string GetRandomLetter(int length)
        {
            char[] chArray = new char[length];
            chArray[0] = _letter[_random.Next(_letter.Length)];
            for (int i = 1; i < length; i++)
            {
                chArray[i] = _letter[_random.Next(_letter.Length)];
            }
            return new string(chArray);
        }

        public static string GetRandomString(int length)
        {
            char[] chArray = new char[length];
            chArray[0] = _codeSerial[_random.Next(0, _codeSerial.Length)];
            for (int i = 1; i < length; i++)
            {
                chArray[i] = _codeSerial[_random.Next(_codeSerial.Length)];
            }
            return new string(chArray);
        }

        public static string GetRandomString(int minLength, int maxLength)
        {
            return GetRandomString(_random.Next(minLength, maxLength));
        }

        public static string GetRandomSymbol(int length)
        {
            char[] chArray = new char[length];
            chArray[0] = _symbol[_random.Next(9, 0x24)];
            for (int i = 1; i < length; i++)
            {
                chArray[i] = _symbol[_random.Next(0x24)];
            }
            return new string(chArray);
        }

        public static string Md5(string str)
        {
            string str2 = "";
            MD5 md = MD5.Create();
            byte[] buffer = md.ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < buffer.Length; i++)
            {
                str2 = str2 + buffer[i].ToString("X");
            }
            md.Dispose();
            return str2;
        }

        public static T[] ToArray<T>(string idString) where T: new()
        {
            string[] strArray = idString.Split(new char[] { ',' });
            List<T> list = new List<T>();
            Type conversionType = typeof(T);
            foreach (string str in strArray)
            {
                try
                {
                    list.Add((T) Convert.ChangeType(str, conversionType));
                }
                catch (InvalidCastException)
                {
                }
                catch (FormatException)
                {
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Convert to Plural
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToPlural(string word)
        {
            PluralizationService service = PluralizationService.CreateService(CultureInfo.CreateSpecificCulture("en-us"));
            if (service.IsPlural(word))
            {
                return word;
            }
            return service.Pluralize(word);
        }

        /// <summary>
        /// Convert to Singular
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToSingular(string word)
        {
            PluralizationService service = PluralizationService.CreateService(CultureInfo.CreateSpecificCulture("en-us"));
            if (service.IsSingular(word))
            {
                return word;
            }
            return service.Singularize(word);
        }

        /// <summary>
        /// Convert to Pascal
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascal(string str)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            if(str.Length<=1)
            {
                return str.ToUpper();
            }
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
    }
}

