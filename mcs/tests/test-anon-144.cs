using System;

namespace UiaAtkBridgeTest
{
	class Test
	{
		public static void Invoke (EventHandler d)
		{
		}
	}

	public class GailTester
	{
		public void ThisCausesACrash<I> ()
		{
			Test.Invoke (delegate { });
		}
		
		public static void Main ()
		{
		}
	}
}
