//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class FormEvent : TestHelper
	{	
		static bool eventhandled = false;
		public void New_EventHandler (object sender, EventArgs e)
		{
			eventhandled = true;
		}

		private Form _form;

		[TearDown]
		protected override void TearDown ()
		{
			if (_form != null)
				_form.Dispose ();
			base.TearDown ();
		}

		[Test]
		public void Activated ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("#3 fails");

			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("Activated"), "#1");
			_form.Activate ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("Activated"), "#2");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#3");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#4");
			_form.Activate ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#5");
			_form.Hide ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#6");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (2, logger.CountEvents ("Activated"), "#7");
		}

		[Test]
		public void Activated_Dialog ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("#4 fails");

			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("Activated"), "#1");
			_form.Activate ();
			Assert.AreEqual (0, logger.CountEvents ("Activated"), "#2");
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#3");
			_form.ShowDialog ();
			Assert.AreEqual (2, logger.CountEvents ("Activated"), "#4");
		}

		[Test]
		public void Closed ()
		{
			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("Closed"), "#1");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Closed"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Closed"), "#3");
		}

		[Test]
		public void Closed_Dialog ()
		{
			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("Closed"), "#1");
			_form.ShowDialog ();
			Assert.AreEqual (2, logger.CountEvents ("Closed"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (2, logger.CountEvents ("Closed"), "#3");
		}

		[Test]
		public void Closing ()
		{
			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("Closing"), "#1");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Closing"), "#2");
		}

		[Test]
		public void Closing_Dialog ()
		{
			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("Closing"), "#1");
			_form.ShowDialog ();
			Assert.AreEqual (2, logger.CountEvents ("Closing"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (2, logger.CountEvents ("Closing"), "#3");
		}

		[Test]
		public void Deactivate ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("#3 or #5 fail");

			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			_form.Activate ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("Deactivate"), "#1");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("Deactivate"), "#2");
			_form.Hide ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#3");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#4");
			_form.Hide ();
			Application.DoEvents ();
			Assert.AreEqual (2, logger.CountEvents ("Deactivate"), "#5");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (2, logger.CountEvents ("Deactivate"), "#6");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (2, logger.CountEvents ("Deactivate"), "#7");
		}

		[Test]
		public void Deactivate_Dialog ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("#2 sometimes fails");

			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			_form.Activate ();
			Assert.AreEqual (0, logger.CountEvents ("Deactivate"), "#1");
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#3");
		}

		[Test] // bug #413898
		public void EventOrder ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("#A3 fails");

			string [] expectedEvents_Show = {
				"Load",
				"VisibleChanged",
				"GotFocus",
				"Activated" };

			string [] expectedEvents_Close = {
				"Closing",
				"FormClosing",
				"Closed",
				"FormClosed",
				"Deactivate",
				"LostFocus",
				"HandleDestroyed",
				"Disposed" };

			_form = new Form ();
			EventLogger logger = new EventLogger (_form);

			_form.Show ();
			Application.DoEvents ();

			Assert.IsTrue (logger.ContainsEventsOrdered (expectedEvents_Show), "#A1:" + logger.EventsJoined());
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#A2");
			Assert.AreEqual (1, logger.CountEvents ("VisibleChanged"), "#A3");
			Assert.AreEqual (1, logger.CountEvents ("GotFocus"), "#A4");
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#A5");
			Assert.AreEqual (0, logger.CountEvents ("Closing"), "#A6");
			Assert.AreEqual (0, logger.CountEvents ("FormClosing"), "#A7");
			Assert.AreEqual (0, logger.CountEvents ("Closed"), "#A8");
			Assert.AreEqual (0, logger.CountEvents ("FormClosed"), "#A9");
			Assert.AreEqual (0, logger.CountEvents ("Deactivate"), "#A10");
			Assert.AreEqual (0, logger.CountEvents ("LostFocus"), "#A11");
			Assert.AreEqual (0, logger.CountEvents ("HandleDestroyed"), "#A12");
			Assert.AreEqual (0, logger.CountEvents ("Disposed"), "#A13");

			logger.Clear ();
			_form.Close ();
			Application.DoEvents ();

			Assert.IsTrue (logger.ContainsEventsOrdered (expectedEvents_Close), "#B1:" + logger.EventsJoined ());
			Assert.AreEqual (0, logger.CountEvents ("Load"), "#B2");
			Assert.AreEqual (0, logger.CountEvents ("VisibleChanged"), "#B3");
			Assert.AreEqual (0, logger.CountEvents ("GotFocus"), "#B4");
			Assert.AreEqual (0, logger.CountEvents ("Activated"), "#B5");
			Assert.AreEqual (1, logger.CountEvents ("Closing"), "#B6");
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"), "#B7");
			Assert.AreEqual (1, logger.CountEvents ("Closed"), "#B8");
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#B9");
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#B10");
			Assert.AreEqual (1, logger.CountEvents ("LostFocus"), "#B11");
			Assert.AreEqual (1, logger.CountEvents ("HandleDestroyed"), "#B12");
			Assert.AreEqual (1, logger.CountEvents ("Disposed"), "#B13");
		}

		[Test] // bug #413898
		public void EventOrder_Dialog ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("#A3 fails");

			string [] expectedEvents = {
				"Load",
				"VisibleChanged",
				"GotFocus",
				"Activated",
				"Closing",
				"FormClosing",
				"Closed",
				"FormClosed",
				"VisibleChanged",
				"Deactivate",
				"LostFocus",
				"HandleDestroyed" };

			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);

			_form.ShowDialog ();
			Assert.IsTrue (logger.ContainsEventsOrdered (expectedEvents), "#A1:" + logger.EventsJoined ());
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#A2");
			Assert.AreEqual (2, logger.CountEvents ("VisibleChanged"), "#A3");
			Assert.AreEqual (1, logger.CountEvents ("GotFocus"), "#A4");
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#A5");
			Assert.AreEqual (1, logger.CountEvents ("Closing"), "#A6");
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"), "#A7");
			Assert.AreEqual (1, logger.CountEvents ("Closed"), "#A8");
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#A9");
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#A10");
			Assert.AreEqual (1, logger.CountEvents ("LostFocus"), "#A11");
			Assert.AreEqual (1, logger.CountEvents ("HandleDestroyed"), "#A12");
			Assert.AreEqual (0, logger.CountEvents ("Disposed"), "#A13");

			logger.Clear ();

			_form.ShowDialog ();
			Assert.IsTrue (logger.ContainsEventsOrdered (expectedEvents), "#B1:" + logger.EventsJoined ());
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#B2");
			Assert.AreEqual (2, logger.CountEvents ("VisibleChanged"), "#B3");
			Assert.AreEqual (1, logger.CountEvents ("GotFocus"), "#B4");
			Assert.AreEqual (1, logger.CountEvents ("Activated"), "#B5");
			Assert.AreEqual (1, logger.CountEvents ("Closing"), "#B6");
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"), "#B7");
			Assert.AreEqual (1, logger.CountEvents ("Closed"), "#B8");
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#B9");
			Assert.AreEqual (1, logger.CountEvents ("Deactivate"), "#B10");
			Assert.AreEqual (1, logger.CountEvents ("LostFocus"), "#B11");
			Assert.AreEqual (1, logger.CountEvents ("HandleDestroyed"), "#B12");
			Assert.AreEqual (0, logger.CountEvents ("Disposed"), "#B13");
		}

		[Test]
		public void FormClosed ()
		{
			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("FormClosed"), "#1");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("FormClosed"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#3");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#4");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#5");
		}

		[Test]
		public void FormClosed_Dialog ()
		{
			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("FormClosed"), "#1");
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#3");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosed"), "#4");
		}

		[Test]
		public void FormClosing ()
		{
			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("FormClosing"), "#1");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (0, logger.CountEvents ("FormClosing"), "#2");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"), "#3");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"), "#4");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"), "#5");
		}

		[Test]
		public void FormClosing_Dialog ()
		{
			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("FormClosing"));
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("FormClosing"));
			_form.ShowDialog ();
			Assert.AreEqual (2, logger.CountEvents ("FormClosing"));
		}

		[Test]
		public void Load ()
		{
			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("Load"), "#1");
			_form.Show ();
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#2");
			_form.Show ();
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#3");
			_form.Hide ();
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#4");
			_form.Show ();
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#5");
		}

		[Test]
		public void Load_Dialog ()
		{
			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("Load"), "#1");
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("Load"), "#2");
			_form.ShowDialog ();
			Assert.AreEqual (2, logger.CountEvents ("Load"), "#3");
		}

		[Test]
		public void Shown ()
		{
			_form = new Form ();
			EventLogger logger = new EventLogger (_form);
			//_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("Shown"), "#1");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Shown"), "#2");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Shown"), "#3");
			_form.Hide ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Shown"), "#4");
			_form.Show ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Shown"), "#5");
			_form.Close ();
			Application.DoEvents ();
			Assert.AreEqual (1, logger.CountEvents ("Shown"), "#6");
		}

		[Test]
		public void Shown_Dialog ()
		{
			_form = new DelayedCloseForm ();
			EventLogger logger = new EventLogger (_form);
			_form.ShowInTaskbar = false;
			Assert.AreEqual (0, logger.CountEvents ("Shown"), "#1");
			_form.ShowDialog ();
			Assert.AreEqual (1, logger.CountEvents ("Shown"), "#2");
			_form.ShowDialog ();
			Assert.AreEqual (2, logger.CountEvents ("Shown"), "#3");
		}

		[Test]
		public void VisibleChangedEvent ()
		{
			_form = new Form ();
			_form.VisibleChanged += new EventHandler (FormVisibleChangedHandler);

			Control control1 = new Control ();
			control1.Location = new Point (5, 5);
			control1.VisibleChanged += new EventHandler (Control1VisibleChangedHandler);
			control1_visiblechanged_count = 0;

			Assert.AreEqual (true, control1.Visible, "#A1");
			Assert.AreEqual (false, _form.Visible, "#A2");

			// case one - change without being added to the form
			control1.Visible = false;
			Assert.AreEqual (false, control1.Visible, "#B1");
			Assert.AreEqual (1, control1_visiblechanged_count, "#B2");

			control1.Visible = true;
			control1_visiblechanged_count = 0;
			_form.Controls.Add (control1);
			Assert.AreEqual (false, control1.Visible, "#C1");
			Assert.AreEqual (1, control1_visiblechanged_count, "#C2");

			// Add a second control that actually is not visible
			Control control2 = new Control ();
			control2.Visible = false;
			_form.Controls.Add (control2);

			control1_visiblechanged_count = control2_visiblechanged_count = form_visiblechanged_count = 0;
			_form.Show ();
			Assert.AreEqual (1, control1_visiblechanged_count, "#D1");
			Assert.AreEqual (0, control2_visiblechanged_count, "#D2");
			Assert.AreEqual (1, form_visiblechanged_count, "#D3");

			_form.Dispose ();
		}

		int control1_visiblechanged_count;
		int control2_visiblechanged_count;
		int form_visiblechanged_count;

		void Control1VisibleChangedHandler (object o, EventArgs args)
		{
			control1_visiblechanged_count++;
		}

		void FormVisibleChangedHandler (object o, EventArgs args)
		{
			form_visiblechanged_count++;
		}

		class DelayedCloseForm : Form
		{
			private Timer _timer;

			public DelayedCloseForm ()
			{
				_timer = new Timer ();
				_timer.Tick += new EventHandler (OnTick);
				_timer.Interval = 50;

				Closed += new EventHandler (OnClosed);
				Load += new EventHandler (OnLoad);
			}

			void OnClosed (object sender, EventArgs e)
			{
				_timer.Enabled = false;
			}

			void OnLoad (object sender, EventArgs e)
			{
				_timer.Enabled = true;
			}

			void OnTick (object sender, EventArgs e)
			{
				Close ();
				Application.DoEvents ();
			}
		}

		class MyForm : Form
		{
			public void MaximizeBoundsTest ()
			{
				this.MaximizedBounds = new Rectangle (10,10,100,100);
			}
		}

		[Test]
		public void MaximizedBoundsChangedTest ()
		{
			_form = new MyForm ();
			_form.MaximizedBoundsChanged += new EventHandler (New_EventHandler);
			eventhandled = false;
			((MyForm) _form).MaximizeBoundsTest ();
			Assert.AreEqual (true, eventhandled, "#A5");
		}

		[Test]
		public void MaximumSizeChangedTest ()
		{
			_form = new Form ();
			_form.ShowInTaskbar = false;
			_form.MaximumSizeChanged += new EventHandler (New_EventHandler);
			eventhandled = false;
			_form.MaximumSize = new Size (500, 500);
			Assert.AreEqual (true, eventhandled, "#A6");
		}

		[Test, Ignore ("Manual Intervention")]
		public void MdiChildActivateTest ()
		{
			Form parent = new Form ();
			parent.ShowInTaskbar = false;
			Form child = new Form ();
			parent.IsMdiContainer = true;
			child.IsMdiContainer = false;
			child.MdiParent = parent;
			parent.MdiChildActivate += new EventHandler (New_EventHandler);
			eventhandled = false;
			using (parent) 
			{
				child.Visible = true;
				parent.Show ();
				Assert.AreEqual (true, eventhandled, "#A7");
				eventhandled = false;
				child.Close ();
				Assert.AreEqual (true, eventhandled, "#A8");
			}
		}

		[Test]
		public void MinimumSizeChangedTest ()
		{
			_form = new Form ();
			_form.ShowInTaskbar = false;
			_form.MinimumSizeChanged += new EventHandler (New_EventHandler);
			eventhandled = false;
			_form.MinimumSize = new Size(100, 100);
			Assert.AreEqual (true, eventhandled, "#A10");
		}

		/**
		 ** This next test is in response to a bug report
		 ** which pointed out that the idle events were being
		 ** sent to every thread rather than just the thread
		 ** they were assigned on.
		 **
		 ** Report: https://bugzilla.novell.com/show_bug.cgi?id=321541
		 **/
		private static Form form1_OneIdlePerThread = null;
		private static Form form2_OneIdlePerThread = null;
		private static int count1_OIPT = 0;
		private static int count2_OIPT = 0; 
		private static ThreadStart OIPT_ThreadStart2;
		private static Thread OIPT_Thread2;
		private static int oipt_t1 = 0;
		private static int oipt_t2 = 0;
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void OneIdlePerThread () {
			Thread t = Thread.CurrentThread;
			oipt_t1 = t.ManagedThreadId;
			count1_OIPT = 0;
			count2_OIPT = 0;
			form1_OneIdlePerThread = new Form ();
			form2_OneIdlePerThread = new Form ();
			form1_OneIdlePerThread.Show (); 

			OIPT_ThreadStart2 = new ThreadStart (TIPT_Two);
			OIPT_Thread2=new Thread (OIPT_ThreadStart2);
			OIPT_Thread2.IsBackground = true;
			OIPT_Thread2.SetApartmentState (ApartmentState.STA);
			OIPT_Thread2.Start ();
			Application.Idle += new EventHandler (TestIdlePerThread);
			Application.Run (form1_OneIdlePerThread);
			if (!OIPT_Thread2.Join (1000)){
				OIPT_Thread2.Abort();
			}

			Assert.AreEqual (true, count1_OIPT == 1, 
				"#Idle: idle #1 hit too many times");
			Assert.AreEqual (true, count2_OIPT == 1, 	
				"#Idle: idle #2 hit too many times");
		}
		public static void TIPT_Two (){
			Thread t = Thread.CurrentThread;
			oipt_t2 = t.ManagedThreadId;
			Application.Idle += 	
				new EventHandler (TestIdlePerThread2);
			Application.Run (form2_OneIdlePerThread);
		}
		public static void TestIdlePerThread (object o, EventArgs e) {
			Thread t = Thread.CurrentThread;
			count1_OIPT++;
			Application.Idle -= 
				new EventHandler (TestIdlePerThread);
			/* Give thread2 time to finish before we close */
			Thread.Sleep(100);
			if (form1_OneIdlePerThread != null)
				form1_OneIdlePerThread.Close ();
			Assert.AreEqual (true, oipt_t1 == t.ManagedThreadId, 
				"#Idle:Wrong Thread-t1");
		}
		public static void TestIdlePerThread2 (object o, EventArgs e) {
			Thread t = Thread.CurrentThread;
			count2_OIPT++;
			Application.Idle -= 
				new EventHandler(TestIdlePerThread2);
			if (form2_OneIdlePerThread != null)
				form2_OneIdlePerThread.Invoke 	
				  (new MethodInvoker (form2_OneIdlePerThread.Close));
			Assert.AreEqual (true, oipt_t2 == t.ManagedThreadId, 
				"#Idle:Wrong Thread-t2");
		}
		
	}

	[TestFixture]
	public class ClosingEvent
	{	
		bool cancel = true;
		CancelEventArgs args = null;
		public void Closing_Handler (object sender, CancelEventArgs e)
		{
			e.Cancel = cancel;
			args = e;
		}

		[Test, Ignore ("visual test")]
		public void ClosingEventTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Closing += new CancelEventHandler (Closing_Handler);
			myform.Show ();
			args = null;
			myform.Close ();
			Assert.AreEqual (true, args != null, "#A11");
			Assert.AreEqual (true, myform.Visible, "#A12");
			cancel = false;
			args = null;
			myform.Close ();
			Assert.AreEqual (true, args != null, "#A13");
			Assert.AreEqual (false, myform.Visible, "#A14");
		}
	}

	[TestFixture]
	public class OnClosedOverride
	{
		[Test]
		public void OnClosedOverride_Test1 ()
		{
			const bool removeButtonOnClick = false;

			using (var f = new ButtonedForm (removeButtonOnClick)) {
				f.Show ();
				Assert.Null (f.onClosedForm.CatchedException, "An exception has been detected in `WndProc`");
			}
		}

		[Test]
		public void OnClosedOverride_Test2 ()
		{
			const bool removeButtonOnClick = true;

			var task = Task.Run (() => {
				using (var f = new ButtonedForm (removeButtonOnClick)) {
					f.Show ();
				}
			});

			var ok = task.Wait (TimeSpan.FromSeconds (1));
			Assert.True (ok, "Infinite loop has been detected in `WndProc`");
		}

		public partial class ButtonedForm : Form
		{
			// We need to controls on this form. One of them must be active (button).
			public Button button = new Button ();
			private Label label = new Label ();
			
			private bool removeButtonOnClick;
			
			public OnClosedForm onClosedForm;

			public ButtonedForm (bool removeButtonOnClick)
			{
				this.removeButtonOnClick = removeButtonOnClick;

				this.Controls.Add( button);
				this.Controls.Add (label);
				
				this.Text = "ButtonedForm";
				button.Text = "Click to Close";

				onClosedForm = new OnClosedForm (this);
				onClosedForm.Show ();

				button.Click += click_Button;
				Shown += shown_ButtonedForm;
			}

			protected void click_Button (object sender, EventArgs e)
			{
				if (removeButtonOnClick)  // This property switches test between to two different issues.
					this.Controls.Remove (button);
				onClosedForm.Close ();
			}

			public void shown_ButtonedForm (object sender, EventArgs e)
			{
				// TODO:
				// It is necessary to add some sleep before click button. This phenomenon apparently
				// is stemmed from the asynchronous nature of `XplatUI.SendMessage`: see method
				// `Form.SetVisibleCore()` in `Froms.cs`.
				Thread.Sleep (200);
				EmulateButtonClick ();
			}

			public void EmulateButtonClick ()
			{
				// When one click a button, the parent form an then the button take focus.
				// This is vital to reproduce the issue https://github.com/mono/mono/issues/13150.
				button.Focus ();
				click_Button (null, null);
			}
		}
		
		public partial class OnClosedForm : Form
		{
			private ButtonedForm buttonedForm;

			public Exception CatchedException;

			public OnClosedForm (ButtonedForm buttonedForm)
			{
				this.buttonedForm = buttonedForm;
				this.CatchedException = null;

				this.Text = "OnClosedForm";
			}

			protected override void OnClosed (EventArgs e)
			{
				base.OnClosed (e);
				buttonedForm.Close ();
			}

			protected override void WndProc (ref Message message)
			{
				try {
					base.WndProc (ref message);
				}
				catch (Exception ex) {
					CatchedException = ex;
				}
			}
		}
	}

	[TestFixture,Ignore ("Test Breaks")]
	public class InputLanguageChangedEvent
	{	
		static bool eventhandled = false;
		public void InputLanguage_Handler (object sender,InputLanguageChangedEventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void InputLanguageChangedEventTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			CultureInfo oldci = Thread.CurrentThread.CurrentCulture;
			CultureInfo oldcui = Thread.CurrentThread.CurrentUICulture;
			InputLanguage oldil = InputLanguage.CurrentInputLanguage;
			try {
				if (InputLanguage.InstalledInputLanguages.Count > 1) {
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[0];
					myform.InputLanguageChanged += new InputLanguageChangedEventHandler (InputLanguage_Handler);
					Thread.CurrentThread.CurrentCulture = new CultureInfo ("ta-IN");
					Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[1];
					Assert.AreEqual (true, eventhandled, "#A15");
				}
			} finally {
				Thread.CurrentThread.CurrentCulture = oldci;
				Thread.CurrentThread.CurrentUICulture = oldcui;
				InputLanguage.CurrentInputLanguage = oldil;
			}
			myform.Dispose ();
		}
	}

	[TestFixture,Ignore ("Test Breaks")]
	public class InputLanguageChangingEvent
	{	
		static bool eventhandled = false;
		public void InputLangChanging_Handler(object sender,InputLanguageChangingEventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void InputLanguageChangingEventTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			CultureInfo oldci = Thread.CurrentThread.CurrentCulture;
			CultureInfo oldcui = Thread.CurrentThread.CurrentUICulture;
			InputLanguage oldil = InputLanguage.CurrentInputLanguage;
			try {
				if (InputLanguage.InstalledInputLanguages.Count > 1) 
				{
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[0];
					myform.InputLanguageChanging += new InputLanguageChangingEventHandler (InputLangChanging_Handler);
					Thread.CurrentThread.CurrentCulture = new CultureInfo ("ta-IN");
					Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[1];
					Assert.AreEqual (true, eventhandled, "#A16");
				}
			} finally {
				Thread.CurrentThread.CurrentCulture = oldci;
				Thread.CurrentThread.CurrentUICulture = oldcui;
				InputLanguage.CurrentInputLanguage = oldil;
			}
			myform.Dispose ();
		}
	}
}
