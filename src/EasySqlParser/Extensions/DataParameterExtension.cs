using System.Data;

namespace EasySqlParser.Extensions
{
    public static class DataParameterExtension
    {
        public static IDbDataParameter AddName(this IDbDataParameter source, string name)
        {
            source.ParameterName = name;
            return source;
        }

        public static IDbDataParameter AddValue(this IDbDataParameter source, object value)
        {
            source.Value = value;
            return source;
        }

        public static IDbDataParameter AddDbType(this IDbDataParameter source, DbType dbType)
        {
            source.DbType = dbType;
            return source;
        }
    }
}
