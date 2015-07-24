//
// ApplicationContextTest.cs
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using CategoryAttribute=NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	class MyForm : Form
	{
		public void DoDestroyHandle ()
		{
			DestroyHandle();
		}
	}


	[TestFixture]
	public class ApplicationContextTest : TestHelper
	{
		ApplicationContext ctx;
		int thread_exit_count;
		bool reached_form_handle_destroyed;

		void thread_exit (object sender, EventArgs e)
		{
			thread_exit_count++;
		}

		void form_handle_destroyed (object sender, EventArgs e)
		{
			Assert.AreEqual (0, thread_exit_count, "1");
			Assert.AreEqual (sender, ctx.MainForm, "2");
			reached_form_handle_destroyed = true;
		}

		void form_handle_destroyed2 (object sender, EventArgs e)
		{
			Assert.AreEqual (1, thread_exit_count, "1");
			Assert.AreEqual (sender, ctx.MainForm, "2");
			reached_form_handle_destroyed = true;
		}

		[Test]
		public void TestEventOrdering ()
		{
			thread_exit_count = 0;
			reached_form_handle_destroyed = false;

			MyForm f1 = new MyForm ();
			f1.ShowInTaskbar = false;
			f1.HandleDestroyed += new EventHandler (form_handle_destroyed);

			ctx = new ApplicationContext (f1);
			ctx.ThreadExit += new EventHandler (thread_exit);

			f1.Show ();
			f1.DoDestroyHandle ();

			Assert.AreEqual (true, reached_form_handle_destroyed, "3");
			Assert.AreEqual (1, thread_exit_count, "4");

			f1.Dispose ();
		}

		[Test]
		public void TestEventOrdering2 ()
		{
			thread_exit_count = 0;
			reached_form_handle_destroyed = false;

			MyForm f1 = new MyForm ();
			f1.ShowInTaskbar = false;

			ctx = new ApplicationContext (f1);
			ctx.ThreadExit += new EventHandler (thread_exit);

			f1.HandleDestroyed += new EventHandler (form_handle_destroyed2);

			f1.Show ();
			f1.DoDestroyHandle ();
			Assert.AreEqual (true, reached_form_handle_destroyed, "3");
			Assert.AreEqual (1, thread_exit_count, "4");
			
			f1.Dispose ();
		}

		[Test]
		public void ThreadExitTest ()
		{
			thread_exit_count = 0;

			MyForm f1 = new MyForm ();
			f1.ShowInTaskbar = false;
			ctx = new ApplicationContext (f1);
			ctx.ThreadExit += new EventHandler (thread_exit);

			Assert.AreEqual (f1, ctx.MainForm, "1");
			f1.ShowInTaskbar = false;
			f1.Show ();
			f1.Dispose ();
			Assert.AreEqual (f1, ctx.MainForm, "2");
			Assert.AreEqual (1, thread_exit_count, "3");

			f1 = new MyForm ();
			ctx = new ApplicationContext (f1);
			ctx.ThreadExit += new EventHandler (thread_exit);
			f1.ShowInTaskbar = false;
			f1.Show ();
			f1.DoDestroyHandle ();
			Assert.AreEqual (f1, ctx.MainForm, "4");
			Assert.AreEqual (2, thread_exit_count, "5");
			f1.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NestedApplicationContextTest ()
		{
			using (NestedForm frm = new NestedForm ()) {
				Application.Run (frm);
			}
		}

		private class NestedForm : Form
		{
			static int counter = 1;
			protected override void OnVisibleChanged (EventArgs e)
			{
				base.OnVisibleChanged (e);

				Text = counter.ToString ();

				if (counter <= 3) {
					counter++;
					Application.Run (new NestedForm ());
				}
				Close ();
			}
		}
	}
}
