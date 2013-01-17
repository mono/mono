using System;

namespace GenericTest
{
	public class OuterGeneric<T>
	{
		public class InnerGeneric<U>
		{
			public static string GetTypeNames ()
			{
				return typeof (T).ToString () + " " + typeof (U).ToString ();
			}
		}
	}

	class Program
	{
		public static int Main ()
		{
			string typeNames = OuterGeneric<int>.InnerGeneric<long>.GetTypeNames ();
			Console.WriteLine (typeNames);
			return 0;
		}
	}
}
