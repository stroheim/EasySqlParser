namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// The exception that is thrown when evaluate a expression text.
    /// </summary>
    public sealed class ExpressionEvaluateException : EspException
    {
        /// <summary>
        /// A expression string in a sql file.
        /// </summary>
        public string ExpressionText { get; }

        /// <summary>
        /// A invalid operator in a expression string.
        /// </summary>
        public string InvalidOperator { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluateException"/> class.
        /// </summary>
        /// <param name="messageId">exception kind <see cref="ExceptionMessageId"/></param>
        /// <param name="expressionText">the expression text </param>
        internal ExpressionEvaluateException(ExceptionMessageId messageId, string expressionText) :
            base(messageId, expressionText)
        {
            ExpressionText = expressionText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluateException"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="expressionText"></param>
        /// <param name="value"></param>
        internal ExpressionEvaluateException(ExceptionMessageId messageId, string expressionText, string value) :
            base(messageId, expressionText, value)
        {
            ExpressionText = expressionText;
        }


        internal ExpressionEvaluateException(ExceptionMessageId messageId, string expressionText, object value) :
            base(messageId, expressionText, value)
        {
            ExpressionText = expressionText;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluateException"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="expressionText"></param>
        /// <param name="current"></param>
        /// <param name="next"></param>
        internal ExpressionEvaluateException(ExceptionMessageId messageId, string expressionText, char current,
            char next) :
            base(messageId, expressionText, current, next)
        {
            ExpressionText = expressionText;
            if (next == '\0' || char.IsWhiteSpace(next))
            {
                InvalidOperator = $"{current}";
            }
            else
            {
                InvalidOperator = $"{current}{next}";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluateException"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="expressionText"></param>
        /// <param name="current"></param>
        internal ExpressionEvaluateException(ExceptionMessageId messageId, string expressionText, char current) :
            base(messageId, expressionText, current)
        {
            ExpressionText = expressionText;
        }
    }
}
