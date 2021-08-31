using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace EasySqlParser.SourceGenerator
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
            Debug.WriteLine("Execute");
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;
            try
            {
                var compilation = context.Compilation as CSharpCompilation;
                var options =
                    compilation?.SyntaxTrees.FirstOrDefault()?.Options as
                    CSharpParseOptions;
                //var ctor = typeof(CSharpCommandLineArguments).GetConstructors(BindingFlags.NonPublic).FirstOrDefault();
                //if (ctor != null)
                //{
                //    var foo = (CommandLineArguments) ctor.Invoke(null);
                //}
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
                        // TODO: error
                        if (!method.isValid) continue;

                        var methodContext = new MethodContext();
                        ProcessMethod(method.methodType, method.methodAttribute, model, methodContext,
                            daoAttrValue.sqlFileRootDirectory,
                            namespaceDirectory);
                        ifContext.MethodContexts.Add(methodContext);
                        context.CancellationToken.ThrowIfCancellationRequested();
                    }
                    ifContexts.Add(ifContext);
                }

                foreach (var ifContext in ifContexts)
                {
                    var writer = new SourceWriter();
                    var code = writer.Write(ifContext);
                    Debug.WriteLine("");
                    Debug.WriteLine(code);
                    //var ms = new MemoryStream();
                    //var sw = new StreamWriter(ms, Encoding.UTF8);
                    //sw.Write(code);
                    //sw.Flush();
                    //var buf = ms.GetBuffer();
                    //var source = SourceText.From(buf, (int) ms.Length, Encoding.UTF8, canBeEmbedded: true);
                    //context.AddSource(ifContext.HintPath, source);
                    context.AddSource(ifContext.HintPath, SourceText.From(code, Encoding.UTF8));
                }



            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }


        private static void ProcessDaoInterface(
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
            Debug.WriteLine(tree.FilePath);
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

        private static string? GetSqlFileRootDirectory(string sourcePath, string sqlFileRootDirectory)
        {
            if (string.IsNullOrEmpty(sqlFileRootDirectory))
            {
                return null;
            }

            var sourceDir = Path.GetDirectoryName(sourcePath);
            if (sourceDir == null)
            {
                return null;
            }
            var realPath = CompareDirectory(sourceDir, sqlFileRootDirectory);
            return realPath;
        }

        private static string? CompareDirectory(string sourceDir, string configRoot)
        {
            var dir = Path.Combine(sourceDir, configRoot);
            if (Directory.Exists(dir))
            {
                return dir;
            }

            var parent = Directory.GetParent(sourceDir);
            var root = Directory.GetDirectoryRoot(sourceDir);
            if (parent == null)
            {
                return null;
            }
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

        private static (GenerationType generationType, LoggerType loggerType, string? sqlFileRootDirectory, string? configName) ProcessDaoAttribute(
            AttributeArgumentListSyntax argumentList, SemanticModel model)
        {
            var generationType = GenerationType.EntityFrameworkCore;
            var loggerType = LoggerType.MicrosoftExtensionsLogging;
            var sqlFileRootDirectory = "SqlResources";
            string? configName = null;
            var attrCount = argumentList.Arguments.Count;
            if (attrCount == 0)
            {
                // use Ef
                return (generationType, loggerType, sqlFileRootDirectory, null);
            }

            for (var i = 0; i < attrCount; i++)
            {
                var arg = argumentList.Arguments[i];
                var expr = arg.Expression;
                // 名前付き引数か？
                var nameColon = arg.NameColon;
                var namedArgument = nameColon?.Name.Identifier.Value;
                if (namedArgument is string namedArgumentName)
                {
                    switch (namedArgumentName)
                    {
                        // 名前付き引数優先→順番無視
                        case "configName":
                            configName = (string?) model.GetConstantValue(expr).Value;
                            continue;
                        case "sqlFileRootDirectory":
                            sqlFileRootDirectory = (string?) model.GetConstantValue(expr).Value;
                            continue;
                        case "generationType" when TryGetGenerationType(expr, out generationType):
                        case "loggerType" when TryGetLoggerType(expr, out loggerType):
                            continue;
                    }
                }

                switch (i)
                {
                    // ここからは位置パラメータ
                    // enum
                    case 0 when TryGetGenerationType(expr, out generationType):
                    case 1 when TryGetLoggerType(expr, out loggerType):
                    // string
                    case 2 when TryGetString(expr, model, out sqlFileRootDirectory):
                        continue;
                    case 3:
                        // string only
                        configName = (string?) model.GetConstantValue(expr).Value;
                        break;
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

        private static bool TryGetString(ExpressionSyntax expr,SemanticModel model, out string? value)
        {
            if (expr is InvocationExpressionSyntax || expr is LiteralExpressionSyntax)
            {
                value = (string?) model.GetConstantValue(expr).Value;
                return true;
            }

            value = null;
            return false;
        }


        private static void ProcessMethod(
            MethodDeclarationSyntax methodType, 
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context,
            string sqlFileRootDirectory,
            string namespaceDirectory)
        {
            var foo = model.GetDeclaredSymbol(methodType);
            //if (foo != null)
            //{
            //    foreach (var attributeData in foo.GetAttributes())
            //    {
            //        if (attributeData.AttributeClass == null) continue;
            //        Debug.WriteLine(attributeData.AttributeClass.Name);
            //        foreach (var attributeDataNamedArgument in attributeData.NamedArguments)
            //        {
            //            Debug.WriteLine($"{attributeDataNamedArgument.Key}\t{attributeDataNamedArgument.Value.Value}");
            //        }
            //        //foreach (var member in attributeData.AttributeClass.GetMembers())
            //        //{
                        
            //        //    Debug.WriteLine($"{member.Kind}\t{member.Name}");
            //        //}
                    
            //    }
            //}
            

            //Debug.WriteLine("------------------------------------------");
            //Debug.WriteLine("print method info");
            //// method name
            //Debug.WriteLine(methodType.Identifier.Text);
            context.Name = methodType.Identifier.Text;
            ProcessMethodParameters(methodType.ParameterList.Parameters, context);
            // method return type
            var typeSyntax = methodType.ReturnType;
            var returnTypeContext = new ReturnTypeContext();
            returnTypeContext.Name = typeSyntax.ToString();
            if (typeSyntax is GenericNameSyntax generic)
            {

                Debug.WriteLine(generic.Identifier.Value);
                //Debug.WriteLine(generic.Identifier.ValueText);
                var typeArg = generic.TypeArgumentList.Arguments.FirstOrDefault();
                if (typeArg != null)
                {
                    if (typeArg is GenericNameSyntax nestedGeneric)
                    {
                        Debug.WriteLine(nestedGeneric.Identifier.Text);
                        var nestedTypeArg = nestedGeneric.TypeArgumentList.Arguments.FirstOrDefault();
                        if (nestedTypeArg != null)
                        {
                            returnTypeContext.GenericArgumentName = nestedTypeArg.ToString();
                            returnTypeContext.NestedGenericTypeName = nestedGeneric.Identifier.Text;
                        }
                    }
                    else
                    {
                        returnTypeContext.GenericArgumentName = typeArg.ToString();
                    }

                    returnTypeContext.GenericTypeName = generic.Identifier.Text;
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

            ApplySqlFilePath(methodAttribute, context, sqlFileRootDirectory, namespaceDirectory);
            var methodSymbol = model.GetDeclaredSymbol(methodType);
            if (methodSymbol == null)
            {
                Debug.WriteLine("ありえなす");
                return;
            }

            returnTypeContext.TypeNamespace = methodSymbol.ReturnType.ContainingNamespace.ToDisplayString();
            if (returnTypeContext.TypeNamespace == "System.Threading.Tasks" &&
                returnTypeContext.GenericTypeName is "Task" or "ValueTask")
            {
                //! IsAsyncはasyncキーワードにしか反応しない。。
                //context.IsAsync = methodSymbol.IsAsync;
                context.IsAsync = true;
            }

            context.ReturnTypeContext = returnTypeContext;


            var attributeDataList = methodSymbol.GetAttributes();
            ProcessMethodAttribute(attributeDataList, methodAttribute, model, context);


        }

        private static void ProcessMethodParameters(SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList, MethodContext context)
        {
            // パラメータが無い場合があるのか？
            // select allの場合にはありうる
            // select以外でパラメータ無しはエラー
            //  パラメータが複数の場合はあるのか？？
            // パラメータチェック
            //  select
            //      TCondition
            //      Expression<Func<TEntity, bool>> 
            //  select以外
            //      TEntity → classならOK
            //      FormattableString
            //      priority
            foreach (var parameterSyntax in parameterSyntaxList)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var isKnownType = false;
                string? parameterTypeName = null;
                if (parameterSyntax.Type is IdentifierNameSyntax syntax)
                {
                    // user definition type
                    // method argument type name
                    //Debug.WriteLine(syntax.Identifier.Text);
                    parameterTypeName = syntax.Identifier.Text;
                }

                if (parameterSyntax.Type is PredefinedTypeSyntax definedSyntax)
                {
                    // known clr type
                    isKnownType = true;
                    //Debug.WriteLine(definedSyntax.Keyword.Text);
                    parameterTypeName = definedSyntax.Keyword.Text;
                }

                if (parameterTypeName == null)
                {
                    // error
                    continue;
                }

                
                context.ParameterContexts.Add(new ParameterContext
                            {
                                TypeName = parameterTypeName,
                                Name = parameterName,
                                IsKnownType = isKnownType
                            });
            }
            
        }

        private static void ProcessMethodAttribute(
            ImmutableArray<AttributeData> attributeDataList,
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context)
        {
            var attributeName = methodAttribute.Name.ToString();
            // TODO: 戻り値チェックを行う
            // select
            //  TEntity
            //  List<TEntity>
            //  Task<TEntity>
            //  Task<List<TEntity>>
            //      TEntity のチェックは？？
            //      classくらいしかない
            // select以外
            //  int
            //  Task<int>
            //  void
            //  Task
            // attributeDataList中からmethodAttributeに一致するモノを取得する
            if (attributeName is "Select" or "SelectAttribute")
            {
                var attributeData = attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == "SelectAttribute");
                if (attributeData == null) return;
                // process select
                ProcessSelectAttribute(attributeData, methodAttribute, model, context);
                context.SqlKind = SqlKind.Select;
                return;
            }

            if (attributeName is "Insert" or "InsertAttribute")
            {
                var attributeData = attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == "InsertAttribute");
                if (attributeData == null) return;
                // process insert
                ProcessInsertAttribute(attributeData, methodAttribute, model, context);
                context.SqlKind = SqlKind.Insert;
                return;
            }

            if (attributeName is "Update" or "UpdateAttribute")
            {
                var attributeData = attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == "UpdateAttribute");
                if (attributeData == null) return;
                // process update
                ProcessUpdateAttribute(attributeData, methodAttribute, model, context);
                context.SqlKind = SqlKind.Update;
                return;
            }

            if (attributeName is "Delete" or "DeleteAttribute")
            {
                var attributeData = attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == "DeleteAttribute");
                if (attributeData == null) return;
                // process delete
                ProcessDeleteAttribute(attributeData, methodAttribute, model, context);
                context.SqlKind = SqlKind.Delete;
                return;
            }

            if (attributeName is "SoftDelete" or "SoftDeleteAttribute")
            {
                var attributeData = attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == "SoftDeleteAttribute");
                if (attributeData == null) return;
                ProcessUpdateAttribute(attributeData, methodAttribute, model, context);
                context.SqlKind = SqlKind.SoftDelete;
                return;
            }

            if (attributeName is "Merge" or "MergeAttribute")
            {
                var attributeData = attributeDataList.FirstOrDefault(x => x.AttributeClass?.Name == "MergeAttribute");
                if (attributeData == null) return;
                ProcessMergeAttribute(attributeData, methodAttribute, model, context);
                context.SqlKind = SqlKind.Merge;
                return;
            }

            // TODO:
            throw new InvalidOperationException("");
        }

        private static void ApplySqlFilePath(
            AttributeSyntax methodAttribute,
            MethodContext context,
            string sqlFileRootDirectory,
            string namespaceDirectory)
        {
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
            }

        }

        private static void ProcessSelectAttribute(
            AttributeData attributeData,
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context)
        {
            // several property default value
            // from base
            // FilePath         :null
            // CommandTimeout   :-1

            // set default value
            context.CommandTimeout = -1;

            foreach (var attributeDataNamedArgument in attributeData.NamedArguments)
            {
                var value = attributeDataNamedArgument.Value.Value;
                if (value == null)
                {
                    // TODO: error
                    continue;
                }
                switch (attributeDataNamedArgument.Key)
                {
                    case "FilePath":
                        context.SqlFilePath = (string) value;
                        continue;
                    case "CommandTimeout":
                        context.CommandTimeout = (int) value;
                        break;
                    case "UseDbSet":
                        context.UseDbSet = (bool) value;
                        if (!context.UseDbSet)
                        {
                            //TODO: 戻り値の型(T)がEntityAttributeで就職されている必要がある

                        }
                        break;
                }
            }
            //if (methodAttribute.ArgumentList == null) return;
            //var attrCount = methodAttribute.ArgumentList.Arguments.Count;
            //for (var i = 0; i < attrCount; i++)
            //{

            //    var arg = methodAttribute.ArgumentList.Arguments[i];
                
            //    var expr = arg.Expression;
            //    Debug.WriteLine(expr.ToString());

            //    // 名前付き引数か？
            //    // 属性でプロパティに指定していてもここでは取れない
            //    // コンストラクタに定義した引数のみが取れる
            //    var nameColon = arg.NameColon;
            //    var namedArgument = nameColon?.Name.Identifier.Value;
            //    if (namedArgument is string namedArgumentName)
            //    {
            //        Debug.WriteLine(namedArgumentName);
            //    }

            //    // コンストラクタは未定義なので位置パラメータではわからない
            //    if (i == 0)
            //    {
            //        // string
            //        if (TryGetString(expr, model, out var filePath))
            //        {
            //            context.SqlFilePath = filePath;
            //        }
            //    }
            //}
        }

        private static void ProcessInsertAttribute(
            AttributeData attributeData,
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context)
        {
            // several property default value
            // from base
            // AutoGenerate     :true
            // FilePath         :null
            // CommandTimeout   :-1

            // ExcludeNull      :false
            context.AutoGenerate = true;
            context.CommandTimeout = -1;
            context.ExcludeNull = false;

            foreach (var attributeDataNamedArgument in attributeData.NamedArguments)
            {
                var value = attributeDataNamedArgument.Value.Value;
                if (value == null)
                {
                    // TODO: error
                    continue;
                }
                switch (attributeDataNamedArgument.Key)
                {
                    case "AutoGenerate":
                        context.AutoGenerate = (bool) value;
                        break;
                    case "FilePath":
                        context.SqlFilePath = (string) value;
                        break;
                    case "CommandTimeout":
                        context.CommandTimeout = (int) value;
                        break;
                    case "ExcludeNull":
                        context.ExcludeNull = (bool) value;
                        break;
                }
            }
        }

        private static void ProcessUpdateAttribute(
            AttributeData attributeData,
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context)
        {
            // several property default value
            // from base
            // AutoGenerate     :true
            // FilePath         :null
            // CommandTimeout   :-1

            // ExcludeNull      :false
            // IgnoreVersion    :false
            // SuppressOptimisticLockException :false

            context.AutoGenerate = true;
            context.CommandTimeout = -1;
            context.ExcludeNull = false;
            context.IgnoreVersion = false;
            context.SuppressOptimisticLockException = false;

            foreach (var attributeDataNamedArgument in attributeData.NamedArguments)
            {
                var value = attributeDataNamedArgument.Value.Value;
                if (value == null)
                {
                    // TODO: error
                    continue;
                }

                switch (attributeDataNamedArgument.Key)
                {
                    case "AutoGenerate":
                        context.AutoGenerate = (bool) value;
                        break;
                    case "FilePath":
                        context.SqlFilePath = (string) value;
                        break;
                    case "CommandTimeout":
                        context.CommandTimeout = (int) value;
                        break;
                    case "ExcludeNull":
                        context.ExcludeNull = (bool) value;
                        break;
                    case "IgnoreVersion":
                        context.IgnoreVersion = (bool) value;
                        break;
                    case "SuppressOptimisticLockException":
                        context.SuppressOptimisticLockException = (bool) value;
                        break;
                }
            }


        }

        private static void ProcessDeleteAttribute(
            AttributeData attributeData,
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context)
        {
            // several property default value
            // from base
            // AutoGenerate     :true
            // FilePath         :null
            // CommandTimeout   :-1

            // IgnoreVersion    :false
            // SuppressOptimisticLockException :false

            //TODO: AutoGenerate : false かつ FilePath 無しはエラー
            context.AutoGenerate = true;
            context.CommandTimeout = -1;
            context.IgnoreVersion = false;
            context.SuppressOptimisticLockException = false;

            foreach (var attributeDataNamedArgument in attributeData.NamedArguments)
            {
                var value = attributeDataNamedArgument.Value.Value;
                if (value == null)
                {
                    // TODO: error
                    continue;
                }
                switch (attributeDataNamedArgument.Key)
                {
                    case "AutoGenerate":
                        context.AutoGenerate = (bool) value;
                        break;
                    case "FilePath":
                        context.SqlFilePath = (string) value;
                        break;
                    case "CommandTimeout":
                        context.CommandTimeout = (int) value;
                        break;
                    case "IgnoreVersion":
                        context.IgnoreVersion = (bool) value;
                        break;
                    case "SuppressOptimisticLockException":
                        context.SuppressOptimisticLockException = (bool) value;
                        break;
                }

            }

        }

        private static void ProcessMergeAttribute(
            AttributeData attributeData,
            AttributeSyntax methodAttribute,
            SemanticModel model,
            MethodContext context)
        {
            // several property default value
            // from base
            // FilePath         :null
            // CommandTimeout   :-1

            // UseSqlServer    :false
            context.AutoGenerate = false;
            context.CommandTimeout = -1;
            context.IgnoreVersion = true;
            context.SuppressOptimisticLockException = false;

            foreach (var attributeDataNamedArgument in attributeData.NamedArguments)
            {
                var value = attributeDataNamedArgument.Value.Value;
                if (value == null)
                {
                    // TODO: error
                    continue;
                }
                switch (attributeDataNamedArgument.Key)
                {
                    case "FilePath":
                        context.SqlFilePath = (string)value;
                        //null ならエラー
                        break;
                    case "CommandTimeout":
                        context.CommandTimeout = (int)value;
                        break;
                    case "UseSqlServer":
                        context.UseSqlServer = (bool)value;
                        break;
                }

            }
        }


    }

    

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        private static readonly string[] AcceptableMethodAttributes = {
                                                                          "Select",
                                                                          "SelectAttribute",
                                                                          "Insert",
                                                                          "InsertAttribute",
                                                                          "Update",
                                                                          "UpdateAttribute",
                                                                          "Delete",
                                                                          "DeleteAttribute",
                                                                          "SoftDelete",
                                                                          "SoftDeleteAttribute",
                                                                          "Merge",
                                                                          "MergeAttribute",
                                                                      };

        internal Dictionary<(InterfaceDeclarationSyntax type, AttributeSyntax attr),
            List<(MethodDeclarationSyntax methodType, AttributeSyntax methodAttribute, bool isValid)>> Targets =
            new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax s &&
                s.AttributeLists.Count > 0)
            {
                var attr = s.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() is "Dao" or 
                                        "DaoAttribute");
                if (attr == null) return;

                var methodList = new List<(MethodDeclarationSyntax methodType, AttributeSyntax methodAttribute, bool isValid)>();
                foreach (var member in s.Members)
                {
                    if (member is not MethodDeclarationSyntax m) continue;

                    if (m.AttributeLists.Count == 0)
                    {
                        //methodList.Add((m, null, false));
                        continue;
                    }
                    var methodAttribute = m.AttributeLists
                        .SelectMany(x => x.Attributes)
                        .FirstOrDefault(
                            x => AcceptableMethodAttributes.Contains(x.Name.ToString()));
                    if (methodAttribute == null)
                    {
                        //methodList.Add((m, null, false));
                        continue;
                    }
                    methodList.Add((m, methodAttribute, true));
                }

                Targets.Add((s, attr), methodList);
            }
        }
    }
}
