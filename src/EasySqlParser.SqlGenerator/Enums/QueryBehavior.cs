namespace EasySqlParser.SqlGenerator.Enums
{
    /// <summary>
    /// INSERT または UPDATE 時の動作
    /// </summary>
    public enum QueryBehavior
    {
        /// <summary>
        /// 無し
        /// エンティティに変更結果は戻されない
        /// </summary>
        None,
        /// <summary>
        /// 自動採番列の値のみエンティティに戻される
        /// </summary>
        IdentityOnly,
        /// <summary>
        /// 全ての値がエンティティに戻される
        /// </summary>
        AllColumns,
        /// <summary>
        /// 自動採番列があればその値のみを
        /// そうでない場合はすべての列がエンティティに戻される
        /// </summary>
        IdentityOrAllColumns
    }

}
