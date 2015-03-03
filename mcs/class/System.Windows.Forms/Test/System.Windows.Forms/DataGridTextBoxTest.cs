//
// Copyright (c) 2007 Novell, Inc.
//
//

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	public class DataGridTextBoxTest  : TestHelper {
		class DataGridMock : DataGrid {
			protected override void ColumnStartedEditing (Rectangle bounds)
			{
				// don't do anything here.
			}

			protected override void GridVScrolled (object sender, ScrollEventArgs se)
			{
				Console.WriteLine (Environment.StackTrace);
			}

			protected override void WndProc (ref Message m)
			{
				Console.WriteLine (Environment.StackTrace);
				base.WndProc (ref m);
			}
		}

		class TextBoxPoker : DataGridTextBox {
			public bool ProcessKeyPreviewCalled = false;
			public bool ProcessKeyPreviewReturnValue;

			public bool ProcessKeyEventArgsCalled = false;
			public bool ProcessKeyEventArgsReturnValue;

			public TextBoxPoker ()
			{
				CreateHandle ();
			}

			public void DoOnKeyPress (KeyPressEventArgs e)
			{
				base.OnKeyPress (e);
			}

			public void DoOnMouseWheel (MouseEventArgs e)
			{
				base.OnMouseWheel (e);
			}

			public bool DoProcessKeyMessage (ref Message m)
			{
				return base.ProcessKeyMessage (ref m);
			}

			public void DoWndProc (ref Message m)
			{
				base.WndProc (ref m);
			}

			protected override bool ProcessKeyEventArgs (ref Message msg)
			{
				bool rv = base.ProcessKeyEventArgs (ref msg);
				ProcessKeyEventArgsCalled = true;
				ProcessKeyEventArgsReturnValue = rv;
				return base.ProcessKeyEventArgs (ref msg);
			}

			protected override bool ProcessKeyPreview(ref Message msg)
			{
				bool rv = base.ProcessKeyPreview (ref msg);
				ProcessKeyPreviewCalled = true;
				ProcessKeyPreviewReturnValue = rv;
				return rv;
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void TestDefaults ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			Assert.AreEqual (SystemColors.Window, tb.BackColor, "1");
			Assert.IsFalse (tb.AcceptsTab, "2");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestKeyPress_NoGrid ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			tb.DoOnKeyPress (new KeyPressEventArgs ('a'));
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void TestKeyPress_GridButNoColumns ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			DataGrid dg = new DataGrid ();
			tb.SetDataGrid (dg);
			tb.DoOnKeyPress (new KeyPressEventArgs ('a'));
		}

		[Test]
		public void TestKeyPress ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			DataGridMock dg = new DataGridMock ();

			tb.SetDataGrid (dg);
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1");

			tb.DoOnKeyPress (new KeyPressEventArgs ('a'));
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "2");
			Assert.AreEqual ("", tb.Text, "3");

			tb.ReadOnly = true;
			tb.IsInEditOrNavigateMode = true;
			tb.DoOnKeyPress (new KeyPressEventArgs ('a'));
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "4");
			Assert.AreEqual ("", tb.Text, "5");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[Category ("NotWorking")]
		public void TestMouseWheel_NoGrid ()
		{
			TextBoxPoker tb = new TextBoxPoker ();

			tb.DoOnMouseWheel (new MouseEventArgs (MouseButtons.None, 0, 0, 0, 10));
		}

		bool mouse_wheel_raised;
		bool mouse_down_raised;

		void mouse_wheel_handler (object sender, MouseEventArgs e)
		{
			mouse_wheel_raised = true;
		}

		void mouse_down_handler (object sender, MouseEventArgs e)
		{
			mouse_down_raised = true;
		}

		[Test]
		[Category ("NotWorking")]
		public void TestMouseWheel ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			DataGridMock dg = new DataGridMock ();

			tb.MouseWheel += new MouseEventHandler (mouse_wheel_handler);

			tb.SetDataGrid (dg);

			mouse_wheel_raised = false;
			tb.DoOnMouseWheel (new MouseEventArgs (MouseButtons.None, 0, 0, 0, 10));
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1");
			Assert.IsFalse (mouse_wheel_raised, "2");

			tb.IsInEditOrNavigateMode = false;
			tb.DoOnMouseWheel (new MouseEventArgs (MouseButtons.None, 0, 0, 0, 10));
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "3");
			Assert.IsFalse (mouse_wheel_raised, "4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestProcessKeyMessage_WM_CHAR ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			DataGridMock dg = new DataGridMock ();
			Message m;

			dg.Controls.Add (tb);

			tb.SetDataGrid (dg);

			tb.IsInEditOrNavigateMode = true;

			/* test Enter key behavior */
			m = new Message ();
			m.Msg = 0x0102 /* WM_CHAR */;
			m.WParam = (IntPtr)Keys.Enter;

			bool rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0102, m.Msg, "1");
			Assert.AreEqual (Keys.Enter, (Keys)m.WParam.ToInt32(), "2");
			Assert.IsTrue  (rv, "3");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestProcessKeyMessage_WM_KEYDOWN ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			DataGridMock dg = new DataGridMock ();
			Message m;
			bool rv;

			dg.Controls.Add (tb);
			tb.SetDataGrid (dg);

			/* test F2 key behavior */
			tb.IsInEditOrNavigateMode = false;
			tb.Text = "hello world";
			tb.SelectionStart = 0;
			tb.SelectionLength = 5;

			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.F2;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "1");
			Assert.AreEqual (Keys.F2, (Keys)m.WParam.ToInt32(), "2");
			Assert.IsTrue  (rv, "3");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "4");

			Assert.AreEqual (0, tb.SelectionLength, "5");
			Assert.AreEqual (tb.Text.Length, tb.SelectionStart, "6");
			Assert.IsFalse (tb.ProcessKeyPreviewCalled, "7");
			Assert.IsFalse (tb.ProcessKeyEventArgsCalled, "8");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test enter behavior */
			tb.IsInEditOrNavigateMode = true;
			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Enter;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "9");
			Assert.AreEqual (Keys.Enter, (Keys)m.WParam.ToInt32(), "10");
			Assert.IsFalse  (rv, "11");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "12");
			Assert.IsTrue (tb.ProcessKeyPreviewCalled, "13");
			Assert.IsFalse (tb.ProcessKeyPreviewReturnValue, "14");
			Assert.IsFalse (tb.ProcessKeyEventArgsCalled, "15");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test left behavior (within the string) */
			tb.IsInEditOrNavigateMode = true;
			tb.Text = "hello world";
			tb.SelectionStart = 5;
			tb.SelectionLength = 0;

			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Left;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "16");
			Assert.AreEqual (Keys.Left, (Keys)m.WParam.ToInt32(), "17");
			Assert.IsFalse  (rv, "18");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "19");
			Assert.IsFalse (tb.ProcessKeyPreviewCalled, "20");
			Assert.IsTrue (tb.ProcessKeyEventArgsCalled, "21");
			Assert.IsFalse (tb.ProcessKeyEventArgsReturnValue, "21.5");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test left behavior (at the left-most position) */
			tb.IsInEditOrNavigateMode = true;
			tb.Text = "hello world";
			tb.SelectionStart = 0;
			tb.SelectionLength = 0;

			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Left;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "22");
			Assert.AreEqual (Keys.Left, (Keys)m.WParam.ToInt32(), "23");
			Assert.IsFalse  (rv, "24");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "25");
			Assert.IsTrue (tb.ProcessKeyPreviewCalled, "26");
			Assert.IsFalse (tb.ProcessKeyPreviewReturnValue, "26");
			Assert.IsFalse (tb.ProcessKeyEventArgsCalled, "27");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test right behavior (within the string) */
			tb.IsInEditOrNavigateMode = true;
			tb.Text = "hello world";
			tb.SelectionStart = 5;
			tb.SelectionLength = 0;

			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Right;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "28");
			Assert.AreEqual (Keys.Right, (Keys)m.WParam.ToInt32(), "29");
			Assert.IsFalse  (rv, "30");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "31");
			Assert.IsFalse (tb.ProcessKeyPreviewCalled, "32");
			Assert.IsTrue (tb.ProcessKeyEventArgsCalled, "33");
			Assert.IsFalse (tb.ProcessKeyEventArgsReturnValue, "33.5");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test right behavior (at the left-most position) */
			tb.IsInEditOrNavigateMode = true;
			tb.Text = "hello world";
			tb.SelectionStart = tb.Text.Length;
			tb.SelectionLength = 0;

			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Right;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "34");
			Assert.AreEqual (Keys.Right, (Keys)m.WParam.ToInt32(), "35");
			Assert.IsFalse  (rv, "36");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "37");
			Assert.IsTrue (tb.ProcessKeyPreviewCalled, "38");
			Assert.IsFalse (tb.ProcessKeyPreviewReturnValue, "39");
			Assert.IsFalse (tb.ProcessKeyEventArgsCalled, "40");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test Tab behavior */
			tb.IsInEditOrNavigateMode = false;
			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Tab;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "41");
			Assert.AreEqual (Keys.Tab, (Keys)m.WParam.ToInt32(), "42");
			Assert.IsFalse  (rv, "43");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "44");
			Assert.IsFalse (tb.ProcessKeyPreviewCalled, "45");
			Assert.IsTrue (tb.ProcessKeyEventArgsCalled, "46");
			Assert.IsFalse (tb.ProcessKeyEventArgsReturnValue, "46.5");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test Up behavior */
			tb.IsInEditOrNavigateMode = false;
			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Up;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "47");
			Assert.AreEqual (Keys.Up, (Keys)m.WParam.ToInt32(), "48");
			Assert.IsFalse  (rv, "49");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "50");
			Assert.IsTrue (tb.ProcessKeyPreviewCalled, "51");
			Assert.IsFalse (tb.ProcessKeyPreviewReturnValue, "52");
			Assert.IsFalse (tb.ProcessKeyEventArgsCalled, "53");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;

			/* test Down behavior */
			tb.IsInEditOrNavigateMode = false;
			m = new Message ();
			m.Msg = 0x0100 /* WM_KEYDOWN */;
			m.WParam = (IntPtr)Keys.Down;

			rv = tb.DoProcessKeyMessage (ref m);
			Assert.AreEqual (0x0100, m.Msg, "54");
			Assert.AreEqual (Keys.Down, (Keys)m.WParam.ToInt32(), "55");
			Assert.IsFalse  (rv, "56");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "57");
			Assert.IsTrue (tb.ProcessKeyPreviewCalled, "58");
			Assert.IsFalse (tb.ProcessKeyPreviewReturnValue, "59");
			Assert.IsFalse (tb.ProcessKeyEventArgsCalled, "60");
			tb.ProcessKeyPreviewCalled = false;
			tb.ProcessKeyEventArgsCalled = false;
		}

		[Test]
		[Category ("NotWorking")]
		public void TestWndProc_WM_LBUTTONDOWN ()
		{
			TextBoxPoker tb = new TextBoxPoker ();
			DataGridMock dg = new DataGridMock ();
			Message m;

			tb.SetDataGrid (dg);

			tb.MouseDown += new MouseEventHandler (mouse_down_handler);

			tb.IsInEditOrNavigateMode = true;

			m = new Message ();
			m.Msg = 0x0201 /* WM_LBUTTONDOWN */;
			m.LParam=(IntPtr) (10 << 16 | 10);

			tb.DoWndProc (ref m);

			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1");
			Assert.IsTrue (mouse_down_raised, "2");

			tb.IsInEditOrNavigateMode = false;

			m = new Message ();
			m.Msg = 0x0201 /* WM_LBUTTONDOWN */;
			m.LParam=(IntPtr) (10 << 16 | 10);

			tb.DoWndProc (ref m);

			Assert.IsFalse (tb.IsInEditOrNavigateMode, "3");
			Assert.IsTrue (mouse_down_raised, "4");
		}
	}
}
