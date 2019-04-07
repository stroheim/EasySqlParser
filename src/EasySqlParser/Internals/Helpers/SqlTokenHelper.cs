using System.Globalization;

namespace EasySqlParser.Internals.Helpers
{
    internal static class SqlTokenHelper
    {
        #region code base org.seasar.doma.internal.util.SqlTokenUtil

        internal static bool IsWordPart(char c)
        {
            if (char.IsWhiteSpace(c))
            {
                return false;
            }
            switch (c)
            {
                case '=':
                case '<':
                case '>':
                case '-':
                case ',':
                case '/':
                case '*':
                case '+':
                case '(':
                case ')':
                case ';':
                    return false;
                default:
                    return true;
            }

        }


        internal static bool IsWhitespace(char c)
        {
            switch (c)
            {
                case '\u0009':
                case '\u000B':
                case '\u000C':
                case '\u001C':
                case '\u001D':
                case '\u001E':
                case '\u001F':
                case '\u0020':
                    return true;
                default:
                    return false;
            }

        }
        #endregion

        internal static string Extract(SqlTokenType tokenType, string token)
        {
            switch (tokenType)
            {
                case SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(2, token.Length - 2 - 2));
                case SqlTokenType.LITERAL_VARIABLE_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(3, token.Length - 2 - 3));
                case SqlTokenType.EMBEDDED_VARIABLE_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(3, token.Length - 2 - 3));
                case SqlTokenType.IF_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(5, token.Length - 2 - 5));
                case SqlTokenType.ELSEIF_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(9, token.Length - 2 - 9));
                case SqlTokenType.FOR_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(6, token.Length - 2 - 6));
                case SqlTokenType.EXPAND_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(9, token.Length - 2 - 9));
                case SqlTokenType.POPULATE_BLOCK_COMMENT:
                    return TrimWhitespace(token.Substring(11, token.Length - 2 - 11));
                default:
                    return token;
            }
        }

        // code base org.seasar.doma.internal.util.StringUtil
        internal static string TrimWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var chars = text.ToCharArray();
            var start = 0;
            var end = chars.Length;
            while ((start < end) && (char.IsWhiteSpace(chars[start])))
            {
                start++;
            }

            while ((start < end) && (char.IsWhiteSpace(chars[end - 1])))
            {
                end--;
            }

            if (start < end)
            {
                return ((start > 0) || (end < chars.Length)) ? new string(chars,
                    start, end - start) : text;
            }

            return "";
        }

        #region Code from Roslyn.Utilities.UnicodeCharacterUtilities.IsIdentifierStartCharacter
        //https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/UnicodeCharacterUtilities.cs
        internal static bool IsIdentifierStartCharacter(char ch)
        {
            // identifier-start-character:
            //   letter-character
            //   _ (the underscore character U+005F)

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return false;
                }

                return ch <= 'Z'  // '\u005A'
                       || ch == '_'; // '\u005F'
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            return IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
        }

        private static bool IsLetterChar(UnicodeCategory category)
        {
            // letter-character:
            //   A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl 
            //   A Unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl

            switch (category)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
            }

            return false;
        }

        #endregion
    }
}
