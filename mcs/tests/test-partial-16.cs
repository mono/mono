namespace A
{
	partial class C
	{
	}
}

namespace A
{
	using B;
	
	partial class C
	{
		public static bool f = C2.Test ();
		object o = new C2().Test_I ();
	}
}

namespace B
{
	partial class C2
	{
		public static bool Test ()
		{
			return false;
		}
		
		public object Test_I ()
		{
			return this;
		}
		
		public static void Main ()
		{
		}

	}
}