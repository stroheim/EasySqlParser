using System;

namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     Attributes to be added when the implementation is automatically generated from the interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class DaoAttribute : Attribute
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="DaoAttribute"/> class.
        /// </summary>
        /// <param name="generationType">the generationType</param>
        /// <param name="loggerType">the loggerType</param>
        /// <param name="sqlFileRootDirectory">Root directory where SQL files are stored </param>
        /// <param name="configName">The name of the Config added with EasySqlParser</param>
        /// <remarks>
        /// SQL file path is estimated automatically.<br/>
        /// SQL file path is
        /// _sqlFileRootDirectory_/The interface namespace [.] Converted to [/]/methodName.sql<br/>
        /// e.g.)
        ///   1. The interface namespace is [aaa.bbb.EmployeeDao]
        ///   2. Method name is [selectById]
        ///   result: _sqlFileRootDirectory_/aaa/bbb/EmployeeDao/selectById.sql
        /// </remarks>
        public DaoAttribute(
            GenerationType generationType = GenerationType.EntityFrameworkCore,
            LoggerType loggerType = LoggerType.MicrosoftExtensionsLogging,
            string sqlFileRootDirectory = "SqlResources",
            string configName = null)
        {
            SqlFileRootDirectory = sqlFileRootDirectory;
            GenerationType = generationType;
            LoggerType = loggerType;
            ConfigName = configName;
        }


        /// <summary>
        ///     Gets the <see cref="Attributes.GenerationType"/>.
        /// </summary>
        public GenerationType GenerationType { get; }

        /// <summary>
        ///     Gets the <see cref="Attributes.LoggerType"/>.
        /// </summary>
        public LoggerType LoggerType { get; }

        /// <summary>
        ///     Gets sql file root directory.
        /// </summary>
        public string SqlFileRootDirectory { get; }

        /// <summary>
        ///     Gets EasySqlParser additional configuration name.
        /// </summary>
        public string ConfigName { get; }
    }

    /// <summary>
    ///     Implementation method to be automatically generated.
    /// </summary>
    public enum GenerationType
    {
        /// <summary> use EntityFrameworkCore </summary>
        EntityFrameworkCore,
        /// <summary> use Dapper </summary>
        Dapper
    }

    /// <summary>
    ///     Logger used in automatically generated classes.
    /// </summary>
    public enum LoggerType
    {
        /// <summary> do not logging </summary>
        Disable,
        /// <summary> use Log4net </summary>
        Log4net,
        /// <summary> use NLog </summary>
        NLog,
        /// <summary> use Serilog </summary>
        Serilog,
        /// <summary> use Diagnostic </summary>
        Diagnostics,
        /// <summary> use Microsoft.Extension.Logging </summary>
        MicrosoftExtensionsLogging,
    }

}
