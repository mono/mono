namespace System.Linq.Expressions
{
    public class ExecutionScope
    {
        #region .ctor
        internal ExecutionScope(ExecutionScope parent, ExpressionCompiler.LambdaInfo lambda, object[] globals)
        {
            Parent = parent;
            this.lambda = lambda;
            Globals = globals;
            Locals = new object[0];

            //TODO:
        }
        #endregion

        #region Fields
        private ExpressionCompiler.LambdaInfo lambda;

        public object[] Globals;
        public object[] Locals;
        public ExecutionScope Parent;
        #endregion

        #region Public Methods
        public Delegate CreateDelegate(int indexLambda)
        {
            //TODO:
            throw new NotImplementedException();
        }

        public Expression IsolateExpression(Expression expression)
        {
            //TODO:
            throw new NotImplementedException();
        }
        #endregion
    }
}