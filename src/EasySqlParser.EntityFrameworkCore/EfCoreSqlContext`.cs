using EasySqlParser.SqlGenerator.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore
{
    /// <summary>
    ///     Generic implementation for <see cref="EfCoreSqlContext"/>.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public class EfCoreSqlContext<TDbContext> : EfCoreSqlContext, ISqlContext<TDbContext>
        where TDbContext : DbContext
    {
        /// <inheritdoc />
        public TDbContext Context { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EfCoreSqlContext"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public EfCoreSqlContext(TDbContext context, IQueryBuilderConfiguration configuration) 
            : base(context, configuration)
        {
            Context = context;
        }
    }
}
