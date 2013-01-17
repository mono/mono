// Compiler options: /unsafe
using System;
using System.Collections.Generic;

unsafe class Test
{
	public static void Main ()
	{
		foreach (int item in GetItems ()) {
			Console.WriteLine (item);
		}
	}

	public static unsafe int GetItem ()
	{
		byte[] value = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

		fixed (byte* valueptr = value) {
			return *(int*) valueptr;
		}
	}

	public static IEnumerable<int> GetItems ()
	{
		yield return GetItem ();
	}
}
