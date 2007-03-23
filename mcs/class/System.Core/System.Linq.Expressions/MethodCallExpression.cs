using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public sealed class MethodCallExpression : Expression
    {
        #region .ctor
        internal MethodCallExpression(ExpressionType type, MethodInfo method, Expression obj, ReadOnlyCollection<Expression> arguments)
            : base(type, method.ReturnType)
        {
            this.obj = obj;
            this.method = method;
            this.arguments = arguments;
        }
        #endregion

        #region Fields
        private ReadOnlyCollection<Expression> arguments;
        private MethodInfo method;
        private Expression obj;
        #endregion

        #region Properties
        public ReadOnlyCollection<Expression> Arguments
        {
            get { return arguments; }
        }

        public MethodInfo Method
        {
            get { return method; }
        }

        public Expression Object
        {
            get { return obj; }
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