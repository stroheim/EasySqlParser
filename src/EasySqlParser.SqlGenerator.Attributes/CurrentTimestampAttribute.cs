using System;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CurrentTimestampAttribute : Attribute
    {
        public CurrentTimestampAttribute(
            string sql,
            GenerationStrategy strategy = GenerationStrategy.Always
        )
        {
            Strategy = strategy;
            Sql = sql;
        }

        public GenerationStrategy Strategy { get; }

        public string Sql { get; }

    }
}
