using EasySqlParser.Exceptions;
using EasySqlParser.Internals.Helpers;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      SqlTokenizer
    // https://github.com/domaframework/doma
    internal class SqlTokenizer
    {
        private int _charPosition = -1;
        private readonly int _stringLength;
        private readonly string _sql;
        private SqlTokenType _tokenType;
        private int _lineStartPosition;
        private int _currentLineNumber;
        private int _startPosition;

        internal SqlTokenizer(string sql)
        {
            _sql = sql;
            _stringLength = sql.Length;
            LineNumber = 1;
            _currentLineNumber = 1;
            _startPosition = 0;
            Peek();
        }

        /// <summary>
        /// line number of sql
        /// </summary>
        internal int LineNumber { get; private set; }

        /// <summary>
        /// char position of line string
        /// </summary>
        internal int Position { get; private set; }


        internal string Token { get; private set; }
        internal SqlTokenType Next()
        {
            switch (_tokenType)
            {
                case SqlTokenType.EOF:
                    Token = null;
                    return SqlTokenType.EOF;
                default:
                    if (_tokenType == SqlTokenType.EOL)
                    {
                        _lineStartPosition = _charPosition;
                    }

                    var currentType = _tokenType;
                    PrepareToken();
                    _startPosition = _charPosition + 1;
                    Peek();
                    return currentType;
            }
        }

        private void AssertUnsupportedTokenType()
        {
            if (_tokenType == SqlTokenType.EXPAND_BLOCK_COMMENT)
            {
                throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(ExpandNode));
            }

            if (_tokenType == SqlTokenType.FOR_BLOCK_COMMENT)
            {
                throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(ForBlockNode));
            }

            if (_tokenType == SqlTokenType.POPULATE_BLOCK_COMMENT)
            {
                throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(PopulateNode));
            }
        }


        private void PrepareToken()
        {
            LineNumber = _currentLineNumber;
            var length = _charPosition - _startPosition;
            if (length + 1 <= _stringLength)
            {
                Token = _sql.Substring(_startPosition, length + 1);
            }
            else
            {
                Token = _sql.Substring(_startPosition, length);
            }
            CalcPosition();
        }

        private void CalcPosition()
        {
            Position = _charPosition - _lineStartPosition;
            if (_tokenType != SqlTokenType.EOL && _currentLineNumber == 1)
            {
                Position = Position + 1;
            }
        }

        private void Peek()
        {
            if (_charPosition < _stringLength)
            {
                _charPosition++;
            }

            if (_charPosition < _stringLength)
            {
                var c = _sql[_charPosition];
                if (_charPosition + 1 < _stringLength)
                {
                    _charPosition++;
                    var c2 = _sql[_charPosition];
                    if (_charPosition + 1 < _stringLength)
                    {
                        _charPosition++;
                        var c3 = _sql[_charPosition];
                        if (_charPosition + 1 < _stringLength)
                        {
                            _charPosition++;
                            var c4 = _sql[_charPosition];
                            if (_charPosition + 1 < _stringLength)
                            {
                                _charPosition++;
                                var c5 = _sql[_charPosition];
                                if (_charPosition + 1 < _stringLength)
                                {
                                    _charPosition++;
                                    var c6 = _sql[_charPosition];
                                    if (_charPosition + 1 < _stringLength)
                                    {
                                        _charPosition++;
                                        var c7 = _sql[_charPosition];
                                        if (_charPosition + 1 < _stringLength)
                                        {
                                            _charPosition++;
                                            var c8 = _sql[_charPosition];
                                            if (_charPosition + 1 < _stringLength)
                                            {
                                                _charPosition++;
                                                var c9 = _sql[_charPosition];
                                                if (_charPosition + 1 < _stringLength)
                                                {
                                                    _charPosition++;
                                                    var c10 = _sql[_charPosition];
                                                    PeekTenChars(c, c2, c3, c4, c5, c6, c7, c8, c9, c10);
                                                }
                                                else
                                                {
                                                    PeekNineChars(c, c2, c3, c4, c5, c6, c7, c8, c9);
                                                }
                                            }
                                            else
                                            {
                                                PeekEightChars(c, c2, c3, c4, c5, c6, c7, c8);
                                            }
                                        }
                                        else
                                        {
                                            PeekSevenChars(c, c2, c3, c4, c5, c6, c7);
                                        }
                                    }
                                    else
                                    {
                                        PeekSixChars(c, c2, c3, c4, c5, c6);
                                    }
                                }
                                else
                                {
                                    PeekFiveChars(c, c2, c3, c4, c5);
                                }
                            }
                            else
                            {
                                PeekFourChars(c, c2, c3, c4);
                            }
                        }
                        else
                        {
                            PeekThreeChars(c, c2, c3);
                        }
                    }
                    else
                    {
                        PeekTwoChars(c, c2);
                    }
                }
                else
                {
                    PeekOneChar(c);
                }
            }
            else
            {
                _tokenType = SqlTokenType.EOF;
            }
        }

        private void PeekTenChars(char c, char c2, char c3, char c4, char c5,
            char c6, char c7, char c8, char c9, char c10)
        {
            if ((c == 'f' || c == 'F') && (c2 == 'o' || c2 == 'O')
                                       && (c3 == 'r' || c3 == 'R') && (SqlTokenHelper.IsWhitespace(c4))
                                       && (c5 == 'u' || c5 == 'U') && (c6 == 'p' || c6 == 'P')
                                       && (c7 == 'd' || c7 == 'D') && (c8 == 'a' || c8 == 'A')
                                       && (c9 == 't' || c9 == 'T') && (c10 == 'e' || c10 == 'E'))
            {
                _tokenType = SqlTokenType.FOR_UPDATE_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }

            _charPosition = _charPosition - 1;
            PeekNineChars(c, c2, c3, c4, c5, c6, c7, c8, c9);
        }

        private void PeekNineChars(char c, char c2, char c3, char c4, char c5,
            char c6, char c7, char c8, char c9)
        {
            if ((c == 'i' || c == 'I') && (c2 == 'n' || c2 == 'N')
                                       && (c3 == 't' || c3 == 'T') && ((c4 == 'e' || c4 == 'E'))
                                       && (c5 == 'r' || c5 == 'R') && (c6 == 's' || c6 == 'S')
                                       && (c7 == 'e' || c7 == 'E') && (c8 == 'c' || c8 == 'C')
                                       && (c9 == 't' || c9 == 'T'))
            {
                _tokenType = SqlTokenType.INTERSECT_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }

            _charPosition = _charPosition - 1;
            PeekEightChars(c, c2, c3, c4, c5, c6, c7, c8);

        }

        private void PeekEightChars(char c, char c2, char c3, char c4, char c5,
            char c6, char c7, char c8)
        {
            if ((c == 'g' || c == 'G') && (c2 == 'r' || c2 == 'R')
                                       && (c3 == 'o' || c3 == 'O') && (c4 == 'u' || c4 == 'U')
                                       && (c5 == 'p' || c5 == 'P') && (SqlTokenHelper.IsWhitespace(c6))
                                       && (c7 == 'b' || c7 == 'B') && (c8 == 'y' || c8 == 'Y'))
            {
                _tokenType = SqlTokenType.GROUP_BY_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'o' || c == 'O') && (c2 == 'r' || c2 == 'R')
                                            && (c3 == 'd' || c3 == 'D') && (c4 == 'e' || c4 == 'E')
                                            && (c5 == 'r' || c5 == 'R') && (char.IsWhiteSpace(c6))
                                            && (c7 == 'b' || c7 == 'B') && (c8 == 'y' || c8 == 'Y'))
            {
                _tokenType = SqlTokenType.ORDER_BY_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'o' || c == 'O') && (c2 == 'p' || c2 == 'P')
                                            && (c3 == 't' || c3 == 'T') && (c4 == 'i' || c4 == 'I')
                                            && (c5 == 'o' || c5 == 'O') && (c6 == 'n' || c6 == 'N')
                                            && (SqlTokenHelper.IsWhitespace(c7)) && (c8 == '('))
            {
                _tokenType = SqlTokenType.OPTION_WORD;
                _charPosition = _charPosition - 2;
                return;
            }

            _charPosition = _charPosition - 1;
            PeekSevenChars(c, c2, c3, c4, c5, c6, c7);
        }

        private void PeekSevenChars(char c, char c2, char c3, char c4, char c5,
            char c6, char c7)
        {
            _charPosition = _charPosition - 1;
            PeekSixChars(c, c2, c3, c4, c5, c6);
        }

        private void PeekSixChars(char c, char c2, char c3, char c4, char c5,
            char c6)
        {
            if ((c == 's' || c == 'S') && (c2 == 'e' || c2 == 'E')
                                       && (c3 == 'l' || c3 == 'L') && (c4 == 'e' || c4 == 'E')
                                       && (c5 == 'c' || c5 == 'C') && (c6 == 't' || c6 == 'T'))
            {
                _tokenType = SqlTokenType.SELECT_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'h' || c == 'H') && (c2 == 'a' || c2 == 'A')
                                            && (c3 == 'v' || c3 == 'V') && (c4 == 'i' || c4 == 'I')
                                            && (c5 == 'n' || c5 == 'N') && (c6 == 'g' || c6 == 'G'))
            {
                _tokenType = SqlTokenType.HAVING_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'e' || c == 'E') && (c2 == 'x' || c2 == 'X')
                                            && (c3 == 'c' || c3 == 'C') && (c4 == 'e' || c4 == 'E')
                                            && (c5 == 'p' || c5 == 'P') && (c6 == 't' || c6 == 'T'))
            {
                _tokenType = SqlTokenType.EXCEPT_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'u' || c == 'U') && (c2 == 'p' || c2 == 'P')
                                            && (c3 == 'd' || c3 == 'D') && (c4 == 'a' || c4 == 'A')
                                            && (c5 == 't' || c5 == 'T') && (c6 == 'e' || c6 == 'E'))
            {
                _tokenType = SqlTokenType.UPDATE_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            _charPosition = _charPosition - 1;
            PeekFiveChars(c, c2, c3, c4, c5);
        }

        private void PeekFiveChars(char c, char c2, char c3, char c4, char c5)
        {
            if ((c == 'w' || c == 'W') && (c2 == 'h' || c2 == 'H')
                                       && (c3 == 'e' || c3 == 'E') && (c4 == 'r' || c4 == 'R')
                                       && (c5 == 'e' || c5 == 'E'))
            {
                _tokenType = SqlTokenType.WHERE_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'u' || c == 'U') && (c2 == 'n' || c2 == 'N')
                                            && (c3 == 'i' || c3 == 'I') && (c4 == 'o' || c4 == 'O')
                                            && (c5 == 'n' || c5 == 'N'))
            {
                _tokenType = SqlTokenType.UNION_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 'm' || c == 'M') && (c2 == 'i' || c2 == 'I')
                                            && (c3 == 'n' || c3 == 'N') && (c4 == 'u' || c4 == 'U')
                                            && (c5 == 's' || c5 == 'S'))
            {
                _tokenType = SqlTokenType.MINUS_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }

            _charPosition = _charPosition - 1;
            PeekFourChars(c, c2, c3, c4);
        }

        private void PeekFourChars(char c, char c2, char c3, char c4)
        {
            if ((c == 'f' || c == 'F') && (c2 == 'r' || c2 == 'R')
                                       && (c3 == 'o' || c3 == 'O') && (c4 == 'm' || c4 == 'M'))
            {
                _tokenType = SqlTokenType.FROM_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }

            _charPosition = _charPosition - 1;
            PeekThreeChars(c, c2, c3);
        }

        private void PeekThreeChars(char c, char c2, char c3)
        {
            if ((c == 'a' || c == 'A') && (c2 == 'n' || c2 == 'N')
                                       && (c3 == 'd' || c3 == 'D'))
            {
                _tokenType = SqlTokenType.AND_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if ((c == 's' || c == 'S') && (c2 == 'e' || c2 == 'E')
                                            && (c3 == 't' || c3 == 'T'))
            {
                _tokenType = SqlTokenType.SET_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }

            _charPosition = _charPosition - 1;
            PeekTwoChars(c, c2);
        }

        private void PeekTwoChars(char c, char c2)
        {
            if ((c == 'o' || c == 'O') && (c2 == 'r' || c2 == 'R'))
            {
                _tokenType = SqlTokenType.OR_WORD;
                if (IsWordEnded())
                {
                    return;
                }
            }
            else if (c == '/' && c2 == '*')
            {
                _tokenType = SqlTokenType.BLOCK_COMMENT;
                if (_charPosition + 1 < _stringLength)
                {
                    _charPosition++;
                    var c3 = _sql[_charPosition];
                    if (IsExpressionIdentifierStart(c3))
                    {
                        _tokenType = SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT;
                    }
                    else if (c3 == '^')
                    {
                        _tokenType = SqlTokenType.LITERAL_VARIABLE_BLOCK_COMMENT;
                    }
                    else if (c3 == '#')
                    {
                        _tokenType = SqlTokenType.EMBEDDED_VARIABLE_BLOCK_COMMENT;
                    }
                    else if (c3 == '%')
                    {
                        if (_charPosition + 1 < _stringLength)
                        {
                            _charPosition++;
                            var c4 = _sql[_charPosition];
                            if (_charPosition + 1 < _stringLength)
                            {
                                _charPosition++;
                                var c5 = _sql[_charPosition];
                                if (c4 == 'i' && c5 == 'f')
                                {
                                    if (IsBlockCommentDirectiveTerminated())
                                    {
                                        _tokenType = SqlTokenType.IF_BLOCK_COMMENT;
                                    }
                                }
                                else if (_charPosition + 1 < _stringLength)
                                {
                                    _charPosition++;
                                    var c6 = _sql[_charPosition];
                                    if (c4 == 'f' && c5 == 'o' && c6 == 'r')
                                    {
                                        if (IsBlockCommentDirectiveTerminated())
                                        {
                                            _tokenType = SqlTokenType.FOR_BLOCK_COMMENT;
                                        }
                                    }
                                    else if (c4 == 'e' && c5 == 'n' && c6 == 'd')
                                    {
                                        if (IsBlockCommentDirectiveTerminated())
                                        {
                                            _tokenType = SqlTokenType.END_BLOCK_COMMENT;
                                        }
                                    }
                                    else if (_charPosition + 1 < _stringLength)
                                    {
                                        _charPosition++;
                                        var c7 = _sql[_charPosition];
                                        if (c4 == 'e' && c5 == 'l' && c6 == 's'
                                            && c7 == 'e')
                                        {
                                            if (IsBlockCommentDirectiveTerminated())
                                            {
                                                _tokenType = SqlTokenType.ELSE_BLOCK_COMMENT;
                                            }
                                            else
                                            {
                                                if (_charPosition + 1 < _stringLength)
                                                {
                                                    _charPosition++;
                                                    var c8 = _sql[_charPosition];
                                                    if (_charPosition + 1 < _stringLength)
                                                    {
                                                        _charPosition++;
                                                        var c9 = _sql[_charPosition];
                                                        if (c8 == 'i' && c9 == 'f')
                                                        {
                                                            if (IsBlockCommentDirectiveTerminated())
                                                            {
                                                                _tokenType = SqlTokenType.ELSEIF_BLOCK_COMMENT;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _charPosition = _charPosition - 6;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _charPosition = _charPosition - 5;
                                                    }
                                                }
                                            }
                                        }
                                        else if (_charPosition + 1 < _stringLength)
                                        {
                                            _charPosition++;
                                            var c8 = _sql[_charPosition];
                                            if (_charPosition + 1 < _stringLength)
                                            {
                                                _charPosition++;
                                                var c9 = _sql[_charPosition];
                                                if (c4 == 'e' && c5 == 'x'
                                                              && c6 == 'p' && c7 == 'a'
                                                              && c8 == 'n' && c9 == 'd')
                                                {
                                                    if (IsBlockCommentDirectiveTerminated())
                                                    {
                                                        _tokenType = SqlTokenType.EXPAND_BLOCK_COMMENT;
                                                    }
                                                }
                                                else if (_charPosition + 1 < _stringLength)
                                                {
                                                    _charPosition++;
                                                    var c10 = _sql[_charPosition];
                                                    if (_charPosition + 1 < _stringLength)
                                                    {
                                                        _charPosition++;
                                                        var c11 = _sql[_charPosition];
                                                        if (c4 == 'p' && c5 == 'o'
                                                                      && c6 == 'p'
                                                                      && c7 == 'u'
                                                                      && c8 == 'l'
                                                                      && c9 == 'a'
                                                                      && c10 == 't'
                                                                      && c11 == 'e')
                                                        {
                                                            if (IsBlockCommentDirectiveTerminated())
                                                            {
                                                                _tokenType = SqlTokenType.POPULATE_BLOCK_COMMENT;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _charPosition = _charPosition - 8;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _charPosition = _charPosition - 7;
                                                    }
                                                }
                                                else
                                                {
                                                    _charPosition = _charPosition - 6;
                                                }
                                            }
                                            else
                                            {
                                                _charPosition = _charPosition - 5;
                                            }
                                        }
                                        else
                                        {
                                            _charPosition = _charPosition - 4;
                                        }
                                    }
                                    else
                                    {
                                        _charPosition = _charPosition - 3;
                                    }
                                }
                                else
                                {
                                    _charPosition = _charPosition - 2;
                                }
                            }
                            else
                            {
                                _charPosition = _charPosition - 1;
                            }
                        }

                        AssertUnsupportedTokenType();

                        if (_tokenType != SqlTokenType.IF_BLOCK_COMMENT &&
                            _tokenType != SqlTokenType.END_BLOCK_COMMENT &&
                            _tokenType != SqlTokenType.ELSE_BLOCK_COMMENT &&
                            _tokenType != SqlTokenType.ELSEIF_BLOCK_COMMENT)
                        {
                            CalcPosition();
                            throw new SqlParseException(ExceptionMessageId.Esp2119, _sql, LineNumber, Position);
                        }
                    }
                    _charPosition = _charPosition - 1;
                }

                while (HasRemaining())
                {
                    _charPosition++;
                    var c3 = _sql[_charPosition];
                    if (_charPosition + 1 < _stringLength)
                    {
                        _charPosition++;
                        var c4 = _sql[_charPosition];
                        if (c3 == '*' && c4 == '/')
                        {
                            return;
                        }
                        if ((c3 == '\r' && c4 == '\n')
                            || (c3 == '\r' || c3 == '\n'))
                        {
                            _currentLineNumber++;
                        }
                    }

                    _charPosition = _charPosition - 1;
                }
                CalcPosition();
                throw new SqlParseException(ExceptionMessageId.Esp2102, _sql, LineNumber, Position);
            }
            else if (c == '-' && c2 == '-')
            {
                _tokenType = SqlTokenType.LINE_COMMENT;
                while (HasRemaining())
                {
                    _charPosition++;
                    var c3 = _sql[_charPosition];
                    if (c3 == '\r' || c3 == '\n')
                    {
                        _charPosition = _charPosition - 1;
                        return;
                    }
                }
                return;
            }
            else if (c == '\r' && c2 == '\n')
            {
                _tokenType = SqlTokenType.EOL;
                _currentLineNumber++;
                return;
            }
            _charPosition = _charPosition - 1;
            PeekOneChar(c);
        }

        private void PeekOneChar(char c)
        {
            if (SqlTokenHelper.IsWhitespace(c))
            {
                _tokenType = SqlTokenType.WHITESPACE;
            }
            else if (c == '(')
            {
                _tokenType = SqlTokenType.OPENED_PARENS;
            }
            else if (c == ')')
            {
                _tokenType = SqlTokenType.CLOSED_PARENS;
            }
            else if (c == ';')
            {
                _tokenType = SqlTokenType.DELIMITER;
            }
            else if (c == '\'')
            {
                _tokenType = SqlTokenType.QUOTE;
                var closed = false;
                while (HasRemaining())
                {
                    _charPosition++;
                    var c2 = _sql[_charPosition];
                    if (c2 == '\'')
                    {
                        if (_charPosition + 1 < _stringLength)
                        {
                            _charPosition++;
                            var c3 = _sql[_charPosition];
                            if (c3 != '\'')
                            {
                                _charPosition = _charPosition - 1;
                                closed = true;
                                break;
                            }
                        }
                        else
                        {
                            closed = true;
                        }
                    }
                }

                if (closed)
                {
                    return;
                }
                CalcPosition();
                throw new SqlParseException(ExceptionMessageId.Esp2101, _sql, LineNumber, Position);
            }
            else if (IsWordStarted(c))
            {
                _tokenType = SqlTokenType.WORD;
                while (HasRemaining())
                {
                    _charPosition++;
                    var c2 = _sql[_charPosition];
                    if (c2 == '\'')
                    {
                        var closed = false;
                        while (HasRemaining())
                        {
                            _charPosition++;
                            var c3 = _sql[_charPosition];
                            if (c3 == '\'')
                            {
                                if (_charPosition + 2 < _stringLength)
                                {
                                    _charPosition++;
                                    var c4 = _sql[_charPosition + 1];
                                    if (c4 != '\'')
                                    {
                                        _charPosition = _charPosition - 1;
                                        closed = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    closed = true;
                                }
                            }
                        }

                        if (closed)
                        {
                            return;
                        }
                        CalcPosition();
                        throw new SqlParseException(ExceptionMessageId.Esp2101, _sql, LineNumber, Position);
                    }

                    if (!SqlTokenHelper.IsWordPart(c2))
                    {
                        _charPosition = _charPosition - 1;
                        return;
                    }
                }
            }
            else if (c == '\r' || c == '\n')
            {
                _tokenType = SqlTokenType.EOL;
                _currentLineNumber++;
            }
            else
            {
                _tokenType = SqlTokenType.OTHER;
            }
        }

        private bool HasRemaining()
        {
            return (_charPosition + 1 < _stringLength);
        }

        private bool IsWordStarted(char c)
        {
            if (c == '+' || c == '-')
            {
                if (_charPosition + 1 < _stringLength)
                {
                    var next = _sql[_charPosition + 1];
                    if (char.IsDigit(next))
                    {
                        return true;
                    }
                }
            }

            return SqlTokenHelper.IsWordPart(c);
        }

        private bool IsWordEnded()
        {
            if (_charPosition + 1 < _stringLength)
            {
                var next = _sql[_charPosition + 1];
                if (!SqlTokenHelper.IsWordPart(next))
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        private bool IsBlockCommentDirectiveTerminated()
        {
            if (_charPosition + 1 < _stringLength)
            {
                var next = _sql[_charPosition + 1];
                if (!SqlTokenHelper.IsWordPart(next))
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }


        #region Code from org.seasar.doma.internal.expr.util.ExpressionUtil

        private static bool IsExpressionIdentifierStart(char c)
        {
            return (SqlTokenHelper.IsIdentifierStartCharacter(c) || char.IsWhiteSpace(c) ||
                    c == '"' || c == '\'' || c == '@');
        }

        

        #endregion

    }
}
