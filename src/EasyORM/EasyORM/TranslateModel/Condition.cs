namespace EasyORM.TranslateModel
{
    public class Condition 
    {
        public Token Left { get; set; }
        public CompareType CompareType { get; set; }
        public Token Right { get; set; }
    }
}
