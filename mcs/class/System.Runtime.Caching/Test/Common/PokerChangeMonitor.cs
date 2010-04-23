using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace MonoTests.Common
{
	class PokerChangeMonitor : ChangeMonitor
	{
		List <string> calls;
		string uniqueId;

		public List<string> Calls
		{
			get
			{
				if (calls == null)
					calls = new List<string> ();

				return calls;
			}
		}

		public override string UniqueId
		{
			get { return uniqueId; }
		}

		public PokerChangeMonitor ()
		{
			uniqueId = "UniqueID";
			InitializationComplete ();
		}

		public void SignalChange ()
		{
			OnChanged (null);
		}

		protected override void Dispose (bool disposing)
		{
			Calls.Add ("Dispose (bool disposing)");
		}
	}
}
