using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EasySqlParser.Internals.Dialect.Transformer;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals.Dialect
{
    // Porting from DOMA
    //   package    org.seasar.doma.jdbc.dialect
    //   class      StandardDialect
    // https://github.com/domaframework/doma
    internal class StandardDialect
    {
        private static readonly char DefaultEscapeChar = '$';
        private static readonly char[] DefaultWildcards = {'%', '_'};

        protected readonly char EscapeChar;

        protected readonly char[] Wildcards;

        internal virtual string ParameterPrefix { get; } = "?";

        internal virtual bool EnableNamedParameter { get; } = false;

        internal bool UseOdbcDateFormat { get; set; }

        internal virtual char OpenQuote { get; } = '"';

        internal virtual char CloseQuote { get; } = '"';


        private readonly Regex _defaultWildcardReplacementPattern;
        private readonly string _defaultReplacement;

        internal StandardDialect() :
            this(DefaultWildcards)
        {

        }

        internal StandardDialect(char[] wildcards) :
            this(DefaultEscapeChar, wildcards)
        {

        }

        internal StandardDialect(char escapeChar, char[] wildcards)
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

        public string Escape(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            return EscapeWildcard(text, escapeChar);
        }

        public string Escape(string text)
        {
            if (text == null)
            {
                return null;
            }
            
            return EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
        }

        public string StartsWith(string text)
        {
            if (text == null)
            {
                return null;
            }

            var escaped = EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
            return escaped + "%";
        }

        public string StartsWith(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            return EscapeWildcard(text, escapeChar) + "%";
        }

        public string EndsWith(string text)
        {
            if (text == null)
            {
                return null;
            }

            var escaped = EscapeWildcard(_defaultWildcardReplacementPattern, text, _defaultReplacement);
            return "%" + escaped;
        }

        public string EndsWith(string text, char escapeChar)
        {
            if (text == null)
            {
                return null;
            }

            return "%" + EscapeWildcard(text, escapeChar);
        }

        public string Contains(string text)
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

        public string Contains(string text, char escapeChar)
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

        protected string EscapeWildcard(string input, char escapeChar)
        {
            var pattern = CreateWildcardReplacementPattern(escapeChar, Wildcards);
            var replacement = CreateWildcardReplacement(escapeChar);
            return EscapeWildcard(pattern, input, replacement);
        }

        protected string EscapeWildcard(Regex regex, string input, String replacement)
        {
            return regex.Replace(input, replacement);
        }


        protected Regex CreateWildcardReplacementPattern(char escapeChar, char[] wildcards)
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

        protected string CreateWildcardReplacement(char escapeChar)
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

        public DateTime TruncateTime(DateTime value)
        {
            var dat = value;
            return new DateTime(dat.Year, dat.Month, dat.Day);
        }

        public DateTimeOffset TruncateTime(DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);
        }

        public virtual string ToLogFormat(string value)
        {
            return $"'{value}'";
        }

        public virtual string ToLogFormat(DateTime value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{ts'{value:yyyy-MM-dd HH:mm:ss}'}}";
            }
            return $"'{value}'";
        }

        public virtual string ToLogFormatDateOnly(DateTime value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{d'{value:yyyy-MM-dd}'}}";
            }
            return $"'{value:yyyy-MM-dd}'";
        }

        public virtual string ToLogFormatDateOnly(DateTimeOffset value)
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


        public virtual string ToLogFormat(bool value)
        {
            return value ? "1" : "0";
        }

        public virtual string ToLogFormat(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string ToLogFormat(byte[] value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(byte value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string ToLogFormat(Enum value)
        {
            return $"'{value.ToString()}'";
        }

        public virtual string ToLogFormat(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string ToLogFormat(int value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(long value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(short value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(TimeSpan value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{t'{value:hh\\:mm\\:ss}'}}";
            }
            return $"'{value.ToString()}'";
        }

        public virtual string ToLogFormat(DateTimeOffset value)
        {
            if (UseOdbcDateFormat)
            {
                return $"{{ts'{value:yyyy-MM-dd HH:mm:ss}'}}";
            }
            return $"'{value.ToString()}'";
        }

        public virtual string ToLogFormat(sbyte value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(uint value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(ulong value)
        {
            return value.ToString();
        }

        public virtual string ToLogFormat(ushort value)
        {
            return value.ToString();
        }

        public string ApplyQuote(string name)
        {
            return $"{OpenQuote}{name}{CloseQuote}";
        }

    }
}
