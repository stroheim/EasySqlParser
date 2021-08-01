using System;
using EasySqlParser.Exceptions;

namespace EasySqlParser.Internals.Helpers
{
    internal static class ExceptionMessageIdHelper
    {
        internal static string ToMessage(ExceptionMessageId messageId, params object[] args)
        {
            switch (messageId)
            {
                #region SqlParseException
                case ExceptionMessageId.Esp2101:
                    return string.Format(MessageFormat.Esp2101, args);
                case ExceptionMessageId.Esp2102:
                    return string.Format(MessageFormat.Esp2102, args);
                case ExceptionMessageId.Esp2104:
                    return string.Format(MessageFormat.Esp2104, args);
                case ExceptionMessageId.Esp2109:
                    return string.Format(MessageFormat.Esp2109, args);
                case ExceptionMessageId.Esp2110:
                    return string.Format(MessageFormat.Esp2110, args);
                case ExceptionMessageId.Esp2119:
                    return string.Format(MessageFormat.Esp2119, args);
                case ExceptionMessageId.Esp2120:
                    return string.Format(MessageFormat.Esp2120, args);
                case ExceptionMessageId.Esp2121:
                    return string.Format(MessageFormat.Esp2121, args);
                case ExceptionMessageId.Esp2133:
                    return string.Format(MessageFormat.Esp2133, args);
                case ExceptionMessageId.Esp2134:
                    return string.Format(MessageFormat.Esp2134, args);
                case ExceptionMessageId.Esp2135:
                    return string.Format(MessageFormat.Esp2135, args);
                case ExceptionMessageId.Esp2138:
                    return string.Format(MessageFormat.Esp2138, args);
                case ExceptionMessageId.Esp2139:
                    return string.Format(MessageFormat.Esp2139, args);
                case ExceptionMessageId.Esp2140:
                    return string.Format(MessageFormat.Esp2140, args);
                case ExceptionMessageId.Esp2141:
                    return string.Format(MessageFormat.Esp2141, args);
                case ExceptionMessageId.Esp2142:
                    return string.Format(MessageFormat.Esp2142, args);
                case ExceptionMessageId.Esp2143:
                    return string.Format(MessageFormat.Esp2143, args);
                case ExceptionMessageId.Esp2228:
                    return string.Format(MessageFormat.Esp2228, args);
                case ExceptionMessageId.Esp2150:
                    return string.Format(MessageFormat.Esp2150, args);
                case ExceptionMessageId.Esp2151:
                    return string.Format(MessageFormat.Esp2151, args);
                case ExceptionMessageId.Esp2152:
                    return string.Format(MessageFormat.Esp2152, args);
                #endregion

                #region SqlBuildException
                case ExceptionMessageId.Esp2112:
                    return string.Format(MessageFormat.Esp2112, args);
                case ExceptionMessageId.Esp2115:
                    return string.Format(MessageFormat.Esp2115, args);
                case ExceptionMessageId.Esp2116:
                    return string.Format(MessageFormat.Esp2116, args);
                case ExceptionMessageId.Esp2117:
                    return string.Format(MessageFormat.Esp2117, args);
                case ExceptionMessageId.Esp2122:
                    return string.Format(MessageFormat.Esp2122, args);
                case ExceptionMessageId.Esp2123:
                    return string.Format(MessageFormat.Esp2123, args);
                case ExceptionMessageId.Esp2224:
                    return string.Format(MessageFormat.Esp2224, args);
                #endregion

                case ExceptionMessageId.Esp2201:
                    return string.Format(MessageFormat.Esp2201);

                case ExceptionMessageId.Esp2003:
                    return string.Format(MessageFormat.Esp2003, args);

                #region ExpressionEvaluateException
                case ExceptionMessageId.EspA001:
                    return string.Format(MessageFormat.EspA001, args);
                case ExceptionMessageId.EspA002:
                    return string.Format(MessageFormat.EspA002, args);
                case ExceptionMessageId.EspA003:
                    return string.Format(MessageFormat.EspA003, args);
                case ExceptionMessageId.EspA011:
                    return string.Format(MessageFormat.EspA011, args);
                case ExceptionMessageId.EspA012:
                    return string.Format(MessageFormat.EspA012, args);
                case ExceptionMessageId.EspA013:
                    return string.Format(MessageFormat.EspA013, args);
                case ExceptionMessageId.EspA014:
                    return string.Format(MessageFormat.EspA014, args);
                case ExceptionMessageId.EspA021:
                    return string.Format(MessageFormat.EspA021, args);
                case ExceptionMessageId.EspA031:
                    return string.Format(MessageFormat.EspA031, args);
                case ExceptionMessageId.EspA032:
                    return string.Format(MessageFormat.EspA032, args);
                case ExceptionMessageId.EspA033:
                    return string.Format(MessageFormat.EspA033, args);
                case ExceptionMessageId.EspA034:
                    return string.Format(MessageFormat.EspA034, args);
                case ExceptionMessageId.EspA035:
                    return string.Format(MessageFormat.EspA035, args);
                case ExceptionMessageId.EspA036:
                    return string.Format(MessageFormat.EspA036, args);
                case ExceptionMessageId.EspA037:
                    return string.Format(MessageFormat.EspA037, args);
                case ExceptionMessageId.EspA038:
                    return string.Format(MessageFormat.EspA038, args);
                case ExceptionMessageId.EspA041:
                    return string.Format(MessageFormat.EspA041, args);
                #endregion

                case ExceptionMessageId.EspB001:
                    return string.Format(MessageFormat.EspB001, args);
                case ExceptionMessageId.EspC001:
                    return string.Format(MessageFormat.EspC001, args);
                case ExceptionMessageId.EspC002:
                    return string.Format(MessageFormat.EspC002, args);
                case ExceptionMessageId.EspD001:
                    return string.Format(MessageFormat.EspD001);
                case ExceptionMessageId.EspD002:
                    return string.Format(MessageFormat.EspD002);
                case ExceptionMessageId.EspD003:
                    return string.Format(MessageFormat.EspD003);
                case ExceptionMessageId.EspD004:
                    return string.Format(MessageFormat.EspD004);
                case ExceptionMessageId.EspE001:
                    return string.Format(MessageFormat.EspE001, args);
                default:
                    throw new InvalidOperationException($"Unknown message id:{messageId}");
            }

        }

