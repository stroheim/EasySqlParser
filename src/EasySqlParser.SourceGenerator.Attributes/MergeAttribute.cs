namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     Attribute for MERGE sql.
    /// </summary>
    public class MergeAttribute : MethodAttributeBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MergeAttribute"/> class.
        /// </summary>
        /// <param name="sqlFilePath"></param>
        public MergeAttribute(string sqlFilePath)
        {
            FilePath = sqlFilePath;
        }

        /// <summary>
        ///     Gets or sets whether use Microsoft SQL Server.
        /// </summary>
        public bool UseSqlServer { get; set; }
    }
}
