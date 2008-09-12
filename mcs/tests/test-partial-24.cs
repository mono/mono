

namespace N
{
	using S = System;
	
	public partial class C
	{
		[S.Obsolete ("A")]
		partial void Foo ();
	
		public static void Main ()
		{
		}
	}
}

namespace N
{
	public partial class C
	{
		partial void Foo ()
		{
		}
	}
}