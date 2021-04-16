using System;

namespace EasySqlParser.SqlGenerator.Enums
{

    /// <summary>
    /// ExcludeNull の動作
    /// </summary>
    public enum ExcludeNullBehavior
    {
        /// <summary>
        /// nullのみを除外
        /// </summary>
        NullOnly,
        /// <summary>
        /// null,<see cref="string.Empty"/>,default value of <see cref="ValueType"/> を除外
        /// </summary>
        NullOrEmptyOrDefaultValue
    }

}
