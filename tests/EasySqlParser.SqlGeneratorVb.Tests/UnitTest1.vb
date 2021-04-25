Imports System
Imports System.Linq.Expressions
Imports EasySqlParser.Configurations
Imports EasySqlParser.SqlGenerator
Imports EasySqlParser.SqlGenerator.Metadata
Imports EasySqlParser.SqlGenerator.Tests
Imports Microsoft.Data.SqlClient
Imports Xunit
Imports Xunit.Abstractions

Namespace EasySqlParser.SqlGeneratorVb.Tests
    Public Class UnitTest1

        Private ReadOnly _output As ITestOutputHelper

        Public Sub New(output As ITestOutputHelper)
            ConfigContainer.AddDefault(
                DbConnectionKind.SqlServer,
                Function() New SqlParameter()
                )
            _output = output
        End Sub

        <Fact>
        Sub TestSub()
            Dim filter As Expression(Of Func(Of Person, Boolean)) =
                    Function(x) x.Name = "John Doe"
            Dim builder = New QueryStringBuilder(ConfigContainer.DefaultConfig, False)
            Dim entityInfo = EntityTypeInfoBuilder.Build(GetType(Person))
            Dim visitor = New PredicateVisitor(builder, entityInfo)
            visitor.BuildPredicate(filter)
            Dim result = builder.GetResult()
            Assert.Equal(1, result.DbDataParameters.Count)
            Assert.Equal("@p_Name", result.DbDataParameters(0).ParameterName)
            Assert.Equal("John Doe", result.DbDataParameters(0).Value)
            Assert.Equal(" WHERE [Name] = @p_Name", result.ParsedSql)
            'result.DbDataParameters(0).ParameterName.Is("@p_Name")
            'result.DbDataParameters(0).Value.Is("John Doe")
            _output.WriteLine(result.ParsedSql)
            'result.ParsedSql.Is(" WHERE [Name] = @p_Name")

        End Sub
    End Class
End Namespace

