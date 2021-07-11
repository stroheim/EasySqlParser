namespace EasySqlParser.SqlGenerator.Enums
{
    /// <summary>
    /// Behavior during save changes.
    /// </summary>
    public enum SaveChangesBehavior
    {
        /// <summary>
        /// Use <see cref="ISqlContext"/> only.
        /// In other words, If you are not using EfCore.
        /// </summary>
        SqlContextOnly,
        /// <summary>
        /// Call DbContext.SaveChanges, SqlContext.SaveChanges in that order.
        /// </summary>
        DbContextFirst,
        /// <summary>
        /// Call SqlContext.SaveChanges, DbContext.SaveChanges in that order.
        /// </summary>
        SqlContextFirst
    }
}
