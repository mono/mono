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
	}
}
