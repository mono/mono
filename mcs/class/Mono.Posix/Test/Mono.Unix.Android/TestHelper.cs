using System;
using System.Reflection;

using NUnit.Framework;

namespace Mono.Unix.Android
{
	public class TestHelper
	{
		static bool areRealTimeSignalsSafe;

		static TestHelper ()
		{
#if MONODROID
			var method = typeof (Mono.Unix.Native.NativeConvert).Assembly.GetType ("Mono.Unix.Android.AndroidUtils").GetMethod ("AreRealTimeSignalsSafe", BindingFlags.Public | BindingFlags.Static);
			areRealTimeSignalsSafe = (bool)method.Invoke (null, null);
#else
			areRealTimeSignalsSafe = true;
#endif
		}

		public static bool CanUseRealTimeSignals ()
		{
			if (!areRealTimeSignalsSafe) {
				Assert.Ignore ("Real-time signals aren't supported on this Android architecture");
				return false;
			}

			return true;
		}
	}
}
