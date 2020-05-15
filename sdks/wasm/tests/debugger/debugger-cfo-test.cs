using System;

namespace DebuggerTests
{
	public class CallFunctionOnTest {
		public static void LocalsTest (int len)
		{
			var big = new int[len];
			for (int i = 0; i < len; i ++)
				big [i] = i + 1000;

			var simple_struct = new Math.SimpleStruct () { dt = new DateTime (2020, 1, 2, 3, 4, 5), gs = new Math.GenericStruct<DateTime> { StringField = $"simple_struct # gs # StringField" } };

			var ss_arr = new Math.SimpleStruct [len];
			for (int i = 0; i < len; i ++)
				ss_arr [i] = new Math.SimpleStruct () { dt = new DateTime (2020+i, 1, 2, 3, 4, 5), gs = new Math.GenericStruct<DateTime> { StringField = $"ss_arr # {i} # gs # StringField" } };

			var nim = new Math.NestedInMath { SimpleStructProperty = new Math.SimpleStruct () { dt = new DateTime (2010, 6, 7, 8, 9, 10) } };
			Action<Math.GenericStruct<int[]>> action = Math.DelegateTargetWithVoidReturn;
			Console.WriteLine("foo");
		}
	}
}
