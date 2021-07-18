using EasySqlParser.SqlGenerator;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore
{
    /// <summary>
    ///     Generic interface for <see cref="ISqlContext"/>
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public interface ISqlContext<out TDbContext> : ISqlContext
        where TDbContext : DbContext
    {
        /// <summary>
        ///     Gets the <see cref="DbContext"/> instance.
        /// </summary>
        TDbContext Context { get; }
    }
}
