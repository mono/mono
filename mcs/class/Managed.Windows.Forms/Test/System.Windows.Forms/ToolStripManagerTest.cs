//
// ToolStripManagerTest.cs
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
	public class ToolStripManagerTest
	{
		[Test]
		[Ignore ("This has to run all by itself.  Toolstrips in other tests register themselves and mess this up.")]
		public void BehaviorFindToolStrip ()
		{
			// Default stuff
			Assert.AreEqual (null, ToolStripManager.FindToolStrip (string.Empty), "A1");
			Assert.AreEqual (null, ToolStripManager.FindToolStrip ("MyStrip"), "A2");
			
			ToolStrip ts = new ToolStrip ();
			ts.Name = "MyStrip";

			// Basic operation
			Assert.AreSame (ts, ToolStripManager.FindToolStrip ("MyStrip"), "A3");
			
			// Dispose removes them
			ts.Dispose ();
			Assert.AreEqual (null, ToolStripManager.FindToolStrip ("MyStrip"), "A4");
			
			ts = new ToolStrip ();
			ts.Name = "MyStrip1";
			MenuStrip ms = new MenuStrip ();
			ms.Name = "MyStrip2";

			// Basic operation
			Assert.AreSame (ts, ToolStripManager.FindToolStrip ("MyStrip1"), "A5");
			Assert.AreSame (ms, ToolStripManager.FindToolStrip ("MyStrip2"), "A6");
		
			// Find unnamed strip
			StatusStrip ss = new StatusStrip ();
			Assert.AreEqual (ss, ToolStripManager.FindToolStrip (string.Empty), "A7");
			
			// Always return first unnamed strip
			ContextMenuStrip cms = new ContextMenuStrip ();
			Assert.AreEqual (ss, ToolStripManager.FindToolStrip (string.Empty), "A8");
			
			// Remove first unnamed strip, returns second one
			ss.Dispose ();
			Assert.AreEqual (cms, ToolStripManager.FindToolStrip (string.Empty), "A9");
			
			// ContextMenuStrips are included
			cms.Name = "Context";
			Assert.AreEqual (cms, ToolStripManager.FindToolStrip ("Context"), "A10");
		}
		
		[Test]
		public void MethodIsShortcutValid ()
		{
			Assert.AreEqual (true, ToolStripManager.IsValidShortcut (Keys.F1), "A1");
			Assert.AreEqual (true, ToolStripManager.IsValidShortcut (Keys.F7), "A1");
			Assert.AreEqual (true, ToolStripManager.IsValidShortcut (Keys.Shift | Keys.F1), "A1");
			Assert.AreEqual (true, ToolStripManager.IsValidShortcut (Keys.Control | Keys.F1), "A1");
			Assert.AreEqual (false, ToolStripManager.IsValidShortcut (Keys.Shift), "A1");
			Assert.AreEqual (false, ToolStripManager.IsValidShortcut (Keys.Alt), "A1");
			Assert.AreEqual (false, ToolStripManager.IsValidShortcut (Keys.D6), "A1");
			Assert.AreEqual (true, ToolStripManager.IsValidShortcut (Keys.Control | Keys.S), "A1");
			Assert.AreEqual (false, ToolStripManager.IsValidShortcut (Keys.L), "A1");
		}
		
		[Test]
		public void TwoShortcuts ()
		{
			ToolStripMenuItem tsmi = new ToolStripMenuItem ();
			tsmi.ShortcutKeys = Keys.Control | Keys.D;
			ToolStripMenuItem tsmi2 = new ToolStripMenuItem ();
			tsmi2.ShortcutKeys = Keys.Control | Keys.D;

			Assert.AreEqual (Keys.Control | Keys.D, tsmi.ShortcutKeys, "A1");
			Assert.AreEqual (Keys.Control | Keys.D, tsmi2.ShortcutKeys, "A2");
			
			ToolStrip ts = new ToolStrip ();
			ts.Items.Add (tsmi);
			ts.Items.Add (tsmi2);

			Assert.AreEqual (Keys.Control | Keys.D, tsmi.ShortcutKeys, "A3");
			Assert.AreEqual (Keys.Control | Keys.D, tsmi2.ShortcutKeys, "A4");
		}
	}
}
#endif