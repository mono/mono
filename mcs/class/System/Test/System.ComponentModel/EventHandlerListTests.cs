//
// System.ComponentModel.EventHandlerList test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
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
	public class EventHandlerListTests : TestCase
	{
#else
	public class EventHandlerListTests
	{
#endif
#if NUNIT
		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (EventHandlerListTests));
			}
		}

		public EventHandlerListTests () :
			base ("MonoTests.System.ComponentModel.EventHandlerListTests testcase") { }

		public EventHandlerListTests (string name) : base (name) { }

		protected override void SetUp ()
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

		public void TestAll ()
		{
			EventHandlerList list = new EventHandlerList ();
			string i1 = "i1";
			string i2 = "i2";
			EventHandler one = new EventHandler (Deleg1);
			EventHandler two = new EventHandler (Deleg2);
			EventHandler d;

			AssertEquals ("TestAll #01", null, list [i1]);
			AssertEquals ("TestAll #02", null, list [i2]);

			list.AddHandler (i1, one);
			d = list [i1] as EventHandler;
			Assert ("TestAll #03", d != null);

			d (this, EventArgs.Empty);
			AssertEquals ("TestAll #04", 1, calls);

			list.AddHandler (i2, two);
			d = list [i1] as EventHandler;
			Assert ("TestAll #05", d != null);

			d (this, EventArgs.Empty);
			AssertEquals ("TestAll #06", 2, calls);

			d = list [i2] as EventHandler;
			Assert ("TestAll #07", d != null);

			d (this, EventArgs.Empty);
			AssertEquals ("TestAll #08", 4, calls);

			list.AddHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert ("TestAll #08", d != null);

			d (this, EventArgs.Empty);
			AssertEquals ("TestAll #09", 16, calls);

			list.RemoveHandler (i1, one);
			d = list [i1] as EventHandler;
			Assert ("TestAll #10", d == null);

			list.RemoveHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert ("TestAll #11", d != null);

			list.RemoveHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert ("TestAll #12", d == null);

			list.AddHandler (i1, one);
			d = list [i1] as EventHandler;
			Assert ("TestAll #13", d != null);

			list.AddHandler (i2, two);
			d = list [i2] as EventHandler;
			Assert ("TestAll #14", d != null);

			list.AddHandler (i1, null);
			Assert ("TestAll #15", list [i1] != null);

			list.AddHandler (i2, null);
			Assert ("TestAll #16", list [i2] != null);

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

