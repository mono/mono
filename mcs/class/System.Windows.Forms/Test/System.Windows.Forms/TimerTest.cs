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
using System.Threading;
using Timer = System.Windows.Forms.Timer;
using Sys_Threading=System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TimerTest : TestHelper
	{
		bool Ticked;
		
		[Test ()]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IntervalException1 ()
		{
			Timer timer = new Timer ();
			timer.Interval = 0;
		}

		[Test ()]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
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
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IntervalException4 ()
		{
			Timer timer = new Timer ();
			timer.Interval = int.MinValue;
		}
		
		[Test]
		[Category ("NotWorking")]
		public void StartTest ()
		{
			// This test fails about 50% of the time on the buildbots.
			Ticked = false;
			using (Timer timer = new Timer ()) {
				timer.Tick += new EventHandler (TickHandler);
				timer.Start ();
				Sys_Threading.Thread.Sleep (500);
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
		
		[Test]
		public void TagTest ()
		{
			Timer timer = new Timer ();
			timer.Tag = "a";
			Assert.AreEqual ("a", timer.Tag, "1");
		}

		/* Application.DoEvents and Sleep are not guarenteed on Linux
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
				Assert.AreEqual (true, timer.Enabled, "3");
				Assert.AreEqual (false, Ticked, "4");
				timer.Enabled = false;
				Assert.AreEqual (false, Ticked, "5"); // This may fail if we are running on a very slow machine...
				Assert.AreEqual (false, timer.Enabled, "6");
			}
		}
		*/

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
			Assert.AreEqual (null, timer.Tag, "T1");
		}

		[Test] // bug #325033
		public void RunningThread ()
		{
			var f1 = new Bug325033Form ();
			Application.Run (f1);
			var f2 = new Bug325033Form2 ();
			Application.Run (f2);

			f1.Dispose ();
			f2.Dispose ();
		}

		class Bug325033Form : Form
		{
			public Bug325033Form ()
			{
				Load += new EventHandler (Form_Load);
			}

			void Form_Load (object sender, EventArgs e)
			{
				Thread t = new Thread (new ThreadStart (Run));
				t.IsBackground = true;
				t.Start ();
				t.Join ();
				Close ();
			}

			void Run ()
			{
				Application.Run (new Bug325033Form2 ());
			}
		}

		class Bug325033Form2 : Form
		{
			public Bug325033Form2 ()
			{
				_label = new Label ();
				_label.AutoSize = true;
				_label.Dock = DockStyle.Fill;
				_label.Text = "It should close automatically.";
				Controls.Add (_label);
				_timer = new Timer ();
				_timer.Tick += new EventHandler (Timer_Tick);
				_timer.Interval = 500;
				_timer.Start ();
			}

			void Timer_Tick (object sender, EventArgs e)
			{
				_timer.Stop ();
				Close ();
			}

			private Label _label;
			private Timer _timer;
		}
	}
}
