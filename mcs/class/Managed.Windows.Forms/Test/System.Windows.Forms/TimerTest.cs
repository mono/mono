//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Rolf Bjarne Kvinge  (RKvinge@novell.com)
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Sys_Threading=System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TimerTest
	{
		bool Ticked;
		
		[Test ()]
		[ExpectedException (typeof (ArgumentException))]
		public void IntervalException1 ()
		{
			Timer timer = new Timer ();
			timer.Interval = 0;
		}

		[Test ()]
		[ExpectedException (typeof (ArgumentException), "'-1' is not a valid value for Interval. Interval must be greater than 0.")]
		public void IntervalException2 ()
		{
			Timer timer = new Timer ();
			timer.Interval = -1;
		}

		[Test ()]
		public void IntervalException3 ()
		{
			Timer timer = new Timer ();
			timer.Interval = int.MaxValue;
		}

		[Test ()]
		[ExpectedException (typeof (ArgumentException), "'-2147483648' is not a valid value for Interval. Interval must be greater than 0.")]
		public void IntervalException4 ()
		{
			Timer timer = new Timer ();
			timer.Interval = int.MinValue;
		}
		
		[Test]
		public void StartTest ()
		{
			Ticked = false;
			using (Timer timer = new Timer ()) {
				timer.Tick += new EventHandler (TickHandler);
				timer.Start ();
				Sys_Threading.Thread.Sleep (150);
				Application.DoEvents ();
				Assert.AreEqual (true, timer.Enabled, "1");
				Assert.AreEqual (true, Ticked, "2");
			}
		}

		[Test]
		public void StopTest ()
		{
			Ticked = false;
			using (Timer timer = new Timer ()) {
				timer.Tick += new EventHandler (TickHandler);
				timer.Interval = 200;
				timer.Start ();
				Assert.AreEqual (true, timer.Enabled, "1");
				Assert.AreEqual (false, Ticked, "2");
				timer.Stop ();
				Assert.AreEqual (false, Ticked, "3"); // This may fail if we are running on a very slow machine...
				Assert.AreEqual (false, timer.Enabled, "4");
				Sys_Threading.Thread.Sleep (500);
				Assert.AreEqual (false, Ticked, "5");
			}
		}
		
#if NET_2_0
		[Test]
		public void TagTest ()
		{
			Timer timer = new Timer ();
			timer.Tag = "a";
			Assert.AreEqual ("a", timer.Tag, "1");
		}
#endif

		[Test]
		public void EnabledTest ()
		{
			Ticked = false;
			using (Timer timer = new Timer ()) {
				timer.Tick += new EventHandler (TickHandler);
				timer.Enabled = true;
				Sys_Threading.Thread.Sleep (150);
				Application.DoEvents ();
				Assert.AreEqual (true, timer.Enabled, "1");
				Assert.AreEqual (true, Ticked, "2");
			}
			
			Ticked = false;
			using (Timer timer = new Timer ()) {
				timer.Tick += new EventHandler (TickHandler);
				timer.Interval = 1000;
				timer.Enabled = true;
				Assert.AreEqual (true, timer.Enabled, "1");
				Assert.AreEqual (false, Ticked, "2");
				timer.Enabled = false;
				Assert.AreEqual (false, Ticked, "3"); // This may fail if we are running on a very slow machine...
				Assert.AreEqual (false, timer.Enabled, "4");
			}
		}

		void TickHandler (object sender, EventArgs e)
		{
			Ticked = true;
		}
		
		[Test]
		public void DefaultProperties ()
		{
			Timer timer = new Timer ();
			Assert.AreEqual (null, timer.Container, "C1");
			Assert.AreEqual (false, timer.Enabled, "E1");
			Assert.AreEqual (100, timer.Interval, "I1");
			Assert.AreEqual (null, timer.Site, "S1");
#if NET_2_0
			Assert.AreEqual (null, timer.Tag, "T1");
#endif
		}
	}
}