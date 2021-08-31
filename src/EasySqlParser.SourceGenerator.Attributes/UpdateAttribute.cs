namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     Attribute for UPDATE sql.
    /// </summary>
    public class UpdateAttribute : MethodAttributeBase
    {

        /// <summary>
        ///     Gets or sets whether the version property is ignored.
        /// </summary>
        /// <remarks>
        ///     Do not include version no in update conditions.<br/>
        ///     Version no itself is updated with the set value.
        /// </remarks>
        public bool IgnoreVersion { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether SQL NULL columns are excluded from SQL INSERT statements.
        /// </summary>
        public bool ExcludeNull { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether OptimisticLockException is suppressed.
        /// </summary>
        /// <remarks>
        ///     Include version no. in update conditions.<br/>
        ///     Do not throw OptimisticLockException even if the number of updates is 0.
        /// </remarks>
        public bool SuppressOptimisticLockException { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether to automatically generate SQL statements. 
        /// </summary>
        public bool AutoGenerateSql { get; set; } = true;
    }
}
