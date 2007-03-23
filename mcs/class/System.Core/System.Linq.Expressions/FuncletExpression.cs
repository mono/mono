using System.Text;

namespace System.Linq.Expressions
{
    public sealed class FuncletExpression : Expression
    {
        #region .ctor
        internal FuncletExpression(Funclet funclet, Type type)
            : base(ExpressionType.Funclet, type)
        {
            this.funclet = funclet;
        }
        #endregion

        #region Fields
        private Funclet funclet;
        #endregion

        #region Properties
        public Funclet Funclet
        {
            get { return funclet; }
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