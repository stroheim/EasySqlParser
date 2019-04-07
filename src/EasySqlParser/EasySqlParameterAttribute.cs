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
        public EasySqlParameterAttribute(DbType dbType)
        {
            DbType = dbType;
        }
        public DbType DbType { get; }
    }
}
