using System;

// ReSharper disable once CheckNamespace
namespace EasySqlParser.SqlGenerator.Enums
{
    /// <summary>
    ///     Strategy for generating current timestamp.
    /// </summary>
    [Flags]
    public enum GenerationStrategy
    {
        /// <summary>
        ///     not generate.
        /// </summary>
        None = 0,
        /// <summary>
        ///     generate on insert.
        /// </summary>
        Insert = 1,
        /// <summary>
        ///     generate on update.
        /// </summary>
        Update = 1 << 1,
        /// <summary>
        ///     generate on soft delete.
        /// </summary>
        SoftDelete = 1 << 2,
        /// <summary>
        ///     generate on always(insert,update,soft delete).
        /// </summary>
        Always = Insert | Update | SoftDelete
    }
}
