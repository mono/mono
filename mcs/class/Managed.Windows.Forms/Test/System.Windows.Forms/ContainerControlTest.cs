//
// ContainerControl class testing unit
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.Permissions;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	public class IContainerControlTest : Control, IContainerControl {

		public bool ActivateControl (Control active)
		{
			return true;
		}

		public Control  ActiveControl {
			get { return null; }
			set { ; }
		}
	}

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ContainerControlTest {

		[Test]
		public void GetContainerControl ()
		{
			ContainerControl cc = new ContainerControl ();
			Assert.IsTrue (Object.ReferenceEquals (cc, cc.GetContainerControl ()), "ContainerControl.GetContainerControl");

			Button b = new Button ();
			Assert.IsNull (b.GetContainerControl (), "Button.GetContainerControl/without parent");
			b.Parent = cc;
			Assert.IsTrue (Object.ReferenceEquals (cc, b.GetContainerControl ()), "Button.GetContainerControl");
		}

		[Test]
		public void GetContainerControl_WithoutStyle ()
		{
			IContainerControlTest cct = new IContainerControlTest ();
			Assert.IsNull (cct.GetContainerControl (), "IContainerControlTest.GetContainerControl");

			Button b = new Button ();
			b.Parent = cct;
			Assert.IsNull (b.GetContainerControl (), "Button.GetContainerControl/without parent");
		}

		[Test]
		[Category ("NotWorking")]
		public void ActiveControl ()
		{
			ContainerControl cc = new ContainerControl ();
			Control c1 = new Control ();
			cc.Controls.Add (c1);
			Control c2 = new Control ();
			cc.Controls.Add (c2);
			Control c3 = new Control ();
			cc.Controls.Add (c3);
			Assert.IsFalse (c1.Focused, "#A1");
			Assert.IsFalse (c2.Focused, "#A2");
			Assert.IsFalse (c3.Focused, "#A3");
			Assert.IsNull (cc.ActiveControl);

			cc.ActiveControl = c1;
			Assert.IsFalse (c1.Focused, "#B1");
			Assert.IsFalse (c2.Focused, "#B2");
			Assert.IsFalse (c3.Focused, "#B3");
			Assert.AreSame (c1, cc.ActiveControl, "#B4");

			cc.ActiveControl = c2;
			Assert.IsFalse (c1.Focused, "#C1");
			Assert.IsFalse (c2.Focused, "#C2");
			Assert.IsFalse (c3.Focused, "#C3");
			Assert.AreSame (c2, cc.ActiveControl, "#C4");

			c1.Focus ();
			Assert.IsFalse (c1.Focused, "#D1");
			Assert.IsFalse (c2.Focused, "#D2");
			Assert.IsFalse (c3.Focused, "#D3");
			Assert.AreSame (c2, cc.ActiveControl, "#D4");

			cc.ActiveControl = c2;
			Assert.IsFalse (c1.Focused, "#E1");
			Assert.IsFalse (c2.Focused, "#E2");
			Assert.IsFalse (c3.Focused, "#E3");
			Assert.AreSame (c2, cc.ActiveControl, "#E4");

			cc.Controls.Remove (c2);
			Assert.IsFalse (c1.Focused, "#F1");
			Assert.IsFalse (c2.Focused, "#F2");
			Assert.IsFalse (c3.Focused, "#F3");
			Assert.AreSame (c1, cc.ActiveControl, "#F3");

			cc.ActiveControl = c3;
			Assert.IsFalse (c1.Focused, "#G1");
			Assert.IsFalse (c2.Focused, "#G2");
			Assert.IsFalse (c3.Focused, "#G3");
			Assert.AreSame (c3, cc.ActiveControl, "#G4");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (cc);
			form.Show ();

			Assert.IsTrue (c1.Focused, "#H1");
			Assert.IsFalse (c2.Focused, "#H2");
			Assert.IsFalse (c3.Focused, "#H3");
			Assert.AreSame (c1, cc.ActiveControl, "#H4");

			cc.ActiveControl = c3;
			Assert.IsFalse (c1.Focused, "#I1");
			Assert.IsFalse (c2.Focused, "#I2");
			Assert.IsTrue (c3.Focused, "#I3");
			Assert.AreSame (c3, cc.ActiveControl, "#I4");

			c1.Focus ();
			Assert.IsTrue (c1.Focused, "#J1");
			Assert.IsFalse (c2.Focused, "#J2");
			Assert.IsFalse (c3.Focused, "#J3");
			Assert.AreSame (c1, cc.ActiveControl, "#J4");
		}

		[Test] // bug #80411
		[Category ("NotWorking")]
		public void ActiveControl_NoChild ()
		{
			ContainerControl cc = new ContainerControl ();
			try {
				cc.ActiveControl = new Control ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.ParamName, "#4");
				Assert.IsNull (ex.InnerException, "#5");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ActiveControl_Invisible ()
		{
			ContainerControl cc = new ContainerControl ();
			Control c1 = new Control ();
			c1.Visible = false;
			cc.Controls.Add (c1);
			Control c2 = new Control ();
			cc.Controls.Add (c2);
			cc.ActiveControl = c1;
			Assert.IsFalse (c1.Focused, "#A1");
			Assert.IsFalse (c2.Focused, "#A2");
			Assert.AreSame (c1, cc.ActiveControl, "#A3");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (cc);
			form.Show ();

			Assert.IsFalse (c1.Focused, "#B1");
			Assert.IsTrue (c2.Focused, "#B2");
			Assert.AreSame (c2, cc.ActiveControl, "#B3");

			cc.ActiveControl = c1;
			Assert.IsFalse (c1.Focused, "#C1");
			Assert.IsFalse (c2.Focused, "#C2");
			Assert.AreSame (c1, cc.ActiveControl, "#C3");
		}

		[Test]
		[Category ("NotWorking")]
		public void ActiveControl_Disabled ()
		{
			ContainerControl cc = new ContainerControl ();
			Control c1 = new Control ();
			c1.Enabled = false;
			cc.Controls.Add (c1);
			Control c2 = new Control ();
			cc.Controls.Add (c2);
			cc.ActiveControl = c1;
			Assert.IsFalse (c1.Focused, "#A1");
			Assert.IsFalse (c2.Focused, "#A2");
			Assert.AreSame (c1, cc.ActiveControl, "#A3");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (cc);
			form.Show ();

			Assert.IsFalse (c1.Focused, "#B1");
			Assert.IsTrue (c2.Focused, "#B2");
			Assert.AreSame (c2, cc.ActiveControl, "#B3");

			cc.ActiveControl = c1;
			Assert.IsFalse (c1.Focused, "#C1");
			Assert.IsTrue (c2.Focused, "#C2");
			Assert.AreSame (c1, cc.ActiveControl, "#C3");
		}

		[Test]
		[Category ("NotWorking")]
		public void ActiveControl_Null ()
		{
			ContainerControl cc = new ContainerControl ();
			Control c1 = new Control ();
			cc.Controls.Add (c1);
			Control c2 = new Control ();
			cc.Controls.Add (c2);
			cc.ActiveControl = c1;
			Assert.IsFalse (c1.Focused, "#A1");
			Assert.IsFalse (c2.Focused, "#A2");
			Assert.AreSame (c1, cc.ActiveControl, "#A3");

			cc.ActiveControl = null;
			Assert.IsFalse (c1.Focused, "#B1");
			Assert.IsFalse (c2.Focused, "#B2");
			Assert.IsNull (cc.ActiveControl, "#B3");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (cc);
			form.Show ();

			Assert.IsTrue (c1.Focused, "#C1");
			Assert.IsFalse (c2.Focused, "#C2");
			Assert.AreSame (c1, cc.ActiveControl, "#C3");

			cc.ActiveControl = c2;
			Assert.IsFalse (c1.Focused, "#D1");
			Assert.IsTrue (c2.Focused, "#D2");
			Assert.AreSame (c2, cc.ActiveControl, "#D3");

			cc.ActiveControl = null;
			Assert.IsFalse (c1.Focused, "#E1");
			Assert.IsFalse (c2.Focused, "#E2");
			Assert.IsNull (cc.ActiveControl, "#E3");
		}
	}
}
