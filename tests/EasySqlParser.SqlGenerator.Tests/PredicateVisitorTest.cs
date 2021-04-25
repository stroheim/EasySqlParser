using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using EasySqlParser.Configurations;
using EasySqlParser.SqlGenerator.Metadata;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace EasySqlParser.SqlGenerator.Tests
{
    public class PredicateVisitorTest
    {
        private readonly ITestOutputHelper _output;
        public PredicateVisitorTest(ITestOutputHelper output)
        {
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                () => new SqlParameter()
            );
            _output = output;
        }


        [Fact]
        public void Test_Equal()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id == 1;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.ParsedSql.Is(" WHERE [Id] = @p_Id");
        }

        [Fact]
        public void Test_NotEqual()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id != 1;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.ParsedSql.Is(" WHERE [Id] <> @p_Id");
        }

        [Fact]
        public void Test_GreaterThan()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.ParsedSql.Is(" WHERE [Id] > @p_Id");

        }

        [Fact]
        public void Test_GreaterThanOrEqual()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id >= 1;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.ParsedSql.Is(" WHERE [Id] >= @p_Id");

        }

        [Fact]
        public void Test_LessThan()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id < 1;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.ParsedSql.Is(" WHERE [Id] < @p_Id");

        }

        [Fact]
        public void Test_LessThanOrEqual()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id <= 1;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.ParsedSql.Is(" WHERE [Id] <= @p_Id");

        }

        [Fact]
        public void Test_Null()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Name == null;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(0);
            result.ParsedSql.Is(" WHERE [Name] IS NULL");
        }

        [Fact]
        public void Test_NotNull()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Name != null;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(0);
            result.ParsedSql.Is(" WHERE [Name] IS NOT NULL");

        }

        [Fact]
        public void Test_And()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1 && x.Name == "John Doe";
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Id] > @p_Id AND [Name] = @p_Name");
        }

        [Fact]
        public void Test_Or()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1 || x.Name == "John Doe";
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(2);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Id] > @p_Id OR [Name] = @p_Name");
        }

        [Fact]
        public void Test_AndOr1()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1 && x.Name == "John Doe" || x.Age <= 30;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            result.DbDataParameters[2].ParameterName.Is("@p_Age");
            result.DbDataParameters[2].Value.Is(30);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE ([Id] > @p_Id AND [Name] = @p_Name) OR [Age] <= @p_Age");

        }

        [Fact]
        public void Test_AndOr2()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1 && (x.Name == "John Doe" || x.Age <= 30);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            result.DbDataParameters[2].ParameterName.Is("@p_Age");
            result.DbDataParameters[2].Value.Is(30);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Id] > @p_Id AND ([Name] = @p_Name OR [Age] <= @p_Age)");
        }

        [Fact]
        public void Test_AndOr3()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1 || x.Name == "John Doe" && x.Age <= 30;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            result.DbDataParameters[2].ParameterName.Is("@p_Age");
            result.DbDataParameters[2].Value.Is(30);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Id] > @p_Id OR ([Name] = @p_Name AND [Age] <= @p_Age)");
        }

        [Fact]
        public void Test_AndOr4()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => (x.Id > 1 || x.Name == "John Doe") && x.Age <= 30;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            result.DbDataParameters[2].ParameterName.Is("@p_Age");
            result.DbDataParameters[2].Value.Is(30);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE ([Id] > @p_Id OR [Name] = @p_Name) AND [Age] <= @p_Age");
        }

        [Fact]
        public void Test_AndOr5()
        {
            var ids = new[] {2, 3, 4};
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => (x.Id > 1 || x.Name == "John Doe") && x.Age <= 30 && ids.Contains(x.Id);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(6);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_Name");
            result.DbDataParameters[1].Value.Is("John Doe");
            result.DbDataParameters[2].ParameterName.Is("@p_Age");
            result.DbDataParameters[2].Value.Is(30);
            result.DbDataParameters[3].ParameterName.Is("@p_in_Id1");
            result.DbDataParameters[3].Value.Is(2);
            result.DbDataParameters[4].ParameterName.Is("@p_in_Id2");
            result.DbDataParameters[4].Value.Is(3);
            result.DbDataParameters[5].ParameterName.Is("@p_in_Id3");
            result.DbDataParameters[5].Value.Is(4);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE ([Id] > @p_Id OR [Name] = @p_Name) AND [Age] <= @p_Age AND [Id] IN (@p_in_Id1, @p_in_Id2, @p_in_Id3)");

        }

        [Fact]
        public void Test_AndOr6()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter =
                x => x.Name == "John Doe" || x.Name == "Jane Doe" || x.Name == "Zero";
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("John Doe");
            result.DbDataParameters[1].ParameterName.Is("@p_Name_1");
            result.DbDataParameters[1].Value.Is("Jane Doe");
            result.DbDataParameters[2].ParameterName.Is("@p_Name_2");
            result.DbDataParameters[2].Value.Is("Zero");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] = @p_Name OR [Name] = @p_Name_1 OR [Name] = @p_Name_2");

        }

        [Fact]
        public void Test_In1()
        {
            var names = new[] {"A", "B", "C"};
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => names.Contains(x.Name);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_in_Name1");
            result.DbDataParameters[0].Value.Is("A");
            result.DbDataParameters[1].ParameterName.Is("@p_in_Name2");
            result.DbDataParameters[1].Value.Is("B");
            result.DbDataParameters[2].ParameterName.Is("@p_in_Name3");
            result.DbDataParameters[2].Value.Is("C");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] IN (@p_in_Name1, @p_in_Name2, @p_in_Name3)");
        }

        [Fact]
        public void Test_In2()
        {
            var names = new[] { "A"};
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => names.Contains(x.Name);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_in_Name1");
            result.DbDataParameters[0].Value.Is("A");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] IN (@p_in_Name1)");
        }

        [Fact]
        public void Test_AndIn()
        {
            var names = new List<string>(new[] { "A", "B" });
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Id > 1 && names.Contains(x.Name);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(3);
            result.DbDataParameters[0].ParameterName.Is("@p_Id");
            result.DbDataParameters[0].Value.Is(1);
            result.DbDataParameters[1].ParameterName.Is("@p_in_Name1");
            result.DbDataParameters[1].Value.Is("A");
            result.DbDataParameters[2].ParameterName.Is("@p_in_Name2");
            result.DbDataParameters[2].Value.Is("B");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Id] > @p_Id AND [Name] IN (@p_in_Name1, @p_in_Name2)");
        }

        [Fact]
        public void Test_EmptyEnumerable()
        {
            var names = Enumerable.Empty<string>();
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => names.Contains(x.Name);
            var visitor = new PredicateVisitor(builder, entityInfo);
            var ex = Assert.Throws<InvalidOperationException>(
                () => visitor.BuildPredicate(filter));
            ex.IsNotNull();
            ex.Message.Is("IEnumerable value is empty");

        }

        [Fact]
        public void Test_StringStartsWith()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Name.StartsWith("J");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("J%");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] LIKE @p_Name");

        }

        [Fact]
        public void Test_NotStringStartsWith()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => !x.Name.StartsWith("J");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("J%");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] NOT LIKE @p_Name");

        }


        [Fact]
        public void Test_StringContains()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Name.Contains("J");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("%J%");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] LIKE @p_Name");
        }

        [Fact]
        public void Test_NotStringContains()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => !x.Name.Contains("J");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("%J%");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] NOT LIKE @p_Name");
        }


        [Fact]
        public void Test_StringEndsWith()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Name.EndsWith("J");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("%J");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] LIKE @p_Name");
        }

        [Fact]
        public void Test_NotStringEndsWith()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => !x.Name.EndsWith("J");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("%J");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] NOT LIKE @p_Name");
        }

        [Fact]
        public void Test_StringIsNullOrEmpty()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => string.IsNullOrEmpty(x.Name);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(0);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] IS NULL OR [Name] = ''");
        }

        [Fact]
        public void Test_NotStringIsNullOrEmpty()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => !string.IsNullOrEmpty(x.Name);
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(0);
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] IS NOT NULL AND [Name] <> ''");
        }

        [Fact]
        public void Test_Equals()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.Name.Equals("John Doe");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("John Doe");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] = @p_Name");

        }

        [Fact]
        public void Test_NotEquals()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => !x.Name.Equals("John Doe");
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_Name");
            result.DbDataParameters[0].Value.Is("John Doe");
            _output.WriteLine(result.ParsedSql);
            result.ParsedSql.Is(" WHERE [Name] <> @p_Name");

        }

        [Fact]
        public void Test_True()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => x.HasChildren;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_HasChildren");
            result.DbDataParameters[0].Value.Is(true);
            _output.WriteLine(result.DebugSql);
            result.ParsedSql.Is(" WHERE [HasChildren] = @p_HasChildren");

        }

        [Fact]
        public void Test_False()
        {
            var builder = new QueryStringBuilder(ConfigContainer.DefaultConfig, false);
            var entityInfo = EntityTypeInfoBuilder.Build(typeof(Person));
            Expression<Func<Person, bool>> filter = x => !x.HasChildren;
            var visitor = new PredicateVisitor(builder, entityInfo);
            visitor.BuildPredicate(filter);
            var result = builder.GetResult();
            result.DbDataParameters.Count.Is(1);
            result.DbDataParameters[0].ParameterName.Is("@p_HasChildren");
            result.DbDataParameters[0].Value.Is(false);
            _output.WriteLine(result.DebugSql);
            result.ParsedSql.Is(" WHERE [HasChildren] = @p_HasChildren");

        }

    }
}
