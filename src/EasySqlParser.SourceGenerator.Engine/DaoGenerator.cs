using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace EasySqlParser.SourceGenerator.Engine
{
    [Generator]
    public class DaoGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //System.Diagnostics.Debugger.Launch();
            }
#endif 
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;
            try
            {
                var compilation = context.Compilation as CSharpCompilation;
                var options =
                    compilation?.SyntaxTrees.FirstOrDefault()?.Options as
                    CSharpParseOptions;
                // Metadata の取得は Reflection と全く違う
                var ifContexts = new List<InterfaceContext>();
                foreach (var target in receiver.Targets)
                {
                    if (target.Key.attr.ArgumentList is null) continue;
                    var ifContext = new InterfaceContext();

                    var model = context.Compilation.GetSemanticModel(target.Key.type.SyntaxTree);
                    var daoAttrValue = ProcessDaoAttribute(target.Key.attr.ArgumentList, model);
                    //Debug.WriteLine(daoAttrValue.contextName);
                    //Debug.WriteLine(daoAttrValue.generationType);
                    //Debug.WriteLine(daoAttrValue.sqlFileRootDirectory);
                    ifContext.GenerationType = daoAttrValue.generationType;
                    ifContext.ConfigName = daoAttrValue.configName;
                    ifContext.LoggerType = daoAttrValue.loggerType;

                    ProcessDaoInterface(target.Key.type, model, ifContext, daoAttrValue.sqlFileRootDirectory);
                    var namespaceDirectory = ConvertToDirectory(ifContext);
                    var methodList = target.Value;
                    foreach (var method in methodList)
                    {
                        var methodContext = new MethodContext();
                        ProcessMethod(method.methodType, method.methodAttribute, model, methodContext,
                            daoAttrValue.sqlFileRootDirectory,
                            namespaceDirectory);
                        ifContext.MethodContexts.Add(methodContext);
                    }
                    ifContexts.Add(ifContext);
                }

                foreach (var ifContext in ifContexts)
                {
                    var writer = new SourceWriter();
                    var code = writer.Write(ifContext);
                    Debug.WriteLine("");
                    Debug.WriteLine(code);
                    context.AddSource(ifContext.HintPath, SourceText.From(code, Encoding.UTF8));
                }



            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }


        private void ProcessDaoInterface(
            InterfaceDeclarationSyntax type, 
            SemanticModel model,
            InterfaceContext context,
            string sqlFileRootDirectory)
        {
            var tree = type.SyntaxTree;
            var root = tree.GetCompilationUnitRoot();
            var namespaces = root.Usings;
            //Debug.WriteLine("------------------------------------------");
            //Debug.WriteLine("print usings");
            foreach (var ns in namespaces)
            {
                //Debug.WriteLine(ns.Name.ToString());
                context.Usings.Add(ns.Name.ToString());
            }

            // interface namespace
            //Debug.WriteLine("------------------------------------------");
            //Debug.WriteLine("print namespace");
            var namedTypeSymbol = model.GetDeclaredSymbol(type);
            if (namedTypeSymbol != null)
            {
                //Debug.WriteLine(namedTypeSymbol.ContainingNamespace.ToString());
                context.TypeNamespace = namedTypeSymbol.ContainingNamespace.ToString();
            }

            context.TypeName = type.Identifier.Text;
            //Debug.WriteLine("------------------------------------------");
            //Debug.WriteLine("print interface name");
            //Debug.WriteLine(type.Identifier.Text);



            // csprojの場所とかは取得できない
            // ここで取れるFilePathからさかのぼる
            // sqlFileRootDirectory が見つかるまで
            context.FilePath = tree.FilePath;
            var filePath = GetSqlFileRootDirectory(tree.FilePath, sqlFileRootDirectory);
            //Debug.WriteLine("------------------------------------------");
            //Debug.WriteLine("print sqlfilepath");
            //Debug.WriteLine(filePath);
            context.SqlFileRootDirectory = filePath;
        }

        private static string ConvertToDirectory(InterfaceContext context)
        {
            return context.TypeNamespace.Replace(".", @"\");
        }

        private static string GetSqlFileRootDirectory(string sourcePath, string sqlFileRootDirectory)
        {
            if (string.IsNullOrEmpty(sqlFileRootDirectory))
            {
                return null;
            }

            var sourceDir = Path.GetDirectoryName(sourcePath);
            var realPath = CompareDirectory(sourceDir, sqlFileRootDirectory);
            return realPath;
        }

        private static string CompareDirectory(string sourceDir, string configRoot)
        {
            var dir = Path.Combine(sourceDir, configRoot);
            if (Directory.Exists(dir))
            {
                return dir;
            }

            var parent = Directory.GetParent(sourceDir);
            var root = Directory.GetDirectoryRoot(sourceDir);
            while (root != parent.FullName)
            {
                var d = CompareDirectory(parent.FullName, configRoot);
                if (Directory.Exists(d))
                {
                    return d;
                }
            }

            return null;

        }

        private (GenerationType generationType, LoggerType loggerType, string sqlFileRootDirectory, string configName) ProcessDaoAttribute(
            AttributeArgumentListSyntax argumentList, SemanticModel model)
        {
            GenerationType generationType = GenerationType.EntityFrameworkCore;
            LoggerType loggerType = LoggerType.MicrosoftExtensionsLogging;
            string sqlFileRootDirectory = "SqlResources";
            string configName = null;
            var attrCount = argumentList.Arguments.Count;
            if (attrCount == 0)
            {
                // use Ef
                return (generationType, loggerType, sqlFileRootDirectory, null);
            }

            for (int i = 0; i < attrCount; i++)
            {
                var arg = argumentList.Arguments[i];
                var expr = arg.Expression;
                // 名前付き引数か？
                var nameColon = arg.NameColon;
                if (nameColon != null)
                {
                    var namedArgument = nameColon.Name.Identifier.Value;
                    if (namedArgument != null && namedArgument is string namedArgumentName)
                    {
                        // 名前付き引数優先→順番無視
                        if (namedArgumentName == "configName")
                        {
                            configName = (string) model.GetConstantValue(expr).Value;
                            continue;
                        }

                        if (namedArgumentName == "sqlFileRootDirectory")
                        {
                            sqlFileRootDirectory = (string) model.GetConstantValue(expr).Value;
                            continue;
                        }

                        if (namedArgumentName == "generationType")
                        {
                            if (TryGetGenerationType(expr, out generationType))
                            {
                                continue;
                            }
                        }

                        if (namedArgumentName == "loggerType")
                        {
                            if (TryGetLoggerType(expr, out loggerType))
                            {
                                continue;
                            }
                        }
                    }
                }

                // ここからは位置パラメータ
                if (i == 0)
                {
                    // enum

                    if (TryGetGenerationType(expr, out generationType))
                    {
                        continue;
                    }

                }

                if (i == 1)
                {
                    if (TryGetLoggerType(expr, out loggerType))
                    {
                        continue;
                    }
                }

                if (i == 2)
                {
                    // string
                    if (TryGetString(expr, model, out sqlFileRootDirectory))
                    {
                        continue;
                    }


                }

                if (i == 3)
                {
                    // string only
                    configName = (string) model.GetConstantValue(expr).Value;
                }

            }

            return (generationType, loggerType, sqlFileRootDirectory, configName);

        }

        private static bool TryGetGenerationType(ExpressionSyntax expr, out GenerationType generationType)
        {
            if (expr is MemberAccessExpressionSyntax syntax)
            {
                var enumText = syntax.Name.Identifier.Text;
                Debug.WriteLine(enumText);
                generationType = (GenerationType)Enum.Parse(typeof(GenerationType), enumText);
                return true;
            }

            generationType = GenerationType.EntityFrameworkCore;
            return false;
        }

        private static bool TryGetLoggerType(ExpressionSyntax expr, out LoggerType loggerType)
        {
            if (expr is MemberAccessExpressionSyntax syntax)
            {
                var enumText = syntax.Name.Identifier.Text;
                loggerType = (LoggerType) Enum.Parse(typeof(LoggerType), enumText);
                return true;
            }

            loggerType = LoggerType.MicrosoftExtensionsLogging;
            return false;
        }

        private static bool TryGetString(ExpressionSyntax expr,SemanticModel model, out string value)
        {
            if (expr is InvocationExpressionSyntax || expr is LiteralExpressionSyntax)
            {
                value = (string) model.GetConstantValue(expr).Value;
                return true;
            }

            value = null;
            return false;
        }


        private void ProcessMethod(
            MethodDeclarationSyntax methodType, 
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context,
            string sqlFileRootDirectory,
            string namespaceDirectory)
        {

            //Debug.WriteLine("------------------------------------------");
            //Debug.WriteLine("print method info");
            //// method name
            //Debug.WriteLine(methodType.Identifier.Text);
            context.Name = methodType.Identifier.Text;
            var paramCount = methodType.ParameterList.Parameters.Count;
            if (paramCount > 1)
            {
                throw new Exception("method parameter count overflow.");
            }

            if (paramCount == 0)
            {
                throw new Exception("require method parameter");
            }
            var paramContext = new ParameterContext();
            var param = methodType.ParameterList.Parameters[0];
            // method argument name
            //Debug.WriteLine(param.Identifier.Text);
            paramContext.Name = param.Identifier.Text;
            if (param.Type != null && param.Type is IdentifierNameSyntax syntax)
            {
                // user definition type
                // method argument type name
                //Debug.WriteLine(syntax.Identifier.Text);
                paramContext.TypeName = syntax.Identifier.Text;
            }

            if (param.Type != null && param.Type is PredefinedTypeSyntax definedSyntax)
            {
                // known clr type
                paramContext.IsKnownType = true;
                //Debug.WriteLine(definedSyntax.Keyword.Text);
                paramContext.TypeName = definedSyntax.Keyword.Text;
            }

            //context.ParameterContexts.Add(paramContext);
            context.ParameterContext = paramContext;
            // method return type
            var typeSyntax = methodType.ReturnType;
            context.ReturnTypeName = typeSyntax.ToString();
            if (typeSyntax is GenericNameSyntax generic)
            {
                //Debug.WriteLine(generic.Identifier.Value);
                //Debug.WriteLine(generic.Identifier.ValueText);
                var typeArg = generic.TypeArgumentList.Arguments.FirstOrDefault();
                if (typeArg != null)
                {
                    context.ReturnTypeGenericArgumentName = typeArg.ToString();
                }
                //Debug.WriteLine(generic.Identifier.Text);
                //var typeArgs = generic.TypeArgumentList.Arguments;
                //if (typeArgs.Count > 0)
                //{
                //    for (int i = 0; i < typeArgs.Count; i++)
                //    {
                //        var typeArg = typeArgs[i];
                //        Debug.WriteLine(typeArg.ToString());
                //    }
                //}
            }

            //if (typeSyntax is IdentifierNameSyntax identifier)
            //{
            //    //Debug.WriteLine(identifier.Identifier.Value);
            //    //Debug.WriteLine(identifier.Identifier.ValueText);
            //    Debug.WriteLine(identifier.Identifier.Text);

            //}

            //// 個別に見る必要はなくこれで型名が取れる
            //// が、Listかどうかの判定は必要かも
            //Debug.WriteLine(typeSyntax.ToString());

            var methodAttrName = methodAttribute.Name.ToString();
            if (methodAttrName == "Query" || methodAttrName == "QueryAttribute")
            {
                context.IsSelectQuery = true;
            }

            void SetSqlFilePath()
            {
                context.SqlFilePath = Path.Combine(sqlFileRootDirectory, namespaceDirectory, $"{context.Name}.sql");
            }

            // attr
            if (methodAttribute.ArgumentList is null)
            {
                SetSqlFilePath();
                return;
            }
            var attrCount = methodAttribute.ArgumentList.Arguments.Count;
            if (attrCount == 0)
            {
                // sql file auto detect
                SetSqlFilePath();
                return;
            }

            for (int i = 0; i < attrCount; i++)
            {
                var attr = methodAttribute.ArgumentList.Arguments[i];
                var expr = attr.Expression;
                if (i == 0)
                {
                    // string
                    if (TryGetString(expr, model, out var filePath))
                    {
                        context.SqlFilePath = filePath;
                    }
                }
            }


        }

        
    }

    internal class SyntaxReceiver : ISyntaxReceiver
    {

        internal Dictionary<(InterfaceDeclarationSyntax type, AttributeSyntax attr),
            List<(MethodDeclarationSyntax methodType, AttributeSyntax methodAttribute)>> Targets =
            new Dictionary<(InterfaceDeclarationSyntax type, AttributeSyntax attr),
                List<(MethodDeclarationSyntax methodType, AttributeSyntax methodAttribute)>>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax s &&
                s.AttributeLists.Count > 0)
            {
                var attr = s.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() is "Dao" or 
                                        "DaoAttribute");
                if (attr != null)
                {
                    var methodList = new List<(MethodDeclarationSyntax methodType, AttributeSyntax methodAttribute)>();
                    foreach (var member in s.Members)
                    {
                        if (member is MethodDeclarationSyntax m &&
                            m.AttributeLists.Count > 0)
                        {
                            var methodAttribute = m.AttributeLists
                                .SelectMany(x => x.Attributes)
                                .FirstOrDefault(
                                    x => x.Name.ToString() is "Query" or
                                        "QueryAttribute" or
                                        "NonQuery" or
                                        "NonQueryAttribute");
                            if (methodAttribute != null)
                            {
                                methodList.Add((m, methodAttribute));
                            }
                        }
                    }

                    Targets.Add((s, attr), methodList);
                }

            }
        }
    }
}
