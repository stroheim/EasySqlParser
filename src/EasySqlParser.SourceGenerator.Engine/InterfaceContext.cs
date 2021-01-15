using System.Collections.Generic;

namespace EasySqlParser.SourceGenerator.Engine
{
    internal class InterfaceContext
    {
        internal GenerationType GenerationType { get; set; }

        internal LoggerType LoggerType { get; set; }

        internal string ConfigName { get; set; }

        internal List<string> Usings { get; set; } = new List<string>();

        internal string TypeNamespace { get; set; }

        internal string TypeName { get; set; }

        internal string FilePath { get; set; }

        internal string HintPath => $"Esp_{TypeName}.cs";

        internal string SqlFileRootDirectory { get; set; }

        internal List<MethodContext> MethodContexts { get; set; } = new List<MethodContext>();
    }

    internal class MethodContext
    {
        /// <summary>
        /// Method name
        /// </summary>
        internal string Name { get; set; }

        internal string ReturnTypeName { get; set; }

        internal string ReturnTypeGenericArgumentName { get; set; }

        internal bool IsScalarResult { get; set; }

        internal bool IsIEnumerableResult { get; set; }

        internal bool IsAsync { get; set; }

        internal bool IsStoredProcedure { get; set; }

        internal bool IsStoredFunction { get; set; }

        internal string SqlFilePath { get; set; }

        //internal List<ParameterContext> ParameterContexts { get; set; } = new List<ParameterContext>();
        internal ParameterContext ParameterContext { get; set; }

        internal bool IsSelectQuery { get; set; }
    }

    internal class ParameterContext
    {
        /// <summary>
        /// ParameterName
        /// </summary>
        internal string Name { get; set; }

        internal string TypeName { get; set; }

        internal bool IsKnownType { get; set; }
    }
}
