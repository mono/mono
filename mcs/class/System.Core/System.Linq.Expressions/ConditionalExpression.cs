using System.Text;

namespace System.Linq.Expressions
{
    public sealed class ConditionalExpression : Expression
    {
        #region .ctor
        internal ConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse, Type type)
            : base(ExpressionType.Conditional, type)
        {
            this.test = test;
            this.ifTrue = ifTrue;
            this.ifFalse = ifFalse;
        }
        #endregion

        #region Fields
        private Expression ifFalse;
        private Expression ifTrue;
        private Expression test;
        #endregion

        #region Properties
        public Expression IfFalse
        {
            get { return ifFalse; }
        }

        public Expression IfTrue
        {
            get { return ifTrue; }
        }

        public Expression Test
        {
            get { return test; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            //TODO:
        }
        #endregion
    }
}