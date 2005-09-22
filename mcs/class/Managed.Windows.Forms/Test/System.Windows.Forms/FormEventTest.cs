//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class FormEvent
	{	
		static bool eventhandled = false;
		public void New_EventHandler (object sender, EventArgs e)
		{
			eventhandled = true;
		}

		[Test, Ignore ("Manual Intervention")]
		public void ActivatedTest ()
		{
			Form myform = new Form ();
			myform.Activated += new EventHandler (New_EventHandler);
			myform.Activate ();
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A1");
		}

		[Test, Ignore ("Manual Intervention")]
		public void ClosedTest ()
		{
			Form myform = new Form ();
			myform.Closed += new EventHandler (New_EventHandler);
			eventhandled = false;
			myform.Close ();
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A2");
		}

		[Test, Ignore ("Manual Intervention")]
		public void DeactivateTest ()
		{
			Form myform = new Form ();
			myform.Deactivate += new EventHandler (New_EventHandler);
			eventhandled = false;
			myform.Close ();
			myform.Activate ();
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A3");
		}

		[Test, Ignore ("Manual Intervention")]
		public void LoadTest ()
		{
			Form myform = new Form ();
			myform.Load += new EventHandler (New_EventHandler);
			eventhandled = false;
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A4");
			myform.Dispose ();
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
			MyForm myform = new MyForm ();
			myform.MaximizedBoundsChanged += new EventHandler (New_EventHandler);
			eventhandled = false;
			myform.MaximizeBoundsTest ();
			Assert.AreEqual (true, eventhandled, "#A5");
			myform.Dispose ();
		}

		[Test]
		public void MaximumSizeChangedTest ()
		{
			Form myform = new Form ();
			myform.MaximumSizeChanged += new EventHandler (New_EventHandler);
			eventhandled = false;
			myform.MaximumSize = new Size (500, 500);
			Assert.AreEqual (true, eventhandled, "#A6");
			myform.Dispose ();
		}

		[Test, Ignore ("Manual Intervention")]
		public void MdiChildActivateTest ()
		{
			Form parent = new Form ();
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
			Form myform = new Form ();
			myform.MinimumSizeChanged += new EventHandler (New_EventHandler);
			eventhandled = false;
			myform.MinimumSize = new Size(100, 100);
			Assert.AreEqual (true, eventhandled, "#A10");
			myform.Dispose ();
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
			CultureInfo oldci = Thread.CurrentThread.CurrentCulture;
			CultureInfo oldcui = Thread.CurrentThread.CurrentUICulture;
			InputLanguage oldil = InputLanguage.CurrentInputLanguage;
			try 
			{
				if (InputLanguage.InstalledInputLanguages.Count > 1) 
				{
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[0];
					myform.InputLanguageChanged += new InputLanguageChangedEventHandler (InputLanguage_Handler);
					Thread.CurrentThread.CurrentCulture = new CultureInfo ("ta-IN");
					Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[1];
					Assert.AreEqual (true, eventhandled, "#A15");
				}
			}
			finally 
			{
				Thread.CurrentThread.CurrentCulture = oldci;
				Thread.CurrentThread.CurrentUICulture = oldcui;
				InputLanguage.CurrentInputLanguage = oldil;
			}
		}
	}

	[TestFixture,Ignore ("Test Breaks")]
	public class InputLanguageChangingdEvent
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
			CultureInfo oldci = Thread.CurrentThread.CurrentCulture;
			CultureInfo oldcui = Thread.CurrentThread.CurrentUICulture;
			InputLanguage oldil = InputLanguage.CurrentInputLanguage;
			try 
			{
				if (InputLanguage.InstalledInputLanguages.Count > 1) 
				{
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[0];
					myform.InputLanguageChanging += new InputLanguageChangingEventHandler (InputLangChanging_Handler);
					Thread.CurrentThread.CurrentCulture = new CultureInfo ("ta-IN");
					Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
					InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[1];
					Assert.AreEqual (true, eventhandled, "#A16");
				}
			}
			finally 
			{
				Thread.CurrentThread.CurrentCulture = oldci;
				Thread.CurrentThread.CurrentUICulture = oldcui;
				InputLanguage.CurrentInputLanguage = oldil;
			}
		}
	}
}
