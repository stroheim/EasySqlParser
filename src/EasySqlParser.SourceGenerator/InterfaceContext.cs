using System;
using System.Collections.Generic;

namespace EasySqlParser.SourceGenerator
{
    /// <summary>
    /// 自動生成する実装方式
    /// </summary>
    internal enum GenerationType
    {
        /// <summary> use EntityFrameworkCore </summary>
        EntityFrameworkCore,
        /// <summary> use Dapper </summary>
        Dapper
    }

    /// <summary>
    /// 自動生成されるクラスで利用されるロガー
    /// </summary>
    internal enum LoggerType
    {
        /// <summary> do not logging </summary>
        Disable,
        /// <summary> use Log4net </summary>
        Log4net,
        /// <summary> use NLog </summary>
        NLog,
        /// <summary> use Serilog </summary>
        Serilog,
        /// <summary> use Diagnostic </summary>
        Diagnostics,
        /// <summary> use Microsoft.Extension.Logging </summary>
        MicrosoftExtensionsLogging,
    }

    internal enum SqlKind
    {
        /// <summary>
        ///     insert.
        /// </summary>
        Insert,
        /// <summary>
        ///     update.
        /// </summary>
        Update,
        /// <summary>
        ///     delete.
        /// </summary>
        Delete,
        /// <summary>
        ///     soft delete.
        /// </summary>
        SoftDelete,
        Select,
        Merge
    }


    internal class InterfaceContext
    {
        internal GenerationType GenerationType { get; set; }

        internal LoggerType LoggerType { get; set; }

        internal string? ConfigName { get; set; }

        /// <summary>
        /// interface using directives
        /// </summary>
        internal List<string> Usings { get; set; } = new List<string>();

        /// <summary>
        /// interface namespace
        /// </summary>
        internal string? TypeNamespace { get; set; }

        /// <summary>
        /// interface type name
        /// </summary>
        internal string? TypeName { get; set; }

        /// <summary>
        /// interface's source file full path
        /// </summary>
        internal string? FilePath { get; set; }

        internal string HintPath => $"Esp_{TypeName}.cs";


        internal string? SqlFileRootDirectory { get; set; }

        internal List<MethodContext> MethodContexts { get; set; } = new List<MethodContext>();
    }

    internal class MethodContext
    {
        /// <summary>
        /// Method name
        /// </summary>
        internal string Name { get; set; } = null!;

        //internal string ReturnTypeName { get; set; } = null!;

        //internal string ReturnTypeGenericArgumentName { get; set; } = null!;

        //internal string ReturnTypeNamespace { get; set; } = null!;

        //internal string ReturnTypeTaskTypeName { get; set; } = null!;

        //internal string ReturnTypeGenericTypeName { get; set; } = null!;

        internal ReturnTypeContext ReturnTypeContext { get; set; } = null!;



        internal bool IsScalarResult { get; set; }

        internal bool IsIEnumerableResult { get; set; }

        internal bool IsAsync { get; set; }

        internal string SqlFilePath { get; set; } = null!;

        internal List<ParameterContext> ParameterContexts { get; set; } = new List<ParameterContext>();

        internal ParameterContext? ParameterContext { get; set; }

        internal bool UseDbSet { get; set; }

        internal int CommandTimeout { get; set; }

        internal bool AutoGenerate { get; set; }

        internal bool ExcludeNull { get; set; }

        internal bool IgnoreVersion { get; set; }

        [Obsolete]
        internal bool UseVersion { get; set; }

        internal bool SuppressOptimisticLockException { get; set; }

        internal SqlKind SqlKind { get; set; }

        internal bool UseSqlServer { get; set; }
    }

    internal class ReturnTypeContext
    {
        /// <summary>
        ///     Gets or sets return type name.
        /// </summary>
        internal string Name { get; set; } = null!;

        /// <summary>
        ///     Gets or sets generic type argument name.
        /// </summary>
        internal string? GenericArgumentName { get; set; }

        /// <summary>
        ///     Gets or sets namespace.
        /// </summary>
        internal string TypeNamespace { get; set; } = null!;

        /// <summary>
        ///     Gets or sets generic type name.
        /// </summary>
        internal string? GenericTypeName { get; set; }

        /// <summary>
        ///     Gets or sets nested generic type name.
        /// </summary>
        internal string? NestedGenericTypeName { get; set; }
    }

    internal class ParameterContext
    {
        /// <summary>
        /// ParameterName
        /// </summary>
        internal string Name { get; set; } = null!;

        internal string TypeName { get; set; } = null!;

        internal bool IsKnownType { get; set; }
    }
}
