namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     Attribute for INSERT sql.
    /// </summary>
    public class InsertAttribute : MethodAttributeBase
    {
        /// <summary>
        ///     Gets or sets whether SQL NULL columns are excluded from SQL INSERT statements.
        /// </summary>
        public bool ExcludeNull { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether to automatically generate SQL statements. 
        /// </summary>
        public bool AutoGenerateSql { get; set; } = true;
    }
}
