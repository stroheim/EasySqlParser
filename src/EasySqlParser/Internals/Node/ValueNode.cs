using System;

namespace EasySqlParser.Internals.Node
{
    internal enum BuiltinFunctionName
    {
        Escape,
        StartsWith,
        Contains,
        EndsWith,
        TruncateTime
    }

    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      ValueNode
    // https://github.com/domaframework/doma
    internal abstract class ValueNode : AbstractSqlNode
    {

        internal SqlLocation Location { get; }
        internal string VariableName { get; set; }
        internal string Text { get; }

        internal WordNode WordNode { get; set; }

        internal ParensNode ParensNode { get; set; }

        internal bool IsWordNodeIgnored => WordNode != null;

        internal bool IsParensNodeIgnored => ParensNode != null;

        internal ValueNode(SqlLocation location, string variableName, string text)
        {
            Location = location;
            VariableName = variableName;
            Text = text;
        }

        private string _builtinFunction;

        internal string BuiltinFunction
        {
            get => _builtinFunction;
            set
            {
                _builtinFunction = value;
                BuiltinFunctionName = (BuiltinFunctionName)Enum.Parse(typeof(BuiltinFunctionName), value);
            }
        }

        internal BuiltinFunctionName BuiltinFunctionName { get; private set; }

        //internal string BuiltinFunctionArgument
        //{
        //    get; set;
        //}

        internal char? EscapeChar { get; set; }

        internal bool UseBuiltinFunction => !string.IsNullOrEmpty(BuiltinFunction);

        internal static readonly string[] BuiltinFunctionNames =
        {
            "Escape",
            "StartsWith",
            "Contains",
            "EndsWith",
            "TruncateTime"
        };

        public new void AddNode(ISqlNode node)
        {
            throw new InvalidOperationException($"TypeName : {GetType().Name}, Method : AddNode");
        }


    }
}
