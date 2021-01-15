using System;

namespace EasySqlParser.SourceGenerator
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DaoAttribute : Attribute
    {
        //public DaoAttribute(GenerationType generationType = GenerationType.Dapper,
        //    string sqlFileRootDirectory = "SqlResources",
        //    string configName = null)
        //    : this(null, generationType, sqlFileRootDirectory, configName)
        //{

        //}

        public DaoAttribute(GenerationType generationType = GenerationType.EntityFrameworkCore,
            LoggerType loggerType = LoggerType.MicrosoftExtensionsLogging,
            string sqlFileRootDirectory = "SqlResources",
            string configName = null)
        {

            SqlFileRootDirectory = sqlFileRootDirectory;
            GenerationType = generationType;
            LoggerType = loggerType;
            ConfigName = configName;
        }


        //public SqlParserConfig Config { get; }
        //public string ConfigName { get; }

        public GenerationType GenerationType { get; }

        public LoggerType LoggerType { get; }


        public string SqlFileRootDirectory { get; }

        public string ConfigName { get; }
    }

    public enum GenerationType
    {
        EntityFrameworkCore,
        Dapper
    }

    public enum LoggerType
    {
        Disable,
        Log4net,
        NLog,
        Serilog,
        Diagnostics,
        MicrosoftExtensionsLogging,
    }

}
