namespace SD {
	public class Sd {
		static public void F (bool b) { }
	}
}

namespace Foo {
	using SD;
	partial class Bar {
		delegate void f_t (bool b);
		f_t f = new f_t (Sd.F);
	}
}

namespace Foo {
	partial class Bar
	{
		public Bar () {}
		public static void Main ()
		{
			if (new Bar ().f == null)
				throw new System.Exception ("Didn't resolve Sd.F?");
		}
	}
}
