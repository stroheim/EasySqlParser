using EasySqlParser.Exceptions;

namespace EasySqlParser.SqlGenerator
{
    /// <summary>
    ///     The exception that is thrown when primary key not found in entity.
    /// </summary>
    public class PrimaryKeyNotFoundException : EspException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryKeyNotFoundException"/> class.
        /// </summary>
        /// <param name="tableName"></param>
        public PrimaryKeyNotFoundException(string tableName) 
            : base(ExceptionMessageId.EspE001, tableName)
        {
        }
    }
}
