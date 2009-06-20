//
// MonitorTest.cs - NUnit test cases for System.Threading.Monitor
//
// Author:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class MonitorTest {

		[Test]
		public void ExitNoEnter ()
		{
			object o = new object ();
			Monitor.Exit (o);
		}

		[Test]
		public void OneEnterSeveralExits ()
		{
			object o = new object ();
			Monitor.Enter (o);
			Monitor.Exit (o);
			Monitor.Exit (o);
			Monitor.Exit (o);
			Monitor.Exit (o);
		}
	}
}
