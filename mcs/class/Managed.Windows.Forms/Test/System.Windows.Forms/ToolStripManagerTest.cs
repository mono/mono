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
		public void BehaviorTwoShortcuts ()
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
		
		[Test]
		public void MethodMergeToolStripsAppend ()
		{
			// MergeAction = Append
			ToolStrip ts1 = new ToolStrip ();
			ToolStrip ts2 = new ToolStrip ();

			ts1.Items.Add ("ts1-A");
			ts1.Items.Add ("ts1-B");
			ts1.Items.Add ("ts1-C");
			ts1.Items.Add ("ts1-D");

			ts2.Items.Add ("ts2-A");
			ts2.Items.Add ("ts2-B");
			ts2.Items.Add ("ts2-C");
			ts2.Items.Add ("ts2-D");

			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (8, ts1.Items.Count, "M1");
			Assert.AreEqual (0, ts2.Items.Count, "M2");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Text, "M3-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Text, "M3-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Text, "M3-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Text, "M3-4");
			Assert.AreEqual ("ts2-A", ts1.Items[4].Text, "M3-5");
			Assert.AreEqual ("ts2-B", ts1.Items[5].Text, "M3-6");
			Assert.AreEqual ("ts2-C", ts1.Items[6].Text, "M3-7");
			Assert.AreEqual ("ts2-D", ts1.Items[7].Text, "M3-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M4");
			Assert.AreEqual (4, ts2.Items.Count, "M5");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Text, "M6-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Text, "M6-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Text, "M6-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Text, "M6-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Text, "M6-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Text, "M6-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Text, "M6-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Text, "M6-8");

			// Do merge twice, as it helps verify things got back
			// to the proper state in the unmerge
			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (8, ts1.Items.Count, "M7");
			Assert.AreEqual (0, ts2.Items.Count, "M8");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Text, "M9-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Text, "M9-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Text, "M9-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Text, "M9-4");
			Assert.AreEqual ("ts2-A", ts1.Items[4].Text, "M9-5");
			Assert.AreEqual ("ts2-B", ts1.Items[5].Text, "M9-6");
			Assert.AreEqual ("ts2-C", ts1.Items[6].Text, "M9-7");
			Assert.AreEqual ("ts2-D", ts1.Items[7].Text, "M9-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M10");
			Assert.AreEqual (4, ts2.Items.Count, "M11");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Text, "M12-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Text, "M12-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Text, "M12-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Text, "M12-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Text, "M12-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Text, "M12-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Text, "M12-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Text, "M12-8");
		}
		
		[Test]
		public void MethodMergeToolStripsInsert ()
		{
			// MergeAction = Insert
			ToolStrip ts1 = new ToolStrip ();
			ToolStrip ts2 = new ToolStrip ();

			ts1.Items.Add ("ts1-A");
			ts1.Items.Add ("ts1-B");
			ts1.Items.Add ("ts1-C");
			ts1.Items.Add ("ts1-D");

			ts2.Items.Add ("ts2-A");
			ts2.Items.Add ("ts2-B");
			ts2.Items.Add ("ts2-C");
			ts2.Items.Add ("ts2-D");

			ts2.Items[0].MergeAction = MergeAction.Insert;
			ts2.Items[0].MergeIndex = 2;
			ts2.Items[1].MergeAction = MergeAction.Insert;
			ts2.Items[2].MergeAction = MergeAction.Insert;
			ts2.Items[2].MergeIndex = 12;
			ts2.Items[3].MergeAction = MergeAction.Insert;
			ts2.Items[3].MergeIndex = 0;
			
			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (7, ts1.Items.Count, "M1");
			Assert.AreEqual (1, ts2.Items.Count, "M2");

			Assert.AreEqual ("ts2-D", ts1.Items[0].Text, "M3-1");
			Assert.AreEqual ("ts1-A", ts1.Items[1].Text, "M3-2");
			Assert.AreEqual ("ts1-B", ts1.Items[2].Text, "M3-3");
			Assert.AreEqual ("ts2-A", ts1.Items[3].Text, "M3-4");
			Assert.AreEqual ("ts1-C", ts1.Items[4].Text, "M3-5");
			Assert.AreEqual ("ts1-D", ts1.Items[5].Text, "M3-6");
			Assert.AreEqual ("ts2-C", ts1.Items[6].Text, "M3-7");
			Assert.AreEqual ("ts2-B", ts2.Items[0].Text, "M3-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M4");
			Assert.AreEqual (4, ts2.Items.Count, "M5");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Text, "M6-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Text, "M6-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Text, "M6-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Text, "M6-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Text, "M6-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Text, "M6-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Text, "M6-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Text, "M6-8");

			// Do merge twice, as it helps verify things got back
			// to the proper state in the unmerge
			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (7, ts1.Items.Count, "M7");
			Assert.AreEqual (1, ts2.Items.Count, "M8");

			Assert.AreEqual ("ts2-D", ts1.Items[0].Text, "M9-1");
			Assert.AreEqual ("ts1-A", ts1.Items[1].Text, "M9-2");
			Assert.AreEqual ("ts1-B", ts1.Items[2].Text, "M9-3");
			Assert.AreEqual ("ts2-A", ts1.Items[3].Text, "M9-4");
			Assert.AreEqual ("ts1-C", ts1.Items[4].Text, "M9-5");
			Assert.AreEqual ("ts1-D", ts1.Items[5].Text, "M9-6");
			Assert.AreEqual ("ts2-C", ts1.Items[6].Text, "M9-7");
			Assert.AreEqual ("ts2-B", ts2.Items[0].Text, "M9-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M10");
			Assert.AreEqual (4, ts2.Items.Count, "M11");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Text, "M12-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Text, "M12-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Text, "M12-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Text, "M12-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Text, "M12-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Text, "M12-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Text, "M12-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Text, "M12-8");
		}

		[Test]
		public void MethodMergeToolStripsRemove ()
		{
			// MergeAction = Remove
			ToolStrip ts1 = new ToolStrip ();
			ToolStrip ts2 = new ToolStrip ();

			ts1.Items.Add ("ts1-A");
			ts1.Items.Add ("ts1-B");
			ts1.Items.Add ("ts1-C");
			ts1.Items.Add ("ts1-D");

			ts2.Items.Add ("ts1-A");
			ts2.Items.Add ("ts1-B");
			ts2.Items.Add ("ts1-C");
			ts2.Items.Add ("ts1-D");

			ts2.Items[0].MergeAction = MergeAction.Remove;
			ts2.Items[1].MergeAction = MergeAction.Remove;
			ts2.Items[2].MergeAction = MergeAction.Remove;
			ts2.Items[3].MergeAction = MergeAction.Remove;

			// Both the item from ts1 and ts2 must have the same Text for Remove to work,
			// so I need to give these a Name so I can differentiate them later.
			ts1.Items[0].Name = "ts1-A";
			ts1.Items[1].Name = "ts1-B";
			ts1.Items[2].Name = "ts1-C";
			ts1.Items[3].Name = "ts1-D";
			ts2.Items[0].Name = "ts2-A";
			ts2.Items[1].Name = "ts2-B";
			ts2.Items[2].Name = "ts2-C";
			ts2.Items[3].Name = "ts2-D";

			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (0, ts1.Items.Count, "M1");
			Assert.AreEqual (4, ts2.Items.Count, "M2");

			Assert.AreEqual ("ts2-A", ts2.Items[0].Name, "M3-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Name, "M3-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Name, "M3-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Name, "M3-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M4");
			Assert.AreEqual (4, ts2.Items.Count, "M5");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Name, "M6-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Name, "M6-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Name, "M6-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Name, "M6-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Name, "M6-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Name, "M6-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Name, "M6-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Name, "M6-8");

			// Do merge twice, as it helps verify things got back
			// to the proper state in the unmerge
			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (0, ts1.Items.Count, "M7");
			Assert.AreEqual (4, ts2.Items.Count, "M8");

			Assert.AreEqual ("ts2-A", ts2.Items[0].Name, "M9-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Name, "M9-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Name, "M9-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Name, "M9-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M10");
			Assert.AreEqual (4, ts2.Items.Count, "M11");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Name, "M12-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Name, "M12-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Name, "M12-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Name, "M12-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Name, "M12-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Name, "M12-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Name, "M12-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Name, "M12-8");
		}

		[Test]
		public void MethodMergeToolStripsReplace ()
		{
			// MergeAction = Replace
			ToolStrip ts1 = new ToolStrip ();
			ToolStrip ts2 = new ToolStrip ();

			ts1.Items.Add ("ts1-A");
			ts1.Items.Add ("ts1-B");
			ts1.Items.Add ("ts1-C");
			ts1.Items.Add ("ts1-D");

			ts2.Items.Add ("ts1-A");
			ts2.Items.Add ("ts1-B");
			ts2.Items.Add ("ts1-C");
			ts2.Items.Add ("ts1-D");

			ts2.Items[0].MergeAction = MergeAction.Replace;
			ts2.Items[1].MergeAction = MergeAction.Replace;
			ts2.Items[2].MergeAction = MergeAction.Replace;
			ts2.Items[3].MergeAction = MergeAction.Replace;

			// Both the item from ts1 and ts2 must have the same Text for Replace to work,
			// so I need to give these a Name so I can differentiate them later.
			ts1.Items[0].Name = "ts1-A";
			ts1.Items[1].Name = "ts1-B";
			ts1.Items[2].Name = "ts1-C";
			ts1.Items[3].Name = "ts1-D";
			ts2.Items[0].Name = "ts2-A";
			ts2.Items[1].Name = "ts2-B";
			ts2.Items[2].Name = "ts2-C";
			ts2.Items[3].Name = "ts2-D";

			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (4, ts1.Items.Count, "M1");
			Assert.AreEqual (0, ts2.Items.Count, "M2");

			Assert.AreEqual ("ts2-A", ts1.Items[0].Name, "M3-5");
			Assert.AreEqual ("ts2-B", ts1.Items[1].Name, "M3-6");
			Assert.AreEqual ("ts2-C", ts1.Items[2].Name, "M3-7");
			Assert.AreEqual ("ts2-D", ts1.Items[3].Name, "M3-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M4");
			Assert.AreEqual (4, ts2.Items.Count, "M5");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Name, "M6-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Name, "M6-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Name, "M6-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Name, "M6-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Name, "M6-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Name, "M6-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Name, "M6-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Name, "M6-8");

			// Do merge twice, as it helps verify things got back
			// to the proper state in the unmerge
			ToolStripManager.Merge (ts2, ts1);

			Assert.AreEqual (4, ts1.Items.Count, "M7");
			Assert.AreEqual (0, ts2.Items.Count, "M8");

			Assert.AreEqual ("ts2-A", ts1.Items[0].Name, "M9-5");
			Assert.AreEqual ("ts2-B", ts1.Items[1].Name, "M9-6");
			Assert.AreEqual ("ts2-C", ts1.Items[2].Name, "M9-7");
			Assert.AreEqual ("ts2-D", ts1.Items[3].Name, "M9-8");

			ToolStripManager.RevertMerge (ts1, ts2);

			Assert.AreEqual (4, ts1.Items.Count, "M10");
			Assert.AreEqual (4, ts2.Items.Count, "M11");

			Assert.AreEqual ("ts1-A", ts1.Items[0].Name, "M12-1");
			Assert.AreEqual ("ts1-B", ts1.Items[1].Name, "M12-2");
			Assert.AreEqual ("ts1-C", ts1.Items[2].Name, "M12-3");
			Assert.AreEqual ("ts1-D", ts1.Items[3].Name, "M12-4");
			Assert.AreEqual ("ts2-A", ts2.Items[0].Name, "M12-5");
			Assert.AreEqual ("ts2-B", ts2.Items[1].Name, "M12-6");
			Assert.AreEqual ("ts2-C", ts2.Items[2].Name, "M12-7");
			Assert.AreEqual ("ts2-D", ts2.Items[3].Name, "M12-8");
		}
		
		[Test]
		public void MethodMergeToolStripsMatchOnly ()
		{
			MenuStrip ms1 = new MenuStrip ();
			MenuStrip ms2 = new MenuStrip ();

			ToolStripMenuItem tsmi1 = (ToolStripMenuItem)ms1.Items.Add ("File");
			ToolStripMenuItem tsmi2 = (ToolStripMenuItem)ms2.Items.Add ("File");

			tsmi1.DropDownItems.Add ("New 1");
			tsmi1.DropDownItems.Add ("Open 1");

			tsmi2.DropDownItems.Add ("New 2");
			tsmi2.DropDownItems.Add ("Open 2");
			
			tsmi2.MergeAction = MergeAction.MatchOnly;

			ToolStripManager.Merge (ms2, ms1);

			Assert.AreEqual (4, tsmi1.DropDownItems.Count, "M1");
			Assert.AreEqual (0, tsmi2.DropDownItems.Count, "M2");

			Assert.AreEqual ("New 1", tsmi1.DropDownItems[0].Text, "M3-1");
			Assert.AreEqual ("Open 1", tsmi1.DropDownItems[1].Text, "M3-2");
			Assert.AreEqual ("New 2", tsmi1.DropDownItems[2].Text, "M3-3");
			Assert.AreEqual ("Open 2", tsmi1.DropDownItems[3].Text, "M3-4");

			ToolStripManager.RevertMerge (ms1, ms2);

			Assert.AreEqual (2, tsmi1.DropDownItems.Count, "M4");
			Assert.AreEqual (2, tsmi2.DropDownItems.Count, "M5");

			Assert.AreEqual ("New 1", tsmi1.DropDownItems[0].Text, "M6-1");
			Assert.AreEqual ("Open 1", tsmi1.DropDownItems[1].Text, "M6-2");
			Assert.AreEqual ("New 2", tsmi2.DropDownItems[0].Text, "M6-3");
			Assert.AreEqual ("Open 2", tsmi2.DropDownItems[1].Text, "M6-4");

			// Do merge twice, as it helps verify things got back
			// to the proper state in the unmerge
			ToolStripManager.Merge (ms2, ms1);

			Assert.AreEqual (4, tsmi1.DropDownItems.Count, "M7");
			Assert.AreEqual (0, tsmi2.DropDownItems.Count, "M8");

			Assert.AreEqual ("New 1", tsmi1.DropDownItems[0].Text, "M9-1");
			Assert.AreEqual ("Open 1", tsmi1.DropDownItems[1].Text, "M9-2");
			Assert.AreEqual ("New 2", tsmi1.DropDownItems[2].Text, "M9-3");
			Assert.AreEqual ("Open 2", tsmi1.DropDownItems[3].Text, "M9-4");

			ToolStripManager.RevertMerge (ms1, ms2);

			Assert.AreEqual (2, tsmi1.DropDownItems.Count, "M10");
			Assert.AreEqual (2, tsmi2.DropDownItems.Count, "M11");

			Assert.AreEqual ("New 1", tsmi1.DropDownItems[0].Text, "M12-1");
			Assert.AreEqual ("Open 1", tsmi1.DropDownItems[1].Text, "M12-2");
			Assert.AreEqual ("New 2", tsmi2.DropDownItems[0].Text, "M12-3");
			Assert.AreEqual ("Open 2", tsmi2.DropDownItems[1].Text, "M12-4");
		}

		[Test]  // For bug #81477
		public void MethodMergeRecursive ()
		{
			MenuStrip ms1 = new MenuStrip ();
			MenuStrip ms2 = new MenuStrip ();

			ToolStripMenuItem tsmi1 = (ToolStripMenuItem)ms1.Items.Add ("File");
			ToolStripMenuItem tsmi2 = (ToolStripMenuItem)ms2.Items.Add ("File");

			tsmi1.DropDownItems.Add ("New 1");
			tsmi1.DropDownItems.Add ("Open 1");

			tsmi2.DropDownItems.Add ("New 2");
			tsmi2.DropDownItems.Add ("Open 2");
			
			tsmi2.DropDownItems[0].MergeAction = MergeAction.Insert;
			tsmi2.DropDownItems[0].MergeIndex = 0;

			tsmi2.MergeAction = MergeAction.MatchOnly;

			ToolStripManager.Merge (ms2, ms1);

			Assert.AreEqual ("New 2", tsmi1.DropDownItems[0].Text, "M13");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodMergeANE1 ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.Name = "moose";
			ToolStripManager.Merge (null, "moose");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodMergeANE2 ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripManager.Merge (ts, (string)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodMergeAE ()
		{
			ToolStrip ts = new ToolStrip ();
			ts.Name = "mergeae";
			ToolStripManager.Merge (ts, "mergeae");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodMergeANE3 ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripManager.Merge (null, ts);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodMergeANE4 ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripManager.Merge (ts, (ToolStrip)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodMergeAE2 ()
		{
			ToolStrip ts = new ToolStrip ();
			ToolStripManager.Merge (ts, ts);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodRevertMergeANE ()
		{
			ToolStrip ts = new ToolStrip ();
			
			ToolStripManager.RevertMerge (ts, null);
		}
		
		[Test]
		public void MethodMergeNothing ()
		{
			// Merge returns false if it can't successfully merge anything
			ToolStrip ts1 = new ToolStrip ();
			ToolStrip ts2 = new ToolStrip ();
			
			Assert.AreEqual (false, ToolStripManager.Merge (ts1, ts2), "K1");
		}
	}
}
#endif