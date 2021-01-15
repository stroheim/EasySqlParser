using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasySqlParser.SourceGenerator.Engine
{
    // interface ごとに実装を生成する
    internal class SourceWriter
    {
        //private StringBuilder _builder = new StringBuilder();
        private readonly IndentedStringBuilder _builder = new IndentedStringBuilder();


        internal string Write(
            InterfaceContext context)
        {
            WriteUsings(context);

            _builder.AppendLine();
            _builder.AppendLine("// ReSharper disable once CheckNamespace");
            _builder.AppendLine($"namespace {context.TypeNamespace}");
            _builder.AppendLine("{");

            using (_builder.Indent())
            {
                WriteClass(context);
            }
            _builder.AppendLine("}");
            return _builder.ToString();
        }

        private void WriteUsings(InterfaceContext context)
        {
            _builder.AppendLine("using System;");
            _builder.AppendLine("using System.Collections.Generic;");

            var names = context.Usings.ToList();


            names.Add("System.Linq");
            names.Add("EasySqlParser");

            switch (context.GenerationType)
            {
                case GenerationType.Dapper:
                    names.Add("Dapper");
                    names.Add("EasySqlParser.Dapper.Extensions");
                    break;
                case GenerationType.EntityFrameworkCore:
                    names.Add("Microsoft.EntityFrameworkCore");
                    break;
            }

            switch (context.LoggerType)
            {
                case LoggerType.Log4net:
                    names.Add("Log4net");
                    break;
                case LoggerType.NLog:
                    names.Add("NLog");
                    break;
                case LoggerType.Serilog:
                    names.Add("Serilog");
                    break;
                case LoggerType.Diagnostics:
                    names.Add("System.Diagnostics");
                    break;
                case LoggerType.MicrosoftExtensionsLogging:
                    names.Add("Microsoft.Extensions.Logging");
                    break;
            }


            var sorted = names
                .Where(ns => ns != "System" && ns != "System.Collections.Generic")
                .Distinct()
                .OrderBy(x => x, new NamespaceComparer());
            foreach (var ns in sorted)
            {
                _builder.AppendLine($"using {ns};");
            }

            _builder.AppendLine();
            _builder.AppendLine("#nullable disable");
        }

        private void WriteClass(InterfaceContext context)
        {
            var className = context.TypeName.Substring(1);
            _builder.AppendLine($"public class {className} : {context.TypeName}");
            _builder.AppendLine("{");
            using (_builder.Indent())
            {
                switch (context.LoggerType)
                {
                    case LoggerType.Log4net:
                        _builder.AppendLine("private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);");
                        break;
                    case LoggerType.NLog:
                        _builder.AppendLine("private static readonly Logger Logger = LogManager.GetCurrentClassLogger();");
                        break;
                    case LoggerType.MicrosoftExtensionsLogging:
                        _builder.AppendLine($"private readonly ILogger<{className}> _logger;");
                        break;
                }

                switch (context.GenerationType)
                {
                    case GenerationType.Dapper:
                        _builder.AppendLine("private readonly IDbConnection _connection;");
                        break;
                    case GenerationType.EntityFrameworkCore:
                        _builder.AppendLine("private readonly DbContext _context;");
                        break;
                }

                WriteConstructor(context, className);
                WriteMethods(context);
                
            }

            _builder.AppendLine("}");
        }

        private void WriteConstructor(InterfaceContext context, string className)
        {
            var init = "_context = context;";
            var useLoggerDi = context.LoggerType == LoggerType.MicrosoftExtensionsLogging;
            _builder.Append($"public {className}(");
            _builder.Append("");
            switch (context.GenerationType)
            {
                case GenerationType.Dapper:
                    _builder.Append("IDbConnection connection");
                    init = "_connection = connection";
                    break;
                case GenerationType.EntityFrameworkCore:
                    _builder.Append("DbContext context");
                    break;
            }

            if (useLoggerDi)
            {
                _builder.Append($", ILogger<{className}> logger");
            }

            _builder.AppendLine(")");

            _builder.AppendLine("{");
            using (_builder.Indent())
            {
                _builder.AppendLine(init);
                if (useLoggerDi)
                {
                    _builder.AppendLine("_logger = logger;");
                }
            }

            _builder.AppendLine("}");
            _builder.AppendLine();
        }


        private void WriteMethods(InterfaceContext context)
        {
            foreach (var methodContext in context.MethodContexts)
            {
                WriteMethod(context, methodContext);
            }
        }

        private void WriteMethod(InterfaceContext context, MethodContext methodContext)
        {
            _builder.Append($"public {methodContext.ReturnTypeName} {methodContext.Name}(");
            // TODO: EasySqlParserがパラメータ1つであることを求めているので
            // Daoメソッドのパラメータも1つでよい
            //foreach (var parameterContext in methodContext.ParameterContexts)
            //{
            //    _builder.Append($"{parameterContext.TypeName} {parameterContext.Name}, ");
            //}
            //_builder.CutBack(2);
            var paramContext = methodContext.ParameterContext;
            _builder.Append($"{paramContext.TypeName} {paramContext.Name}");
            _builder.AppendLine(")");

            _builder.AppendLine("{");

            using (_builder.Indent())
            {
                // create sqlparser instance.
                _builder.Append("var parser = new SqlParser(");
                _builder.Append($"@\"{methodContext.SqlFilePath}\"");
                if (paramContext.IsKnownType)
                {
                    _builder.Append($", \"{paramContext.Name}\"");
                }
                _builder.Append($", {paramContext.Name}");
                if (!string.IsNullOrEmpty(context.ConfigName))
                {
                    _builder.Append($", ConfigContainer.AdditionalConfigs[\"{context.ConfigName}\"]");
                }

                _builder.AppendLine(");");

                // parse
                _builder.AppendLine("var parserResult = parser.Parse();");

                WriteLogging(context);

                WriteDapperQuery(context, methodContext);

                WriteEfCoreQuery(context, methodContext);

            }

            _builder.AppendLine("}");
        }

        private void WriteLogging(InterfaceContext context)
        {
            switch (context.LoggerType)
            {
                case LoggerType.Log4net:
                    _builder.AppendLine("Logger.Debug(parserResult.DebugSql);");
                    break;
                case LoggerType.NLog:
                    _builder.AppendLine("Logger.Debug(parserResult.DebugSql);");
                    break;
                case LoggerType.Serilog:
                    _builder.AppendLine("Log.Logger.Debug(parserResult.DebugSql);");
                    break;
                case LoggerType.Diagnostics:
                    _builder.AppendLine("Debug.WriteLine(parserResult.DebugSql);");
                    break;
                case LoggerType.MicrosoftExtensionsLogging:
                    _builder.AppendLine("_logger.LogDebug(parserResult.DebugSql);");
                    break;
            }
        }

        private void WriteDapperQuery(InterfaceContext context, MethodContext methodContext)
        {
            if (context.GenerationType != GenerationType.Dapper) return;
            if (!methodContext.IsSelectQuery) return;
            if (methodContext.IsAsync) return;
            _builder.Append("return _connection.");
            var queryTypeName = methodContext.ReturnTypeName;
            if (string.IsNullOrEmpty(methodContext.ReturnTypeGenericArgumentName))
            {
                _builder.Append("QuerySingleOrDefault");
            }
            else
            {
                queryTypeName = methodContext.ReturnTypeGenericArgumentName;
                _builder.Append("Query");
            }

            _builder.AppendLine($"<{queryTypeName}>(");
            using (_builder.Indent())
            {
                _builder.AppendLine("parserResult.ParsedSql,");
                _builder.AppendLine("parserResult.DbDataParameters.ToDynamicParameters()");
            }

            _builder.AppendLine(");");

        }

        private void WriteDapperNonQuery(InterfaceContext context, MethodContext methodContext)
        {
            if (context.GenerationType != GenerationType.Dapper) return;
            if (methodContext.IsSelectQuery) return;
            if (methodContext.IsAsync) return;
            _builder.Append("return _connection.");
            _builder.Append("Execute(");
            using (_builder.Indent())
            {
                _builder.AppendLine("parserResult.ParsedSql,");
                _builder.AppendLine("parserResult.DbDataParameters.ToDynamicParameters()");
            }

            _builder.AppendLine(");");
        }

        private void WriteEfCoreQuery(InterfaceContext context, MethodContext methodContext)
        {
            if (context.GenerationType != GenerationType.EntityFrameworkCore) return;
            if (!methodContext.IsSelectQuery) return;
            _builder.Append("return _context.Set");
            var queryTypeName = methodContext.ReturnTypeName;
            var terminateMethod = ".SingleOrDefault()";
            if (!string.IsNullOrEmpty(methodContext.ReturnTypeGenericArgumentName))
            {
                queryTypeName = methodContext.ReturnTypeGenericArgumentName;
                terminateMethod = ".ToList()";
            }

            _builder.AppendLine($"<{queryTypeName}>().FromSqlRaw(");
            using (_builder.Indent())
            {
                _builder.AppendLine("parserResult.ParsedSql,");
                _builder.AppendLine("parserResult.DbDataParameters.Cast<object>().ToArray()");
            }
            _builder.AppendLine(")");
            _builder.AppendLine($"{terminateMethod};");

        }
    }
}
