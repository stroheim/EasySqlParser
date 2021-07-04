using System;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CurrentUserAttribute : Attribute
    {
        public CurrentUserAttribute(GenerationStrategy strategy = GenerationStrategy.Always)
        {
            Strategy = strategy;
        }

        public GenerationStrategy Strategy { get; }

    }
}
