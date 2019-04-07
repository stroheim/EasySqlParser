namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      SqlLocation
    // https://github.com/domaframework/doma
    internal class SqlLocation
    {
        internal string Sql { get; }

        internal int LineNumber { get; }
        internal int Position { get; }

        internal SqlLocation(string sql, int lineNumber, int position)
        {
            Sql = sql;
            LineNumber = lineNumber;
            Position = position;
        }

        public override string ToString()
        {
            return $"{Sql}:{LineNumber}:{Position}";
        }
    }
}
