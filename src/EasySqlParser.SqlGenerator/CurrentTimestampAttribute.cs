using System;

namespace EasySqlParser.SqlGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CurrentTimestampAttribute : Attribute
    {
        public CurrentTimestampAttribute(
            GenerationStrategy strategy = GenerationStrategy.Always)
        {
            Strategy = strategy;
        }
        public GenerationStrategy Strategy { get; }

        public string Format { get; set; }
    }
}
