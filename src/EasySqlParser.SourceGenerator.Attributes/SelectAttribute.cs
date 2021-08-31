namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     Attribute for SELECT sql.
    /// </summary>
    public class SelectAttribute : MethodAttributeBase
    {
        /// <summary>
        ///     Gets or sets whether to map via DbSet.
        /// </summary>
        public bool UseDbSet { get; set; }
    }
}
