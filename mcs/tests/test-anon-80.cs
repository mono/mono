using System;

namespace MonoBug
{
	sealed public class MyTest
	{
		sealed private class EventHandlers
		{
			private EventHandler _handler = DoNothingEventHandler;

			public static EventHandler DoNothingEventHandler
			{
				get
				{
					return delegate {
					};
				}
			}

			private int i;
			public EventHandler DoSomethingEventHandler
			{
				get
				{
					return delegate {
						++i;
					};
				}
			}

			public EventHandler Handler
			{
				get
				{
					return _handler;
				}
				set
				{
					_handler = value;
				}
			}
		}

		public static int Main ()
		{
			EventHandlers handlers = new EventHandlers ();
			handlers.Handler = handlers.DoSomethingEventHandler;

			Console.WriteLine ("Is handlers.Handler == handlers.DoSomethingEventHandler (instance)?");
			Console.WriteLine ("Expected: True");
			Console.Write ("Actual:   ");
			bool instanceEqual = handlers.Handler == handlers.DoSomethingEventHandler;
			Console.WriteLine (instanceEqual);
			Console.WriteLine ();

			handlers.Handler = EventHandlers.DoNothingEventHandler;
			Console.WriteLine ("Is handlers.Handler == EventHandlers.DoNothingEventHandler (static)?");
			Console.WriteLine ("Expected: True");
			Console.Write ("Actual:   ");
			bool staticEqual = handlers.Handler == EventHandlers.DoNothingEventHandler;
			Console.WriteLine (staticEqual);

			if (instanceEqual)
				if (staticEqual)
					return 0; // instance passed, static passed
				else
					return 1; // instance passed, static failed
			else
				if (staticEqual)
					return 2; // instance failed, static passed
				else
					return 3; // instance failed, static failed
		}
	}
}

