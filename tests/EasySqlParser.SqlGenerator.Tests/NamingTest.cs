using System;
using System.Collections.Generic;
using System.Text;
using EasySqlParser.SqlGenerator.Attributes;
using EasySqlParser.SqlGenerator.Attributes.Extensions;
using Xunit;

namespace EasySqlParser.SqlGenerator.Tests
{
    public class NamingTest
    {
        [Fact]
        public void testFromSnakeCaseToPascalCase()
        {
            NamingExtension.FromSnakeCaseToPascalCase("AAA_BBB_CCC").Is("AaaBbbCcc");
            NamingExtension.FromSnakeCaseToPascalCase("aaa_bbb_ccc").Is("AaaBbbCcc");
            NamingExtension.FromSnakeCaseToPascalCase("ABC").Is("Abc");
            NamingExtension.FromSnakeCaseToPascalCase("abc").Is("Abc");
        }

        [Fact]
        public void testFromPascalCaseToSnakeCase()
        {
            NamingExtension.FromPascalCaseToSnakeCase("AaaBbbCcc").Is("aaa_bbb_ccc");
            NamingExtension.FromPascalCaseToSnakeCase("Abc").Is("abc");
            NamingExtension.FromPascalCaseToSnakeCase("Aa1BbbCcc").Is("aa1_bbb_ccc");

        }
    }
}
