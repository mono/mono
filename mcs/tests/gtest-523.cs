using System;
using System.Collections.Generic;

namespace Test
{
	internal struct TestClass4<T> : IEquatable<TestClass4<T>>, IEquatable<T>, IEqualityComparer<TestClass4<T>> where T : class
	{
		public bool Equals (T obj)
		{
			return true;
		}

		public bool Equals (TestClass4<T> entry)
		{
			return true;
		}

		public bool Equals (TestClass4<T> x, TestClass4<T> y)
		{
			return x.Equals (y);
		}

		public int GetHashCode (TestClass4<T> obj)
		{
			return obj.GetHashCode ();
		}

		public override int GetHashCode ()
		{
			return 1;
		}

		public override bool Equals (object obj)
		{
			return false;
		}

		public static bool operator == (TestClass4<T> entry1, TestClass4<T> entry2)
		{
			return entry1.Equals (entry2);
		}

		public static bool operator == (T entry1, TestClass4<T> entry2)
		{
			return entry2.Equals (entry1);
		}

		public static bool operator == (TestClass4<T> entry1, T entry2)
		{
			return entry1.Equals (entry2);
		}

		public static bool operator == (object entry1, TestClass4<T> entry2)
		{
			return entry2.Equals (entry1);
		}

		public static bool operator == (TestClass4<T> entry1, object entry2)
		{
			return entry1.Equals (entry2);
		}

		public static bool operator != (TestClass4<T> entry1, TestClass4<T> entry2)
		{
			return !(entry1 == entry2);
		}

		public static bool operator != (T entry1, TestClass4<T> entry2)
		{
			return !(entry1 == entry2);
		}

		public static bool operator != (TestClass4<T> entry1, T entry2)
		{
			return !(entry1 == entry2);
		}

		public static bool operator != (object entry1, TestClass4<T> entry2)
		{
			return !(entry1 == entry2);
		}

		public static bool operator != (TestClass4<T> entry1, object entry2)
		{
			return !(entry1 == entry2);
		}
	}

	class C
	{
		public static void Main ()
		{
			new TestClass4<string> ();
		}
	}
}

