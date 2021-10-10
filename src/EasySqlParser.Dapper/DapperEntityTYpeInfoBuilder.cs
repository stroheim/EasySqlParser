using System;
using System.Collections.Generic;
using System.Reflection;
using EasySqlParser.SqlGenerator.Metadata;

namespace EasySqlParser.Dapper
{
    /// <summary>
    ///     <see cref="IEntityTYpeInfoBuilder"/> implementation for Dapper.
    /// </summary>
    public class DapperEntityTYpeInfoBuilder : IEntityTYpeInfoBuilder
    {
        /// <inheritdoc />
        public KeyValuePair<Type, EntityTypeInfo>[] Build(IEnumerable<Assembly> assemblies)
        {
            return DapperMapBuilder.CreateMap(assemblies);
        }
    }
}
