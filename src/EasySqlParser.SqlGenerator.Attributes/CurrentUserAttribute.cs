using System;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Attributes
{
    /// <summary>
    ///     Attribute for generating the current user as an SQL statement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CurrentUserAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentUserAttribute"/> class.
        /// </summary>
        /// <param name="strategy"></param>
        public CurrentUserAttribute(GenerationStrategy strategy = GenerationStrategy.Always)
        {
            Strategy = strategy;
        }


        /// <summary>
        ///     Gets the <see cref="GenerationStrategy"/>.
        /// </summary>
        public GenerationStrategy Strategy { get; }

    }
}
