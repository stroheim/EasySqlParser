using System;
using System.Collections.Generic;
using System.Text;

namespace EasySqlParser.SqlGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CurrentUserAttribute : Attribute
    {
        public CurrentUserAttribute(GenerationStrategy strategy = GenerationStrategy.Always)
        {
            Strategy = strategy;
        }

        public GenerationStrategy Strategy { get; }

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
