using System;
using System.Data;

namespace EasySqlParser
{
    /// <summary>
    /// Attributes for rewriting the value of <see cref="System.Data.DbType"/> .
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EasySqlParameterAttribute : Attribute
    {
        /// <summary>
        /// Create a new EasySqlParameterAttribute instance
        /// </summary>
        /// <param name="dbType"></param>
        public EasySqlParameterAttribute(DbType dbType)
        {
            DbType = dbType;
        }

        /// <summary>
        /// A new value of <see cref="System.Data.DbType"/>
        /// </summary>
        public DbType DbType { get; }
    }
}
