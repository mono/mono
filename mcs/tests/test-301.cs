// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

class A
{
	[StructLayout (LayoutKind.Sequential)]
	struct S { int x; }

	public class B
	{
		[StructLayout (LayoutKind.Sequential)]
		struct S { int x; int y; }
		S s;

		public B () {
			string error = "";

			unsafe {
				if (typeof (S *).GetElementType () != typeof (A.B.S))
					error += " composed cast (pointer),";

				if (sizeof (S) != sizeof (A.B.S))
					error += " sizeof,";

				S *p1 = stackalloc S [1];

				if ((*p1).GetType () != typeof (A.B.S))
					error += " local declaration, 'stackalloc' keyword,";

				fixed (S *p2 = &s) {
					if ((*p2).GetType () != typeof (A.B.S))
						error += " class declaration, 'fixed' statement,";
				}
			}

			if (error.Length != 0)
				throw new Exception ("The following couldn't resolve S as A+B+S:" + error);
		}
	}

	public static void Main()
	{
		object o = new A.B();
	}
}
