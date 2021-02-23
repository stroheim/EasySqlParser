using System;
using System.Data;
using EasySqlParser.Internals.Dialect;

namespace EasySqlParser.Configurations
{
    /// <summary>
    /// Configuration of sql parser.
    /// </summary>
    public sealed class SqlParserConfig
    {
        internal SqlParserConfig()
        {

        }

        /// <summary>
        /// kind of database connection
        /// </summary>
        public DbConnectionKind DbConnectionKind { get; set; } = DbConnectionKind.Unknown;

        internal StandardDialect Dialect { get; set; }

        /// <summary>
        /// Delegate for create <see cref="IDbDataParameter"/> instance.
        /// </summary>
        internal Func<IDbDataParameter> DataParameterCreator { get; set; }
    }

}
