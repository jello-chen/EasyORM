namespace EasyORM.Configuration
{
    public class Config
    {

        /// <summary>
        /// sequence table
        /// </summary>
        public static string SequenceTable { get; set; }

        /// <summary>
        /// custom SqlBuilder class full name
        /// </summary>
        public static string SqlBuilder { get; internal set; }

        /// <summary>
        /// whether that tables are created is enabled
        /// </summary>
        public static bool IsEnableAutoCreateTables { get; internal set; }

        /// <summary>
        /// whether that tables are allways created is enabled,
        /// if false,when the specific database has no tables,then tables are created
        /// </summary>
        public static bool IsEnableAllwayAutoCreateTables { get; internal set; }

        /// <summary>
        /// whether log is enabled
        /// </summary>
        public static bool IsEnableLog { get; set; }

        public static string DataBase { get; set; }

    }
}
