using System;
using System.Configuration;

namespace EasyORM.Configuration
{
    /// <summary>
    /// Configuration Section
    /// </summary>
    public class ConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("loggers", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(LoggerCollection), AddItemName = "add")]
        public LoggerCollection Loggers
        {
            get
            {
                return (LoggerCollection)base["loggers"];
            }
        }
        [ConfigurationProperty("dataBase")]
        public string DataBase
        {
            get { return (string)base["dataBase"]; }
            set
            {
                base["dataBase"] = value;
            }
        }

        [ConfigurationProperty("connectionStringName")]
        public string ConnectionStringName
        {
            get
            {
                return (string)base["connectionStringName"];
            }
            set
            {
                base["connectionStringName"] = value;
            }
        }

        [ConfigurationProperty("dbFactoryName")]
        public string DbFactoryName
        {
            get
            {
                return (string)base["dbFactoryName"];
            }
            set
            {
                base["dbFactoryName"] = value;
            }
        }

        [ConfigurationProperty("sequenceTable")]
        public string SequenceTable
        {
            get
            {
                return (string)base["sequenceTable"];
            }
            set
            {
                base["sequenceTable"] = value;
            }
        }

        [ConfigurationProperty("sqlBuilder")]
        public string SqlBuilder
        {
            get
            {
                return (string)base["sqlBuilder"];
            }
            set
            {
                base["sqlBuilder"] = value;
            }
        }

        [ConfigurationProperty("autoCreateTables")]
        public string IsAutoCreateTables
        {
            get
            {
                return (string)base["autoCreateTables"];
            }
            set
            {
                base["autoCreateTables"] = value;
            }
        }
        [ConfigurationProperty("allwayAutoCreateTables")]
        public string IsEnableAllwayAutoCreateTables
        {
            get
            {
                return (string)base["allwayAutoCreateTables"];
            }
            set
            {
                base["allwayAutoCreateTables"] = value;
            }
        }

        [ConfigurationProperty("enableLog")]
        public bool IsEnableLog
        {
            get
            {
                var enableLog = Convert.ToString(this["enableLog"]);
                if (string.IsNullOrWhiteSpace(enableLog))
                {
                    return false;
                }
                var isEnableLog = false;
                bool.TryParse(enableLog, out isEnableLog);
                return isEnableLog;
            }
        }
    }
}
