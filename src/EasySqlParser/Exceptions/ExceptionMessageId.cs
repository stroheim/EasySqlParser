namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// An enumeration that defines the type of exception message.
    /// 例外メッセージの種類を定義した列挙型
    /// </summary>
    /// <remarks>
    /// Code based on org.seasar.doma.message.Message
    /// https://github.com/domaframework/doma
    /// </remarks>
    public enum ExceptionMessageId
    {
        #region SqlParseException
        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// The single quotation mark \"''\" for the end of the string literal is not found. SQL=[{0}]
        /// </summary>
        Esp2101,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// The string \"*/\" for the end of the multi-line comment is not found. SQL=[{0}]
        /// </summary>
        Esp2102,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// \"/*%if ...*/\" that corresponds to \"/*%end*/\" is not found. SQL=[{0}]
        /// </summary>
        Esp2104,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// The open parenthesis that corresponds to the close parenthesis is not found. SQL=[{0}]
        /// </summary>
        Esp2109,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// The directive \"{3}\" must be followed immediately by either a test literal or an open parenthesis. SQL=[{0}]
        /// </summary>
        Esp2110,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// When the directive starts with \"/*%\", the following string must be either \"if\", \"else\", \"elseif\", or \"end\" . SQL=[{0}]
        /// </summary>
        Esp2119,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// While the bind variable directive \"{3}\" is defined, the expression is none. SQL=[{0}]
        /// </summary>
        Esp2120,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// While the embedded variable comment \"{3}\" is defined, the expression is none. SQL=[{0}]
        /// </summary>
        Esp2121,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// \"/%if ...*/\" is not closed with \"/*%end*/\". The pair of \"/%if ...*/\" and \"/*%end*/\" must exist in the same clause such as SELECT, FROM, WHERE and so on. SQL=[{0}]
        /// </summary>
        Esp2133,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// \"/%for ...*/\" is not closed with \"/*%end*/\". The pair of \"/%for ...*/\" and \"/*%end*/\" must exist in the same clause such as SELECT, FROM, WHERE and so on. SQL=[{0}]
        /// </summary>
        Esp2134,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// The parenthesis is not closed. Otherwise, the open parenthesis and the close parenthesis are not in a same directive block. SQL=[{0}]
        /// </summary>
        Esp2135,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// \"/*%if ...*/\" that corresponds to \"/*%elseif ...*/\" is not found. SQL=[{0}]
        /// </summary>
        Esp2138,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// \"/*%elseif ...*/\" is behind \"/*%else*/\". SQL=[{0}]
        /// </summary>
        Esp2139,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// \"/*%if ...*/\" that corresponds to \"/*%else ...*/\" is not found. SQL=[{0}]
        /// </summary>
        Esp2140,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// There are multiple \"/*%else*/\". SQL=[{0}]
        /// </summary>
        Esp2141,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// The value \"{3}\" is illegal as a literal format. SQL=[{0}]
        /// </summary>
        Esp2142,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// An asterisk \"*\" must follow immediately the expansion directive \"{3}\". SQL=[{0}]
        /// </summary>
        Esp2143,

        /// <summary>
        /// Failed to parse the SQL on line {1} at column {2}.
        /// While the literal directive \"{3}\" is defined, the expression is none. SQL=[{0}]
        /// </summary>
        Esp2228,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// A built-in function \"{3}\" is not defined. SQL=[{0}]
        /// </summary>
        Esp2150,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// Too many arguments to function \"{3}\". SQL=[{0}]
        /// </summary>
        Esp2151,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// \"{3}\" is invalid escape character.  SQL=[{0}]
        /// </summary>
        Esp2152,
        #endregion

        /// <summary>
        /// The original SQL statement must have an "ORDER BY" clause to translate the statement for pagination.
        /// </summary>
        Esp2201,

        /// <summary>
        /// The SQL execution is failed because of optimistic locking. PATH=[{0}] SQL=[{1}]
        /// </summary>
        Esp2003,

        #region SqlBuildException
        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// The type of the expression \"{3}\" must be a subtype of System.Collections.IEnumerable, but it is [{4}]. SQL=[{0}]
        /// </summary>
        Esp2112,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// The null value is found in the elements of the expression \"{3}\" at index {4}. SQL=[{0}]
        /// </summary>
        Esp2115,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// A single quotation mark \"''\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]
        /// </summary>
        Esp2116,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// A semi-colon \";\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]
        /// </summary>
        Esp2117,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// A string \"--\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]
        /// </summary>
        Esp2122,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// A string \"/*\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]
        /// </summary>
        Esp2123,

        /// <summary>
        /// Failed to build the SQL on line {1} at column {2}.
        /// A single quotation mark \"''\" is contained in the expression \"{3}\" but the literal variable directive doesn't allow it. SQL=[{0}]
        /// </summary>
        Esp2224,

        #endregion

        #region ExpressionEvaluateException

        /// <summary>
        /// Unsupported character found {1} ExpressionText={0}
        /// </summary>
        EspA001,

        /// <summary>
        /// The close parenthesis is not found ExpressionText={0}
        /// </summary>
        EspA002,

        /// <summary>
        /// Literal constant value must always be on the right side ExpressionText={0}
        /// </summary>
        EspA003,

        /// <summary>
        /// Unknown parameter found {1} ExpressionText={0}
        /// </summary>
        EspA011,

        /// <summary>
        /// Invalid property name {1} ExpressionText={0}
        /// </summary>
        EspA012,

        /// <summary>
        /// Property not found {1} ExpressionText={0}
        /// </summary>
        EspA013,

        /// <summary>
        /// Invalid string literal {1} ExpressionText={0}
        /// </summary>
        EspA014,

        /// <summary>
        /// Unsupported operator {1}{2} ExpressionText={0}
        /// </summary>
        EspA021,

        /// <summary>
        /// Invalid int literal {1} ExpressionText={0}
        /// </summary>
        EspA031,
        /// <summary>
        /// Invalid long literal {1} ExpressionText={0}
        /// </summary>
        EspA032,
        /// <summary>
        /// Invalid float literal {1} ExpressionText={0}
        /// </summary>
        EspA033,
        /// <summary>
        /// Invalid double literal {1} ExpressionText={0}
        /// </summary>
        EspA034,
        /// <summary>
        /// Invalid decimal literal {1} ExpressionText={0}
        /// </summary>
        EspA035,
        /// <summary>
        /// Invalid uint literal {1} ExpressionText={0}
        /// </summary>
        EspA036,
        /// <summary>
        /// Invalid ulong literal {1} ExpressionText={0}
        /// </summary>
        EspA037,
        /// <summary>
        /// Invalid numeric literal {1} ExpressionText={0}
        /// </summary>
        EspA038,

        /// <summary>
        /// Unexpected expression evaluate result {1} ExpressionText={0}
        /// </summary>
        EspA041,

        #endregion

        // other exception

        /// <summary>
        /// Unsupported sql comment {0}
        /// </summary>
        EspB001,

        /// <summary>
        /// SQL file not found FilePath={0}
        /// </summary>
        EspC001,

        /// <summary>
        /// SQL statement is empty FilePath={0}
        /// </summary>
        EspC002,

        /// <summary>
        /// configuration not registered
        /// </summary>
        EspD001,

        /// <summary>
        /// DbConnectionKind is not specified
        /// </summary>
        EspD002,

        /// <summary>
        /// DataParameterCreator is not set
        /// </summary>
        EspD003,

        /// <summary>
        /// Configuration name is not set
        /// </summary>
        EspD004,

        /// <summary>
        /// Primary key not found. Table=[{0}]
        /// </summary>
        EspE001,

    }

}
