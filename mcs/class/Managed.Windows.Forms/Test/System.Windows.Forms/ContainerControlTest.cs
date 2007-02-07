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
	}
}
