using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals.Helpers;
using EasySqlParser.Internals.Node;

namespace EasySqlParser.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      SqlParser
    // https://github.com/domaframework/doma
    internal class DomaSqlParser
    {
        private const string LiteralPattern = "[-+'.0-9]|.*'|true|false|null";

        private static readonly Regex LiteralRegex =
            new Regex(LiteralPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Stack<IAddableSqlNode> _nodeStack = new Stack<IAddableSqlNode>();
        private readonly string _sql;
        private readonly SqlTokenizer _tokenizer;
        private readonly AnonymousNode _rootNode;
        private SqlTokenType _tokenType;
        private string _token;

        public DomaSqlParser(string sql)
        {
            _sql = sql;
            _tokenizer = new SqlTokenizer(sql);
            _rootNode = new AnonymousNode();
            _nodeStack.Push(_rootNode);
        }

        public ISqlNode Parse()
        {

            ParseInternal();

            Validate();
            ValidateParensClosed();

            return _rootNode;
        }

        private void ParseInternal()
        {
            var isLoop = true;
            while (isLoop)
            {
                _tokenType = _tokenizer.Next();
                _token = _tokenizer.Token;
                switch (_tokenType)
                {
                    case SqlTokenType.WHITESPACE:
                        ParseWhitespace();
                        break;
                    case SqlTokenType.WORD:
                    case SqlTokenType.QUOTE:
                        ParseWord();
                        break;
                    case SqlTokenType.SELECT_WORD:
                        ParseSelectWord();
                        break;
                    case SqlTokenType.FROM_WORD:
                        ParseFromWord();
                        break;
                    case SqlTokenType.WHERE_WORD:
                        ParseWhereNode();
                        break;
                    case SqlTokenType.GROUP_BY_WORD:
                        ParseGroupByWord();
                        break;
                    case SqlTokenType.HAVING_WORD:
                        ParseHavingWord();
                        break;
                    case SqlTokenType.ORDER_BY_WORD:
                        ParseOrderByWord();
                        break;
                    case SqlTokenType.FOR_UPDATE_WORD:
                        ParseForUpdateWord();
                        break;
                    case SqlTokenType.OPTION_WORD:
                        ParseOptionWord();
                        break;
                    case SqlTokenType.UPDATE_WORD:
                        ParseUpdateWord();
                        break;
                    case SqlTokenType.SET_WORD:
                        ParseSetWord();
                        break;
                    case SqlTokenType.AND_WORD:
                    case SqlTokenType.OR_WORD:
                        ParseLogicalWord();
                        break;
                    case SqlTokenType.OPENED_PARENS:
                        ParseOpenedParens();
                        break;
                    case SqlTokenType.CLOSED_PARENS:
                        ParseClosedParens();
                        break;
                    case SqlTokenType.BIND_VARIABLE_BLOCK_COMMENT:
                        ParseBindVariableBlockComment();
                        break;
                    case SqlTokenType.LITERAL_VARIABLE_BLOCK_COMMENT:
                        ParseLiteralVariableBlockComment();
                        break;
                    case SqlTokenType.EMBEDDED_VARIABLE_BLOCK_COMMENT:
                        ParseEmbeddedVariableBlockComment();
                        break;
                    case SqlTokenType.IF_BLOCK_COMMENT:
                        ParseIfBlockComment();
                        break;
                    case SqlTokenType.ELSEIF_BLOCK_COMMENT:
                        ParseElseIfBlockComment();
                        break;
                    case SqlTokenType.ELSE_BLOCK_COMMENT:
                        ParseElseBlockComment();
                        break;
                    case SqlTokenType.END_BLOCK_COMMENT:
                        ParseEndBlockComment();
                        break;
                    //case SqlTokenType.FOR_BLOCK_COMMENT:
                    //    ParseForBlockComment();
                    //    break;
                    //case SqlTokenType.EXPAND_BLOCK_COMMENT:
                    //    ParseExpandBlockComment();
                    //    break;
                    //case SqlTokenType.POPULATE_BLOCK_COMMENT:
                    //    ParsePopulateBlockComment();
                    //    break;
                    case SqlTokenType.UNION_WORD:
                    case SqlTokenType.EXCEPT_WORD:
                    case SqlTokenType.MINUS_WORD:
                    case SqlTokenType.INTERSECT_WORD:
                        ParseSetOperatorWord();
                        break;
                    case SqlTokenType.BLOCK_COMMENT:
                    case SqlTokenType.LINE_COMMENT:
                        ParseComment();
                        break;
                    case SqlTokenType.OTHER:
                        ParseOther();
                        break;
                    case SqlTokenType.EOL:
                        ParseEol();
                        break;
                    case SqlTokenType.DELIMITER:
                    case SqlTokenType.EOF:
                        isLoop = false;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown SqlTokenType:{_tokenType}");
                }
            }
        }

        private void ParseSetOperatorWord()
        {
            Validate();
            var node = new AnonymousNode();
            node.AddNode(new WordNode(_token, true));
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                Pop();
            }
            AppendNode(node);
            _nodeStack.Push(node);
        }

        private void ParseSelectWord()
        {
            Validate();
            var selectStatementNode = new SelectStatementNode();
            AppendNode(selectStatementNode);
            _nodeStack.Push(selectStatementNode);
            var selectClauseNode = new SelectClauseNode(_token);
            selectStatementNode.SelectClauseNode = selectClauseNode;
            _nodeStack.Push(selectClauseNode);
        }

        private void ParseFromWord()
        {
            Validate();
            var node = new FromClauseNode(_token);
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                var selectStatementNode = (SelectStatementNode)Peek();
                selectStatementNode.FromClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseWhereNode()
        {
            Validate();
            var node = new WhereClauseNode(_token);
            if (IsInWhereClauseAwareNode())
            {
                RemoveNodesTo<IWhereClauseAwareNode>();
                var whereClauseAwareNode = (IWhereClauseAwareNode)Peek();
                whereClauseAwareNode.WhereClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseGroupByWord()
        {
            Validate();
            var node = new GroupByClauseNode(_token);
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                var selectStatementNode = (SelectStatementNode)Peek();
                selectStatementNode.GroupByClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseHavingWord()
        {
            Validate();
            var node = new HavingClauseNode(_token);
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                var selectStatementNode = (SelectStatementNode)Peek();
                selectStatementNode.HavingClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseOrderByWord()
        {
            Validate();
            var node = new OrderByClauseNode(_token);
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                var selectStatementNode = (SelectStatementNode)Peek();
                selectStatementNode.OrderByClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseForUpdateWord()
        {
            Validate();
            var node = new ForUpdateClauseNode(_token);
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                var selectStatementNode = (SelectStatementNode)Peek();
                selectStatementNode.ForUpdateClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseOptionWord()
        {
            Validate();
            var node = new OptionClauseNode(_token);
            if (IsInSelectStatementNode())
            {
                RemoveNodesTo<SelectStatementNode>();
                var selectStatementNode = (SelectStatementNode)Peek();
                selectStatementNode.OptionClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseUpdateWord()
        {
            Validate();
            var updateStatementNode = new UpdateStatementNode();
            AppendNode(updateStatementNode);
            _nodeStack.Push(updateStatementNode);
            var updateClauseNode = new UpdateClauseNode(_token);
            updateStatementNode.UpdateClauseNode = updateClauseNode;
            _nodeStack.Push(updateClauseNode);
        }

        private void ParseSetWord()
        {
            Validate();
            var node = new SetClauseNode(_token);
            if (IsInUpdateStatementNode())
            {
                RemoveNodesTo<UpdateStatementNode>();
                var updateStatementNode = (UpdateStatementNode)Peek();
                updateStatementNode.SetClauseNode = node;
            }
            else
            {
                AppendNode(node);
            }
            _nodeStack.Push(node);
        }

        private void ParseLogicalWord()
        {
            var word = SqlTokenHelper.Extract(_tokenType, _token);
            var node = new LogicalOperatorNode(word);
            AppendNode(node);
            _nodeStack.Push(node);
        }

        private void ParseWord()
        {
            var node = new WordNode(_token);
            AppendNode(node);
        }

        private void ParseComment()
        {
            var node = new CommentNode(_token);
            AppendNode(node);
        }

        private void ParseOpenedParens()
        {
            var parensNode = new ParensNode(Location);
            AppendNode(parensNode);
            _nodeStack.Push(parensNode);
        }

        private void ParseClosedParens()
        {
            if (!IsInParensNode())
            {
                throw new SqlParseException(ExceptionMessageId.Esp2109, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position);
            }
            Validate();
            RemoveNodesTo<ParensNode>();
            var parensNode = (ParensNode)Pop();
            foreach (var child in parensNode.Children)
            {
                if (!(child is WhitespaceNode) && !(child is CommentNode))
                {
                    parensNode.IsEmpty = false;
                    break;
                }
            }
            parensNode.Close();
        }

        private void ParseBindVariableBlockComment()
        {
            var variableName = SqlTokenHelper.Extract(_tokenType, _token);
            if (string.IsNullOrEmpty(variableName))
            {
                throw new SqlParseException(ExceptionMessageId.Esp2120, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position, _token);
            }



            var node = new BindVariableNode(Location, variableName, _token);
            ValidateBindVariableBlockComment(node);
            AppendNode(node);
            _nodeStack.Push(node);
        }

        private void ValidateBindVariableBlockComment(ValueNode node)
        {
            var variableName = node.VariableName;
            var firstChar = variableName[0];
            if (firstChar != '@')
            {
                return;
            }
            var position = variableName.IndexOf('(');
            var functionName = variableName.Substring(1, position - 1);
            if (!ValueNode.BuiltinFunctionNames.Contains(functionName))
            {
                throw new SqlParseException(ExceptionMessageId.Esp2150, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position, functionName);
            }

            node.BuiltinFunction = functionName;

            var argPosition = variableName.LastIndexOf(')');
            var argument = variableName.Substring(position + 1, argPosition - position - 1);
            var args = argument.Split(',').Select(e => e.Trim()).ToList();
            var maxArgCount = 2;
            if (node.BuiltinFunctionName == BuiltinFunctionName.TruncateTime)
            {
                maxArgCount = 1;
            }

            if (args.Count > maxArgCount)
            {
                throw new SqlParseException(ExceptionMessageId.Esp2151, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position, functionName);
            }


            if (args.Count == 2)
            {
                var additionalArg = args[1];
                if (additionalArg.Length > 3)
                {
                    throw new SqlParseException(ExceptionMessageId.Esp2152, _sql, _tokenizer.LineNumber,
                        _tokenizer.Position, additionalArg);
                }
                var c = additionalArg[0];
                var c2 = additionalArg[1];
                var c3 = additionalArg[2];
                if (c != '\'' || c3 != '\'')
                {
                    throw new SqlParseException(ExceptionMessageId.Esp2152, _sql, _tokenizer.LineNumber,
                        _tokenizer.Position, additionalArg);
                }

                node.EscapeChar = c2;
            }

            // update variable name
            node.VariableName = args[0];

        }

        private void ParseLiteralVariableBlockComment()
        {
            var variableName = SqlTokenHelper.Extract(_tokenType, _token);
            if (string.IsNullOrEmpty(variableName))
            {
                throw new SqlParseException(ExceptionMessageId.Esp2228, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position, _token);
            }

            var node = new LiteralVariableNode(Location, variableName, _token);
            ValidateBindVariableBlockComment(node);
            AppendNode(node);
            _nodeStack.Push(node);
        }

        private void ParseEmbeddedVariableBlockComment()
        {
            var variableName = SqlTokenHelper.Extract(_tokenType, _token);
            if (string.IsNullOrEmpty(variableName))
            {
                throw new SqlParseException(ExceptionMessageId.Esp2121, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position, _token);
            }

            var node = new EmbeddedVariableNode(Location, variableName, _token);
            AppendNode(node);
            _nodeStack.Push(node);
        }

        private void ParseIfBlockComment()
        {
            var ifBlockNode = new IfBlockNode();
            AppendNode(ifBlockNode);
            _nodeStack.Push(ifBlockNode);
            var expression = SqlTokenHelper.Extract(_tokenType, _token);
            var ifNode = new IfNode(Location, expression, _token);
            ifBlockNode.IfNode = ifNode;
            _nodeStack.Push(ifNode);
        }

        private void ParseElseIfBlockComment()
        {
            if (!IsInIfBlockNode())
            {
                throw new SqlParseException(ExceptionMessageId.Esp2138, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position);
            }
            RemoveNodesTo<IfBlockNode>();
            var ifBlockNode = (IfBlockNode)Peek();
            if (ifBlockNode.IsElseNodeExists)
            {
                throw new SqlParseException(ExceptionMessageId.Esp2139, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position);
            }

            var expression = SqlTokenHelper.Extract(_tokenType, _token);
            var node = new ElseifNode(Location, expression, _token);
            ifBlockNode.AddElseifNode(node);
            _nodeStack.Push(node);
        }

        private void ParseElseBlockComment()
        {
            if (!IsInIfBlockNode())
            {
                throw new SqlParseException(ExceptionMessageId.Esp2140, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position);
            }
            RemoveNodesTo<IfBlockNode>();
            var ifBlockNode = (IfBlockNode)Peek();
            if (ifBlockNode.IsElseNodeExists)
            {
                throw new SqlParseException(ExceptionMessageId.Esp2141, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position);
            }

            var node = new ElseNode(_token);
            ifBlockNode.ElseNode = node;
            _nodeStack.Push(node);
        }

        private void ParseEndBlockComment()
        {
            if (!IsInBlockNode())
            {
                throw new SqlParseException(ExceptionMessageId.Esp2104, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position);
            }
            RemoveNodesTo<IBlockNode>();
            var blockNode = (IBlockNode)Pop();
            var node = new EndNode(_token);
            blockNode.SetEndNode(node);
            _nodeStack.Push(node);
        }

        private void ParseForBlockComment()
        {
            throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(ForBlockNode));
        }


        private void ParseExpandBlockComment()
        {
            throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(ExpandNode));
        }

        private void ParsePopulateBlockComment()
        {
            throw new UnsupportedSqlCommentException(ExceptionMessageId.EspB001, nameof(PopulateNode));
        }

        private void ParseOther()
        {
            AppendNode(OtherNode.Of(_token));
        }

        private void ParseEol()
        {
            var node = new EolNode(_token);
            AppendNode(node);
        }

        private bool ContainsOnlyWhitespaces(ISqlNode node)
        {
            foreach (var child in _nodeStack)
            {
                if (!(child is WhitespaceNode))
                {
                    return false;
                }
            }

            return true;
        }

        private void ParseWhitespace()
        {
            AppendNode(WhitespaceNode.Of(_token));
        }

        private void RemoveNodesTo<T>()
            where T : ISqlNode
        {
            var newStack = new Stack<IAddableSqlNode>(_nodeStack.Reverse());
            foreach (var node in _nodeStack)
            {
                if (node is T)
                {
                    break;
                }

                newStack.Pop();
            }

            _nodeStack = newStack;
        }

        private bool IsInSelectStatementNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return false;
                }

                if (node is SelectStatementNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInWhereClauseAwareNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return false;
                }

                if (node is IWhereClauseAwareNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInUpdateStatementNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return false;
                }

                if (node is UpdateStatementNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInIfBlockNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return false;
                }

                if (node is IfBlockNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInForBlockNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return false;
                }

                if (node is ForBlockNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInParensNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInBlockNode()
        {
            foreach (var node in _nodeStack)
            {
                if (node is ParensNode)
                {
                    return false;
                }

                if (node is IBlockNode)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAfterValueNode()
        {
            return Peek() is ValueNode;
        }

        private bool IsAfterExpandNode()
        {
            return Peek() is ExpandNode;
        }

        private bool IsAfterOrderByClauseNode()
        {
            return Peek() is OrderByClauseNode;
        }

        private void AppendNode(ISqlNode node)
        {
            if (IsAfterValueNode())
            {
                var valueNode = (ValueNode)Pop();
                if (node is WordNode wordNode)
                {
                    var word = wordNode.Word;
                    var matchResult = LiteralRegex.Match(word);
                    if (matchResult.Success)
                    {
                        valueNode.WordNode = wordNode;
                    }
                    else
                    {
                        throw new SqlParseException(ExceptionMessageId.Esp2142, _sql, _tokenizer.LineNumber,
                            _tokenizer.Position, word);
                    }
                }
                else if (node is ParensNode parensNode)
                {
                    parensNode.IsAttachedWithValue = true;
                    valueNode.ParensNode = parensNode;
                }
                else
                {
                    throw new SqlParseException(ExceptionMessageId.Esp2110, _sql, _tokenizer.LineNumber,
                        _tokenizer.Position, valueNode.Text);
                }
            }
            else if (IsAfterExpandNode())
            {
                var expandNode = (ExpandNode)Pop();
                if (node is OtherNode otherNode)
                {
                    if (otherNode.Other != "*")
                    {
                        throw new SqlParseException(ExceptionMessageId.Esp2143, _sql, _tokenizer.LineNumber,
                            _tokenizer.Position, expandNode.Text);
                    }
                }
                else
                {
                    throw new SqlParseException(ExceptionMessageId.Esp2143, _sql, _tokenizer.LineNumber,
                        _tokenizer.Position, expandNode.Text);
                }
            }
            else
            {
                Peek().AddNode(node);
            }
        }

        private IAddableSqlNode Peek()
        {
            return _nodeStack.Peek();
        }

        private ISqlNode Pop()
        {
            return _nodeStack.Pop();
        }

        private SqlLocation Location => new SqlLocation(_sql, _tokenizer.LineNumber, _tokenizer.Position);


        private void Validate()
        {
            if (IsAfterValueNode())
            {
                var valueNode = (ValueNode)Pop();
                throw new SqlParseException(ExceptionMessageId.Esp2110, _sql, _tokenizer.LineNumber,
                    _tokenizer.Position, valueNode.Text);
            }

            if (IsInIfBlockNode())
            {
                RemoveNodesTo<IfBlockNode>();
                var ifBlockNode = (IfBlockNode)Pop();
                var location = ifBlockNode.IfNode.Location;
                throw new SqlParseException(ExceptionMessageId.Esp2133, _sql, location.LineNumber,
                    location.Position);
            }

            if (IsInForBlockNode())
            {
                RemoveNodesTo<ForBlockNode>();
                var forBlockNode = (ForBlockNode)Pop();
                var location = forBlockNode.ForNode.Location;
                throw new SqlParseException(ExceptionMessageId.Esp2134, _sql, location.LineNumber,
                    location.Position);
            }
        }

        private void ValidateParensClosed()
        {
            if (IsInParensNode())
            {
                RemoveNodesTo<ParensNode>();
                var parensNode = (ParensNode)Pop();
                var location = parensNode.Location;
                throw new SqlParseException(ExceptionMessageId.Esp2135, _sql, location.LineNumber,
                    location.Position);
            }
        }
    }
}
