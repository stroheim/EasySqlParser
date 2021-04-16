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

        internal bool IsAvailable(SqlKind sqlKind)
        {
            switch (Strategy)
            {
                case GenerationStrategy.Always:
                    return true;
                case GenerationStrategy.Insert:
                    if (sqlKind == SqlKind.Insert)
                    {
                        return true;
                    }
                    break;
                case GenerationStrategy.InsertOrUpdate:
                    if (sqlKind == SqlKind.Insert ||
                        sqlKind == SqlKind.Update)
                    {
                        return true;
                    }
                    break;
                case GenerationStrategy.Update:
                    if (sqlKind == SqlKind.Update)
                    {
                        return true;
                    }
                    break;
                case GenerationStrategy.SoftDelete:
                    if (sqlKind == SqlKind.SoftDelete)
                    {
                        return true;
                    }
                    break;
                default:
                    return false;
            }

            return false;
        }
    }
}
