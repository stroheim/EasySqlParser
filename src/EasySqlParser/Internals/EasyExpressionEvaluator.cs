using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using EasySqlParser.Exceptions;
using EasySqlParser.Internals.Helpers;

namespace EasySqlParser.Internals
{

    /// <summary>
    /// Class for evaluate expression text in sql comment using expression tree.
    /// </summary>
    /// <remarks>
    /// Code based on the ExpressionEvaluator.cs
    /// https://github.com/Giorgi/Math-Expression-Evaluator
    /// </remarks>
    internal class EasyExpressionEvaluator
    {
        private class ExpressionWrapper
        {
            internal Expression Expression { get; set; }
            internal bool IsNullLiteral { get; set; }
            internal bool IsTrueLiteral { get; set; }
            internal bool IsFalseLiteral { get; set; }
            internal bool IsNumericLiteral { get; set; }
            internal bool IsProperty { get; set; }
            internal bool IsStringLiteral { get; set; }

            internal bool IsLiteral
            {
                get
                {
                    if (IsNullLiteral || 
                        IsTrueLiteral || 
                        IsFalseLiteral || 
                        IsNumericLiteral || 
                        IsStringLiteral)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

       

        //private readonly Stack<Expression> _expressionStack = new Stack<Expression>();
        private readonly Stack<ExpressionWrapper> _expressionStack = new Stack<ExpressionWrapper>();
        private readonly Stack<Symbol> _operatorStack = new Stack<Symbol>();
        private Dictionary<string, ValueWrapper> _propertyValues;

        private readonly Dictionary<string, object> _numericLiteralValues = new Dictionary<string, object>();
        //private readonly Dictionary<string, ParameterExpression> _usedParameters = new Dictionary<string, ParameterExpression>();
        private readonly Dictionary<string, ExpressionWrapper> _usedParameters = new Dictionary<string, ExpressionWrapper>();

        private static readonly ConcurrentDictionary<Tuple<string, string>, Lazy<Delegate>> DelegateCache =
            new ConcurrentDictionary<Tuple<string, string>, Lazy<Delegate>>();

        private int _charPosition = -1;
        private int _expressionLength;
        private string _expressionText;
        private readonly bool _useCache;
        private readonly string _sqlFilePath;

        // for unit test
        internal EasyExpressionEvaluator()
        {
            _useCache = false;
        }

        internal EasyExpressionEvaluator(string sqlFilePath)
        {
            _sqlFilePath = sqlFilePath;
            _useCache = true;
        }

        private void Initialize(string expression)
        {
            _expressionText = expression;
            _expressionLength = expression.Length;
            _usedParameters.Clear();
            _expressionStack.Clear();
            _operatorStack.Clear();
            //_numericLiteralValues.Clear();
            _charPosition = -1;
        }

        internal bool Evaluate(string expression, Dictionary<string, ValueWrapper> propertyValues)
        {
            Initialize(expression);
            _propertyValues = propertyValues ?? new Dictionary<string, ValueWrapper>();

            return Evaluate();
        }

        internal static void ClearCache(string sqlFilePath)
        {
            var keys = DelegateCache.Keys;
            var target = keys.Where(tuple => tuple.Item1 == sqlFilePath).ToList();
            foreach (var tuple in target)
            {
                DelegateCache.TryRemove(tuple, out _);
            }
        }

        internal static void ClearCacheAll()
        {
            DelegateCache.Clear();
        }

        private bool Evaluate()
        {
            ProcessCharacters();
            var compiled = Compile();
            return Execute(compiled);
        }

        private static readonly Regex DoubleQuoteRegex = new Regex(@"\"".*\""", RegexOptions.Compiled);
        private bool Execute(Delegate target)
        {

            var values = new List<object>();
            foreach (var usedParameter in _usedParameters)
            {
                var s = usedParameter.Key;
                if (DoubleQuoteRegex.IsMatch(s))
                {
                    string param = s.Substring(1, s.Length - 2);
                    values.Add(param);
                }
                else if (s == "null")
                {
                    values.Add(null);
                }
                else if (s == "true")
                {
                    values.Add(true);
                }
                else if (s == "false")
                {
                    values.Add(false);
                }
                else
                {
                    if (_propertyValues.ContainsKey(s))
                    {
                        values.Add(_propertyValues[s].Value);
                    }
                    else if (_numericLiteralValues.ContainsKey(s))
                    {
                        values.Add(_numericLiteralValues[s]);
                    }
                    else
                    {
                        values.Add(s);
                    }
                }
            }
            var invokeResult= target.DynamicInvoke(values.ToArray());
            if (!(invokeResult is bool))
            {
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA041, _expressionText, invokeResult);
            }

            return (bool)invokeResult;

        }

        private void ProcessCharacters()
        {
            while (true)
            {
                NextChar();
                if (_current == '\0')
                {
                    break;
                }
                if (char.IsLetterOrDigit(_current) || _current == '_' || _current == '"' || _current == '.')
                {
                    _expressionStack.Push(ReadParameter());
                    continue;
                }

                if (TryGetOperation(out var token))
                {
                    var operationSign = OperationSign.GetOperationSign(token);
                    EvaluateWhile(() =>
                                      _operatorStack.Count > 0 &&
                                      _operatorStack.Peek() != Parentheses.Left &&
                                      operationSign.Precedence <=
                                      ((OperationSign) _operatorStack.Peek()).Precedence);
                    _operatorStack.Push(operationSign);
                    if (token == "!")
                    {
                        if (_current == '(' || _next == '(')
                        {
                            ProcessCharacters();
                            continue;
                        }
                        NextChar();
                        _expressionStack.Push(ReadBoolean());
                    }
                    continue;

                }
                if (_current == '(')
                {
                    _operatorStack.Push(Parentheses.Left);
                    continue;
                }

                if (_current == ')')
                {
                    EvaluateWhile(() =>
                                      _operatorStack.Count > 0 &&
                                      _operatorStack.Peek() != Parentheses.Left);
                    _operatorStack.Pop();
                    continue;
                }
                if (char.IsWhiteSpace(_current))
                {
                    continue;
                }

                throw new ExpressionEvaluateException(ExceptionMessageId.EspA001, _expressionText, _current);
            }
            EvaluateWhile(() => _operatorStack.Count > 0);

        }

        private Delegate Compile()
        {
            if (_useCache)
            {
                var key = new Tuple<string, string>(_sqlFilePath, _expressionText);
                //if (DelegateCache.TryGetValue(key, out var value))
                //{
                //    return value;
                //}
                // https://ufcpp.net/blog/2016/12/tipsconcurrentcollections/
                // valueFactory is possible multiple call
                var lazy = DelegateCache.GetOrAdd(key, new Lazy<Delegate>(CreateDelegate));
                return lazy.Value;
            }

            return CreateDelegate();
        }

        private Delegate CreateDelegate()
        {
            //var paramList = new List<ParameterExpression>();
            //paramList.AddRange(_usedParameters.Values);
            var paramList = new List<ParameterExpression>();
            var values = _usedParameters.Values;
            foreach (var wrapper in values)
            {
                paramList.Add((ParameterExpression)wrapper.Expression);
            }
            var lambda = Expression.Lambda(_expressionStack.Pop().Expression, paramList);
            Debug.WriteLine($"+++> {lambda}");
            var compiled = lambda.Compile();
            return compiled;
        }

        private char _current;
        private char _next;
        private void NextChar()
        {
            if (_charPosition < _expressionLength)
            {
                _charPosition++;
            }

            if (_charPosition < _expressionLength)
            {
                _current = _expressionText[_charPosition];
                _next = _charPosition + 1 < _expressionLength ?
                    _expressionText[_charPosition + 1] : '\0';
            }
            else
            {
                _current = '\0';
                _next = '\0';
            }
        }

        private void EvaluateWhile(Func<bool> condition)
        {
            while (condition())
            {
                var symbol = _operatorStack.Pop();
                if (symbol is Parentheses)
                {
                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA002, _expressionText);
                }
                var operation = (OperationSign)symbol;

                var expressions = new Expression[operation.NumberOfOperands];
                var wrappers = new ExpressionWrapper[operation.NumberOfOperands];

                for (var i = operation.NumberOfOperands - 1; i >= 0; i--)
                {
                    //expressions[i] = _expressionStack.Pop();
                    wrappers[i] = _expressionStack.Pop();
                    expressions[i] = wrappers[i].Expression;
                }

                if (operation.NumberOfOperands == 2 && wrappers[0].IsLiteral && !wrappers[1].IsLiteral)
                {
                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA003, _expressionText);
                }

                var wrapper = new ExpressionWrapper
                              {
                                  Expression = operation.Apply(expressions)
                              };

                //_expressionStack.Push(operation.Apply(expressions));
                _expressionStack.Push(wrapper);

            }
        }

        private ExpressionWrapper ReadBoolean()
        {
            var parameter = "";
            while (true)
            {
                if (_current == '\0')
                {
                    break;
                }

                if (char.IsLetterOrDigit(_current) || _current == '_' || _current == '"')
                {
                    parameter += _current;
                    if (IsCharBreak())
                    {
                        break;
                    }
                    NextChar();
                }
                else
                {
                    break;
                }


            }

            if (parameter == "true" || parameter == "false")
            {
                return CreateBooleanExpression(parameter);
            }

            if (_propertyValues.ContainsKey(parameter))
            {
                return CreateParameterExpression(parameter);
            }

            throw new ExpressionEvaluateException(ExceptionMessageId.EspA011, _expressionText, parameter);

        }

        private bool IsCharBreak()
        {
            if (_next == ')' ||
                _next == '(' ||
                _next == '&' ||
                _next == '|' ||
                _next == '>' ||
                _next == '<' ||
                _next == '!' ||
                _next == '=' ||
                char.IsWhiteSpace(_next))
            {
                return true;
            }

            return false;
        }

        private ExpressionWrapper ReadProperty(Expression instance, Type type)
        {
            var propertyName = "";
            while (true)
            {
                if (_current == '\0')
                {
                    break;
                }



                if (char.IsLetterOrDigit(_current) || _current == '_')
                {
                    propertyName += _current;
                    if (IsCharBreak())
                    {
                        break;
                    }
                    NextChar();
                }
                else
                {
                    break;
                }

            }

            //if (type == null)
            //{
            //    return instance;
            //    //return Expression.Property(instance, propertyName);
            //}

            if (char.IsDigit(propertyName[0]))
            {
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA012, _expressionText, propertyName);
            }

            var propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
            {
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA013, _expressionText, propertyName);
            }

            var wrapper = new ExpressionWrapper
                          {
                              Expression = Expression.Property(instance, propInfo),
                              IsProperty = true
                          };
            return wrapper;
            //return Expression.Property(instance, propInfo);
        }

        private ExpressionWrapper CreateParameterExpression(string parameter)
        {
            if (_usedParameters.ContainsKey(parameter))
            {
                return _usedParameters[parameter];
            }

            var param = _propertyValues[parameter];
            //if (param.Value == null)
            //{
            //    instance = Expression.Parameter(Expression.Constant(null).Type, parameter);
            //}
            //else
            //{
            //    var paramType = param.Type;
            //    instance = Expression.Parameter(paramType, parameter);
            //}

            var paramType = param.Type;
            var instance = Expression.Parameter(paramType, parameter);
            var wrapper = new ExpressionWrapper
                          {
                              Expression = instance
                          };
            _usedParameters.Add(parameter, wrapper);
            return wrapper;
        }


        private ExpressionWrapper CreateNumericExpression(string parameter)
        {
            if (_maybeNumericPattern.IsMatch(parameter))
            {
                var lastChar = parameter.Last();
                if (char.IsDigit(lastChar))
                {
                    // int
                    if (int.TryParse(parameter, out int intResult))
                    {
                        return CreateNumericExpression(intResult, parameter, typeof(int));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA031, _expressionText, parameter);
                }
                else if (lastChar == 'L')
                {
                    // long
                    var longValue = parameter.Substring(0, parameter.Length - 1);
                    if (long.TryParse(longValue, out long longResult))
                    {
                        return CreateNumericExpression(longResult, parameter, typeof(long));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA032, _expressionText, parameter);
                }
                else if (lastChar == 'F')
                {
                    // float
                    var floatValue = parameter.Substring(0, parameter.Length - 1);
                    if (float.TryParse(floatValue, out float floatResult))
                    {
                        return CreateNumericExpression(floatResult, parameter, typeof(float));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA033, _expressionText, parameter);
                }
                else if (lastChar == 'D')
                {
                    // double
                    var doubleValue = parameter.Substring(0, parameter.Length - 1);
                    if (double.TryParse(doubleValue, out double doubleResult))
                    {
                        return CreateNumericExpression(doubleResult, parameter, typeof(double));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA034, _expressionText, parameter);
                }
                else if (lastChar == 'M')
                {
                    // decimal
                    var decimalValue = parameter.Substring(0, parameter.Length - 1);
                    if (decimal.TryParse(decimalValue, out decimal decimalResult))
                    {
                        return CreateNumericExpression(decimalResult, parameter, typeof(decimal));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA035, _expressionText, parameter);
                }
                else if (lastChar == 'U')
                {
                    // uint
                    var uintValue = parameter.Substring(0, parameter.Length - 1);
                    if (uint.TryParse(uintValue, out uint uintResult))
                    {
                        return CreateNumericExpression(uintResult, parameter, typeof(uint));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA036, _expressionText, parameter);
                }
            }

            if (_maybeNumericWithScalePattern.IsMatch(parameter))
            {
                var lastChar = parameter.Last();
                if (lastChar == 'F')
                {
                    // float
                    var floatValue = parameter.Substring(0, parameter.Length - 1);
                    if (float.TryParse(floatValue, out float floatResult))
                    {
                        return CreateNumericExpression(floatResult, parameter, typeof(float));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA033, _expressionText, parameter);
                }
                else if (lastChar == 'D')
                {
                    // double
                    var doubleValue = parameter.Substring(0, parameter.Length - 1);
                    if (double.TryParse(doubleValue, out double doubleResult))
                    {
                        return CreateNumericExpression(doubleResult, parameter, typeof(double));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA034, _expressionText, parameter);
                }
                else if (lastChar == 'M')
                {
                    // decimal
                    var decimalValue = parameter.Substring(0, parameter.Length - 1);
                    if (decimal.TryParse(decimalValue, out decimal decimalResult))
                    {
                        return CreateNumericExpression(decimalResult, parameter, typeof(decimal));
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA035, _expressionText, parameter);
                }
            }

            if (_ulongPattern.IsMatch(parameter))
            {
                var ulongValue = parameter.Substring(0, parameter.Length - 2);
                if (ulong.TryParse(ulongValue, out ulong ulongResult))
                {
                    return CreateNumericExpression(ulongResult, parameter, typeof(ulong));
                }

                throw new ExpressionEvaluateException(ExceptionMessageId.EspA037, _expressionText, parameter);
            }

            throw new ExpressionEvaluateException(ExceptionMessageId.EspA038, _expressionText, parameter);
        }

        private ExpressionWrapper CreateNumericExpression(object numericValue, string parameter, Type dataType)
        {
            if (_usedParameters.ContainsKey(parameter))
            {
                return _usedParameters[parameter];
            }

            if (!_numericLiteralValues.ContainsKey(parameter))
            {
                _numericLiteralValues.Add(parameter, numericValue);
            }

            var wrapper = new ExpressionWrapper
                          {

                              Expression = Expression.Parameter(Expression.Constant(numericValue).Type, parameter),
                              IsNumericLiteral = true
                          };
            _usedParameters.Add(parameter, wrapper);
            return wrapper;
        }

        private ExpressionWrapper CreateNullExpression(string parameter)
        {
            if (_usedParameters.ContainsKey(parameter))
            {
                return _usedParameters[parameter];
            }

            ParameterExpression instance;
            // _usedParameters can't use
            // because expression text contains different properties
            // e.g)
            //  BirthDateFrom != null && BirthDateTo != null
            //if (_usedParameters.Count == 0 || !_usedParameters.Any(e => e.Value.IsNullLiteral))
            if (_usedParameters.Count == 0)
            {
                instance = Expression.Parameter(Expression.Constant(null).Type, parameter);
            }
            else
            {
                // Operations on null need to match types
                instance = Expression.Parameter(_usedParameters.Last().Value.Expression.Type, parameter);
                //var exp = _usedParameters.Single(e => e.Value.IsNullLiteral);
                //instance = Expression.Parameter(exp.Value.Expression.Type, parameter);
            }

            var wrapper = new ExpressionWrapper
                          {
                              Expression = instance,
                              IsNullLiteral = true
                          };
            _usedParameters.Add(parameter, wrapper);
            return wrapper;
        }

        private ExpressionWrapper CreateBooleanExpression(string parameter)
        {
            if (_usedParameters.ContainsKey(parameter))
            {
                return _usedParameters[parameter];
            }

            var value = parameter == "true";
            var instance = Expression.Parameter(Expression.Constant(value).Type, parameter);
            var wrapper = new ExpressionWrapper
                          {
                              Expression = instance,
                              IsTrueLiteral = value,
                              IsFalseLiteral = !value
                          };
            _usedParameters.Add(parameter, wrapper);
            return wrapper;
        }

        private ExpressionWrapper CreateEmptyStringExpression(string parameter)
        {
            if (_usedParameters.ContainsKey(parameter))
            {
                return _usedParameters[parameter];
            }
            var instance = Expression.Parameter(Expression.Constant(parameter).Type, "empty");
            var wrapper = new ExpressionWrapper
                          {
                              Expression = instance,
                              IsStringLiteral = true
                          };
            _usedParameters.Add(parameter, wrapper);
            return wrapper;
        }

        private ExpressionWrapper CreateLiteralStringExpression(string parameter)
        {
            if (_usedParameters.ContainsKey(parameter))
            {
                return _usedParameters[parameter];
            }
            var instance = Expression.Parameter(Expression.Constant(parameter).Type, parameter);
            var wrapper = new ExpressionWrapper
                          {
                              Expression = instance,
                              IsStringLiteral = true
                          };
            _usedParameters.Add(parameter, wrapper);
            return wrapper;
        }

        private static readonly Regex _maybeNumericPattern = new Regex("^[0-9]+.?$", RegexOptions.Compiled);
        private static readonly Regex _maybeNumericWithScalePattern = new Regex("^[0-9]+\\.[0-9]+.$", RegexOptions.Compiled);
        private static readonly Regex _ulongPattern = new Regex("^[0-9]+UL$", RegexOptions.Compiled);

        private ExpressionWrapper ReadParameter()
        {
            var parameter = "";
            var doubleQuotationCount = 0;
            while (true)
            {
                if (_current == '\0')
                {
                    break;
                }


                if (char.IsLetterOrDigit(_current) || _current == '_' || _current == '"')
                {
                    if (_current == '"')
                    {
                        doubleQuotationCount++;
                    }
                    parameter += _current;
                    if (IsCharBreak())
                    {
                        break;
                    }
                    NextChar();
                }
                else if (_current == '.' && SqlTokenHelper.IsIdentifierStartCharacter(parameter[0]))
                {
                    NextChar();
                    if (_propertyValues.ContainsKey(parameter))
                    {
                        var instance = CreateParameterExpression(parameter);
                        var param = _propertyValues[parameter];
                        var paramType = param.Type;
                        var prop = ReadProperty(instance.Expression, paramType);
                        return prop;
                    }

                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA011, _expressionText, parameter);
                }
                else
                {
                    break;
                }
            }

            if (doubleQuotationCount == 1 || doubleQuotationCount > 2)
            {
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA014, _expressionText, parameter);
            }

            if (char.IsDigit(parameter[0]))
            {
                return CreateNumericExpression(parameter);
            }

            if (parameter == "null")
            {
                return CreateNullExpression(parameter);
            }

            if (parameter == "true" || parameter == "false")
            {
                return CreateBooleanExpression(parameter);
            }

            if (parameter == "\"\"")
            {
                return CreateEmptyStringExpression(parameter);
            }

            if (_propertyValues.ContainsKey(parameter))
            {
                return CreateParameterExpression(parameter);
            }

            if (DoubleQuoteRegex.IsMatch(parameter))
            {
                return CreateLiteralStringExpression(parameter);
            }
            throw new ExpressionEvaluateException(ExceptionMessageId.EspA011, _expressionText, parameter);
        }

        private static readonly List<char> InvalidCharacters = new List<char>(
            new[] { '-', '^', '\\', '@', '[', ':', ']', '/', '~', '`', '{', '*', '}', '?', ',', '+', ';', '#', '$', '%' }
        );

        private bool TryGetOperation(out string token)
        {
            if (_current == '!')
            {
                if (_next == '=')
                {
                    NextChar();
                    token = "!=";
                    return true;
                }

                if (_next == ')' || InvalidCharacters.Contains(_next) || _next == '\0')
                {
                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
                }
                token = "!";
                return true;
            }
            if (_current == '=')
            {
                if (_next == '=')
                {
                    NextChar();
                    token = "==";
                    return true;
                }
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
            }
            if (_current == '<')
            {
                if (_next == '>')
                {
                    NextChar();
                    token = "<>";
                    return true;
                }
                if (_next == '=')
                {
                    NextChar();
                    token = "<=";
                    return true;
                }

                if (_next == '(' || _next == ')' || InvalidCharacters.Contains(_next) || _next == '\0')
                {
                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
                }
                token = "<";
                return true;
            }
            if (_current == '>')
            {
                if (_next == '=')
                {
                    NextChar();
                    token = ">=";
                    return true;
                }
                if (_next == '(' || _next == ')' || InvalidCharacters.Contains(_next) || _next == '\0')
                {
                    throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
                }
                token = ">";
                return true;
            }
            if (_current == '&')
            {
                if (_next == '&')
                {
                    NextChar();
                    token = "&&";
                    return true;
                }
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
            }
            if (_current == '|')
            {
                if (_next == '|')
                {
                    NextChar();
                    token = "||";
                    return true;
                }
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
            }

            if (InvalidCharacters.Contains(_current))
            {
                throw new ExpressionEvaluateException(ExceptionMessageId.EspA021, _expressionText, _current, _next);
            }
            token = "";
            return false;

        }
    }

    internal class OperationSign : Symbol
    {
        private readonly Func<Expression, Expression, Expression> _operation;
        private readonly Func<Expression, Expression> _unaryOperation;

        public static readonly OperationSign AndAlsoSign =
            new OperationSign(1, Expression.AndAlso, "AndAlsoSign");

        public static readonly OperationSign OrElseSign =
            new OperationSign(1, Expression.OrElse, "OrElseSign");

        public static readonly OperationSign EqualSign =
            new OperationSign(2, Expression.Equal, "EqualSign");

        public static readonly OperationSign NotEqualSign =
            new OperationSign(2, Expression.NotEqual, "NotEqualSign");

        public static readonly OperationSign GreaterThanSign =
            new OperationSign(2, Expression.GreaterThan, "GreaterThanSign");

        public static readonly OperationSign GreaterThanEqualSign =
            new OperationSign(2, Expression.GreaterThanOrEqual, "GreaterThanEqualSign");

        public static readonly OperationSign LessThanSign =
            new OperationSign(2, Expression.LessThan, "LessThanSign");

        public static readonly OperationSign LessThanEqualSign =
            new OperationSign(2, Expression.LessThanOrEqual, "LessThanEqualSign");

        public static readonly OperationSign NotSign =
            new OperationSign(2, Expression.Not, "NotSign");

        private static readonly Dictionary<string, OperationSign> Operations =
            new Dictionary<string, OperationSign>
            {
                {"&&", AndAlsoSign},
                {"||", OrElseSign},
                {"==", EqualSign},
                {"!=", NotEqualSign},
                {"<>", NotEqualSign},
                {">", GreaterThanSign},
                {">=", GreaterThanEqualSign},
                {"<", LessThanSign},
                {"<=", LessThanEqualSign},
                {"!" ,NotSign}
            };

        private OperationSign(int precedence, string name)
        {
            Precedence = precedence;

            Name = name;
        }

        private OperationSign(int precedence, Func<Expression, Expression> unaryOperation, string name)
            : this(precedence, name)
        {
            _unaryOperation = unaryOperation;
            NumberOfOperands = 1;
        }

        private OperationSign(int precedence, Func<Expression, Expression, Expression> operation, string name)
            : this(precedence, name)
        {
            _operation = operation;
            NumberOfOperands = 2;

        }

        internal string Name { get; }

        internal int Precedence { get; }

        internal int NumberOfOperands { get; }

        public static explicit operator OperationSign(string sign)
        {
            if (Operations.TryGetValue(sign, out var result))
            {
                return result;
            }
            throw new InvalidCastException("Can't get operation");
        }

        private Expression Apply(Expression expression)
        {
            return _unaryOperation(expression);
        }

        private Expression Apply(Expression left, Expression right)
        {

            return _operation(left, right);
        }


        internal static OperationSign GetOperationSign(string sign)
        {
            return Operations[sign];
        }

        internal Expression Apply(params Expression[] expressions)
        {
            if (expressions.Length == 1)
            {
                return Apply(expressions[0]);
            }

            if (expressions.Length == 2)
            {
                return Apply(expressions[0], expressions[1]);
            }
            throw new NotImplementedException();
        }
    }

    internal class Parentheses : Symbol
    {


        public static readonly Parentheses Left = new Parentheses();
        public static readonly Parentheses Right = new Parentheses();

        private Parentheses()
        {

        }
    }

    internal class Symbol
    {
    }
}
