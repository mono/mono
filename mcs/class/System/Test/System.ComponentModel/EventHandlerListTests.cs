//
// System.ComponentModel.EventHandlerList test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.ComponentModel;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class EventHandlerListTests
	{
		[SetUp]
		public void GetReady ()
		{
		}

		int calls = 0;

		void Deleg1 (object o, EventArgs e)
		{
			calls++;
		}

		void Deleg2 (object o, EventArgs e)
		{
			calls <<= 1;
		}

		[Test]
		public void All ()
		{
			EventHandlerList list = new EventHandlerList ();
			string i1 = "i1";
			string i2 = "i2";
			EventHandler one = new EventHandler (Deleg1);
			EventHandler two = new EventHandler (Deleg2);
			EventHandler d;

			Assert.IsNull (list [i1], "All #01");
			Assert.IsNull (list [i2], "All #02");

			list.AddHandler (i1, one);
			d = list [i1] as EventHandler;
			Assert.IsNotNull (d, "All #03");

			d (this, EventArgs.Empty);
			Assert.AreEqual (1, calls, "All #04");

			list.AddHandler (i2, two);
			d = list [i1] as EventHandler;
			Assert.IsNotNull (d, "All #05");

			d (this, EventArgs.Empty);
			Assert.AreEqual (2, calls, "All #06");

			d = list [i2] as EventHandler;
			Assert.IsNotNull (d, "All #07");

			d (this, EventArgs.Empty);
			Assert.AreEqual (4, calls, "All #08");

			list.AddHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert.IsNotNull (d, "All #08");

			d (this, EventArgs.Empty);
			Assert.AreEqual (16, calls, "All #09");

			list.RemoveHandler (i1, one);
			d = list [i1] as EventHandler;
			Assert.IsNull (d, "All #10");

			list.RemoveHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert.IsNotNull (d, "All #11");

			list.RemoveHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert.IsNull (d, "All #12");

			list.AddHandler (i1, one);
			d = list [i1] as EventHandler;
			Assert.IsNotNull (d, "All #13");

			list.AddHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert.IsNotNull (d, "All #14");

			list.AddHandler (i1, null);
			Assert.IsNotNull (list [i1], "All #15");

			list.AddHandler (i2, null);
			Assert.IsNotNull (list [i2], "All #16");

			list.Dispose ();
		}
		
		[Test]
		public void NullKey ()
		{
			EventHandlerList list = new EventHandlerList ();
			EventHandler one = new EventHandler (Deleg1);
			
			list.AddHandler (null, one);
			EventHandler d = list [null] as EventHandler;
			Assert.IsNotNull (d, "NullKey #01");
			
			list.RemoveHandler (null, one);
			d = list [null] as EventHandler;
			Assert.IsNull (d, "NullKey #02");
		}
	}
}

