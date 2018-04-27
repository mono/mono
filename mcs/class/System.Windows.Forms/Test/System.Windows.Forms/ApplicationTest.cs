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
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ApplicationTest : TestHelper
	{
		ApplicationContext ctx;

		void form_visible_changed (object sender, EventArgs e)
		{
			Assert.AreEqual (sender, ctx.MainForm, "1");
			((Form)sender).Close();
		}

		[Test]
		public void ContextMainFormTest ()
		{
			Form f1 = new Form ();
			f1.ShowInTaskbar = false;
			ctx = new ApplicationContext (f1);

			f1.VisibleChanged += new EventHandler (form_visible_changed);

			Application.Run (ctx);

			Assert.IsNull (ctx.MainForm, "2");
			f1.Dispose ();
		}

		[Test]
		public void Bug694908 ()
		{
			Application.ThreadException += CrashingForm.HandleThreadException;

			using (var form = new CrashingForm ())
			{
				form.Show ();
				Application.DoEvents ();
			}
			// with bug 694908 we don't come here. Instead NUnit exits.
			Assert.IsTrue (CrashingForm.HasHandledException);

			Application.ThreadException -= CrashingForm.HandleThreadException;
		}

		class CrashingForm: Form
		{
			private static Form _thisForm;

			public CrashingForm ()
			{
				_thisForm = this;
				var btn = new Button ();
				SuspendLayout ();

				btn.Paint += OnButtonPaint;
				Controls.Add (btn);

				ResumeLayout (false);
				PerformLayout ();
			}

			private void OnButtonPaint (object sender, PaintEventArgs e)
			{
				throw new ArgumentException ();
			}

			public static bool HasHandledException { get; private set; }

			public static void HandleThreadException (object sender, ThreadExceptionEventArgs args)
			{
				_thisForm.Refresh ();
				Application.DoEvents ();
				HasHandledException = true;
				_thisForm.Close ();
			}
		}

		[Test]
		[Ignore ("causes an infinite restart loop since we're not in a separate AppDomain with nunit-lite")]
		[ExpectedException (typeof (NotSupportedException))]
		public void RestartNotSupportedExceptionTest ()
		{
			Application.Restart ();
		}
		
		[Test]
		public void OpenFormsTest ()
		{
			IntPtr dummy = IntPtr.Zero;
			
			Form f1 = null, f2 = null;
			try {
				f1 = new OpenFormsTestForm ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#1");
				dummy = f1.Handle; 
				Assert.AreEqual (0, Application.OpenForms.Count, "#2");
				f1.Close ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#3");
				f1.Dispose ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#4");

				f1 = new OpenFormsTestForm ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#5");
				f1.Show ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#6");
				f1.Close ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#7");
				f1.Dispose ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#8");

				f1 = new OpenFormsTestForm ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#9");
				dummy = f1.Handle; 
				Assert.AreEqual (0, Application.OpenForms.Count, "#10");
				f1.GetType ().GetMethod ("RecreateHandle", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.ExactBinding).Invoke (f1, new object [] {});
				Assert.AreEqual (0, Application.OpenForms.Count, "#11");
				f1.Dispose ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#12");

				f1 = new OpenFormsTestForm ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#13");
				f1.Show ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#14");
				f2 = new OpenFormsTestForm ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#15");
				f2.Show ();
				Assert.AreEqual (2, Application.OpenForms.Count, "#16");
				f1.Dispose ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#17");
				f2.Close ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#18");


				f1 = new OpenFormsTestForm ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#19");
				f1.Show ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#20");
				f2 = new OpenFormsTestForm ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#21");
				f2.Show ();
				Assert.AreEqual (2, Application.OpenForms.Count, "#22");
				f1.Visible = false;
				Assert.AreEqual (2, Application.OpenForms.Count, "#23");
				f2.Visible = false;
				Assert.AreEqual (2, Application.OpenForms.Count, "#24");
				f1.Dispose ();
				Assert.AreEqual (1, Application.OpenForms.Count, "#25");
				f2.Close ();
				Assert.AreEqual (0, Application.OpenForms.Count, "#26");

			} finally {
				if (f1 != null) {
					f1.Dispose ();
				}
				if (f2 != null) {
					f2.Dispose ();
				}
			}
			
			TestHelper.RemoveWarning (dummy);
		}
		
		[Test]
		public void MethodRaiseIdle ()
		{
			bool idle_raised = false;

			Application.Idle += new EventHandler (delegate (Object obj, EventArgs e) { idle_raised = true; });
			Application.RaiseIdle (EventArgs.Empty);
			
			Assert.AreEqual (true, idle_raised, "R1");
		}

		void Application_Idle (object sender, EventArgs e)
		{
			throw new Exception ("The method or operation is not implemented.");
		}
		
		class OpenFormsTestForm : Form 
		{
			public bool have_been_opened;
			
			public bool IsInOpenForms ()
			{
				foreach (Form form in Application.OpenForms) {
					if (form == this) {
						have_been_opened = true;
						return true;
					}
				}
				return false;
			}

			protected override void OnLoad (EventArgs e)
			{
				Assert.AreEqual (false, IsInOpenForms (), "#OnLoad-A");
				base.OnLoad (e);
				Assert.AreEqual (true, IsInOpenForms (), "#OnLoad-B");
			}

			protected override void OnCreateControl ()
			{
				Assert.AreEqual (false, IsInOpenForms (), "#OnCreateControl-A");
				base.OnCreateControl ();
				Assert.AreEqual (true, IsInOpenForms (), "#OnCreateControl-B");
			}

			// Activation may not be synchronous, causing too many false positives
			protected override void OnActivated (EventArgs e)
			{
				bool dummy = IsInOpenForms ();
				//Assert.AreEqual (true, IsInOpenForms (), "#OnActivated-A");
				base.OnActivated (e);
				dummy = IsInOpenForms ();
				//Assert.AreEqual (true, IsInOpenForms (), "#OnActivated-B");
			}

			protected override void OnClosed (EventArgs e)
			{
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnClosed-A");
				base.OnClosed (e);
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnClosed-B");
			}

			protected override void OnClosing (CancelEventArgs e)
			{
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnClosing-A");
				base.OnClosing (e);
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnClosing-B");
			}

			protected override void OnDeactivate (EventArgs e)
			{
				Assert.AreEqual (true, IsInOpenForms (), "#OnDeactivate-A");
				base.OnDeactivate (e);
				Assert.AreEqual (true, IsInOpenForms (), "#OnDeactivate-B");
			}

			protected override void OnFormClosed (FormClosedEventArgs e)
			{
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnFormClosed-A");
				base.OnFormClosed (e);
				Assert.AreEqual (false, IsInOpenForms (), "#OnFormClosed-B");
			}

			protected override void OnFormClosing (FormClosingEventArgs e)
			{
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnFormClosing-A");
				base.OnFormClosing (e);
				Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnFormClosing-B");
			}

			protected override void OnHandleCreated (EventArgs e)
			{
				Assert.AreEqual (false, IsInOpenForms (), "#OnHandleCreated-A");
				base.OnHandleCreated (e);
				Assert.AreEqual (false, IsInOpenForms (), "#OnHandleCreated-B");
			}

			protected override void OnHandleDestroyed (EventArgs e)
			{
				//Assert.AreEqual (have_been_opened, IsInOpenForms (), "#OnHandleDestroyed-A");
				base.OnHandleDestroyed (e);
				Assert.AreEqual (false, IsInOpenForms (), "#OnHandleDestroyed-B");
			}

			protected override void SetVisibleCore (bool value)
			{
				if (value)
					Assert.AreEqual (false && value, IsInOpenForms (), "#SetVisibleCore-A");
				base.SetVisibleCore (value);
				Assert.AreEqual (true, IsInOpenForms (), "#SetVisibleCore-B");
			}

			protected override void OnVisibleChanged (EventArgs e)
			{
				Assert.AreEqual (true, IsInOpenForms (), "#OnVisibleChanged-A");
				base.OnVisibleChanged (e);
				Assert.AreEqual (true, IsInOpenForms (), "#OnVisibleChanged-B");
			}

			protected override void CreateHandle ()
			{
				Assert.AreEqual (false, IsInOpenForms (), "#CreateHandle-A");
				base.CreateHandle ();
				// We have a different stack trace here, so we're not matching MS
				// Assert.AreEqual (false, IsInOpenForms (), "#CreateHandle-B");
			}

			protected override void DestroyHandle ()
			{
				//Dispose may be called several times, so this isn't correct
				//Assert.AreEqual (have_been_opened, IsInOpenForms (), "#DestroyHandle-A");
				base.DestroyHandle ();
				Assert.AreEqual (false, IsInOpenForms (), "#DestroyHandle-B");
			}

			protected override void Dispose (bool disposing)
			{
				//Dispose may be called several times, so this isn't correct
				//Assert.AreEqual (have_been_opened, IsInOpenForms (), "#Dispose-A");
				base.Dispose (disposing);
				Assert.AreEqual (false, IsInOpenForms (), "#Dispose-B");
			}
		}
	}
}
