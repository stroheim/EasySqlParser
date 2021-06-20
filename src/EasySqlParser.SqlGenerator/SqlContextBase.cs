using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator.Enums;
using EasySqlParser.SqlGenerator.Helpers;

namespace EasySqlParser.SqlGenerator
{
    public abstract class SqlContextBase : ISqlContext
    {
        public SaveChangesBehavior SaveChangesBehavior { get; } = SaveChangesBehavior.SqlContextOnly;

        //protected abstract DbConnection Connection { get; }

        public abstract void Add(FormattableString sqlTemplate, bool forceFirst = false);

        public abstract void Add(SqlParserResult parserResult, bool merge = false, bool forceFirst = false);

        public abstract void Add(QueryBuilderParameter builderParameter, bool forceFirst = false);

        public abstract int SaveChanges();

        public abstract Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        //protected int Save(QueryBuilderParameter builderParameter)
        //{
        //    SequenceHelper.Generate(Connection, builderParameter);
        //    var builderResult = QueryBuilder.GetQueryBuilderResult(builderParameter);
        //    builderParameter.SaveExpectedVersion();
        //    int affectedCount;
        //    switch (builderParameter.CommandExecutionType)
        //    {
        //        case CommandExecutionType.ExecuteNonQuery:
        //            affectedCount = ConsumeNonQuery();
        //            break;
        //        case CommandExecutionType.ExecuteReader:
        //            affectedCount = ConsumeReader();
        //            break;
        //        case CommandExecutionType.ExecuteScalar:
        //            affectedCount = ConsumeScalar();
        //            break;
        //        default:
        //            // TODO: error
        //            throw new InvalidOperationException("");

        //    }
        //    ThrowIfOptimisticLockException(builderParameter, affectedCount, builderResult);
        //    builderParameter.IncrementVersion();
        //    return affectedCount;

        //}

        //protected abstract int ConsumeNonQuery();

        //protected abstract int ConsumeReader();

        //protected abstract int ConsumeScalar();

        //protected abstract class SqlItem
        //{
        //    protected string Sql;
        //}

        private static void ThrowIfOptimisticLockException(
            QueryBuilderParameter parameter,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int affectedCount,
            QueryBuilderResult builderResult)
        {
            if (parameter.ThrowableOptimisticLockException(affectedCount))
            {
                throw new OptimisticLockException(builderResult.ParsedSql, builderResult.DebugSql, parameter.SqlFile);
            }
        }

    }
}
