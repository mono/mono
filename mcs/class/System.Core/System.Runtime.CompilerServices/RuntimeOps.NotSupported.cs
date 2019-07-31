using System;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices {

	public partial class RuntimeOps {
		
		[Obsolete ("do not use this method")]
		public static IRuntimeVariables MergeRuntimeVariables (IRuntimeVariables first, IRuntimeVariables second, int[] indexes) => throw new PlatformNotSupportedException ();

		[Obsolete ("do not use this method")]
		public static Expression Quote (Expression expression, object hoistedLocals, object[] locals) => throw new PlatformNotSupportedException ();
	}
}
