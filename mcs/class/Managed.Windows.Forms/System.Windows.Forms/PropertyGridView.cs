#define DOUBLEBUFFER

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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//      Jonathan Chambers	(jonathan.chambers@ansys.com)
//
//

// NOT COMPLETE

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms.Design;

namespace System.Windows.Forms.PropertyGridInternal {
	internal class PropertyGridView : ScrollableControl, IWindowsFormsEditorService {

		#region Private Members
		private double splitter_percent = .5;
		private const int V_INDENT = 16;
		private int row_height;
		private int font_height_padding = 3;
		private const int RESIZE_WIDTH = 3;
		private const int BUTTON_WIDTH = 25;
		private PropertyGridTextBox grid_textbox;
		internal PropertyGrid property_grid;
		private bool resizing_grid;
		private int open_grid_item_count = -1;
		private int skipped_grid_items;
		private PropertyGridDropDown dropdown_form;
		private Form dialog_form;
		private ImplicitVScrollBar vbar;
		private StringFormat string_format;
		private Font bold_font;
#if !DOUBLEBUFFER
		private int cached_splitter_location;
#endif
		#endregion

		#region Contructors
		public PropertyGridView (PropertyGrid propertyGrid) {
			property_grid = propertyGrid;

			property_grid.SelectedGridItemChanged += new SelectedGridItemChangedEventHandler (SelectedGridItemChanged);
			property_grid.PropertyValueChanged += new PropertyValueChangedEventHandler (PropertyValueChanged);
			property_grid.SelectedObjectsChanged += new EventHandler (SelectedObjectsChanged);

			string_format = new StringFormat();
			string_format.FormatFlags = StringFormatFlags.NoWrap;
			string_format.Trimming = StringTrimming.None;

			grid_textbox = new PropertyGridTextBox();
			grid_textbox.DropDownButtonClicked +=new EventHandler(DropDownButtonClicked);
			grid_textbox.DialogButtonClicked +=new EventHandler(DialogButtonClicked);

			dropdown_form = new PropertyGridDropDown();
			dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.StartPosition = FormStartPosition.Manual;
			dropdown_form.ShowInTaskbar = false;

			dialog_form = new Form ();
			dialog_form.StartPosition = FormStartPosition.Manual;
			dialog_form.FormBorderStyle = FormBorderStyle.None;
			dialog_form.ShowInTaskbar = false;

			skipped_grid_items = 0;
			row_height = Font.Height + font_height_padding;

			grid_textbox.Visible = false;
			grid_textbox.Font = this.Font;
			grid_textbox.BackColor = SystemColors.Window;
			// Not working at all, used to??
			grid_textbox.Validating += new CancelEventHandler(TextBoxValidating);
			grid_textbox.ToggleValue+=new EventHandler(grid_textbox_ToggleValue);
			this.Controls.Add(grid_textbox);

			vbar = new ImplicitVScrollBar();
			vbar.Visible = false;
			vbar.ValueChanged+=new EventHandler(HandleValueChanged);
			vbar.Dock = DockStyle.Right;
			this.Controls.AddImplicit(vbar);

			resizing_grid = false;

			bold_font = new Font(this.Font, FontStyle.Bold);

			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
			
			SetStyle(ControlStyles.Selectable, true);
#if DOUBLEBUFFER
			SetStyle(ControlStyles.DoubleBuffer, true);
#endif
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
#if DOUBLEBUFFER
			SetStyle(ControlStyles.ResizeRedraw, true);
#endif
		}

		#endregion

		#region Protected Instance Methods

		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);

			bold_font = new Font(this.Font, FontStyle.Bold);
			row_height = Font.Height + font_height_padding;
		}

		void InvalidateGridItemLabel (GridItem item)
		{
			Invalidate (new Rectangle (0, item.Top, SplitterLocation, row_height));
		}

#if !DOUBLEBUFFER
		void InvalidateGridItem (GridItem item)
		{
			Invalidate (new Rectangle (0, item.Top, Width, row_height));
		}
