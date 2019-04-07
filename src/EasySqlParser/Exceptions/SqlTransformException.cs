namespace EasySqlParser.Exceptions
{
    /// <summary>
    /// The exception that is thrown when convert sql
    /// </summary>
    public class SqlTransformException : EspException
    {


        internal SqlTransformException(ExceptionMessageId messageId) :
            base(messageId)
        {
        }
    }
}
