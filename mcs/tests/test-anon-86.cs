using System;

namespace Bug
{
	public class A : B
	{
		public A ()
		{
			var dingus = new B ();

			dingus.Event += delegate {
				int prop = dingus.Prop;
				Test ();
			};
		}

		void Test ()
		{
		}

		public static int Main ()
		{
			return 0;
		}
	}

	public class B
	{
		public B ()
		{
		}

		public int Prop { get; set; }

		public event EventHandler Event;
	}

	public delegate void EventHandler ();
}
