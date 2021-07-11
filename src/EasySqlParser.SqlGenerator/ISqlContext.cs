using System;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator
{
    /// <summary>
    ///     A context for stacking various SQLs(INSERT,UPDATE,DELETE) and executing them in the same transaction.
    /// </summary>
    public interface ISqlContext
    {
        /// <summary>
        ///     Gets a <see cref="EasySqlParser.SqlGenerator.Enums.SaveChangesBehavior"/> .
        /// </summary>
        SaveChangesBehavior SaveChangesBehavior { get; }

        /// <summary>
        ///     Add <see cref="FormattableString"/> .
        /// </summary>
        /// <param name="sqlTemplate"></param>
        /// <param name="forceFirst">Set true if you want to execute before DbContext.SaveChanges at SaveChanges, otherwise set false.</param>
        void Add(FormattableString sqlTemplate, bool forceFirst = false);

        /// <summary>
        ///     Add <see cref="SqlParserResult"/> .
        /// </summary>
        /// <param name="parserResult"></param>
        /// <param name="merge">Set true for SQL Server merge statements, otherwise set false.</param>
        /// <param name="forceFirst">Set true if you want to execute before DbContext.SaveChanges at SaveChanges, otherwise set false.</param>
        void Add(SqlParserResult parserResult, bool merge = false, bool forceFirst = false);

        /// <summary>
        ///     Add <see cref="QueryBuilderParameter"/> .
        /// </summary>
        /// <param name="builderParameter"></param>
        /// <param name="forceFirst">Set true if you want to execute before DbContext.SaveChanges at SaveChanges, otherwise set false.</param>
        void Add(QueryBuilderParameter builderParameter, bool forceFirst = false);

        /// <summary>
        ///     Saves all changes made in this context to the database.
        /// </summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
