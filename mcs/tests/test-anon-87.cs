using System;

namespace Bug
{
	public delegate void D ();
	
	class AA : BB
	{
		public AA (BB bb)
			: base (bb.Value)
		{
			D d = delegate () {
				bb.Foo ();
				TestMe ();
			};
		}
		
		void TestMe ()
		{
		}
		
		public static int Main ()
		{
			new AA (new BB ("a"));
			return 0;
		}
	}
	
	class BB
	{
		public string Value = "test";
		
		public BB (string s)
		{
		}
		
		public void Foo ()
		{
		}
	}
}
