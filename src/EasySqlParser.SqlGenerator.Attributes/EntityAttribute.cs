using System;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Attributes
{
    /// <summary>
    ///     Attribute for specifying the database table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityAttribute : Attribute
    {
        /// <summary>
        ///     Gets the <see cref="Naming"/>.
        /// </summary>
        public Naming Naming { get; set; } = Naming.None;
    }
}
