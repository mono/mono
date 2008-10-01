using System;
using System.Reflection;

namespace N
{
	class Nested
	{
		public interface I<T>
		{
			T P { get; }
		}

		public class C : I<int>
		{
			int I<int>.P
			{
				get { return 2; }
			}
		}
	}

	class M
	{
		public static int Main ()
		{
			int count = 0;
			foreach (MethodInfo method in typeof (Nested.C).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic)) {
				Console.WriteLine (method.Name);
				if (method.Name == "N.Nested.I<int>.get_P")
					++count;
			}

			foreach (PropertyInfo pi in typeof (Nested.C).GetProperties (BindingFlags.Instance | BindingFlags.NonPublic)) {
				Console.WriteLine (pi.Name);
				if (pi.Name == "N.Nested.I<int>.P")
					count += 2;
			}
			
			return 3 - count;
		}
	}
}
