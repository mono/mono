// Compiler options: -doc:xml-016.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// public event EventHandler MyEvent
		/// </summary>
		public event EventHandler MyEvent;

		/// private event EventHandler MyEvent; without markup - it is OK.
		private event EventHandler MyEvent2;
	}
}

