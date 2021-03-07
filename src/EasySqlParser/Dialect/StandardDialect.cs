using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EasySqlParser.Internals.Node;
using EasySqlParser.Internals.Transformer;

namespace EasySqlParser.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      StandardDialect
    // https://github.com/domaframework/doma
    /// <summary>
    /// A standard RDB dialect.
    /// This class for ODBC.
    /// </summary>
    public class StandardDialect
    {
        private static readonly char DefaultEscapeChar = '$';
        private static readonly char[] DefaultWildcards = {'%', '_'};

        /// <summary>
        /// The escape character for the SQL LIKE operator.
        /// </summary>
        protected readonly char EscapeChar;

        /// <summary>
        /// The wild card characters for the SQL LIKE operator.
        /// </summary>
        protected readonly char[] Wildcards;

        /// <summary>
        /// represents for ADO.NET command parameter prefix.
        /// In ODBC, "?" is a parameter.
        /// </summary>
        public virtual string ParameterPrefix { get; } = "?";

        /// <summary>
        /// Gets whether named parameters are supported. 
        /// </summary>
        public virtual bool EnableNamedParameter { get; } = false;

        internal bool UseOdbcDateFormat { get; set; }

        internal virtual char OpenQuote { get; } = '"';

        internal virtual char CloseQuote { get; } = '"';

        /// <summary>
        /// Gets whether "SEQUENCE" are supported. 
        /// </summary>
        public virtual bool SupportsSequence { get; } = false;

        /// <summary>
        /// Gets whether "IDENTITY column" are supported. 
        /// </summary>
        public virtual bool SupportsIdentity { get; } = false;

        /// <summary>
        /// Gets whether "FINAL TABLE clause" are supported. 
        /// </summary>
        public virtual bool SupportsFinalTable { get; } = false;

        /// <summary>
        /// Gets whether "RETURNING INTO clause" are supported. 
        /// </summary>
        public virtual bool SupportsReturningInto { get; } = false;

        /// <summary>
        /// Gets whether "RETURNING clause" are supported. 
        /// </summary>
        public virtual bool SupportsReturning { get; } = false;

        /// <summary>
        /// Gets terminator to be used for SQL statements.
        /// </summary>
        public virtual string StatementTerminator { get; } = ";";

        /// <summary>
        /// Gets IDENTITY column where clause.
        /// </summary>
        /// <param name="columnName">IDENTITY column name</param>
        /// <returns></returns>
        public virtual string GetIdentityWhereClause(string columnName)
        {
            throw new NotImplementedException();
        }


        private readonly Regex _defaultWildcardReplacementPattern;
        private readonly string _defaultReplacement;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialect"/> class.
        /// </summary>
        public StandardDialect() :
            this(DefaultWildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialect"/> class.
        /// </summary>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public StandardDialect(char[] wildcards) :
            this(DefaultEscapeChar, wildcards)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialect"/> class.
        /// </summary>
        /// <param name="escapeChar">escape character for the SQL LIKE operator</param>
        /// <param name="wildcards">wild card characters for the SQL LIKE operator</param>
        public StandardDialect(char escapeChar, char[] wildcards)
        {
            EscapeChar = escapeChar;
            Wildcards = wildcards ?? DefaultWildcards;
            _defaultWildcardReplacementPattern = CreateWildcardReplacementPattern(escapeChar, Wildcards);
            _defaultReplacement = CreateWildcardReplacement(escapeChar);
        }

        //[Obsolete("will remove",true)]
        //internal ISqlNode TransformSelectSqlNode(ISqlNode node, SelectOptions options)
        //{
        //    /*
        //     * Q：これは何をやっているの？？
        //     * A：ページング用のSqlNodeの構築
        //     * Q：え？Countっぽいのもしてるけど？？
        //     * A：それはMySQL用の特殊処理です
        //     * Q：だからどういうことだってばよ
        //     * A：よろしい
        //     * 　　説明しようMySQLでは下記のクエリでページングと件数の取得ができる
        //     * 　　1) SELECT SQL_CALC_FOUND_ROWS * FROM employees LIMIT 5, 10;
        //     * 　　2) SELECT FOUND_ROWS();
        //     *　　簡単に言えば1のSQL_CALC_FOUND_ROWSを付加するための特殊処理ととらえればよい
        //     *
        //     * 正直わかりにくいのでMysqlPagingTransformerでSQL_CALC_FOUND_ROWSを付加したらよいのでは。。。
        //     */
        //    var transformed = node;
        //    if (options.CallCount)
        //    {
        //        transformed = ToCountCalculatingSqlNode(node);
        //    }

        //    if (options.Skip >= 0 || options.Take >= 0)
        //    {
        //        transformed = ToPagingSqlNode(transformed, options.Skip, options.Take);
        //    }

        //    return transformed;
        //}

        //[Obsolete("will remove", true)]
        //protected virtual ISqlNode ToCountCalculatingSqlNode(ISqlNode node)
        //{
        //    return node;
        //}

        internal virtual ISqlNode ToCountGettingSqlNode(ISqlNode node)
        {
            var transformer = new StandardCountGettingTransformer();
            return transformer.Transform(node);
        }

        internal virtual ISqlNode ToPagingSqlNode(ISqlNode node, long offset, long limit, string rowNumberColumn)
        {
            var transformer = new StandardPagingTransformer(offset, limit, rowNumberColumn);
            return transformer.Transform(node);
        }

        internal string Escape(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            return EscapeWildcard(text, escapeChar);
        }

        internal string Escape(string text)
        {
            if (text == null)
            {
                return null;
            }
            
            return EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
        }

        internal string StartsWith(string text)
        {
            if (text == null)
            {
                return null;
            }

            var escaped = EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
            return escaped + "%";
        }

        internal string StartsWith(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            return EscapeWildcard(text, escapeChar) + "%";
        }

        internal string EndsWith(string text)
        {
            if (text == null)
            {
                return null;
            }

            var escaped = EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
            return "%" + escaped;
        }

        internal string EndsWith(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            return "%" + EscapeWildcard(text, escapeChar);
        }

        internal string Contains(string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.Length == 0)
            {
                return "%";
            }

            var escaped = EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
            return $"%{escaped}%";
        }

        internal string Contains(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            if (text.Length == 0)
            {
                return "%";
            }

            return $"%{EscapeWildcard(text, escapeChar)}%";
        }

        private string EscapeWildcard(string input, char escapeChar)
        {
            var pattern = CreateWildcardReplacementPattern(escapeChar, Wildcards);
            var replacement = CreateWildcardReplacement(escapeChar);
            return EscapeWildcard(pattern, input, replacement);
        }

        private static string EscapeWildcard(Regex regex, string input, string replacement)
        {
            return regex.Replace(input, replacement);
        }


        private static Regex CreateWildcardReplacementPattern(char escapeChar, char[] wildcards)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            foreach (var wildcard in wildcards)
            {
                if (escapeChar == '[' || escapeChar == ']')
                {
                    builder.Append("\\");
                }

                if (escapeChar == '\\' || escapeChar == '$')
                {
                    builder.Append('\\');
                }
                builder.Append(escapeChar);

                if (wildcard == '[' || wildcard == ']')
                {
                    builder.Append("\\");
                }

                builder.Append(wildcard);
            }

            builder.Append("]");
            return new Regex(builder.ToString(), RegexOptions.Compiled);
        }

        private static string CreateWildcardReplacement(char escapeChar)
        {
            var builder = new StringBuilder();
            //if (escapeChar == '\\')
            //{
            //    builder.Append('\\');
            //}

            if (escapeChar == '$')
            {
                builder.Append("$");
            }
            builder.Append(escapeChar);
            builder.Append("$0");
            return builder.ToString();
        }

        internal DateTime TruncateTime(DateTime value)
        {
            var dat = value;
            return new DateTime(dat.Year, dat.Month, dat.Day);
        }

        internal DateTimeOffset TruncateTime(DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);
        }

        /// <summary>
        /// Convert value to log format.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ConvertToLogFormat(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string stringValue)
            {
                return ToLogFormat(stringValue);
            }

            if (value is DateTime dateTimeValue)
            {
                return ToLogFormat(dateTimeValue);
            }

            if (value is int intValue)
            {
                return ToLogFormat(intValue);
            }

            if (value is long longValue)
            {
                return ToLogFormat(longValue);
            }

            if (value is bool boolValue)
            {
                return ToLogFormat(boolValue);
            }

            if (value is decimal decimalValue)
            {
                return ToLogFormat(decimalValue);
            }

            if (value is byte byteValue)
            {
                return ToLogFormat(byteValue);
            }

            if (value is byte[] bytesValue)
            {
                return ToLogFormat(bytesValue);
            }

            if (value is Enum enumValue)
            {
                return ToLogFormat(enumValue);
            }

            if (value is double doubleValue)
            {
                return ToLogFormat(doubleValue);
            }

            if (value is float floatValue)
            {
                return ToLogFormat(floatValue);
            }

            if (value is sbyte sbyteValue)
            {
                return ToLogFormat(sbyteValue);
            }

            if (value is short shortValue)
            {
                return ToLogFormat(shortValue);
            }

            if (value is uint uintValue)
            {
                return ToLogFormat(uintValue);
            }

            if (value is ulong ulongValue)
            {
                return ToLogFormat(ulongValue);
            }

            if (value is ushort ushortValue)
            {
                return ToLogFormat(ushortValue);
            }

            if (value is DateTimeOffset dateTimeOffsetValue)
            {
                return ToLogFormat(dateTimeOffsetValue);
            }

            if (value is TimeSpan timeSpanValue)
            {
                return ToLogFormat(timeSpanValue);
            }
            return value.ToString();

        }

        internal virtual string ToLogFormat(string value)
        {
            return $"'{value}'";
        }

        internal virtual string ToLogFormat(DateTime value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{ts'{value:yyyy-MM-dd HH:mm:ss}'}}";
            }
            return $"'{value}'";
        }

        internal virtual string ToLogFormatDateOnly(DateTime value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{d'{value:yyyy-MM-dd}'}}";
            }
            return $"'{value:yyyy-MM-dd}'";
        }

        internal virtual string ToLogFormatDateOnly(DateTimeOffset value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{d'{value:yyyy-MM-dd}'}}";
            }
            return $"'{value:yyyy-MM-dd}'";
        }


        //public virtual string ToLogFormatDateAndTime(DateTime value)
        //{
        //    return $"'{value:yyyy-MM-dd HH:mm:ss}'";
        //}

        //public virtual string ToLogFormatTimeOnly(DateTime value)
        //{
        //    return $"'{value:HH:mm:ss}'";
        //}

        //public virtual string ToLogFormatTimeOnly(DateTimeOffset value)
        //{
        //    return $"'{value:HH:mm:ss}'";
        //}


        //public virtual string ToLogFormatTimeOnly(TimeSpan value)
        //{
        //    return $"'{value:hh\\:mm\\:ss}'";
        //}


        internal virtual string ToLogFormat(bool value)
        {
            return value ? "1" : "0";
        }

        internal virtual string ToLogFormat(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        internal virtual string ToLogFormat(byte[] value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(byte value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        internal virtual string ToLogFormat(Enum value)
        {
            return $"'{value.ToString()}'";
        }

        internal virtual string ToLogFormat(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        internal virtual string ToLogFormat(int value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(long value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(short value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(TimeSpan value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{t'{value:hh\\:mm\\:ss}'}}";
            }
            return $"'{value.ToString()}'";
        }

        internal virtual string ToLogFormat(DateTimeOffset value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{ts'{value:yyyy-MM-dd HH:mm:ss}'}}";
            }
            return $"'{value.ToString()}'";
        }

        internal virtual string ToLogFormat(sbyte value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(uint value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(ulong value)
        {
            return value.ToString();
        }

        internal virtual string ToLogFormat(ushort value)
        {
            return value.ToString();
        }

        internal string GetEscapedValue(string value, char? escapeChar = null)
        {
            if (escapeChar.HasValue)
            {
                return Escape(value, escapeChar.Value);
            }
            return Escape(value);

        }

        internal string GetStartsWithValue(string value, char? escapeChar = null)
        {
            if (escapeChar.HasValue)
            {
                return StartsWith(value, escapeChar.Value);
            }

            return StartsWith(value);
        }

        internal string GetContainsValue(string value, char? escapeChar = null)
        {
            if (escapeChar.HasValue)
            {
                return Contains(value, escapeChar.Value);
            }

            return Contains(value);
        }

        internal string GetEndsWithValue(string value, char? escapeChar = null)
        {
            if (escapeChar.HasValue)
            {
                return EndsWith(value, escapeChar.Value);
            }

            return EndsWith(value);
        }


        /// <summary>
        /// Enclose the name with quotation marks.
        /// </summary>
        /// <param name="name">the name of a database object such as a table, a column, and so on</param>
        /// <returns></returns>
        public string ApplyQuote(string name)
        {
            return $"{OpenQuote}{name}{CloseQuote}";
        }

        internal string GetSequenceName(string name, string schema)
        {
            return (!string.IsNullOrEmpty(schema) ? ApplyQuote(schema) + "." : "") + ApplyQuote(name);
        }

        internal string GetSequencePrefix(string prefix, string concatExpression)
        {
            return !string.IsNullOrEmpty(prefix) ? $"'{prefix}' {concatExpression} " : "";
        }

        /// <summary>
        /// Gets SEQUENCE generator sql.
        /// </summary>
        /// <param name="name">the SEQUENCE name</param>
        /// <param name="schema">the SEQUENCE's schema name</param>
        /// <returns></returns>
        public virtual string GetNextSequenceSql(string name, string schema)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets SEQUENCE generator sql.
        /// The SEQUENCE is zero padded.
        /// </summary>
        /// <param name="name">the SEQUENCE name</param>
        /// <param name="schema">the SEQUENCE's schema name</param>
        /// <param name="length">padding length</param>
        /// <param name="prefix">prefix string(optional)</param>
        /// <returns></returns>
        public virtual string GetNextSequenceSqlZeroPadding(string name, string schema, int length, string prefix = null)
        {
            throw new NotImplementedException();
        }


    }
}