        // code base org.seasar.doma.message.Message
        private static class MessageFormat
        {
            #region SqlParseException

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 文字列リテラルの終了を示すクォテーション['']が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2101 = "Failed to parse the SQL on line {1} at column {2}. The single quotation mark \"''\" for the end of the string literal is not found. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// ブロックコメントの終了を示す文字列[*/]が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2102 = "Failed to parse the SQL on line {1} at column {2}. The string \"*/\" for the end of the multi-line comment is not found. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// /*%end*/に対応する/*%if ...*/が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2104 = "Failed to parse the SQL on line {1} at column {2}. \"/*%if ...*/\" that corresponds to \"/*%end*/\" is not found. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 閉じ括弧に対応する開き括弧が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2109 = "Failed to parse the SQL on line {1} at column {2}. The open parenthesis that corresponds to the close parenthesis is not found. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// バインド変数コメントもしくはリテラル変数コメント[{3}]の直後にテスト用のリテラルもしくは括弧が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2110 = "Failed to parse the SQL on line {1} at column {2}. The directive \"{3}\" must be followed immediately by either a test literal or an open parenthesis. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// ブロックコメントを/*%で開始する場合、続く文字列は、if、else、elseif、endのいずれかでなければいけません。SQL[{0}]
            /// </summary>
            internal const string Esp2119 = "Failed to parse the SQL on line {1} at column {2}. When the directive starts with \"/*%\", the following string must be either \"if\", \"else\", \"elseif\", or \"end\" . SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// バインド変数コメント[{3}]が定義されていますが、バインド変数が空文字です。SQL[{0}]
            /// </summary>
            internal const string Esp2120 = "Failed to parse the SQL on line {1} at column {2}. While the bind variable directive \"{3}\" is defined, the expression is none. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 埋め込み変数コメント[{3}]が定義されていますが、埋め込み変数が空文字です。SQL[{0}]
            /// </summary>
            internal const string Esp2121 = "Failed to parse the SQL on line {1} at column {2}. While the embedded variable comment \"{3}\" is defined, the expression is none. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// /%if ...*/が/*%end*/で閉じられていません。/%if ...*/と/*%end*/の組は、同じ節（たとえばSELECT、FROM、WHERE節など）の中に存在しなければいけません。SQL[{0}]
            /// </summary>
            internal const string Esp2133 = "Failed to parse the SQL on line {1} at column {2}. \"/%if ...*/\" is not closed with \"/*%end*/\". The pair of \"/%if ...*/\" and \"/*%end*/\" must exist in the same clause such as SELECT, FROM, WHERE and so on. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// /%for ...*/が/*%end*/で閉じられていません/%for ...*/と/*%end*/の組は、同じ節（たとえばSELECT、FROM、WHERE節など）の中に存在しなければいけません。SQL[{0}]
            /// </summary>
            internal const string Esp2134 = "Failed to parse the SQL on line {1} at column {2}. \"/%for ...*/\" is not closed with \"/*%end*/\". The pair of \"/%for ...*/\" and \"/*%end*/\" must exist in the same clause such as SELECT, FROM, WHERE and so on. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 括弧が閉じられていません。もしくは/%if ...*/～/*%end*/を使用していて、開き括弧と閉じ括弧が同じブロック内にありません。SQL[{0}]
            /// </summary>
            internal const string Esp2135 = "Failed to parse the SQL on line {1} at column {2}. The parenthesis is not closed. Otherwise, the open parenthesis and the close parenthesis are not in a same directive block. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// /*%elseif ...*/に対応する/*%if ...*/が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2138 = "Failed to parse the SQL on line {1} at column {2}. \"/*%if ...*/\" that corresponds to \"/*%elseif ...*/\" is not found. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// /*%else*/の後ろに/*%elseif ...*/が存在します。SQL[{0}]
            /// </summary>
            internal const string Esp2139 = "Failed to parse the SQL on line {1} at column {2}. \"/*%elseif ...*/\" is behind \"/*%else*/\". SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// /*%else*/に対応する/*%if ...*/が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2140 = "Failed to parse the SQL on line {1} at column {2}. \"/*%if ...*/\" that corresponds to \"/*%else ...*/\" is not found. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 複数の/*%else*/が存在します。SQL[{0}]
            /// </summary>
            internal const string Esp2141 = "Failed to parse the SQL on line {1} at column {2}. There are multiple \"/*%else*/\". SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// バインド変数コメントもしくはリテラル変数コメント[{3}]の直後の値[{4}]はテスト用のリテラルとして不正です。バインド変数コメントもしくはリテラル変数コメントの直後は、文字列、数値、日時を表すリテラル、もしくは開き括弧でなければいけません。SQL[{0}]
            /// </summary>
            internal const string Esp2142 = "Failed to parse the SQL on line {1} at column {2}. The value \"{3}\" is illegal as a literal format. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// カラム展開コメント[{3}]の直後にアスタリスク(*)が見つかりません。SQL[{0}]
            /// </summary>
            internal const string Esp2143 = "Failed to parse the SQL on line {1} at column {2}. An asterisk \"*\" must follow immediately the expansion directive \"{3}\". SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// リテラル変数コメント[{3}]が定義されていますが、リテラル変数が空文字です。SQL[{0}]
            /// </summary>
            internal const string Esp2228 = "Failed to parse the SQL on line {1} at column {2}. While the literal directive \"{3}\" is defined, the expression is none. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 組み込み関数[{3}]は定義されていせん。SQL[{0}]
            /// </summary>
            internal const string Esp2150 = "Failed to build the SQL on line {1} at column {2}. A bulit-in function \"{3}\" is not defined. SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 組み込み関数[{3}]の引数が多すぎます。SQL[{0}]
            /// </summary>
            internal const string Esp2151 =
                "Failed to build the SQL on line {1} at column {2}. Too many arguments to built-in function \"{3}\". SQL=[{0}]";

