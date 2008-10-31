//
// ToolStripProgressBarTests.cs
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
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripProgressBarTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();

			Assert.AreEqual (100, tsi.MarqueeAnimationSpeed, "A1");
			Assert.AreEqual (100, tsi.Maximum, "A2");
			Assert.AreEqual (0, tsi.Minimum, "A3");
			Assert.AreEqual ("System.Windows.Forms.ProgressBar", tsi.ProgressBar.GetType ().ToString (), "A4");
			Assert.AreEqual (false, tsi.RightToLeftLayout, "A5");
			Assert.AreEqual (10, tsi.Step, "A6");
			Assert.AreEqual (ProgressBarStyle.Blocks, tsi.Style, "A7");
			Assert.AreEqual (string.Empty, tsi.Text, "A8");
			Assert.AreEqual (0, tsi.Value, "A9");

			tsi = new ToolStripProgressBar ("Bob");
			Assert.AreEqual ("Bob", tsi.Name, "A10");
			Assert.AreEqual (string.Empty, tsi.Control.Name, "A11");
		}
	
		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (1, 2, 1, 1), epp.DefaultMargin, "C1");
			Assert.AreEqual (new Size (100, 15), epp.DefaultSize, "C2");
		}

		[Test]
		public void PropertyMarqueeAnimationSpeed ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.MarqueeAnimationSpeed = 200;
			Assert.AreEqual (200, tsi.MarqueeAnimationSpeed, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.MarqueeAnimationSpeed = 200;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMaximum ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Maximum = 200;
			Assert.AreEqual (200, tsi.Maximum, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Maximum = 200;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMinimum ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Minimum = 200;
			Assert.AreEqual (200, tsi.Minimum, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Minimum = 200;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRightToLeft ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.RightToLeftLayout = true;
			Assert.AreEqual (true, tsi.RightToLeftLayout, "B1");
			Assert.AreEqual ("RightToLeftLayoutChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.RightToLeftLayout = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}


		[Test]
		public void PropertyStep ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Step = 200;
			Assert.AreEqual (200, tsi.Step, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Step = 200;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyStyle ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Style = ProgressBarStyle.Continuous;
			Assert.AreEqual (ProgressBarStyle.Continuous, tsi.Style, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Style = ProgressBarStyle.Continuous;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyText ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Text = "Hi";
			Assert.AreEqual ("Hi", tsi.Text, "B1");
			Assert.AreEqual ("Hi", tsi.ProgressBar.Text, "B2");
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");

			ew.Clear ();
			tsi.Text = "Hi";
			Assert.AreEqual (string.Empty, ew.ToString (), "B4");
		}

		[Test]
		public void PropertyValue ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Value = 30;
			Assert.AreEqual (30, tsi.Value, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Value = 30;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PropertyValueAOORE ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();

			tsi.Value = 200;
		}

		[Test]
		public void BehaviorIncrement ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();
			
			tsi.Increment (14);
			Assert.AreEqual (14, tsi.Value, "B1");

			tsi.Increment (104);
			Assert.AreEqual (100, tsi.Value, "B2");

			tsi.Increment (-245);
			Assert.AreEqual (0, tsi.Value, "B3");
		}

		[Test]
		public void BehaviorPerformStep ()
		{
			ToolStripProgressBar tsi = new ToolStripProgressBar ();

			tsi.PerformStep ();
			Assert.AreEqual (10, tsi.Value, "B1");
		}

		//[Test]
		//public void PropertyAnchorAndDocking ()
		//{
		//        ToolStripItem ts = new NullToolStripItem ();

		//        ts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Bottom, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.None, ts.Dock, "A2");

		//        ts.Anchor = AnchorStyles.Left | AnchorStyles.Right;

		//        Assert.AreEqual (AnchorStyles.Left | AnchorStyles.Right, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.None, ts.Dock, "A2");

		//        ts.Dock = DockStyle.Left;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.Left, ts.Dock, "A2");

		//        ts.Dock = DockStyle.None;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.None, ts.Dock, "A2");

		//        ts.Dock = DockStyle.Top;

		//        Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, ts.Anchor, "A1");
		//        Assert.AreEqual (DockStyle.Top, ts.Dock, "A2");
		//}
		
		//[Test]
		//[Ignore ("Accessibility still needs some work")]
		//public void Accessibility ()
		//{
		//        ToolStripControlHost tsi = new ToolStripControlHost (new Button ());
		//        AccessibleObject ao = tsi.AccessibilityObject;

		//        Assert.AreEqual ("Press", ao.DefaultAction, "L2");
		//        Assert.AreEqual (null, ao.Description, "L3");
		//        Assert.AreEqual (null, ao.Help, "L4");
		//        Assert.AreEqual (null, ao.KeyboardShortcut, "L5");
		//        Assert.AreEqual (null, ao.Name, "L6");
		//        Assert.AreEqual (AccessibleRole.PushButton, ao.Role, "L8");
		//        Assert.AreEqual (AccessibleStates.None, ao.State, "L9");
		//        Assert.AreEqual (null, ao.Value, "L10");

		//        tsi.Name = "Label1";
		//        tsi.Text = "Test Label";
		//        tsi.AccessibleDescription = "Label Desc";

		//        Assert.AreEqual ("Press", ao.DefaultAction, "L12");
		//        Assert.AreEqual ("Label Desc", ao.Description, "L13");
		//        Assert.AreEqual (null, ao.Help, "L14");
		//        Assert.AreEqual (null, ao.KeyboardShortcut, "L15");
		//        //Assert.AreEqual ("Test Label", ao.Name, "L16");
		//        Assert.AreEqual (AccessibleRole.PushButton, ao.Role, "L18");
		//        Assert.AreEqual (AccessibleStates.None, ao.State, "L19");
		//        Assert.AreEqual (null, ao.Value, "L20");

		//        tsi.AccessibleName = "Access Label";
		//        Assert.AreEqual ("Access Label", ao.Name, "L21");

		//        tsi.Text = "Test Label";
		//        Assert.AreEqual ("Access Label", ao.Name, "L22");

		//        tsi.AccessibleDefaultActionDescription = "AAA";
		//        Assert.AreEqual ("AAA", tsi.AccessibleDefaultActionDescription, "L23");
		//}

		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStripProgressBar tsi)
			{
				tsi.RightToLeftLayoutChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("RightToLeftLayoutChanged;"); });
			}

			public override string ToString ()
			{
				return events.TrimEnd (';');
			}
			
			public void Clear ()
			{
				events = string.Empty;
			}
		}
		
		private class ExposeProtectedProperties : ToolStripProgressBar
		{
			public ExposeProtectedProperties () : base () {}

			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
		}
	}
}
#endif