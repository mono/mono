using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Test
{
	public static class Program
	{
		public static int Main ()
		{
			var m = typeof (Program).GetMethod ("Test");
			var attr = m.GetCustomAttribute<IteratorStateMachineAttribute> ();
			if (attr != null)
				return 1;

			return 0;
		}

		public static IEnumerable<object> Test ()
		{
			yield return null;
		}
	}
}
