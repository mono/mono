using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {

	public partial class LambdaExpression {
		
		public void CompileToMethod (MethodBuilder method) => throw new PlatformNotSupportedException ();

		public void CompileToMethod (MethodBuilder method, DebugInfoGenerator debugInfoGenerator) => throw new PlatformNotSupportedException ();
	}
}
