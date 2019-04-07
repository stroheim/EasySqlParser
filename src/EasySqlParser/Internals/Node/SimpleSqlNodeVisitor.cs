namespace EasySqlParser.Internals.Node
{
    // Porting from DOMA
    //   package    org.seasar.doma.internal.jdbc.sql
    //   class      SimpleSqlNodeVisitor
    // https://github.com/domaframework/doma
    internal class SimpleSqlNodeVisitor<TParameter, TReturn> : ISqlNodeVisitor<TParameter, TReturn>
    {
        protected virtual TReturn DefaultAction(ISqlNode node, TParameter parameter)
        {
            return default;
        }


        public TReturn VisitAnonymousNode(AnonymousNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitBindVariableNode(BindVariableNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitCommentNode(CommentNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitElseifNode(ElseifNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitElseNode(ElseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitEmbeddedVariableNode(EmbeddedVariableNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitEndNode(EndNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitEolNode(EolNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitExpandNode(ExpandNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitForBlockNode(ForBlockNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitForNode(ForNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitForUpdateClauseNode(ForUpdateClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitFragmentNode(FragmentNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitFromClauseNode(FromClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitGroupByClauseNode(GroupByClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitHavingClauseNode(HavingClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitIfBlockNode(IfBlockNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitIfNode(IfNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitLiteralVariableNode(LiteralVariableNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitLogicalOperatorNode(LogicalOperatorNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitOptionClauseNode(OptionClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitOrderByClauseNode(OrderByClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitOtherNode(OtherNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitParensNode(ParensNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitPopulateNode(PopulateNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitSelectClauseNode(SelectClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public virtual TReturn VisitSelectStatementNode(SelectStatementNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitSetClauseNode(SetClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitUpdateClauseNode(UpdateClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitUpdateStatementNode(UpdateStatementNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitWhereClauseNode(WhereClauseNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitWhitespaceNode(WhitespaceNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }

        public TReturn VisitWordNode(WordNode node, TParameter parameter)
        {
            return DefaultAction(node, parameter);
        }
    }
}
