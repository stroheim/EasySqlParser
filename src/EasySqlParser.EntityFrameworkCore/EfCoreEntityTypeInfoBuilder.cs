using System;
using System.Collections.Generic;
using System.Reflection;
using EasySqlParser.SqlGenerator.Metadata;
using Microsoft.EntityFrameworkCore;

namespace EasySqlParser.EntityFrameworkCore
{
    /// <summary>
    ///     <see cref="IEntityTYpeInfoBuilder"/> implementation for EntityFrameworkCore.
    /// </summary>
    public class EfCoreEntityTypeInfoBuilder : IEntityTYpeInfoBuilder
    {
        private readonly DbContext _dbContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EfCoreEntityTypeInfoBuilder"/> class.
        /// </summary>
        /// <param name="dbContext"></param>
        public EfCoreEntityTypeInfoBuilder(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public KeyValuePair<Type, EntityTypeInfo>[] Build(IEnumerable<Assembly> assemblies)
        {
            return InternalEfCoreEntityTypeInfoBuilder.Build(_dbContext, assemblies);
        }
    }
}
