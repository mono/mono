using System;
using System.Linq.Expressions;

namespace CompilerCrashTest
{
	public static class QueryCompiler
	{
		public static D Compile<D> (Expression<D> query)
		{
			return (D) (object) Compile ((LambdaExpression) query);
		}

		public static Delegate Compile (LambdaExpression query)
		{
			throw new NotImplementedException ();
		}

		public static void Main ()
		{
		}
	}
}
