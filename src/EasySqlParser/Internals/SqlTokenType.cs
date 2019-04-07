// ReSharper disable InconsistentNaming

namespace EasySqlParser.Internals
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      SqlTokenType
    // https://github.com/domaframework/doma
    internal enum SqlTokenType
    {
        QUOTE,

        OPENED_PARENS,

        CLOSED_PARENS,

        LINE_COMMENT,

        BLOCK_COMMENT,

        BIND_VARIABLE_BLOCK_COMMENT,

        LITERAL_VARIABLE_BLOCK_COMMENT,

        EMBEDDED_VARIABLE_BLOCK_COMMENT,

        IF_BLOCK_COMMENT,

        ELSEIF_BLOCK_COMMENT,

        ELSE_BLOCK_COMMENT,

        FOR_BLOCK_COMMENT,

        EXPAND_BLOCK_COMMENT,

        POPULATE_BLOCK_COMMENT,

        END_BLOCK_COMMENT,

        DELIMITER,

        SELECT_WORD,

        WHERE_WORD,

        FROM_WORD,

        GROUP_BY_WORD,

        HAVING_WORD,

        ORDER_BY_WORD,

        FOR_UPDATE_WORD,

        OPTION_WORD,

        AND_WORD,

        OR_WORD,

        UNION_WORD,

        MINUS_WORD,

        EXCEPT_WORD,

        INTERSECT_WORD,

        UPDATE_WORD,

        SET_WORD,

        WORD,

        OTHER,

        WHITESPACE,

        EOL,

        EOF
    }
}
