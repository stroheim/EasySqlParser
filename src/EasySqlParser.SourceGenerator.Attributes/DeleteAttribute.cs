namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     Attribute for DELETE sql.
    /// </summary>
    public class DeleteAttribute : MethodAttributeBase
    {
        /// <summary>
        ///     Gets or sets whether the version property is ignored.
        /// </summary>
        /// <remarks>
        ///     Do not include version no in update conditions.<br/>
        ///     Version no itself is updated with the set value.
        /// </remarks>
        public bool IgnoreVersion { get; set; } = false;

        ///// <summary>
        ///// EfCoreが想定しているRowVersion型など特殊なものではなく、longなど一般的な型を使って楽観排他を行う
        ///// 更新件数が0件の場合は `OptimisticLockException` をスローする
        ///// </summary>
        //public bool UseVersion { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether OptimisticLockException is suppressed.
        /// </summary>
        /// <remarks>
        ///     Include version no. in update conditions.<br/>
        ///     Do not throw OptimisticLockException even if the number of updates is 0.
        /// </remarks>
        public bool SuppressOptimisticLockException { get; set; } = false;
    }
}
