// Compiler options: test-657-p2.cs -out:test-657.exe

using System;
using System.Diagnostics;

namespace TestDebug
{
	class Program
	{
		[Conditional ("DEBUG")]
		public static void Excluded ()
		{
			throw new ApplicationException ("1");
		}

		public static int Main ()
		{
			C.Method (); // Only checks that DEBUG is defined in second file
			
			Excluded ();
#if DEBUG
			throw new ApplicationException ("1");
#endif
			return 0;
		}
	}
}
