using System;

namespace EasySqlParser.SourceGenerator
{
    // TODO: DOC
    /// <summary>
    /// インターフェースから実装を自動生成する際に付加する属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class DaoAttribute : Attribute
    {

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="generationType">the generationType</param>
        /// <param name="loggerType">the loggerType</param>
        /// <param name="sqlFileRootDirectory">SQLファイルが格納されているルートディレクトリ</param>
        /// <param name="configName">EasySqlParserで追加したConfigの名前</param>
        /// <remarks>
        /// sqlFileRootDirectoryが設定されている場合はSQLファイルパスは自動的に推定されます<br/>
        /// その際のSQLファイルパスは
        /// _sqlFileRootDirectory_/インターフェースの名前空間の[.]を[/]に変換したもの/メソッド名.sql
        /// となります
        /// </remarks>
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


        /// <summary>
        /// GenerationType
        /// </summary>
        public GenerationType GenerationType { get; }

        /// <summary>
        /// LoggerType
        /// </summary>
        public LoggerType LoggerType { get; }


        /// <summary>
        /// SqlFileRootDirectory
        /// </summary>
        public string SqlFileRootDirectory { get; }

        /// <summary>
        /// EasySqlParser additional configuration name
        /// </summary>
        public string ConfigName { get; }
    }

    /// <summary>
    /// 自動生成する実装方式
    /// </summary>
    public enum GenerationType
    {
        /// <summary> use EntityFrameworkCore </summary>
        EntityFrameworkCore,
        /// <summary> use Dapper </summary>
        Dapper
    }

    /// <summary>
    /// 自動生成されるクラスで利用されるロガー
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
