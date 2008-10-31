//
// ToolStripContentPanelTests.cs
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
	public class ToolStripContentPanelTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripContentPanel tsp = new ToolStripContentPanel ();

			Assert.AreEqual (SystemColors.Control, tsp.BackColor, "A1");
			Assert.AreEqual ("System.Windows.Forms.ToolStripSystemRenderer", tsp.Renderer.ToString (), "A2");
			Assert.AreEqual (ToolStripRenderMode.System, tsp.RenderMode, "A3");
		}

		[Test]
		public void PropertyBackColor ()
		{
			ToolStripContentPanel tsp = new ToolStripContentPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.BackColor = Color.Green;
			Assert.AreEqual (Color.Green, tsp.BackColor, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsp.BackColor = Color.Green;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRenderer ()
		{
			ToolStripContentPanel tsp = new ToolStripContentPanel ();
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
			ToolStripContentPanel tsp = new ToolStripContentPanel ();
			EventWatcher ew = new EventWatcher (tsp);

			tsp.RenderMode = ToolStripRenderMode.ManagerRenderMode;
			Assert.AreEqual (ToolStripRenderMode.ManagerRenderMode, tsp.RenderMode, "B1");
			// I refuse to call the event twice like .Net does.
			//Assert.AreEqual ("RendererChanged;RendererChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsp.RenderMode = ToolStripRenderMode.ManagerRenderMode;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		private class EventWatcher
		{
			private string events = string.Empty;

			public EventWatcher (ToolStripContentPanel tsp)
			{
				tsp.Load += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Load;"); });
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
	}
}
#endif