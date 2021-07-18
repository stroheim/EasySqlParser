using System;

namespace EasySqlParser.SqlGenerator.Attributes
{
    /// <summary>
    ///     Attribute to indicate the soft delete key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SoftDeleteKeyAttribute : Attribute
    {
    }
}
