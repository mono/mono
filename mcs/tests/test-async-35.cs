using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq;

namespace N.M
{
	class C
	{
		public static async Task<int> AsyncMethod ()
		{
			await Task.Delay (1);
			return 0;
		}

		public static int Main ()
		{
			var m = typeof (C).GetMethod ("AsyncMethod");
			var attr = m.GetCustomAttribute<AsyncStateMachineAttribute> ();
			if (attr == null)
				return 1;

			if (attr.StateMachineType == null)
				return 2;

			Func<Task<int>> a = async () => await AsyncMethod ();

			var c = typeof (C).GetMethods (BindingFlags.NonPublic | BindingFlags.Static).Where (l =>
				l.IsDefined (typeof (AsyncStateMachineAttribute))).Count ();

			if (c != 1)
				return 3;

			return 0;
		}
	}
}
