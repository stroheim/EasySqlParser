using System;
using System.Data;
using EasySqlParser.Dialect;

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
        public DbConnectionKind DbConnectionKind { get; internal set; } = DbConnectionKind.Unknown;

        /// <summary>
        /// A RDB dialect
        /// </summary>
        public StandardDialect Dialect { get; internal set; }

        /// <summary>
        /// Delegate for create <see cref="IDbDataParameter"/> instance.
        /// </summary>
        public Func<IDbDataParameter> DataParameterCreator { get; internal set; }
    }

}
