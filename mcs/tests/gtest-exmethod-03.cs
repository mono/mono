

using System;

namespace A
{
	public static class A
	{
		public static int Foo (this int i)
		{
			return 1;
		}
		
		public static int Foo (this int i, string s)
		{
			return 30;
		}
	}
}

namespace B
{
	public static class X
	{
		public static int Foo (this int i)
		{
			return 2;
		}
		
		public static int Foo (this int i, bool b)
		{
			return 20;
		}
	}
}

namespace C
{
	using A;
	using B;
	using D;
	
	public static class F
	{
		public static bool Foo (this byte i)
		{
			return false;
		}
	}
	
	namespace D
	{
		public static class F
		{
			public static int Foo (this int i)
			{
				return 66;
			}
			
			public static void TestX ()
			{
				int i = 2.Foo (false);
			}
		}
	}
	
	public static class M
	{
		public static int Foo (this int i)
		{
			return 4;
		}

		public static int Main ()
		{
			if (3.Foo ("a") != 30)
				return 1;
			
			if (((byte)0).Foo ())
				return 2;
			
			if (4.Foo (false) != 20)
				return 3;
			
			Console.WriteLine ("OK");
			return 0;
		}
	}
}