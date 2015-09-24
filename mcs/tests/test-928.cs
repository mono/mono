// Compiler options: -unsafe

using System;
using System.Reflection;
using System.Linq;

unsafe class Program
{
	public static void Test ()
	{
		string s = "";
		unsafe {
			fixed (char *chars = s) {
			}
		}
	}

	public static int Main ()
	{
		Test ();

		var m = typeof (Program).GetMethod ("Test");
		var lv = m.GetMethodBody ().LocalVariables.Where (l => l.LocalType == typeof (char*)).Single ();
		if (lv.IsPinned)
			return 1;

		return 0;
	}
}
