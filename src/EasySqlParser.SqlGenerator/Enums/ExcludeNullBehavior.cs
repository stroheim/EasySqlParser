using System;

namespace EasySqlParser.SqlGenerator.Enums
{
    // TODO: name change to ExcludeNullBehaviors

    /// <summary>
    ///     Behavior during INSERT or UPDATE using 'exclude null'.
    /// </summary>
    [Flags]
    public enum ExcludeNullBehavior
    {
        /// <summary>
        ///     Not exclude
        /// </summary>
        None = 0,
        /// <summary>
        ///     exclude null
        /// </summary>
        Null = 1,   // 1
        /// <summary>
        ///     exclude <see cref="string.Empty"/>.
        /// </summary>
        Empty = 1 << 1, // 2
        /// <summary>
        ///     exclude default value of embedded <see cref="ValueType"/>.<br/>
        ///     e.g) int, long, decimal..
        /// </summary>
        NumericDefault = 1 << 2,    // 4

        /// <summary>
        ///     exclude null or <see cref="string.Empty"/>.
        /// </summary>
        NullOrEmpty = Null | Empty,

        /// <summary>
        ///     exclude null or <see cref="string.Empty"/> or default value of embedded <see cref="ValueType"/>.
        /// </summary>
        All = Null | Empty | NumericDefault,

        /// <summary>
        ///     null only
        /// </summary>
        [Obsolete]
        NullOnly,
        /// <summary>
        /// null,<see cref="string.Empty"/>,default value of <see cref="ValueType"/> を除外
        /// </summary>
        [Obsolete]
        NullOrEmptyOrDefaultValue
    }

}
