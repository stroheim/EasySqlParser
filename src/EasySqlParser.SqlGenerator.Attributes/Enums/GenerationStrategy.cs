using System;

// ReSharper disable once CheckNamespace
namespace EasySqlParser.SqlGenerator.Enums
{
    [Flags]
    public enum GenerationStrategy
    {
        None = 0,
        Insert = 1,
        Update = 1 << 1,
        SoftDelete = 1 << 2,
        Always = Insert | Update | SoftDelete
    }
}
