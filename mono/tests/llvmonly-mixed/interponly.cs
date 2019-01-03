using System;
using System.Runtime.CompilerServices;

//
// This assembly is not AOT-ed, so all calls into it transition to the interpreter
//

public class InterpOnly
{
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int entry_1 (int i) {
		return i + 1;
	}
}
