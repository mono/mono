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

#define NUNIT // Comment out this one if you wanna play with the test without using NUnit

#if NUNIT
using NUnit.Framework;
#else
using System.Reflection;
#endif

using System;
using System.ComponentModel;

namespace MonoTests.System.ComponentModel
{
#if NUNIT
	[TestFixture]
	public class EventHandlerListTests
	{
#else
	public class EventHandlerListTests
	{
#endif
#if NUNIT
		[SetUp]
		public void GetReady ()
		{
#else
		static EventHandlerListTests ()
		{
#endif
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

			Assertion.AssertEquals ("All #01", null, list [i1]);
			Assertion.AssertEquals ("All #02", null, list [i2]);

			list.AddHandler (i1, one);
			d = list [i1] as EventHandler;
			Assertion.Assert ("All #03", d != null);

			d (this, EventArgs.Empty);
			Assertion.AssertEquals ("All #04", 1, calls);

			list.AddHandler (i2, two);
			d = list [i1] as EventHandler;
			Assertion.Assert ("All #05", d != null);

			d (this, EventArgs.Empty);
			Assertion.AssertEquals ("All #06", 2, calls);

			d = list [i2] as EventHandler;
			Assertion.Assert ("All #07", d != null);

			d (this, EventArgs.Empty);
			Assertion.AssertEquals ("All #08", 4, calls);

			list.AddHandler (i2, two);
			d = list [i2] as EventHandler;
			Assertion.Assert ("All #08", d != null);

			d (this, EventArgs.Empty);
			Assertion.AssertEquals ("All #09", 16, calls);

			list.RemoveHandler (i1, one);
			d = list [i1] as EventHandler;
			Assertion.Assert ("All #10", d == null);

			list.RemoveHandler (i2, two);
			d = list [i2] as EventHandler;
			Assertion.Assert ("All #11", d != null);

			list.RemoveHandler (i2, two);
			d = list [i2] as EventHandler;
			Assertion.Assert ("All #12", d == null);

			list.AddHandler (i1, one);
			d = list [i1] as EventHandler;
			Assertion.Assert ("All #13", d != null);

			list.AddHandler (i2, two);
			d = list [i2] as EventHandler;
			Assertion.Assert ("All #14", d != null);

			list.AddHandler (i1, null);
			Assertion.Assert ("All #15", list [i1] != null);

			list.AddHandler (i2, null);
			Assertion.Assert ("All #16", list [i2] != null);

			list.Dispose ();
		}
		
#if !NUNIT
		void Assert (string msg, bool result)
		{
			if (!result)
				Console.WriteLine (msg);
		}

		void AssertEquals (string msg, object expected, object real)
		{
			if (expected == null && real == null)
				return;

			if (expected != null && expected.Equals (real))
				return;

			Console.WriteLine ("{0}: expected: '{1}', got: '{2}'", msg, expected, real);
		}

		void Fail (string msg)
		{
			Console.WriteLine ("Failed: {0}", msg);
		}

		static void Main ()
		{
			EventHandlerListTests p = new EventHandlerListTests ();
			Type t = p.GetType ();
			MethodInfo [] methods = t.GetMethods ();
			foreach (MethodInfo m in methods) {
				if (m.Name.Substring (0, 4) == "Test") {
					m.Invoke (p, null);
				}
			}
		}
#endif
	}
}

