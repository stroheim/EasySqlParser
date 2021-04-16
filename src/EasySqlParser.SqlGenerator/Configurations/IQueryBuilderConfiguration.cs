using System;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.SqlGenerator.Configurations
{
    public interface IQueryBuilderConfiguration
    {
        int CommandTimeout { get; }
        bool WriteIndented { get; }
        QueryBehavior QueryBehavior { get; }
        ExcludeNullBehavior ExcludeNullBehavior { get; }
        Action<string> LoggerAction { get; }

        EntityTypeInfo GetEntityTypeInfo(Type type);


    }
}
