//
// ToolStripSeparatorTests.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripSeparatorTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripSeparator tsi = new ToolStripSeparator ();

			Assert.AreEqual (false, tsi.CanSelect, "A1");
		}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (0), epp.DefaultMargin, "C1");
			Assert.AreEqual (new Size (6, 6), epp.DefaultSize, "C2");
		}
		
		[Test]
		public void Accessibility ()
		{
			ToolStripSeparator tsi = new ToolStripSeparator ();
			AccessibleObject ao = tsi.AccessibilityObject;

			Assert.AreEqual ("ToolStripItemAccessibleObject: Owner = " + tsi.ToString (), ao.ToString (), "L");
			Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L1");
			Assert.AreEqual ("Press", ao.DefaultAction, "L2");
			Assert.AreEqual (null, ao.Description, "L3");
			Assert.AreEqual (null, ao.Help, "L4");
			Assert.AreEqual (string.Empty, ao.KeyboardShortcut, "L5");
			Assert.AreEqual (string.Empty, ao.Name, "L6");
			Assert.AreEqual (null, ao.Parent, "L7");
			Assert.AreEqual (AccessibleRole.Separator, ao.Role, "L8");
			Assert.AreEqual (AccessibleStates.None, ao.State, "L9");
			Assert.AreEqual (string.Empty, ao.Value, "L10");

			tsi.Name = "Label1";
			tsi.Text = "Test Label";
			tsi.AccessibleDescription = "Label Desc";

			Assert.AreEqual (Rectangle.Empty, ao.Bounds, "L11");
			Assert.AreEqual ("Press", ao.DefaultAction, "L12");
			Assert.AreEqual ("Label Desc", ao.Description, "L13");
			Assert.AreEqual (null, ao.Help, "L14");
			Assert.AreEqual (string.Empty, ao.KeyboardShortcut, "L15");
			Assert.AreEqual ("Test Label", ao.Name, "L16");
			Assert.AreEqual (null, ao.Parent, "L17");
			Assert.AreEqual (AccessibleRole.Separator, ao.Role, "L18");
			Assert.AreEqual (AccessibleStates.None, ao.State, "L19");
			Assert.AreEqual (string.Empty, ao.Value, "L20");

			tsi.AccessibleName = "Access Label";
			Assert.AreEqual ("Access Label", ao.Name, "L21");

			tsi.Text = "Test Label";
			Assert.AreEqual ("Access Label", ao.Name, "L22");

			tsi.AccessibleDefaultActionDescription = "AAA";
			Assert.AreEqual ("AAA", tsi.AccessibleDefaultActionDescription, "L23");
		}
		
		private class ExposeProtectedProperties : ToolStripSeparator
		{
			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
		}
	}
}
#endif