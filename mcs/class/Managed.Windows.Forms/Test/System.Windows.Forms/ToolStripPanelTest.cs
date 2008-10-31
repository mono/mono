//
// ToolStripPanelTests.cs
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
	public class ToolStripPanelTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();

			Assert.AreEqual (true, tsp.AutoSize, "A1");
			Assert.AreEqual (DockStyle.None, tsp.Dock, "A2");
			Assert.AreEqual ("System.Windows.Forms.Layout.FlowLayout", tsp.LayoutEngine.ToString (), "A3");
			Assert.AreEqual (false, tsp.Locked, "A4");
			Assert.AreEqual (Orientation.Horizontal, tsp.Orientation, "A5");
			Assert.AreSame (ToolStripManager.Renderer, tsp.Renderer, "A6");
			Assert.AreEqual (ToolStripRenderMode.ManagerRenderMode, tsp.RenderMode, "A7");
			Assert.AreEqual (new Padding (3, 0, 0, 0), tsp.RowMargin, "A8");
			Assert.AreEqual ("System.Windows.Forms.ToolStripPanelRow[]", tsp.Rows.ToString (), "A9");
		}

		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (0), epp.DefaultMargin, "C1");
			Assert.AreEqual (new Padding (0), epp.DefaultPadding, "C2");
		}

		[Test]
		public void PropertyAutoSize ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.AutoSize = false;
			Assert.AreEqual (false, tsp.AutoSize, "B1");
			Assert.AreEqual ("AutoSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsp.AutoSize = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDock ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.Dock = DockStyle.Left;
			Assert.AreEqual (DockStyle.Left, tsp.Dock, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsp.Dock = DockStyle.Left;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyLocked ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.Locked = true;
			Assert.AreEqual (true, tsp.Locked, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsp.Locked = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyOrientation ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.Orientation = Orientation.Vertical;
			Assert.AreEqual (Orientation.Vertical, tsp.Orientation, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsp.Orientation = Orientation.Vertical;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRenderer ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			ToolStripProfessionalRenderer pr = new ToolStripProfessionalRenderer ();

			tsp.Renderer = pr;
			Assert.AreSame (pr, tsp.Renderer, "B1");
			Assert.AreEqual (ToolStripRenderMode.Custom, tsp.RenderMode, "B1-2");
			// I refuse to call the event twice like .Net does.
			//Assert.AreEqual ("RendererChanged;RendererChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsp.Renderer = pr;
			//Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRenderMode ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.RenderMode = ToolStripRenderMode.System;
			Assert.AreEqual (ToolStripRenderMode.System, tsp.RenderMode, "B1");
			// I refuse to call the event twice like .Net does.
			//Assert.AreEqual ("RendererChanged;RendererChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsp.RenderMode = ToolStripRenderMode.System;
			//Assert.AreEqual ("RendererChanged", ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRowMargin ()
		{
			ToolStripPanel tsp = new ToolStripPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.RowMargin = new Padding (4);
			Assert.AreEqual (new Padding (4), tsp.RowMargin, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsp.RowMargin = new Padding (4);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void MethodCreateControlsInstance ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual ("System.Windows.Forms.ToolStripPanel+ToolStripPanelControlCollection", epp.CreateControlsInstance (). GetType ().ToString (), "B1");
		}

		private class EventWatcher
		{
			private string events = string.Empty;

			public EventWatcher (ToolStripPanel tsp)
			{
				tsp.AutoSizeChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("AutoSizeChanged;"); });
				tsp.RendererChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("RendererChanged;"); });
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

		private class ExposeProtectedProperties : ToolStripPanel
		{
			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
			public new ControlCollection CreateControlsInstance () { return base.CreateControlsInstance (); }
		}
	}
}
#endif