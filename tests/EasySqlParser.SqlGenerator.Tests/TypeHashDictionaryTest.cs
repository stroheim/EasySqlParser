using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.SqlGenerator.Tests.SqlServer;
using Xunit;

namespace EasySqlParser.SqlGenerator.Tests
{
    public class TypeHashDictionaryTest
    {
        [Fact]
        public void Test1()
        {
            var values = EntityTypeInfoBuilder.Build(typeof(Employee));
            var dic = TypeHashDictionary<EntityTypeInfo>.Create(new KeyValuePair<Type, EntityTypeInfo>[]
                                                                {
                                                                    new KeyValuePair<Type, EntityTypeInfo>(
                                                                        typeof(Employee), values)
                                                                });
            dic.Count.Is(1);
            dic.TryGetValue(typeof(EmployeeIdentity), out _).IsFalse();
            var result = dic.Get(typeof(Employee));
            result.IsNotNull();

        }
    }
}
