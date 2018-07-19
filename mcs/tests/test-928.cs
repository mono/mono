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

	public static bool StringNull (string s)
	{
		unsafe {
			fixed (char *a = s) {
				return a == null;
			}
		}
	}

	public static bool ArrayNull (int[] a)
	{
		unsafe {
			fixed (int *e = a) {
				return e == null;
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

		if (!StringNull (null))
			return 1;

		if (!ArrayNull (null))
			return 2;

		return 0;
	}
}
