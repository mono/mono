using System;

namespace BaseTest
{
	public class MainClass
	{
		EventHandler myEvent;

		public event EventHandler MyEvent
		{
			add { myEvent += delegate { value (this, EventArgs.Empty); };  }
			remove { myEvent += delegate { value (this, EventArgs.Empty); }; }
		}

		public void RaiseMyEvent (object o, EventArgs e)
		{
			myEvent (o, e);
		}

		public static void Main ()
		{
			MainClass c = new MainClass ();
			c.MyEvent += (o, e) => Console.WriteLine ("Hey! from {0} / {1}", o, e);
			c.RaiseMyEvent (null, null);
		}
	}
}

