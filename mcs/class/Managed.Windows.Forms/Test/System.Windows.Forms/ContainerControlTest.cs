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
using C=System.ComponentModel;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Collections;
using NUnit.Framework;
using System.Drawing;

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

	public class FormCustom: Form {
		public bool record;
		public Queue events;

		public FormCustom(string name, bool record, Queue events) {
			base.Name = name;
			this.record = record;
			this.events = events;
		}

		protected override void OnValidating(C.CancelEventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnValidating", this, this.Name));
			base.OnValidating (e);
		}

		protected override void OnValidated(EventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnValidated", this, this.Name));
			base.OnValidated (e);
		}

		protected override void OnGotFocus(EventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnGotFocus", this, this.Name));
			base.OnGotFocus (e);
		}

	}

	public class ContainerControlCustom: ContainerControl {
		public bool record;
		public Queue events;

		public ContainerControlCustom(string name, bool record, Queue events) {
			base.Name = name;
			this.record = record;
			this.events = events;
		}

		protected override void OnValidating(C.CancelEventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnValidating", this, this.Name));
			base.OnValidating (e);
		}

		protected override void OnValidated(EventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnValidated", this, this.Name));
			base.OnValidated (e);
		}
		
		protected override void OnGotFocus(EventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnGotFocus", this, this.Name));
			base.OnGotFocus (e);
		}

		protected override void Select(bool directed, bool forward) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:Select", this, this.Name));
			base.Select (directed, forward);
		}
	}

	public class UserControlCustom: UserControl {
		public bool record;
		public Queue events;

		public UserControlCustom(string name, bool record, Queue events) {
			base.Name = name;
			this.record = record;
			this.events = events;
		}
		protected override void OnValidating(C.CancelEventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnValidating", this, this.Name));
			base.OnValidating (e);
		}

		protected override void OnValidated(EventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnValidated", this, this.Name));
			base.OnValidated (e);
		}

		protected override void OnGotFocus(EventArgs e) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:OnGotFocus", this, this.Name));
			base.OnGotFocus (e);
		}

		protected override void Select(bool directed, bool forward) {
			if (this.record)
				events.Enqueue(String.Format("{0}:{1}:Select", this, this.Name));
			base.Select (directed, forward);
		}
	}

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ContainerControlTest {
		[Test]
		public void RemoveActiveControlTest ()
		{
			ContainerControl cc = new ContainerControl();
			TextBox txt = new TextBox ();
			cc.Controls.Add (txt);
			Assert.IsFalse (cc.ActiveControl == txt, "#01");
			cc.ActiveControl = txt;
			Assert.AreSame (cc.ActiveControl, txt, "#02");
			cc.Controls.Remove (txt);
			Assert.IsNull (cc.ActiveControl, "#03");
		}
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
		[ExpectedException (typeof (ArgumentException))]
		public void ActiveControlNotChildTest ()
		{
			ContainerControl c = new ContainerControl ();
			c.ActiveControl = new Control ();
		}

		[Test]
		public void Validation() {
			Queue events = new Queue();

			FormCustom form = new FormCustom("form1", true, events);
			ContainerControlCustom container1 = new ContainerControlCustom("container1", true, events);
			ContainerControlCustom container2 = new ContainerControlCustom("container2", true, events);
			ContainerControlCustom container3 = new ContainerControlCustom("container3", true, events);
			UserControlCustom userctl1 = new UserControlCustom("userctl1", true, events);
			UserControlCustom userctl2 = new UserControlCustom("userctl2", true, events);
			UserControlCustom userctl3 = new UserControlCustom("userctl3", true, events);

			container2.Controls.Add(userctl2);
			container2.Controls.Add(userctl3);
			container1.Controls.Add(userctl1);
			form.Controls.Add(container1);
			form.Controls.Add(container2);
			form.Controls.Add(container3);

			form.Show();

			object s;

			events.Enqueue("START");
			container3.Select();
			events.Enqueue("END");
			events.Enqueue("START");
			container1.Select();
			events.Enqueue("END");
			events.Enqueue("START");
			container2.Select();
			events.Enqueue("END");
			events.Enqueue("START");
			userctl1.Select();
			events.Enqueue("END");
			events.Enqueue("START");
			userctl2.Select();
			events.Enqueue("END");
			events.Enqueue("START");
			userctl2.Select();
			events.Enqueue("END");


			while (events.Count > 0) {
				s = events.Dequeue();
				Console.WriteLine(s.ToString());
			}

			events.Clear();

			form.Close();
			userctl1.Dispose();
			userctl2.Dispose();
			userctl3.Dispose();
			container1.Dispose();
			container1.Dispose();
			form.Dispose();

		}
		
		[Test]
		public void MnemonicCalledWhenCanSelectFalse ()
		{
			MyForm f = new MyForm ();
			f.ShowInTaskbar = false;
			
			MyControl c = new MyControl ();
			
			f.Controls.Add (c);
			f.Show ();
			
			Assert.AreEqual (false, c.CanSelect, "A1");
			f.PublicProcessMnemonic ('b');
			
			Assert.AreEqual (true, c.mnemonic_called, "A2");
		}
		
		private class MyForm : Form
		{
			public bool PublicProcessMnemonic (char charCode)
			{
				return this.ProcessMnemonic (charCode);
			}
		}
		
		private class MyControl : Control
		{
			public bool mnemonic_called;
			
			public MyControl ()
			{
				SetStyle (ControlStyles.Selectable, false);
			}
			
			protected override bool ProcessMnemonic (char charCode)
			{
				mnemonic_called = true;
				return base.ProcessMnemonic (charCode);
			}
		}

#if NET_2_0
		[Test]
		[Category ("NotWorking")]  // Depends on fonts *AND* DPI, how useless is that? (Values are Vista/96DPI)
		public void AutoScaling ()
		{
			ContainerControl c = new ContainerControl ();
			c.ClientSize = new Size (100, 100);

			Assert.AreEqual (new SizeF (0, 0), c.CurrentAutoScaleDimensions, "A1");
			Assert.AreEqual (new SizeF (0, 0), c.AutoScaleDimensions, "A2");
			Assert.AreEqual (new Size (100, 100), c.ClientSize, "A3");

			c.AutoScaleMode = AutoScaleMode.Dpi;
			Assert.AreEqual (new SizeF (96, 96), c.CurrentAutoScaleDimensions, "A4");
			Assert.AreEqual (new SizeF (96, 96), c.AutoScaleDimensions, "A5");
			Assert.AreEqual (new Size (100, 100), c.ClientSize, "A6");

			c.AutoScaleMode = AutoScaleMode.Font;
			Assert.AreEqual (new SizeF (6, 13), c.CurrentAutoScaleDimensions, "A7");
			Assert.AreEqual (new SizeF (6, 13), c.AutoScaleDimensions, "A8");
			Assert.AreEqual (new Size (100, 100), c.ClientSize, "A9");

			c.Font = new Font ("Arial", 15);
			Assert.AreEqual (new SizeF (11, 23), c.CurrentAutoScaleDimensions, "A10");
			Assert.AreEqual (new SizeF (11, 23), c.AutoScaleDimensions, "A11");
			Assert.AreEqual (new Size (183, 177), c.ClientSize, "A12");

			c.Font = new Font ("Tahoma", 12);
			Assert.AreEqual (new SizeF (9, 19), c.CurrentAutoScaleDimensions, "A13");
			Assert.AreEqual (new SizeF (9, 19), c.AutoScaleDimensions, "A14");
			Assert.AreEqual (new Size (150, 146), c.ClientSize, "A15");

			c.Font = new Font ("Times New Roman", 14);
			Assert.AreEqual (new SizeF (10, 21), c.CurrentAutoScaleDimensions, "A16");
			Assert.AreEqual (new SizeF (10, 21), c.AutoScaleDimensions, "A17");
			Assert.AreEqual (new Size (167, 161), c.ClientSize, "A18");
		}
#endif
	}
}