            /// <summary>
            /// SQLの解析に失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// [{3}]は無効なエスケープ文字です。SQL[{0}]
            /// </summary>
            internal const string Esp2152 =
                "Failed to build the SQL on line {1} at column {2}. \"{3}\" is invalid escape character.  SQL=[{0}]";
            #endregion

            #region SqlBuildException

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 括弧の前に位置するバインド変数コメントもしくはリテラル変数コメント[{3}]に対応するオブジェクトの型[{4}]がSystem.Collections.IEnumerableのサブタイプではありません。SQL[{0}]
            /// </summary>
            internal const string Esp2112 = "Failed to build the SQL on line {1} at column {2}. The type of the expression \"{3}\" must be a subtype of System.Collections.IEnumerable, but it is [{4}]. SQL=[{0}]";

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 括弧の前に位置するバインド変数コメントもしくはリテラル変数コメント[{3}]に対応するSystem.Collections.Generic.IEnumerableの[{4}]番目の要素がnullです。SQL[{0}]
            /// </summary>
            internal const string Esp2115 = "Failed to build the SQL on line {1} at column {2}. The null value is found in the elements of the expression \"{3}\" at index {4}. SQL=[{0}]";

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 埋め込み変数コメント[{3}]にシングルクォテーションが含まれています。SQL[{0}]
            /// </summary>
            internal const string Esp2116 = "Failed to build the SQL on line {1} at column {2}. A single quotation mark \"''\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]";

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 埋め込み変数コメント[{3}]にセミコロンが含まれています。SQL[{0}]
            /// </summary>
            internal const string Esp2117 = "Failed to build the SQL on line {1} at column {2}. A semi-colon \";\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]";

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 埋め込み変数コメント[{3}]に行コメントが含まれています。SQL[{0}]
            /// </summary>
            internal const string Esp2122 = "Failed to build the SQL on line {1} at column {2}. A string \"--\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]";

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// 埋め込み変数コメント[{3}]にブロックコメントが含まれています。SQL[{0}]
            /// </summary>
            internal const string Esp2123 = "Failed to build the SQL on line {1} at column {2}. A string \"/*\" is contained in the expression \"{3}\" but the embedded variable directive doesn't allow it. SQL=[{0}]";

