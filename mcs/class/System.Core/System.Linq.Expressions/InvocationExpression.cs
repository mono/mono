using System.Text;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    public sealed class InvocationExpression : Expression
    {
        #region .ctor
        internal InvocationExpression(Expression lambda, Type returnType, ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Invoke, returnType)
        {
            this.lambda = lambda;
            this.arguments = arguments;
        }
        #endregion

        #region Fields
        private Expression lambda;
        private ReadOnlyCollection<Expression> arguments;
        #endregion

        #region Properties
        public ReadOnlyCollection<Expression> Arguments
        {
            get { return arguments; }
        }

        public Expression Lambda
        {
            get { return lambda; }
        }
        #endregion

        #region Internal Methods
        internal override void BuildString(StringBuilder builder)
        {
            builder.Append("invoke(");

            // build the lamba expression first
            lambda.BuildString(builder);

            int argc = arguments.Count;
            for (int i = 0; i < argc; i++)
            {
                arguments[i].BuildString(builder);
                if (i < argc - 1)
                    builder.Append(", ");
            }

            builder.Append(")");
        }
        #endregion
    }
}