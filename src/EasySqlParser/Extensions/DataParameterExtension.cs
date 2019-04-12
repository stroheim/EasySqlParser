using System.Data;

namespace EasySqlParser.Extensions
{
    /// <summary>
    /// Extension of <see cref="IDbDataParameter"/>
    /// </summary>
    public static class DataParameterExtension
    {
        /// <summary>
        /// Add parameter name
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDbDataParameter AddName(this IDbDataParameter source, string name)
        {
            source.ParameterName = name;
            return source;
        }

        /// <summary>
        /// Add parameter value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDbDataParameter AddValue(this IDbDataParameter source, object value)
        {
            source.Value = value;
            return source;
        }

        /// <summary>
        /// Add parameter DbType
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static IDbDataParameter AddDbType(this IDbDataParameter source, DbType dbType)
        {
            source.DbType = dbType;
            return source;
        }
    }
}
