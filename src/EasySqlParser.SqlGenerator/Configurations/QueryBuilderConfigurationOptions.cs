using System.Collections.Generic;
using System.Reflection;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Configurations
{
    /// <summary>
    ///     Options for <see cref="IQueryBuilderConfiguration"/>.
    /// </summary>
    public class QueryBuilderConfigurationOptions
    {
        /// <summary>
        ///     Gets or sets the command timeout (in seconds).
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        ///     Gets or sets a value that defines whether SQL should use pretty printing. 
        /// </summary>
        public bool WriteIndented { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Enums.QueryBehavior"/> .
        /// </summary>
        public QueryBehavior QueryBehavior { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Enums.ExcludeNullBehavior"/> .
        /// </summary>
        public ExcludeNullBehavior ExcludeNullBehavior { get; set; }

        /// <summary>
        ///     Additional assemblies containing classes not under DbSet control.
        /// </summary>
        public IEnumerable<Assembly> AdditionalAssemblies { get; set; }
    }
}