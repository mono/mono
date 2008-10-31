//
// ToolStripContainerTests.cs
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
// Copyright (c) 2007 Jonathan Pobst
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
	public class ToolStripContainerTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripContainer tsc = new ToolStripContainer ();

			Assert.AreEqual ("System.Windows.Forms.ToolStripPanel", tsc.BottomToolStripPanel.ToString (), "A1");
			Assert.AreEqual (true, tsc.BottomToolStripPanelVisible, "A2");
			Assert.AreEqual ("System.Windows.Forms.ToolStripContentPanel", tsc.ContentPanel.GetType ().ToString (), "A3");
			Assert.AreEqual ("System.Windows.Forms.ToolStripPanel", tsc.LeftToolStripPanel.ToString (), "A4");
			Assert.AreEqual (true, tsc.LeftToolStripPanelVisible, "A5");
			Assert.AreEqual ("System.Windows.Forms.ToolStripPanel", tsc.RightToolStripPanel.ToString (), "A6");
			Assert.AreEqual (true, tsc.RightToolStripPanelVisible, "A7");
			Assert.AreEqual ("System.Windows.Forms.ToolStripPanel", tsc.TopToolStripPanel.ToString (), "A8");
			Assert.AreEqual (true, tsc.TopToolStripPanelVisible, "A9");
		}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Size (150, 175), epp.DefaultSize, "C1");
		}

		[Test]
		public void PropertyBottomToolStripPanelVisible ()
		{
			ToolStripContainer tsc = new ToolStripContainer ();

			tsc.BottomToolStripPanelVisible = false; ;
			Assert.AreEqual (false, tsc.BottomToolStripPanelVisible, "B1");
		}

		[Test]
		public void PropertyLeftToolStripPanelVisible ()
		{
			ToolStripContainer tsc = new ToolStripContainer ();

			tsc.LeftToolStripPanelVisible = false; ;
			Assert.AreEqual (false, tsc.LeftToolStripPanelVisible, "B1");
		}

		[Test]
		public void PropertyRightToolStripPanelVisible ()
		{
			ToolStripContainer tsc = new ToolStripContainer ();

			tsc.RightToolStripPanelVisible = false; ;
			Assert.AreEqual (false, tsc.RightToolStripPanelVisible, "B1");
		}

		[Test]
		public void PropertyTopToolStripPanelVisible ()
		{
			ToolStripContainer tsc = new ToolStripContainer ();

			tsc.TopToolStripPanelVisible = false; ;
			Assert.AreEqual (false, tsc.TopToolStripPanelVisible, "B1");
		}

		[Test]
		public void MethodCreateControlsInstance ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual ("System.Windows.Forms.ToolStripContainer+ToolStripContainerTypedControlCollection", epp.CreateControlsInstance (). GetType ().ToString (), "B1");
		}

		[Test]
		public void ControlStyle ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			ControlStyles cs = ControlStyles.ContainerControl;
			cs |= ControlStyles.UserPaint;
			cs |= ControlStyles.StandardClick;
			cs |= ControlStyles.SupportsTransparentBackColor;
			cs |= ControlStyles.StandardDoubleClick;
			cs |= ControlStyles.Selectable;
			cs |= ControlStyles.ResizeRedraw;
			cs |= ControlStyles.UseTextForAccessibility;

			Assert.AreEqual (cs, epp.GetControlStyles (), "Styles");
		}

		private class ExposeProtectedProperties : ToolStripContainer
		{
			public new Size DefaultSize { get { return base.DefaultSize; } }
			public new ControlCollection CreateControlsInstance () { return base.CreateControlsInstance (); }

			public ControlStyles GetControlStyles ()
			{
				ControlStyles retval = (ControlStyles)0;

				foreach (ControlStyles cs in Enum.GetValues (typeof (ControlStyles)))
					if (this.GetStyle (cs) == true)
						retval |= cs;

				return retval;
			}
		}
	}
}
#endif