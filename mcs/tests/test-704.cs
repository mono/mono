// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

namespace NObjective
{
	public class Program
	{
		static volatile bool ProcessExiting = false;

		[DllImport ("libc.dylib")]
		public extern static void printf (string format, __arglist);

		private static void ArglistMethod (__arglist)
		{
			var iter = new ArgIterator (__arglist);

			for (int n = iter.GetRemainingCount (); n > 0; n--)
				Console.WriteLine (TypedReference.ToObject (iter.GetNextArg ()));
		}

		unsafe public static void Main (string[] args)
		{
			ArglistMethod (__arglist (1, 2, 3));
		}
	}
}
