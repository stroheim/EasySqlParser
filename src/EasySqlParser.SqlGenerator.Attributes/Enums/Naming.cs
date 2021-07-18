// ReSharper disable once CheckNamespace
namespace EasySqlParser.SqlGenerator.Enums
{
    /// <summary>
    ///     A naming convention.
    /// </summary>
    public enum Naming
    {
        /// <summary>
        ///     Converts nothing.
        /// </summary>
        None,
        /// <summary>
        ///     Converts the camel case text to the lower case text.
        /// </summary>
        LowerCase,
        /// <summary>
        ///     Converts the camel case text to the upper case text.
        /// </summary>
        UpperCase,
        /// <summary>
        ///     Converts the camel case text to the text that is snake case and lower case.
        /// </summary>
        SnakeLowerCase,
        /// <summary>
        ///     Converts the camel case text to the text that is snake case and upper case.
        /// </summary>
        SnakeUpperCase
    }

}
