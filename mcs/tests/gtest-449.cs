// Compiler options: -platform:anycpu

using System;
using System.Reflection;

class Program {

	public static int Main ()
	{
		PortableExecutableKinds pekind;
		ImageFileMachine machine;

		typeof (Program).Module.GetPEKind (out pekind, out machine);

		if ((pekind & PortableExecutableKinds.ILOnly) == 0)
			return 1;

		if ((pekind & PortableExecutableKinds.Required32Bit) != 0)
			return 2;

		if (machine != ImageFileMachine.I386)
			return 3;

		return 0;
	}
}
