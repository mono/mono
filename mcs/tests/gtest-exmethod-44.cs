// Compiler options: -warnaserror

using System;
using System.Linq;

namespace UnusedFieldWarningTest2
{
	class Repro
	{
		int[] a = new int[] { 1 };
		
		void Foo ()
		{
			Console.Write (a.FirstOrDefault ());
		}

		public static void Main ()
		{
		}
	}
}