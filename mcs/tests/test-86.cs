using System;

namespace T {
	public class T {
		
		static int method1 (Type t, int val)
		{
			Console.WriteLine ("You passed in " + val);
			return 1;
		}

		static int method1 (Type t, Type[] types)
		{
			Console.WriteLine ("Wrong method called !");
			return 2;
		}
		
		public static int Main()
		{
			int i = method1 (null, 1);

			if (i == 1)
				return 0;
			else
				return 1;
		}
	}
}
