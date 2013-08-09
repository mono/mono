using System;

namespace TestAnonSwitch
{
	public class MyClass
	{
		public event EventHandler<EventArgs> FirstEvent;
		public event EventHandler<EventArgs> SecondEvent;

		public void Trigger ()
		{
			if (FirstEvent != null)
				FirstEvent (this, EventArgs.Empty);
		}
	}

	public class Tester
	{
		MyClass myobj;

		public void Test ()
		{
			myobj = new MyClass ();
			var something = "key";

			switch (something) {
			case "key":
				myobj.FirstEvent += (sender, e) => {
					Console.WriteLine ("FirstEvent: {0}", myobj);
				};
				break;
			case "somethingelse":
				bool? woot = null;
				myobj.SecondEvent += (sender, e) => {
					Console.WriteLine ("woot {0}", woot);
				};
				break;
			}

			myobj.Trigger ();
		}
	}

	class MainClass
	{
		public static void Main ()
		{
			var tester = new Tester ();
			tester.Test ();
		}
	}
}
