#if FEATURE_COMPILE_TO_METHODBUILDER

using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions
{
	partial class LambdaExpression
	{
		public void CompileToMethod (MethodBuilder method, DebugInfoGenerator debugInfoGenerator)
		{
			CompileToMethod (method);
		}
	}
}

#endif