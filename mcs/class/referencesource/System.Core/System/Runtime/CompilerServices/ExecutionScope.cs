using System;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices
{
    // Kept for backwards compatibility. Unused
    [Obsolete("do not use this type", true)]
    public class ExecutionScope
    {
        public ExecutionScope Parent;
        public object[] Globals;
        public object[] Locals;

        internal ExecutionScope()
        {
            Parent = null;
            Globals = null;
            Locals = null;
        }

        public object[] CreateHoistedLocals()
        {
            throw new NotSupportedException();
        }

        public Delegate CreateDelegate(int indexLambda, object[] locals)
        {
            throw new NotSupportedException();
        }

        public Expression IsolateExpression(Expression expression, object[] locals)
        {
            throw new NotSupportedException();
        }
    }
}