#endif

		void CalcHeightOfGridItems (GridItemCollection col, ref int amount)
		{
			foreach (GridItem i in col) {
				amount += row_height;
				if (i.Expanded)
					CalcHeightOfGridItems (i.GridItems, ref amount);
			}
		}

		public void RedrawBelowItemOnExpansion (GridItem item)
		{
			grid_textbox_Hide ();
#if DOUBLEBUFFER
			Invalidate(new Rectangle (0, item.Top, Width, Height - item.Top));
#else
			// ugh, we need to recurse down into all our
			// group items to figure out the space to
			// scroll..
			int amount = 0;

			CalcHeightOfGridItems (item.GridItems, ref amount);

			if (item.Expanded) {
				XplatUI.ScrollWindow (Handle,
						      new Rectangle (0, item.Top + row_height,
								     Width, Height - item.Top - row_height),
						      0, amount, false);
			}
			else {
				XplatUI.ScrollWindow (Handle,
						      new Rectangle (0, item.Top + row_height,
								     Width, Height - item.Top - row_height),
						      0, -amount, false);
			}
			InvalidateGridItem (item);
			Update();
#endif
			grid_textbox_Show (property_grid.SelectedGridItem);
		}

		protected override void OnDoubleClick(EventArgs e) {
			if (property_grid.SelectedGridItem.Expandable) {
				property_grid.SelectedGridItem.Expanded = !property_grid.SelectedGridItem.Expanded;
			}
			else {
				GridItem item = property_grid.SelectedGridItem;
				if (item.GridItemType == GridItemType.Property
				    && !item.PropertyDescriptor.IsReadOnly) {
					ToggleValue();
					Invalidate();
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			// Decide if we need a scrollbar
			open_grid_item_count = 0;

			// Background
			e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
			
			int yLoc = -vbar.Value*row_height;

			if (property_grid.root_grid_item != null)
				DrawGridItems(property_grid.root_grid_item.GridItems, e, 1, ref yLoc);

			UpdateScrollBar();
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			if (vbar == null || !vbar.Visible) {
				return;
			}

			if (e.Delta < 0) {
				vbar.Value = Math.Min(vbar.Value + SystemInformation.MouseWheelScrollLines, vbar.Maximum);
			} else {
				vbar.Value = Math.Max(0, vbar.Value - SystemInformation.MouseWheelScrollLines);
			}
			base.OnMouseWheel (e);
		}


		protected override void OnMouseMove (MouseEventArgs e) {
			if (property_grid.root_grid_item == null)
				return;

			if (resizing_grid) {
				int loc = Math.Max(e.X,2*V_INDENT);
				SplitterPercent = 1.0*loc/Width;
			}
			if (e.X > SplitterLocation - RESIZE_WIDTH && e.X < SplitterLocation + RESIZE_WIDTH) 
				this.Cursor = Cursors.SizeWE;
			else
				this.Cursor = Cursors.Default;
			base.OnMouseMove (e);
		}

		protected override void OnMouseDown (MouseEventArgs e) {
			if (property_grid.root_grid_item == null)
				return;

			if (e.X > SplitterLocation - RESIZE_WIDTH && e.X < SplitterLocation + RESIZE_WIDTH) {
				resizing_grid = true;
			}
			else {
				int offset = -vbar.Value*row_height;
				GridItem foundItem = GetSelectedGridItem(property_grid.root_grid_item.GridItems, e.Y, ref offset);
				
				if (foundItem != null) {
					if (foundItem.Expandable) {
						if (e.X >=3 && e.X <= 11 && (e.Y % row_height >= row_height/2-2 && e.Y % row_height <= row_height/2+4)) {
							foundItem.Expanded = !foundItem.Expanded;
						}
					}
					this.property_grid.SelectedGridItem = foundItem;
					
					grid_textbox.SendMouseDown (grid_textbox.PointToClient (PointToScreen (e.Location)));
				}
				
				base.OnMouseDown (e);
			}
		}

		protected override void OnMouseUp (MouseEventArgs e) {
			resizing_grid = false;
			base.OnMouseUp (e);
		}

		protected override void OnResize(EventArgs e) {
			SuspendLayout ();
			grid_textbox.Hide ();
			ResumeLayout (false);

#if !DOUBLEBUFFER
			// we need to explicitly handle this redraw
			// before getting to the scrollwindow, or we
			// end up having to redraw the entire item
			// value area because the exposed areas are
			// unioned.
			Update ();
#endif

			base.OnResize (e);
#if !DOUBLEBUFFER
			if (cached_splitter_location != SplitterLocation) {
				int x = cached_splitter_location > SplitterLocation ? SplitterLocation : cached_splitter_location;
				XplatUI.ScrollWindow (Handle,
						      new Rectangle (x, 0, Width - x - (vbar.Visible ? vbar.Width : 0), Height),
						      SplitterLocation - cached_splitter_location, 0, false);
				Update();
			}
			cached_splitter_location = SplitterLocation;
#endif
			SuspendLayout ();
			grid_textbox_Show (property_grid.SelectedGridItem);
			ResumeLayout (false);
		}

		private void UnfocusSelection ()
		{
			Select (this);
		}

		protected override bool ProcessDialogKey (Keys keyData) {
			GridItem selectedItem = property_grid.SelectedGridItem;
			if (selectedItem != null
			    && grid_textbox.Visible
			    /* if the textbox has focus? */) {
				switch (keyData) {
				case Keys.Enter:
					SetPropertyValue(selectedItem.PropertyDescriptor.Converter.ConvertFromString(grid_textbox.Text));
					UnfocusSelection ();
					return true;
				case Keys.Escape:
					grid_textbox.Text = selectedItem.PropertyDescriptor.Converter.ConvertToString(selectedItem.Value);
					UnfocusSelection ();
					return true;
				default:
					return false;
				}
			}
			return base.ProcessDialogKey (keyData);
		}


		protected override bool IsInputKey (Keys keyData) {
			switch (keyData) {
			case Keys.Left:
			case Keys.Right:
			case Keys.Enter:
			case Keys.Escape:
			case Keys.Up:
			case Keys.Down:
			case Keys.PageDown:
			case Keys.PageUp:
			case Keys.Home:
			case Keys.End:
				return true;
			default:
				return false;
			}
		}

		private GridEntry MoveUpFromItem (GridEntry item, int up_count)
		{
			GridItemCollection items;
			int index;

			/* move back up the visible rows (and up the hierarchy as necessary) until
			   up_count == 0, or we reach the top of the display */
			while (up_count > 0) {
				items = item.Parent != null ? item.Parent.GridItems : property_grid.root_grid_item.GridItems;
				index = items.IndexOf (item);

				if (index == 0) {
					if (item.Parent.GridItemType == GridItemType.Root) // we're at the top row
						return item;
					item = (GridEntry)item.Parent;
					up_count --;
				}
				else {
					GridEntry prev_item = (GridEntry)items[index-1];
					if (prev_item.Expandable && prev_item.Expanded) {
						item = (GridEntry)prev_item.GridItems[prev_item.GridItems.Count - 1];
					}
					else {
						item = prev_item;
					}
					up_count --;
				}
			}
			return item;
		}

		private GridEntry MoveDownFromItem (GridEntry item, int down_count)
		{
			while (down_count > 0) {
				/* if we're a parent node and we're expanded, move to our first child */
				if (item.Expandable && item.Expanded) {
					item = (GridEntry)item.GridItems[0];
					down_count--;
				}
				else {
					GridItem searchItem = item;
					GridItemCollection searchItems = searchItem.Parent.GridItems;
					int searchIndex = searchItems.IndexOf (searchItem);

					while (searchIndex == searchItems.Count - 1) {
						searchItem = searchItem.Parent;

						if (searchItem == null || searchItem.Parent == null)
							break;

						searchItems = searchItem.Parent.GridItems;
						searchIndex = searchItems.IndexOf (searchItem);
					}

					if (searchIndex == searchItems.Count - 1) {
						/* if we got all the way back to the root with no nodes after
						   us, the original item was the last one */
						return item;
					}
					else {
						item = (GridEntry)searchItems[searchIndex+1];
						down_count--;
					}
				}
			}

			return item;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			GridEntry selectedItem = (GridEntry)property_grid.SelectedGridItem;

			if (selectedItem == null) {
				/* XXX not sure what MS does, but at least we shouldn't crash */
				base.OnKeyDown (e);
				return;
			}

			switch (e.KeyData & Keys.KeyCode) {
			case Keys.Left:
				if (e.Control) {
					if (SplitterLocation > 2 * V_INDENT)
						SplitterPercent -= 0.01;

					e.Handled = true;
					break;
				}
				else {
					/* if the node is expandable and is expanded, collapse it.
					   otherwise, act just like the user pressed up */
					if (selectedItem.Expandable && selectedItem.Expanded) {
						selectedItem.Expanded = false;
						e.Handled = true;
						break;
					}
					else
						goto case Keys.Up;
				}
			case Keys.Right:
				if (e.Control) {
					if (SplitterLocation < Width)
						SplitterPercent += 0.01;

					e.Handled = true;
					break;
				}
				else {
					/* if the node is expandable and not expanded, expand it.
					   otherwise, act just like the user pressed down */
					if (selectedItem.Expandable && !selectedItem.Expanded) {
						selectedItem.Expanded = true;
						e.Handled = true;
						break;
					}
					else
						goto case Keys.Down;
				}
			case Keys.Enter:
				/* toggle the expanded state of the selected item */
				if (selectedItem.Expandable) {
					selectedItem.Expanded = !selectedItem.Expanded;
				}
				e.Handled = true;
				break;
			case Keys.Up:
				property_grid.SelectedGridItem = MoveUpFromItem (selectedItem, 1);
				e.Handled = true;
				break;
			case Keys.Down:
				property_grid.SelectedGridItem = MoveDownFromItem (selectedItem, 1);
				e.Handled = true;
				break;
			case Keys.PageUp:
				property_grid.SelectedGridItem = MoveUpFromItem (selectedItem, vbar.LargeChange);
				e.Handled = true;
				break;
			case Keys.PageDown:
				property_grid.SelectedGridItem = MoveDownFromItem (selectedItem, vbar.LargeChange);
				e.Handled = true;
				break;
			case Keys.End:
				/* find the last, most deeply nested visible item */
				GridEntry item = (GridEntry)property_grid.root_grid_item.GridItems[property_grid.root_grid_item.GridItems.Count - 1];
				while (item.Expandable && item.Expanded)
					item = (GridEntry)item.GridItems[item.GridItems.Count - 1];
				property_grid.SelectedGridItem = item;
				e.Handled = true;
				break;
			case Keys.Home:
				property_grid.SelectedGridItem = property_grid.root_grid_item.GridItems[0];
				e.Handled = true;
				break;
			}

			base.OnKeyDown (e);
		}

		#endregion

		#region Private Helper Methods

		private int SplitterLocation{
			get {
				return (int)(splitter_percent*Width);
			}
		}

		private double SplitterPercent{
			set {
				int old_splitter_location = SplitterLocation;
				
				splitter_percent = Math.Max(Math.Min(value, .9),.1);

				if (old_splitter_location != SplitterLocation) {
					grid_textbox_Hide ();
					int x = old_splitter_location > SplitterLocation ? SplitterLocation : old_splitter_location;
#if DOUBLEBUFFER
					Invalidate(new Rectangle (x, 0, Width - x - (vbar.Visible ? vbar.Width : 0), Height));
#else
					XplatUI.ScrollWindow (Handle,
							      new Rectangle (x, 0, Width - x - (vbar.Visible ? vbar.Width : 0), Height),
							      SplitterLocation - old_splitter_location, 0, false);
					Update ();
#endif
					grid_textbox_Show (property_grid.SelectedGridItem);
				}

#if !DOUBLEBUFFER
				cached_splitter_location = SplitterLocation;
#endif
			}
			get {
				return splitter_percent;
			}
		}

		private GridItem GetSelectedGridItem (GridItemCollection grid_items, int y, ref int current) {
			foreach (GridItem child_grid_item in grid_items) {
				if (y > current && y < current + row_height) {
					return child_grid_item;
				}
				current += row_height;
				if (child_grid_item.Expanded) {
					GridItem foundItem = GetSelectedGridItem(child_grid_item.GridItems, y, ref current);
					if (foundItem != null)
						return foundItem;
				}
			}
			return null;
		}

		private void UpdateScrollBar() {
			int visible_rows = this.ClientRectangle.Height/row_height;
			if (open_grid_item_count > visible_rows) {
				vbar.Visible = true;
				vbar.SmallChange = 1;
				vbar.Minimum = 0;
				vbar.Maximum = open_grid_item_count-1;
				vbar.LargeChange = visible_rows;
			}
			else {
				vbar.Visible = false;
			}

		}

		#region Drawing Code

		private void DrawGridItems(GridItemCollection grid_items, PaintEventArgs pevent, int depth, ref int yLoc) {
			foreach (GridItem grid_item in grid_items) {
				DrawGridItem (grid_item, pevent, depth, ref yLoc);
				if (grid_item.Expanded)
					DrawGridItems(grid_item.GridItems, pevent, (grid_item.GridItemType == GridItemType.Category) ? depth : depth+1, ref yLoc);
			}
		}

		private void DrawGridItemLabel(GridItem grid_item, PaintEventArgs pevent, int depth, Rectangle rect) {
			Font font = this.Font;
			Brush brush;

			if (grid_item.GridItemType == GridItemType.Category) {
				font = bold_font;
				brush = SystemBrushes.ControlDark;

				pevent.Graphics.DrawString (grid_item.Label, font, brush, rect.X + 1, rect.Y + 2);
				if (grid_item == property_grid.SelectedGridItem) {
					SizeF size = pevent.Graphics.MeasureString (grid_item.Label, font);
					ControlPaint.DrawFocusRectangle (pevent.Graphics, new Rectangle(rect.X + 1, rect.Y+2, (int)size.Width, (int)size.Height));
				}
			}
			else {
				if (grid_item == property_grid.SelectedGridItem) {
					Rectangle highlight = rect;
					if (depth > 1) {
						highlight.X -= V_INDENT;
						highlight.Width += V_INDENT;
					}
					pevent.Graphics.FillRectangle (SystemBrushes.Highlight, highlight);
					// Label
					brush = SystemBrushes.HighlightText;
				}
				else {
					brush = SystemBrushes.WindowText;
					if (grid_item.PropertyDescriptor.IsReadOnly
					    && !grid_item.Expandable)
						brush = SystemBrushes.InactiveCaption;
				}
			}

			pevent.Graphics.DrawString (grid_item.Label, font, brush,
						    new Rectangle (rect.X + 1, rect.Y + 2, rect.Width - 2, rect.Height - 2),
						    string_format);
		}

		private void DrawGridItemValue(GridItem grid_item, PaintEventArgs pevent, int depth, Rectangle rect) {
			// Value
			if (grid_item.PropertyDescriptor != null) {

				bool paintsValue = false;
				UITypeEditor editor = (UITypeEditor)grid_item.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				if (editor != null) {
					paintsValue = editor.GetPaintValueSupported();
				}

				int xLoc = SplitterLocation+1;
				if (paintsValue) {
					pevent.Graphics.DrawRectangle(Pens.Black, SplitterLocation+2,rect.Y+2, 20, row_height-4);
					try {
						editor.PaintValue(grid_item.Value, pevent.Graphics, new Rectangle(SplitterLocation+3,rect.Y+3, 19, row_height-5));
					}
					catch (Exception) {
					}
					xLoc += 27;
				}

				try {
					if (grid_item.PropertyDescriptor.Converter != null) {
						Font font = this.Font;
						Brush brush;

						string value = grid_item.PropertyDescriptor.Converter.ConvertToString(grid_item.Value);
						if (((GridEntry)grid_item).CanResetValue ())
							font = bold_font;

						brush = SystemBrushes.WindowText;
						if (grid_item.PropertyDescriptor.IsReadOnly
						    && !grid_item.Expandable)
							brush = SystemBrushes.InactiveCaption;

						pevent.Graphics.DrawString(value, font,
									   brush,
									   new RectangleF(xLoc, rect.Y+2,
											  ClientRectangle.Width-(xLoc), row_height),string_format);
					}
					else {
						Console.WriteLine("No converter for type {0}",grid_item.PropertyDescriptor.PropertyType);
					}

				}
				catch (Exception) {
				}
			}
		}

		private void DrawGridItem (GridItem grid_item, PaintEventArgs pevent, int depth, ref int yLoc) {
			if (yLoc > -row_height && yLoc < ClientRectangle.Height) {

				// Left column
				pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor),
							       0, yLoc, V_INDENT, row_height);
			
				if (grid_item.Expandable) {
					grid_item.PlusMinusBounds = DrawPlusMinus(pevent.Graphics, 3, yLoc+row_height/2-3, grid_item.Expanded, grid_item.GridItemType == GridItemType.Category);
				}
			
				if (grid_item.GridItemType == GridItemType.Category) {
					pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.CategoryForeColor), depth*V_INDENT,yLoc,ClientRectangle.Width-(depth*V_INDENT), row_height);
				}

				DrawGridItemLabel(grid_item, pevent,
						  depth,
						  new Rectangle(depth * V_INDENT, yLoc, SplitterLocation - depth * V_INDENT, row_height));
				DrawGridItemValue(grid_item, pevent,
						  depth,
						  new Rectangle(SplitterLocation + 2 , yLoc, ClientRectangle.Width - SplitterLocation - 2 - (vbar.Visible ? vbar.Width : 0), row_height));

				if (grid_item.GridItemType != GridItemType.Category) {
					Pen pen = ThemeEngine.Current.ResPool.GetPen(property_grid.LineColor);
					// vertical divider line
					pevent.Graphics.DrawLine(pen, SplitterLocation, yLoc, SplitterLocation, yLoc + row_height);
			
					// draw the horizontal line
					pevent.Graphics.DrawLine(pen, 0, yLoc + row_height, ClientRectangle.Width, yLoc + row_height);
				}
			}
			grid_item.Top = yLoc;
			yLoc += row_height;
			open_grid_item_count++;
		}

		private Rectangle DrawPlusMinus (Graphics g, int x, int y, bool expanded, bool category) {
			Rectangle bounds = new Rectangle(x, y, 8, 8);
			if (!category) g.FillRectangle (Brushes.White, bounds);
			Pen pen = ThemeEngine.Current.ResPool.GetPen (property_grid.ViewForeColor);
			g.DrawRectangle (pen, bounds);
			g.DrawLine (pen, x+2, y+4, x + 6, y+4);
			if (!expanded)
				g.DrawLine (pen, x+4, y+2, x+4, y+6);

			return bounds;
		}

		#endregion

		#region Event Handling
		private void RedrawEvent (object sender, System.EventArgs e) {
			Refresh();
		}

		private void TextBoxValidating (object sender, CancelEventArgs e) {
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					try {
						if (desc.Converter != null) {
							SetPropertyValue(desc.Converter.ConvertFromString(grid_textbox.Text));
						}
						else {
							Console.WriteLine("No converter for type {0}",desc.PropertyType);
						}
					}
					catch (Exception ex) {
						Console.WriteLine("Error converting string: " + ex.Message);
					}
				}
			}
		}

		#endregion

		private void ToggleValue() {
			if (property_grid.SelectedGridItem.GridItemType == GridItemType.Property) {
				if (property_grid.SelectedGridItem.PropertyDescriptor != null) {
					if (property_grid.SelectedGridItem.PropertyDescriptor.PropertyType == typeof(bool))
						SetPropertyValue(!(bool)property_grid.SelectedGridItem.Value);
					else if (property_grid.SelectedGridItem.PropertyDescriptor.Converter.GetStandardValuesSupported()){
						System.ComponentModel.TypeConverter.StandardValuesCollection coll = 
							(System.ComponentModel.TypeConverter.StandardValuesCollection)property_grid.SelectedGridItem.PropertyDescriptor.Converter.GetStandardValues();
						for (int i = 0; i < coll.Count; i++) {
							if (property_grid.SelectedGridItem.Value.Equals(coll[i])){
								if (i < coll.Count-1)
									SetPropertyValue(coll[i+1]);
								else
									SetPropertyValue(coll[0]);
								break;
							}

						}
					}
				}
			}
		}

		private void listBox_MouseUp(object sender, MouseEventArgs e) {
			AcceptListBoxSelection (sender);
		}

		private void listBox_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData & Keys.KeyCode) {
			case Keys.Enter:
				AcceptListBoxSelection (sender);
				return;
			case Keys.Escape:
				CloseDropDown ();
				return;
			}
		}

		void AcceptListBoxSelection (object sender) {
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					SetPropertyValue(((ListBox)sender).SelectedItem);
				}
			}
			CloseDropDown ();
		}

		private void SetPropertyValue(object newVal) {
			if (property_grid.SelectedGridItem == null)
				return;

			PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
			if (desc == null)
				return;

			for (int i = 0; i < ((GridEntry)property_grid.SelectedGridItem).SelectedObjects.Length; i ++) {
				object target = property_grid.GetTarget (property_grid.SelectedGridItem, i);
				desc.SetValue(target, newVal);
			}

			property_grid.PropertyValueChangedInternal ();
		}

		private void DropDownButtonClicked (object sender, EventArgs e) {
			UITypeEditor editor = property_grid.SelectedGridItem.PropertyDescriptor.GetEditor (typeof (UITypeEditor)) as UITypeEditor;
			if (editor == null) {
				if (dropdown_form.Visible) {
					CloseDropDown ();
				}
				else {
					TypeConverter converter = property_grid.SelectedGridItem.PropertyDescriptor.Converter;
					ICollection std_values;

					if (!converter.GetStandardValuesSupported ())
						return;

					std_values = converter.GetStandardValues();
					if (std_values == null)
						return;

					ListBox listBox = new ListBox();
					listBox.BorderStyle = BorderStyle.FixedSingle;
					int selected_index = 0;
					int i = 0;
					object selected_value = property_grid.SelectedGridItem.Value;
					foreach (object obj in std_values) {
						listBox.Items.Add(obj);
						if (selected_value != null && selected_value.Equals(obj))
							selected_index = i;
						i++;
					}
					listBox.Height = row_height * Math.Min (listBox.Items.Count, 15);
					listBox.SelectedIndex = selected_index;
					listBox.KeyDown += new KeyEventHandler(listBox_KeyDown);
					listBox.MouseUp+=new MouseEventHandler(listBox_MouseUp);

					DropDownControl (listBox);
				}
			} else { // use editor
				SetPropertyValueFromUITypeEditor (editor);
			}
		}

		void SetPropertyValueFromUITypeEditor (UITypeEditor editor)
		{
			ServiceContainer service_container = new ServiceContainer ();
			service_container.AddService (typeof (IWindowsFormsEditorService), this);
			object initial_value = property_grid.SelectedGridItem.Value;
			object value = editor.EditValue (
				(ITypeDescriptorContext)property_grid.SelectedGridItem,
				service_container,
				initial_value);

			SetPropertyValue (value);
		}
		
		private void DialogButtonClicked(object sender, EventArgs e) {
			UITypeEditor editor = property_grid.SelectedGridItem.PropertyDescriptor.GetEditor (typeof (UITypeEditor)) as UITypeEditor;
			if (editor != null)
				SetPropertyValueFromUITypeEditor (editor);
		}

		private void HandleValueChanged(object sender, EventArgs e) {
			if (vbar.Value <= 0) {
				vbar.Value = 0;
			}
			if (vbar.Value > vbar.Maximum-ClientRectangle.Height/row_height) {
				vbar.Value = vbar.Maximum-ClientRectangle.Height/row_height+1;
			}

			int scroll_amount = (skipped_grid_items-vbar.Value)*row_height;

			if (scroll_amount == 0)
				return;

			grid_textbox_Hide ();

			skipped_grid_items = vbar.Value;
			XplatUI.ScrollWindow(Handle, 0, scroll_amount, false);
#if DOUBLEBUFFER
			Invalidate ();
#endif
			Update ();

			if (property_grid.SelectedGridItem != null)
				grid_textbox_Show (property_grid.SelectedGridItem);
		}

		private void grid_textbox_ToggleValue(object sender, EventArgs e) {
			if (!property_grid.SelectedGridItem.PropertyDescriptor.IsReadOnly) {
				ToggleValue();
				Invalidate();
			}
		}

		internal void grid_textbox_Show (GridItem forItem)
		{
			if (forItem == null || forItem.PropertyDescriptor == null)
				return;

			SuspendLayout ();

			if (((GridEntry)forItem).CanResetValue ())
				grid_textbox.Font = bold_font;
			else
				grid_textbox.Font = this.Font;

			grid_textbox.ReadOnly = false;
			grid_textbox.DropDownButtonVisible = false;
			grid_textbox.DialogButtonVisible = false;

			bool paintsValue = false;
			UITypeEditor editor = (UITypeEditor)forItem.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
			if (editor != null) {
				paintsValue = editor.GetPaintValueSupported();
			}

			if (editor != null) {
				UITypeEditorEditStyle style = editor.GetEditStyle();
					
				switch (style) {
				case UITypeEditorEditStyle.DropDown:
					grid_textbox.DropDownButtonVisible = true;
					break;
				case UITypeEditorEditStyle.Modal:
					grid_textbox.DialogButtonVisible = true;
					break;
				}
			}
			else {
				try {
					if (forItem.PropertyDescriptor.Converter != null) {
						if (forItem.PropertyDescriptor.Converter.GetStandardValuesSupported()) {
							
							grid_textbox.DropDownButtonVisible = true;
							grid_textbox.ReadOnly = true;
						}
					}
					else {
						Console.WriteLine("Converter not available for type {0}",forItem.PropertyDescriptor.PropertyType);
					}
						
				}
				catch (Exception) {
				}
			}
				
			grid_textbox.ReadOnly = grid_textbox.ReadOnly || forItem.PropertyDescriptor.IsReadOnly;
			grid_textbox.ForeColor = grid_textbox.ReadOnly ? SystemColors.InactiveCaption : SystemColors.WindowText;

			int xloc = SplitterLocation + 1 + (paintsValue ? 27 : 0);
			grid_textbox.SetBounds (xloc,
						forItem.Top + 2,
						ClientRectangle.Width - xloc - (vbar.Visible ? vbar.Width : 0),
						row_height - 2);
			grid_textbox.Visible = true;

			ResumeLayout (false);
		}

		private void grid_textbox_Hide ()
		{
			SuspendLayout ();
			grid_textbox.Bounds = Rectangle.Empty;
			grid_textbox.Visible = false;
			ResumeLayout (false);
		}

		void EnsureItemIsVisible (GridItem item)
		{
			if (item.Top < 0) {
				// the new item is above the viewable area
				vbar.Value += item.Top / row_height;
			}
			else if (item.Top + row_height > Height) {
				// the new item is below the viewable area
				vbar.Value += ((item.Top + row_height) - Height) / row_height + 1;
			}
			else {
				// do nothing
			}
		}


		private void SelectedObjectsChanged (object sender, EventArgs args) {
			grid_textbox_Hide ();
			SelectedGridItemChanged (sender, new SelectedGridItemChangedEventArgs (property_grid.SelectedGridItem, property_grid.SelectedGridItem));
		}

		private void SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e) {
			if (e.OldSelection != null)
				InvalidateGridItemLabel (e.OldSelection);
				
			if (e.NewSelection == null)
				return;
			
			InvalidateGridItemLabel (e.NewSelection);

			EnsureItemIsVisible (e.NewSelection);

			if (e.NewSelection.GridItemType == GridItemType.Property) {
				if (e.NewSelection.PropertyDescriptor != null) {

					if (e.NewSelection.Value == null)
						grid_textbox.Text = "";
					else
						grid_textbox.Text = e.NewSelection.PropertyDescriptor.Converter.ConvertToString(e.NewSelection.Value);
					if (((GridEntry)e.NewSelection).CanResetValue ())
						grid_textbox.Font = bold_font;
					else
						grid_textbox.Font = this.Font;

					grid_textbox_Show (e.NewSelection);
				}
			}
			else {
				grid_textbox_Hide ();
			}
		}

		private void PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
			if (e.ChangedItem.PropertyDescriptor != null) {
				grid_textbox.Text = e.ChangedItem.PropertyDescriptor.Converter.ConvertToString(e.ChangedItem.Value);
				if (((GridEntry)e.ChangedItem).CanResetValue())
					grid_textbox.Font = bold_font;
				else
					grid_textbox.Font = this.Font;
			}
		}

		private void DropDownControl(Control control, bool block) {
			Object	queue_id;

			Form owner = FindForm ();

			Point location;
			dropdown_form.Size = control.Size;
			control.Dock = DockStyle.Fill;
			dropdown_form.Controls.Clear();
			dropdown_form.Controls.Add(control);
			dropdown_form.Location = PointToScreen (new Point (SplitterLocation, grid_textbox.Location.Y + row_height));
			location = dropdown_form.Location;

			dropdown_form.Width = ClientRectangle.Width - SplitterLocation - (vbar.Visible ? vbar.Width : 0);

			owner.AddOwnedForm (dropdown_form);

			dropdown_form.Show();

			if (dropdown_form.Location != location) {
				dropdown_form.Location = location;
			}
			if (block) {
				System.Windows.Forms.MSG msg = new MSG();
				queue_id = XplatUI.StartLoop(Thread.CurrentThread);
				while (dropdown_form.Visible && XplatUI.GetMessage(queue_id, ref msg, IntPtr.Zero, 0, 0)) {

					if ((msg.message == Msg.WM_NCLBUTTONDOWN ||
					     msg.message == Msg.WM_NCMBUTTONDOWN ||
					     msg.message == Msg.WM_NCRBUTTONDOWN ||
					     msg.message == Msg.WM_LBUTTONDOWN ||
					     msg.message == Msg.WM_MBUTTONDOWN ||
					     msg.message == Msg.WM_RBUTTONDOWN)
					    && !HwndInControl (dropdown_form, msg.hwnd)) {
						CloseDropDown ();
						break;
					}
					XplatUI.TranslateMessage(ref msg);
					XplatUI.DispatchMessage(ref msg);
				}

				property_grid.PropertyValueChangedInternal ();
			}
		}

		private bool HwndInControl (Control c, IntPtr hwnd)
		{
			if (hwnd == c.window.Handle)
				return true;
			foreach (Control cc in c.Controls.GetAllControls ())
				if (HwndInControl (cc, hwnd))
					return true;
			return false;
		}

		#endregion

		#region IWindowsFormsEditorService Members

		public void CloseDropDown() {
			Control c = dropdown_form.Controls[0];
			c.Capture = false;
			dropdown_form.Hide();
		}

		public void DropDownControl(Control control) {
			DropDownControl (control, true);
		}

		public System.Windows.Forms.DialogResult ShowDialog(Form dialog) {
			return dialog.ShowDialog(this);
		}

		#endregion

		/*
			class ComboListBox
		*/
		internal class PropertyGridDropDown : Form {
			protected override CreateParams CreateParams {
				get {
					CreateParams cp = base.CreateParams;
					cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
					cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);				
					return cp;
				}
			}

		}
	}
}
