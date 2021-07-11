using System;

namespace EasySqlParser.SqlGenerator.Enums
{

    /// <summary>
    ///     Behavior during INSERT or UPDATE using 'exclude null'.
    /// </summary>
    public enum ExcludeNullBehavior
    {
        /// <summary>
        /// null only
        /// </summary>
        NullOnly,
        /// <summary>
        /// null,<see cref="string.Empty"/>,default value of <see cref="ValueType"/> を除外
        /// </summary>
        NullOrEmptyOrDefaultValue
    }

}
