namespace EasyORM.TranslateModel
{
    public class Join
    {
        public Column Left { get; set; }
        public JoinType JoinType { get; set; }
        public Column Right { get; set; }

        public override string ToString()
        {
            return Left.Table.Name + " " + JoinType.ToString() + " join " + Right.Table.Name + " on " + Left.Name + " = " + Right.Name;
        }
    }

    public enum JoinType
    {
        Inner,
        Left
    }
}
