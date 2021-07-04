using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Enums;

namespace EasySqlParser.SqlGenerator.Helpers
{
    public static class SequenceHelper
    {
        public static void Generate(DbConnection connection, QueryBuilderParameter builderParameter)
        {
            if (builderParameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            builderParameter.WriteLog("[Start] Sequence Generate");
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                var value = GenerateSequence(connection, builderParameter, columnInfo.SequenceGeneratorAttribute,
                    columnInfo.PropertyInfo.PropertyType);
                columnInfo.PropertyInfo.SetValue(builderParameter.Entity, value);

            }
            builderParameter.WriteLog("[End] Sequence Generate");
        }

        public static async Task GenerateAsync(
            DbConnection connection, 
            QueryBuilderParameter builderParameter,
            CancellationToken cancellationToken = default)
        {
            if (builderParameter.SqlKind != SqlKind.Insert) return;
            var entityInfo = builderParameter.EntityTypeInfo;
            var config = builderParameter.Config;
            if (!config.Dialect.SupportsSequence) return;
            if (entityInfo.SequenceColumns.Count == 0) return;
            builderParameter.WriteLog("[Start] Sequence GenerateAsync");
            foreach (var columnInfo in entityInfo.SequenceColumns)
            {
                var value = await GenerateSequenceAsync(connection, builderParameter,
                    columnInfo.SequenceGeneratorAttribute, 
                    columnInfo.PropertyInfo.PropertyType,
                    cancellationToken);
                columnInfo.PropertyInfo.SetValue(builderParameter.Entity, value);

            }
            builderParameter.WriteLog("[End] Sequence GenerateAsync");
        }

        private static object GenerateSequence(
            DbConnection connection,
            QueryBuilderParameter builderParameter,
            SequenceGeneratorAttribute attribute,
            Type expectedType)
        {
            var config = builderParameter.Config;
            var sql = GetSequenceGeneratorSql(config, attribute);
            builderParameter.WriteLog(sql);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                var rawResult = command.ExecuteScalar();
                if (rawResult.GetType() == expectedType)
                {
                    return rawResult;
                }

                return Convert.ChangeType(rawResult, expectedType);
            }
        }

        private static async Task<object> GenerateSequenceAsync(
            DbConnection connection,
            QueryBuilderParameter builderParameter,
            SequenceGeneratorAttribute attribute,
            Type expectedType,
            CancellationToken cancellationToken = default)
        {
            var config = builderParameter.Config;
            var sql = GetSequenceGeneratorSql(config, attribute);
            builderParameter.WriteLog(sql);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                var rawResult = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (rawResult.GetType() == expectedType)
                {
                    return rawResult;
                }

                return Convert.ChangeType(rawResult, expectedType);
            }

        }

        private static string GetSequenceGeneratorSql(SqlParserConfig config, SequenceGeneratorAttribute attribute)
        {
            return attribute.PaddingLength == 0
                ? config.Dialect.GetNextSequenceSql(attribute.SequenceName, attribute.SchemaName)
                : config.Dialect.GetNextSequenceSqlZeroPadding(attribute.SequenceName, attribute.SchemaName,
                    attribute.PaddingLength, attribute.Prefix);
        }


    }
}
