// CS1618: Cannot create delegate with `System.Diagnostics.Debug.Assert(bool)' because it has a Conditional attribute
// Line: 8

namespace Foo {
	using System.Diagnostics;
	partial class Bar {
		delegate void assert_t (bool condition);
		assert_t assert = new assert_t (Debug.Assert);
	}
}

namespace Foo {
	using System;
	partial class Bar
	{
		public Bar () {}
		static void Main ()
		{
			if (new Bar ().assert == null)
				throw new Exception ("Didn't resolve Debug.Assert?");
		}
	}
}