            /// <summary>
            /// SQLの組み立てに失敗しました。（[{1}]行目[{2}]番目の文字付近）。
            /// リテラル変数コメント[{3}]にシングルクォテーションが含まれています。SQL[{0}]
            /// </summary>
            internal const string Esp2224 = "Failed to build the SQL on line {1} at column {2}. A single quotation mark \"''\" is contained in the expression \"{3}\" but the literal variable directive doesn't allow it. SQL=[{0}]";

            #endregion

            internal const string Esp2201 =
                "The original SQL statement must have an \"ORDER BY\" clause to translate the statement for pagination.";

            internal const string Esp2003 = "The SQL execution is failed because of optimistic locking. PATH=[{0}] SQL=[{1}]";

            #region ExpressionEvaluateException

            internal const string EspA001 = "Unsupported character found {1} ExpressionText={0}";
            internal const string EspA002 = "The close parenthesis is not found ExpressionText={0}";
            internal const string EspA003 = "Literal constant value must always be on the right side ExpressionText={0}";
            internal const string EspA011 = "Unknown parameter found {1} ExpressionText={0}";
            internal const string EspA012 = "Invalid property name {1} ExpressionText={0}";
            internal const string EspA013 = "Property not found {1} ExpressionText={0}";
            internal const string EspA014 = "Invalid string literal {1} ExpressionText={0}";
            internal const string EspA021 = "Unsupported operator {1}{2} ExpressionText={0}";
            internal const string EspA031 = "Invalid int literal {1} ExpressionText={0}";
            internal const string EspA032 = "Invalid long literal {1} ExpressionText={0}";
            internal const string EspA033 = "Invalid float literal {1} ExpressionText={0}";
            internal const string EspA034 = "Invalid double literal {1} ExpressionText={0}";
            internal const string EspA035 = "Invalid decimal literal {1} ExpressionText={0}";
            internal const string EspA036 = "Invalid uint literal {1} ExpressionText={0}";
            internal const string EspA037 = "Invalid ulong literal {1} ExpressionText={0}";
            internal const string EspA038 = "Invalid numeric literal {1} ExpressionText={0}";
            internal const string EspA041 = "Unexpected expression evaluate result {1} ExpressionText={0}";

            #endregion

            internal const string EspB001 = "Unsupported sql comment {0}";

            internal const string EspC001 = "SQL file not found FilePath={0}";

            internal const string EspC002 = "SQL statement is empty FilePath={0}";

            internal const string EspD001 = "configuration not registered";
            internal const string EspD002 = "DbConnectionKind is not specified";
            internal const string EspD003 = "DataParameterCreator is not set";
            internal const string EspD004 = "Configuration name is not set";
            internal const string EspE001 = "Primary key not found. Table=[{0}]";
        }
    }
}
