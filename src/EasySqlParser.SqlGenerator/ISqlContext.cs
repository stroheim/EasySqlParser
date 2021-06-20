using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator
{
    public interface ISqlContext
    {
        SaveChangesBehavior SaveChangesBehavior { get; }

        void Add(FormattableString sqlTemplate, bool forceFirst = false);

        void Add(SqlParserResult parserResult, bool merge = false, bool forceFirst = false);

        void Add(QueryBuilderParameter builderParameter, bool forceFirst = false);

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
