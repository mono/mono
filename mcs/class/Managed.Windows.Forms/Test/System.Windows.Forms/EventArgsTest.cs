#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace MonoTestSuite
{
	[TestFixture]
	public class EventArgsTest
	{
		[Test]
		public void TestBindingCompleteEventArgs ()
		{
			Binding b = new Binding ("TestBind", null, "TestMember");
			BindingCompleteContext c = new BindingCompleteContext ();
			string errorText = "This is an error!";
			Exception ex = new ArgumentNullException ();

			BindingCompleteEventArgs e = new BindingCompleteEventArgs (b, BindingCompleteState.Success, c);

			Assert.AreEqual (b, e.Binding, "A1");
			Assert.AreEqual (BindingCompleteState.Success, e.BindingCompleteState, "A2");
			Assert.AreEqual (c, e.BindingCompleteContext, "A3");
			Assert.AreEqual (false, e.Cancel, "A4");
			Assert.AreEqual (String.Empty, e.ErrorText, "A5");
			Assert.AreEqual (null, e.Exception, "A6");

			BindingCompleteEventArgs e2 = new BindingCompleteEventArgs (b, BindingCompleteState.Success, c, errorText);

			Assert.AreEqual (b, e2.Binding, "B1");
			Assert.AreEqual (BindingCompleteState.Success, e2.BindingCompleteState, "B2");
			Assert.AreEqual (c, e2.BindingCompleteContext, "B3");
			Assert.AreEqual (false, e2.Cancel, "B4");
			Assert.AreEqual (errorText, e2.ErrorText, "B5");
			Assert.AreEqual (null, e2.Exception, "B6");

			BindingCompleteEventArgs e3 = new BindingCompleteEventArgs (b, BindingCompleteState.Success, c, errorText, ex);

			Assert.AreEqual (b, e3.Binding, "C1");
			Assert.AreEqual (BindingCompleteState.Success, e3.BindingCompleteState, "C2");
			Assert.AreEqual (c, e3.BindingCompleteContext, "C3");
			Assert.AreEqual (false, e3.Cancel, "C4");
			Assert.AreEqual (errorText, e3.ErrorText, "C5");
			Assert.AreEqual (ex, e3.Exception, "C6");

			BindingCompleteEventArgs e4 = new BindingCompleteEventArgs (b, BindingCompleteState.Success, c, errorText, ex, true);

			Assert.AreEqual (b, e4.Binding, "D1");
			Assert.AreEqual (BindingCompleteState.Success, e4.BindingCompleteState, "D2");
			Assert.AreEqual (c, e4.BindingCompleteContext, "D3");
			Assert.AreEqual (true, e4.Cancel, "D4");
			Assert.AreEqual (errorText, e4.ErrorText, "D5");
			Assert.AreEqual (ex, e4.Exception, "D6");

		}

		[Test]
		public void TestBindingManagerDataErrorEventArgs ()
		{
			Exception ex = new ArgumentNullException ();

			BindingManagerDataErrorEventArgs e = new BindingManagerDataErrorEventArgs (ex);

			Assert.AreEqual (ex, e.Exception, "A1");
		}

		[Test]
		public void TestCacheVirtualItemsEventArgs ()
		{
			int start = 7;
			int end = 26;

			CacheVirtualItemsEventArgs e = new CacheVirtualItemsEventArgs (start, end);

			Assert.AreEqual (start, e.StartIndex, "A1");
			Assert.AreEqual (end, e.EndIndex, "A2");
		}

		[Test]
		public void TestColumnReorderedEventArgs ()
		{
			int oldindex = 7;
			int newindex = 26;
			ColumnHeader ch = new ColumnHeader ();
			ch.Text = "TestHeader";

			ColumnReorderedEventArgs e = new ColumnReorderedEventArgs (oldindex, newindex, ch);

			Assert.AreEqual (oldindex, e.OldDisplayIndex, "A1");
			Assert.AreEqual (newindex, e.NewDisplayIndex, "A2");
			Assert.AreEqual (ch, e.Header, "A3");
			Assert.AreEqual (false, e.Cancel, "A4");
		}

		[Test]
		public void TestColumnWidthChangedEventArgs ()
		{
			int col = 42;

			ColumnWidthChangedEventArgs e = new ColumnWidthChangedEventArgs (col);

			Assert.AreEqual (col, e.ColumnIndex, "A1");
		}

		[Test]
		public void TestColumnWidthChangingEventArgs ()
		{
			int col = 27;
			int width = 543;

			ColumnWidthChangingEventArgs e = new ColumnWidthChangingEventArgs (col, width);

			Assert.AreEqual (col, e.ColumnIndex, "A1");
			Assert.AreEqual (width, e.NewWidth, "A2");
			Assert.AreEqual (false, e.Cancel, "A3");

			ColumnWidthChangingEventArgs e2 = new ColumnWidthChangingEventArgs (col, width, true);

			Assert.AreEqual (col, e2.ColumnIndex, "B1");
			Assert.AreEqual (width, e2.NewWidth, "B2");
			Assert.AreEqual (true, e2.Cancel, "B3");
		}

		[Test]
		public void TestFormClosedEventArgs ()
		{
			CloseReason cr = CloseReason.WindowsShutDown;

			FormClosedEventArgs e = new FormClosedEventArgs (cr);

			Assert.AreEqual (cr, e.CloseReason, "A1");
		}

		[Test]
		public void TestFormClosingEventArgs ()
		{
			CloseReason cr = CloseReason.WindowsShutDown;

			FormClosingEventArgs e = new FormClosingEventArgs (cr, true);

			Assert.AreEqual (cr, e.CloseReason, "A1");
			Assert.AreEqual (true, e.Cancel, "A2");
		}

		[Test]
		public void TestItemCheckedEventArgs ()
		{
			ListViewItem item = new ListViewItem ("TestItem");

			ItemCheckedEventArgs e = new ItemCheckedEventArgs (item);

			Assert.AreEqual (item, e.Item, "A1");
		}

		[Test]
		public void TestListControlConvertEventArgs ()
		{
			ListViewItem item = new ListViewItem ("TestItem");
			object value = (object)"TestObject";
			Type t = typeof (string);

			ListControlConvertEventArgs e = new ListControlConvertEventArgs (value, t, item);

			Assert.AreEqual (item, e.ListItem, "A1");
			Assert.AreEqual (value, e.Value, "A2");
			Assert.AreEqual (t, e.DesiredType, "A3");
		}

		[Test]
		public void TestListViewItemMouseHoverEventArgs ()
		{
			ListViewItem item = new ListViewItem ("TestItem");

			ListViewItemMouseHoverEventArgs e = new ListViewItemMouseHoverEventArgs (item);

			Assert.AreEqual (item, e.Item, "A1");
		}

		[Test]
		public void TestListViewItemSelectionChangedEventArgs ()
		{
			ListViewItem item = new ListViewItem ("TestItem");
			bool selected = false;
			int index = 35;

			ListViewItemSelectionChangedEventArgs e = new ListViewItemSelectionChangedEventArgs (item, index, selected);

			Assert.AreEqual (item, e.Item, "A1");
			Assert.AreEqual (selected, e.IsSelected, "A2");
			Assert.AreEqual (index, e.ItemIndex, "A3");
		}

		[Test]
		public void TestListViewVirtualItemsSelectionRangeChangedEventArgs ()
		{
			bool selected = false;
			int start = 3;
			int end = 76;

			ListViewVirtualItemsSelectionRangeChangedEventArgs e = new ListViewVirtualItemsSelectionRangeChangedEventArgs (start, end, selected);

			Assert.AreEqual (selected, e.IsSelected, "A1");
			Assert.AreEqual (start, e.StartIndex, "A2");
			Assert.AreEqual (end, e.EndIndex, "A3");
		}

		[Test]
		public void TestMaskInputRejectedEventArgs ()
		{
			int pos = 2;
			MaskedTextResultHint hint = MaskedTextResultHint.InvalidInput;

			MaskInputRejectedEventArgs e = new MaskInputRejectedEventArgs (pos, hint);

			Assert.AreEqual (pos, e.Position, "A1");
			Assert.AreEqual (hint, e.RejectionHint, "A2");
		}

		[Test]
		public void TestPopupEventArgs ()
		{
			Control c = new ListBox ();
			IWin32Window w = null;
			bool balloon = true;
			Size s = new Size (123, 54);

			PopupEventArgs e = new PopupEventArgs (w, c, balloon, s);

			Assert.AreEqual (c, e.AssociatedControl, "A1");
			Assert.AreEqual (w, e.AssociatedWindow, "A2");
			Assert.AreEqual (balloon, e.IsBalloon, "A3");
			Assert.AreEqual (s, e.ToolTipSize, "A4");
		}

		[Test]
		public void TestPreviewKeyDownEventArgs ()
		{
			Keys k = (Keys)196674;  // Control-Shift-B

			PreviewKeyDownEventArgs e = new PreviewKeyDownEventArgs (k);

			Assert.AreEqual (false, e.Alt, "A1");
			Assert.AreEqual (true, e.Control, "A2");
			Assert.AreEqual (false, e.IsInputKey, "A3");
			Assert.AreEqual ((Keys)66, e.KeyCode, "A4");  // B
			Assert.AreEqual (k, e.KeyData, "A5");
			Assert.AreEqual (Convert.ToInt32 (k), e.KeyValue, "A6");
			Assert.AreEqual ((Keys)196608, e.Modifiers, "A7");  // Control + Shift
			Assert.AreEqual (true, e.Shift, "A8");

			e.IsInputKey = true;

			Assert.AreEqual (true, e.IsInputKey, "A9");
		}

		[Test]
		public void TestRetrieveVirtualItemEventArgs()
		{
			ListViewItem item = new ListViewItem("TestItem");
			int index = 75;
			
			RetrieveVirtualItemEventArgs e = new RetrieveVirtualItemEventArgs(index);
			
			Assert.AreEqual(index, e.ItemIndex, "A1");
			Assert.AreEqual(null, e.Item, "A2");
			
			e.Item = item;
			
			Assert.AreEqual(item, e.Item, "A3");
		}
		
		[Test]
		public void TestSearchForVirtualItemEventArgs()
		{
			SearchDirectionHint sdh = SearchDirectionHint.Right;
			bool includesubitems = true;
			int index = 84;
			bool isprefix = true;
			bool istext = false;
			int start = 34;
			Point startpoint = new Point(64,35);
			string text = "HiThere!";
			
			SearchForVirtualItemEventArgs e = new SearchForVirtualItemEventArgs(istext, isprefix, includesubitems, text, startpoint, sdh, start);
			
			Assert.AreEqual(sdh, e.Direction, "A1");
			Assert.AreEqual(includesubitems, e.IncludeSubItemsInSearch, "A2");
			Assert.AreEqual(-1, e.Index, "A3");
			Assert.AreEqual(isprefix, e.IsPrefixSearch, "A4");
			Assert.AreEqual(istext, e.IsTextSearch, "A5");
			Assert.AreEqual(start, e.StartIndex, "A6");
			Assert.AreEqual(startpoint, e.StartingPoint, "A7");
			Assert.AreEqual(text, e.Text, "A8");
			
			e.Index = index;
			Assert.AreEqual(index, e.Index, "A9");
		}
		
		[Test]
		public void TestSplitterCancelEventArgs()
		{
			int mx = 23;
			int my = 33;
			int sx = 43;
			int sy = 53;
			
			SplitterCancelEventArgs e = new SplitterCancelEventArgs(mx, my, sx, sy);
			
			Assert.AreEqual(mx, e.MouseCursorX, "A1");
			Assert.AreEqual(my, e.MouseCursorY, "A2");
			Assert.AreEqual(sx, e.SplitX, "A3");
			Assert.AreEqual(sy, e.SplitY, "A4");
			
			e.SplitX = 11;
			e.SplitY = 12;
			
			Assert.AreEqual(11, e.SplitX, "A5");
			Assert.AreEqual(12, e.SplitY, "A6");
		}
		
		[Test]
		public void TestTabControlCancelEventArgs()
		{
			TabControlAction tca = TabControlAction.Deselecting;
			TabPage tp = new TabPage("HI!");
			int index = 477;
			
			TabControlCancelEventArgs e = new TabControlCancelEventArgs(tp, index, true, tca);
			
			Assert.AreEqual(tca, e.Action, "A1");
			Assert.AreEqual(tp, e.TabPage, "A2");
			Assert.AreEqual(index, e.TabPageIndex, "A3");
			Assert.AreEqual(true, e.Cancel, "A4");
		}

		[Test]
		public void TestTabControlEventArgs ()
		{
			TabControlAction tca = TabControlAction.Selected;
			TabPage tp = new TabPage ("HI!");
			int index = 477;

			TabControlEventArgs e = new TabControlEventArgs (tp, index, tca);

			Assert.AreEqual (tca, e.Action, "A1");
			Assert.AreEqual (tp, e.TabPage, "A2");
			Assert.AreEqual (index, e.TabPageIndex, "A3");
		}
		
		[Test]
		public void TestTableLayoutCellPaintEventArgs()
		{
			Rectangle bounds = new Rectangle(0, 0, 100, 200);
			Rectangle clip = new Rectangle(50, 50, 50, 50);
			int col = 54;
			int row = 77;
			Bitmap b = new Bitmap(100, 100);
			Graphics g = Graphics.FromImage(b);
			
			TableLayoutCellPaintEventArgs e = new TableLayoutCellPaintEventArgs(g, clip, bounds, col, row);
			
			Assert.AreEqual(bounds, e.CellBounds, "A1");
			Assert.AreEqual(col, e.Column, "A2");
			Assert.AreEqual(row, e.Row, "A3");
			Assert.AreEqual(g, e.Graphics, "A4");
			Assert.AreEqual(clip, e.ClipRectangle, "A5");
		}
		
		[Test]
		public void TestToolStripDropDownClosedEventArgs()
		{
			ToolStripDropDownCloseReason cr = ToolStripDropDownCloseReason.CloseCalled;
			
			ToolStripDropDownClosedEventArgs e = new ToolStripDropDownClosedEventArgs(cr);
			
			Assert.AreEqual(cr, e.CloseReason, "A1");
		}

		[Test]
		public void TestToolStripDropDownClosingEventArgs ()
		{
			ToolStripDropDownCloseReason cr = ToolStripDropDownCloseReason.CloseCalled;

			ToolStripDropDownClosingEventArgs e = new ToolStripDropDownClosingEventArgs (cr);

			Assert.AreEqual (cr, e.CloseReason, "A1");
		}
		
		[Test]
		public void TestTreeNodeMouseClickEventArgs()
		{
			TreeNode tn = new TreeNode("HI");
			int clicks = 4;
			int x = 75;
			int y = 34;
			MouseButtons mb = MouseButtons.Right;
			
			TreeNodeMouseClickEventArgs e = new TreeNodeMouseClickEventArgs(tn, mb, clicks, x, y);
			
			Assert.AreEqual(tn, e.Node, "A1");
			Assert.AreEqual(clicks, e.Clicks, "A2");
			Assert.AreEqual(x, e.X, "A3");
			Assert.AreEqual(y, e.Y, "A4");
			Assert.AreEqual(mb, e.Button, "A5");
		}

		[Test]
		public void TestTreeNodeMouseHoverEventArgs ()
		{
			TreeNode tn = new TreeNode ("HI");

			TreeNodeMouseHoverEventArgs e = new TreeNodeMouseHoverEventArgs (tn);

			Assert.AreEqual (tn, e.Node, "A1");
		}

		[Test]
		public void TestTypeValidationEventArgs()
		{
			bool valid = true;
			string message = "This is a test.";
			object rv = (object) "MyObject";
			Type vt = typeof(int);
			
			TypeValidationEventArgs e = new TypeValidationEventArgs (vt, valid, rv, message);
			
			Assert.AreEqual(valid, e.IsValidInput, "A1");
			Assert.AreEqual(message, e.Message, "A2");
			Assert.AreEqual(rv, e.ReturnValue, "A3");
			Assert.AreEqual(vt, e.ValidatingType, "A4");
			Assert.AreEqual(false, e.Cancel, "A5");
			
			e.Cancel = true;
			
			Assert.AreEqual(true, e.Cancel, "A6");
		}
		
		[Test]
		public void TestWebBrowserDocumentCompletedEventArgs()
		{
			Uri url = new Uri("http://www.mono-project.com/");
			
			WebBrowserDocumentCompletedEventArgs e = new WebBrowserDocumentCompletedEventArgs(url);
			
			Assert.AreEqual(url, e.Url, "A1");
		}

		[Test]
		public void TestWebBrowserNavigatedEventArgs ()
		{
			Uri url = new Uri ("http://www.mono-project.com/");

			WebBrowserNavigatedEventArgs e = new WebBrowserNavigatedEventArgs (url);

			Assert.AreEqual (url, e.Url, "A1");
		}

		[Test]
		public void TestWebBrowserNavigatingEventArgs ()
		{
			Uri url = new Uri ("http://www.mono-project.com/");
			string frame = "TOP";

			WebBrowserNavigatingEventArgs e = new WebBrowserNavigatingEventArgs (url, frame);

			Assert.AreEqual (url, e.Url, "A1");
			Assert.AreEqual(frame, e.TargetFrameName, "A2");
		}

		[Test]
		public void TestWebBrowserProgressChangedEventArgs ()
		{
			long current = 3000;
			long max = 5000;

			WebBrowserProgressChangedEventArgs e = new WebBrowserProgressChangedEventArgs (current, max);

			Assert.AreEqual (current, e.CurrentProgress, "A1");
			Assert.AreEqual (max, e.MaximumProgress, "A2");
		}

	}
}
#endif
