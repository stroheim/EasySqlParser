using System;

namespace EasySqlParser.SqlGenerator.Attributes
{
    /// <summary>
    ///     Attribute to indicate that it is a column for optimistic concurrency control.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VersionAttribute : Attribute
    {
    }
}
