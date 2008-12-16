using System;

namespace Bug
{
	public class A : B
	{
		public A ()
		{
			var dingus = new B ();

			EventHandler a = delegate {
				int prop = dingus.Prop;
				Test ();
			};
			
			a ();
		}

		void Test ()
		{
		}

		public static int Main ()
		{
			new A ();
			return 0;
		}
	}

	public class B
	{
		public B ()
		{
		}

		public int Prop { get { return 1; } }
	}

	public delegate void EventHandler ();
}
