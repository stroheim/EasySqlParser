using System;

namespace EasySqlParser.SourceGenerator.Attributes
{
    /// <summary>
    ///     The base implementation of the attributes attached to the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MethodAttributeBase : Attribute
    {
        /// <summary>
        ///     Gets or sets 2-way-sql file path.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        ///     Gets or sets the command timeout (in seconds).
        ///     If not set, IQueryBuilderConfiguration.CommandTimeout is used.
        /// </summary>
        public int CommandTimeout { get; set; } = -1;

    }
}
