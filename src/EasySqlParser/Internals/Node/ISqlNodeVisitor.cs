namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql.node
    //   class      SqlNodeVisitor
    // https://github.com/domaframework/doma
    internal interface ISqlNodeVisitor<in TParameter, out TResult>
    {
        TResult VisitAnonymousNode(AnonymousNode node, TParameter parameter);

        TResult VisitBindVariableNode(BindVariableNode node, TParameter parameter);

        TResult VisitCommentNode(CommentNode node, TParameter parameter);

        TResult VisitElseifNode(ElseifNode node, TParameter parameter);

        TResult VisitElseNode(ElseNode node, TParameter parameter);

        TResult VisitEmbeddedVariableNode(EmbeddedVariableNode node, TParameter parameter);

        TResult VisitEndNode(EndNode node, TParameter parameter);

        TResult VisitEolNode(EolNode node, TParameter parameter);

        TResult VisitExpandNode(ExpandNode node, TParameter parameter);

        TResult VisitForBlockNode(ForBlockNode node, TParameter parameter);

        TResult VisitForNode(ForNode node, TParameter parameter);

        TResult VisitForUpdateClauseNode(ForUpdateClauseNode node, TParameter parameter);

        TResult VisitFragmentNode(FragmentNode node, TParameter parameter);

        TResult VisitFromClauseNode(FromClauseNode node, TParameter parameter);

        TResult VisitGroupByClauseNode(GroupByClauseNode node, TParameter parameter);

        TResult VisitHavingClauseNode(HavingClauseNode node, TParameter parameter);

        TResult VisitIfBlockNode(IfBlockNode node, TParameter parameter);

        TResult VisitIfNode(IfNode node, TParameter parameter);

        TResult VisitLiteralVariableNode(LiteralVariableNode node, TParameter parameter);

        TResult VisitLogicalOperatorNode(LogicalOperatorNode node, TParameter parameter);

        TResult VisitOptionClauseNode(OptionClauseNode node, TParameter parameter);

        TResult VisitOrderByClauseNode(OrderByClauseNode node, TParameter parameter);

        TResult VisitOtherNode(OtherNode node, TParameter parameter);

        TResult VisitParensNode(ParensNode node, TParameter parameter);

        TResult VisitPopulateNode(PopulateNode node, TParameter parameter);

        TResult VisitSelectClauseNode(SelectClauseNode node, TParameter parameter);

        TResult VisitSelectStatementNode(SelectStatementNode node, TParameter parameter);

        TResult VisitSetClauseNode(SetClauseNode node, TParameter parameter);

        TResult VisitUpdateClauseNode(UpdateClauseNode node, TParameter parameter);

        TResult VisitUpdateStatementNode(UpdateStatementNode node, TParameter parameter);

        TResult VisitWhereClauseNode(WhereClauseNode node, TParameter parameter);

        TResult VisitWhitespaceNode(WhitespaceNode node, TParameter parameter);

        TResult VisitWordNode(WordNode node, TParameter parameter);

    }
}
