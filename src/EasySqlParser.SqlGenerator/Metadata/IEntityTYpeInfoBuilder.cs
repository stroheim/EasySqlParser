using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasySqlParser.SqlGenerator.Metadata
{
    /// <summary>
    ///     A interface for building entity information.
    /// </summary>
    public interface IEntityTYpeInfoBuilder
    {
        /// <summary>
        ///     Build entity information from assembly.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        KeyValuePair<Type, EntityTypeInfo>[] Build(IEnumerable<Assembly> assemblies);
    }
}
