
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
// Copyright (c) 2005-2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//      Jonathan Chambers	(jonathan.chambers@ansys.com)
//      Ivan N. Zlatev		(contact@i-nz.net)
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
		private const char PASSWORD_PAINT_CHAR = '\u25cf'; // the dot char
		private const char PASSWORD_TEXT_CHAR = '*';
		private const int V_INDENT = 16;
		private const int ENTRY_SPACING = 2;
		private const int RESIZE_WIDTH = 3;
		private const int BUTTON_WIDTH = 25;
		private const int VALUE_PAINT_WIDTH = 19;
		private const int VALUE_PAINT_INDENT = 27;
		private double splitter_percent = .5;
		private int row_height;
		private int font_height_padding = 3;
		private PropertyGridTextBox grid_textbox;
		private PropertyGrid property_grid;
		private bool resizing_grid;
		private PropertyGridDropDown dropdown_form;
		private Form dialog_form;
		private ImplicitVScrollBar vbar;
		private StringFormat string_format;
		private Font bold_font;
		private Brush inactive_text_brush;
		private ListBox dropdown_list;
		private Point last_click;
		private Padding dropdown_form_padding;
		#endregion

		#region Contructors
		public PropertyGridView (PropertyGrid propertyGrid) {
			property_grid = propertyGrid;

			string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.NoWrap;
			string_format.Trimming = StringTrimming.None;

			grid_textbox = new PropertyGridTextBox ();
			grid_textbox.DropDownButtonClicked +=new EventHandler (DropDownButtonClicked);
			grid_textbox.DialogButtonClicked +=new EventHandler (DialogButtonClicked);

			dropdown_form = new PropertyGridDropDown ();
			dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.StartPosition = FormStartPosition.Manual;
			dropdown_form.ShowInTaskbar = false;

			dialog_form = new Form ();
			dialog_form.StartPosition = FormStartPosition.Manual;
			dialog_form.FormBorderStyle = FormBorderStyle.None;
			dialog_form.ShowInTaskbar = false;

			dropdown_form_padding = new Padding (0, 0, 2, 2);

			row_height = Font.Height + font_height_padding;

			grid_textbox.Visible = false;
			grid_textbox.Font = this.Font;
			grid_textbox.BackColor = SystemColors.Window;
			grid_textbox.Validate += new CancelEventHandler (grid_textbox_Validate);
			grid_textbox.ToggleValue+=new EventHandler (grid_textbox_ToggleValue);
			grid_textbox.KeyDown+=new KeyEventHandler (grid_textbox_KeyDown);
			this.Controls.Add (grid_textbox);

			vbar = new ImplicitVScrollBar ();
			vbar.Visible = false;
			vbar.Value = 0;
			vbar.ValueChanged+=new EventHandler (VScrollBar_HandleValueChanged);
			vbar.Dock = DockStyle.Right;
			this.Controls.AddImplicit (vbar);

			resizing_grid = false;

			bold_font = new Font (this.Font, FontStyle.Bold);
			inactive_text_brush = new SolidBrush (ThemeEngine.Current.ColorGrayText);

			ForeColorChanged+=new EventHandler (RedrawEvent);
			BackColorChanged+=new System.EventHandler (RedrawEvent);
			FontChanged+=new EventHandler (RedrawEvent);
			
			SetStyle (ControlStyles.Selectable, true);
			SetStyle (ControlStyles.DoubleBuffer, true);
			SetStyle (ControlStyles.UserPaint, true);
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw, true);
		}

		#endregion

		private GridEntry RootGridItem {
			get { return (GridEntry)property_grid.RootGridItem; }
		}

		private GridEntry SelectedGridItem {
			get { return (GridEntry)property_grid.SelectedGridItem; }
			set { property_grid.SelectedGridItem = value; }
		}

		#region Protected Instance Methods

		protected override void OnFontChanged (EventArgs e) {
			base.OnFontChanged (e);

			bold_font = new Font (this.Font, FontStyle.Bold);
			row_height = Font.Height + font_height_padding;
		}

		private void InvalidateItemLabel (GridEntry item)
		{
			Invalidate (new Rectangle (0, ((GridEntry)item).Top, SplitterLocation, row_height));
		}

		private void InvalidateItem (GridEntry item)
		{
			if (item == null)
				return;

			Rectangle rect = new Rectangle (0, item.Top, Width, row_height);
			Invalidate (rect);

			if (item.Expanded) {
				rect = new Rectangle (0, item.Top + row_height, Width,
						      Height - (item.Top + row_height));
				Invalidate (rect);
			}
		}

		// [+] expanding is handled in OnMouseDown, so in order to prevent 
		// duplicate expanding ignore it here.
		// 
		protected override void OnDoubleClick (EventArgs e) 
		{
			if (this.SelectedGridItem != null && this.SelectedGridItem.Expandable && 
			    !this.SelectedGridItem.PlusMinusBounds.Contains (last_click))
				this.SelectedGridItem.Expanded = !this.SelectedGridItem.Expanded;
			else
				ToggleValue (this.SelectedGridItem);
		}

		protected override void OnPaint (PaintEventArgs e) 
		{
			// Background
			e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
			
			int yLoc = -vbar.Value*row_height;
			if (this.RootGridItem != null)
				DrawGridItems (this.RootGridItem.GridItems, e, 1, ref yLoc);

			UpdateScrollBar ();
		}

		protected override void OnMouseWheel (MouseEventArgs e) 
		{
			if (vbar == null || !vbar.Visible)
				return;
			if (e.Delta < 0)
				vbar.Value = Math.Min (vbar.Maximum - GetVisibleRowsCount () + 1, vbar.Value + SystemInformation.MouseWheelScrollLines);
			else
				vbar.Value = Math.Max (0, vbar.Value - SystemInformation.MouseWheelScrollLines);
			base.OnMouseWheel (e);
		}


		protected override void OnMouseMove (MouseEventArgs e) {
			if (this.RootGridItem == null)
				return;

			if (resizing_grid) {
				int loc = Math.Max (e.X,2*V_INDENT);
				SplitterPercent = 1.0*loc/Width;
			}
			if (e.X > SplitterLocation - RESIZE_WIDTH && e.X < SplitterLocation + RESIZE_WIDTH) 
				this.Cursor = Cursors.SizeWE;
			else
				this.Cursor = Cursors.Default;
			base.OnMouseMove (e);
		}

		protected override void OnMouseDown (MouseEventArgs e) 
		{
			base.OnMouseDown (e);
			last_click = e.Location;
			if (this.RootGridItem == null)
				return;

			if (e.X > SplitterLocation - RESIZE_WIDTH && e.X < SplitterLocation + RESIZE_WIDTH) {
				resizing_grid = true;
			}
			else {
				int offset = -vbar.Value*row_height;
				GridItem foundItem = GetSelectedGridItem (this.RootGridItem.GridItems, e.Y, ref offset);

				if (foundItem != null) {
					if (foundItem.Expandable && ((GridEntry)foundItem).PlusMinusBounds.Contains (e.X, e.Y))
						foundItem.Expanded = !foundItem.Expanded;
					
					this.SelectedGridItem = (GridEntry)foundItem;
					if (!GridLabelHitTest (e.X)) {
						// send mouse down so we get the carret under cursor
						grid_textbox.SendMouseDown (PointToScreen (e.Location));
					}
				}
			}
		}

		protected override void OnMouseUp (MouseEventArgs e) {
			resizing_grid = false;
			base.OnMouseUp (e);
		}

		protected override void OnResize (EventArgs e) {
			base.OnResize (e);
			if (this.SelectedGridItem != null) // initialized already
				UpdateView ();
		}

		private void UnfocusSelection ()
		{
			Select (this);
		}

		private void FocusSelection ()
		{
			Select (grid_textbox);
		}

		protected override bool ProcessDialogKey (Keys keyData) {
			GridEntry selectedItem = this.SelectedGridItem;
			if (selectedItem != null
			    && grid_textbox.Visible) {
				switch (keyData) {
				case Keys.Enter:
					if (TrySetEntry (selectedItem, grid_textbox.Text))
						UnfocusSelection ();
					return true;
				case Keys.Escape:
					if (selectedItem.IsEditable)
						UpdateItem (selectedItem); // reset value
					UnfocusSelection ();
					return true;
				case Keys.Tab:
					FocusSelection ();
					return true;
				default:
					return false;
				}
			}
			return base.ProcessDialogKey (keyData);
		}

		private bool TrySetEntry (GridEntry entry, object value)
		{
			if (entry == null || grid_textbox.Text.Equals (entry.ValueText))
				return true;

			if (entry.IsEditable || !entry.IsEditable && (entry.HasCustomEditor || entry.AcceptedValues != null) ||
			    !entry.IsMerged || entry.HasMergedValue || 
			    (!entry.HasMergedValue && grid_textbox.Text != String.Empty)) {
				string error = null;
				bool changed = entry.SetValue (value, out error);
				if (!changed && error != null) {
					if (property_grid.ShowError (error, MessageBoxButtons.OKCancel) == DialogResult.Cancel) {
						UpdateItem (entry); // restore value, repaint, etc
						UnfocusSelection ();
					}
					return false;
				}
			}
			UpdateItem (entry); // restore value, repaint, etc
			return true;
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
				items = item.Parent != null ? item.Parent.GridItems : this.RootGridItem.GridItems;
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

		protected override void OnKeyDown (KeyEventArgs e) 
		{
			GridEntry selectedItem = this.SelectedGridItem;

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
				this.SelectedGridItem = MoveUpFromItem (selectedItem, 1);
				e.Handled = true;
				break;
			case Keys.Down:
				this.SelectedGridItem = MoveDownFromItem (selectedItem, 1);
				e.Handled = true;
				break;
			case Keys.PageUp:
				this.SelectedGridItem = MoveUpFromItem (selectedItem, vbar.LargeChange);
				e.Handled = true;
				break;
			case Keys.PageDown:
				this.SelectedGridItem = MoveDownFromItem (selectedItem, vbar.LargeChange);
				e.Handled = true;
				break;
			case Keys.End:
				/* find the last, most deeply nested visible item */
				GridEntry item = (GridEntry)this.RootGridItem.GridItems[this.RootGridItem.GridItems.Count - 1];
				while (item.Expandable && item.Expanded)
					item = (GridEntry)item.GridItems[item.GridItems.Count - 1];
				this.SelectedGridItem = item;
				e.Handled = true;
				break;
			case Keys.Home:
				this.SelectedGridItem = (GridEntry)this.RootGridItem.GridItems[0];
				e.Handled = true;
				break;
			}

			base.OnKeyDown (e);
		}

		#endregion

		#region Private Helper Methods

		private int SplitterLocation {
			get {
				return (int)(splitter_percent*Width);
			}
		}

		private double SplitterPercent {
			set {
				int old_splitter_location = SplitterLocation;
				
				splitter_percent = Math.Max (Math.Min (value, .9),.1);

				if (old_splitter_location != SplitterLocation) {
					int x = old_splitter_location > SplitterLocation ? SplitterLocation : old_splitter_location;
					Invalidate (new Rectangle (x, 0, Width - x - (vbar.Visible ? vbar.Width : 0), Height));
					UpdateItem (this.SelectedGridItem);
				}
			}
			get {
				return splitter_percent;
			}
		}

		private bool GridLabelHitTest (int x)
		{
			if (0 <= x && x <= splitter_percent * this.Width)
				return true;
			return false;
		}

		private GridItem GetSelectedGridItem (GridItemCollection grid_items, int y, ref int current) {
			foreach (GridItem child_grid_item in grid_items) {
				if (y > current && y < current + row_height) {
					return child_grid_item;
				}
				current += row_height;
				if (child_grid_item.Expanded) {
					GridItem foundItem = GetSelectedGridItem (child_grid_item.GridItems, y, ref current);
					if (foundItem != null)
						return foundItem;
				}
			}
			return null;
		}

		private int GetVisibleItemsCount (GridEntry entry)
		{
			if (entry == null)
				return 0;

			int count = 0;
			foreach (GridEntry e in entry.GridItems) {
				count += 1;
				if (e.Expandable && e.Expanded)
					count += GetVisibleItemsCount (e);
			}
			return count;
		}

		private int GetVisibleRowsCount ()
		{
			return this.Height / row_height;
		}

		private void UpdateScrollBar ()
		{
			if (this.RootGridItem == null)
				return;

			int visibleRows = GetVisibleRowsCount ();
			int openedItems = GetVisibleItemsCount (this.RootGridItem);
			if (openedItems > visibleRows) {
				vbar.Visible = true;
				vbar.SmallChange = 1;
				vbar.LargeChange = visibleRows;
				vbar.Maximum = Math.Max (0, openedItems - 1);
			} else {
				vbar.Value = 0;
				vbar.Visible = false;
			}
			UpdateGridTextBoxBounds (this.SelectedGridItem);
		}

		// private bool GetScrollBarVisible ()
		// {
		// 	if (this.RootGridItem == null)
		// 		return false;
                // 
		// 	int visibleRows = GetVisibleRowsCount ();
		// 	int openedItems = GetVisibleItemsCount (this.RootGridItem);
		// 	if (openedItems > visibleRows)
		// 		return true;
		// 	return false;
		// }
		#region Drawing Code

		private void DrawGridItems (GridItemCollection grid_items, PaintEventArgs pevent, int depth, ref int yLoc) {
			foreach (GridItem grid_item in grid_items) {
				DrawGridItem ((GridEntry)grid_item, pevent, depth, ref yLoc);
				if (grid_item.Expanded)
					DrawGridItems (grid_item.GridItems, pevent, (grid_item.GridItemType == GridItemType.Category) ? depth : depth+1, ref yLoc);
			}
		}

		private void DrawGridItemLabel (GridEntry grid_item, PaintEventArgs pevent, int depth, Rectangle rect) {
			Font font = this.Font;
			Brush brush;

			if (grid_item.GridItemType == GridItemType.Category) {
				font = bold_font;
				brush = ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.CategoryForeColor);

				pevent.Graphics.DrawString (grid_item.Label, font, brush, rect.X + 1, rect.Y + ENTRY_SPACING);
				if (grid_item == this.SelectedGridItem) {
					SizeF size = pevent.Graphics.MeasureString (grid_item.Label, font);
					ControlPaint.DrawFocusRectangle (pevent.Graphics, new Rectangle (rect.X + 1, rect.Y+ENTRY_SPACING, (int)size.Width, (int)size.Height));
				}
			}
			else {
				if (grid_item == this.SelectedGridItem) {
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
					brush = grid_item.IsReadOnly ? inactive_text_brush : SystemBrushes.ControlText;
				}
			}
			pevent.Graphics.DrawString (grid_item.Label, font, brush,
						    new Rectangle (rect.X + 1, rect.Y + ENTRY_SPACING, rect.Width - ENTRY_SPACING, rect.Height - ENTRY_SPACING),
						    string_format);
		}

		private void DrawGridItemValue (GridEntry grid_item, PaintEventArgs pevent, int depth, Rectangle rect) 
		{
			if (grid_item.PropertyDescriptor == null)
				return; 

			int xLoc = SplitterLocation+ENTRY_SPACING;
			if (grid_item.PaintValueSupported) {
				pevent.Graphics.DrawRectangle (Pens.Black, SplitterLocation + ENTRY_SPACING, 
							       rect.Y + 2, VALUE_PAINT_WIDTH + 1, row_height - ENTRY_SPACING*2);
				grid_item.PaintValue (pevent.Graphics, 
						      new Rectangle (SplitterLocation + ENTRY_SPACING + 1, 
								     rect.Y + ENTRY_SPACING + 1,
								     VALUE_PAINT_WIDTH, row_height - (ENTRY_SPACING*2 +1)));
				xLoc += VALUE_PAINT_INDENT;
			}

			Font font = this.Font;
			if (grid_item.IsResetable || !grid_item.HasDefaultValue)
				font = bold_font;
			Brush brush = grid_item.IsReadOnly ? inactive_text_brush : SystemBrushes.ControlText;
			string valueText = String.Empty;
			if (!grid_item.IsMerged || grid_item.IsMerged && grid_item.HasMergedValue) {
				if (grid_item.IsPassword)
					valueText = new String (PASSWORD_PAINT_CHAR, grid_item.ValueText.Length);
				else
					valueText = grid_item.ValueText;
			}
			pevent.Graphics.DrawString (valueText, font,
						    brush,
						    new RectangleF (xLoc + ENTRY_SPACING, rect.Y + ENTRY_SPACING,
								    ClientRectangle.Width-(xLoc), row_height - ENTRY_SPACING*2), 
						    string_format);
		}

		private void DrawGridItem (GridEntry grid_item, PaintEventArgs pevent, int depth, ref int yLoc) {
			if (yLoc > -row_height && yLoc < ClientRectangle.Height) {
				// Left column
				pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor),
							       0, yLoc, V_INDENT, row_height);
			
				if (grid_item.GridItemType == GridItemType.Category) {
					pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor), depth*V_INDENT,yLoc,ClientRectangle.Width-(depth*V_INDENT), row_height);
				}

				DrawGridItemLabel (grid_item, pevent,
						  depth,
						  new Rectangle (depth * V_INDENT, yLoc, SplitterLocation - depth * V_INDENT, row_height));
				DrawGridItemValue (grid_item, pevent,
						  depth,
						  new Rectangle (SplitterLocation + ENTRY_SPACING , yLoc, 
								 ClientRectangle.Width - SplitterLocation - ENTRY_SPACING - (vbar.Visible ? vbar.Width : 0), 
								 row_height));

				if (grid_item.GridItemType != GridItemType.Category) {
					Pen pen = ThemeEngine.Current.ResPool.GetPen (property_grid.LineColor);
					// vertical divider line
					pevent.Graphics.DrawLine (pen, SplitterLocation, yLoc, SplitterLocation, yLoc + row_height);
			
					// draw the horizontal line
					pevent.Graphics.DrawLine (pen, 0, yLoc + row_height, ClientRectangle.Width, yLoc + row_height);
				}				
				
				if (grid_item.Expandable) {
					int y = yLoc + row_height / 2 - ENTRY_SPACING + 1;
					grid_item.PlusMinusBounds = DrawPlusMinus (pevent.Graphics, (depth - 1) * V_INDENT + ENTRY_SPACING + 1, 
										   y, grid_item.Expanded, grid_item.GridItemType == GridItemType.Category);
				}

			}
			grid_item.Top = yLoc;
			yLoc += row_height;
		}

		private Rectangle DrawPlusMinus (Graphics g, int x, int y, bool expanded, bool category) {
			Rectangle bounds = new Rectangle (x, y, 8, 8);
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
		private void RedrawEvent (object sender, System.EventArgs e) 
		{
			Refresh ();
		}

		#endregion

		private void listBox_MouseUp (object sender, MouseEventArgs e) {
			AcceptListBoxSelection (sender);
		}

		private void listBox_KeyDown (object sender, KeyEventArgs e)
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

		void AcceptListBoxSelection (object sender) 
		{
			GridEntry entry = this.SelectedGridItem as GridEntry;
			if (entry != null) {
				grid_textbox.Text = (string) ((ListBox) sender).SelectedItem;
				CloseDropDown ();
				if (TrySetEntry (entry, grid_textbox.Text))
					UnfocusSelection ();
			}
		}

		private void DropDownButtonClicked (object sender, EventArgs e) 
		{
			DropDownEdit ();
		}

		private void DropDownEdit ()
		{
			GridEntry entry = this.SelectedGridItem as GridEntry;
			if (entry == null)
				return;

			if (entry.HasCustomEditor) {
				entry.EditValue ((IWindowsFormsEditorService) this);
			} else {
				if (dropdown_form.Visible) {
					CloseDropDown ();
				}
				else {
					ICollection std_values = entry.AcceptedValues;
					if (std_values != null) {
						if (dropdown_list == null) {
							dropdown_list = new ListBox ();
							dropdown_list.KeyDown += new KeyEventHandler (listBox_KeyDown);
							dropdown_list.MouseUp += new MouseEventHandler (listBox_MouseUp);
						}
						dropdown_list.Items.Clear ();
						dropdown_list.BorderStyle = BorderStyle.FixedSingle;
						int selected_index = 0;
						int i = 0;
						string valueText = entry.ValueText;
						foreach (object obj in std_values) {
							dropdown_list.Items.Add (obj);
							if (valueText != null && valueText.Equals (obj))
								selected_index = i;
							i++;
						}
						dropdown_list.Height = row_height * Math.Min (dropdown_list.Items.Count, 15);
						dropdown_list.Width = ClientRectangle.Width - SplitterLocation - (vbar.Visible ? vbar.Width : 0);
						if (std_values.Count > 0)
							dropdown_list.SelectedIndex = selected_index;
						DropDownControl (dropdown_list);
					}
				}
			}
		}

		private void DialogButtonClicked (object sender, EventArgs e) 
		{
			GridEntry entry = this.SelectedGridItem as GridEntry;
			if (entry != null && entry.HasCustomEditor)
				entry.EditValue ((IWindowsFormsEditorService) this);
		}

		private void VScrollBar_HandleValueChanged (object sender, EventArgs e) 
		{
			UpdateView ();
		}

		private void grid_textbox_ToggleValue (object sender, EventArgs args) 
		{
			ToggleValue (this.SelectedGridItem);
		}

		private void grid_textbox_KeyDown (object sender, KeyEventArgs e) 
		{
			switch (e.KeyData & Keys.KeyCode) {
			case Keys.Down:
				if (e.Alt) {
					DropDownEdit ();
					e.Handled = true;
				}
				break;
			}
		}

		private void grid_textbox_Validate (object sender, CancelEventArgs args)
		{
			if (!TrySetEntry (this.SelectedGridItem, grid_textbox.Text))
				args.Cancel = true;
		}

		private void ToggleValue (GridEntry entry)
		{
			if (entry != null && !entry.IsReadOnly && entry.GridItemType == GridItemType.Property)
				entry.ToggleValue ();
		}

		internal void UpdateItem (GridEntry entry)
		{
			if (entry == null || entry.GridItemType == GridItemType.Category || 
			    entry.GridItemType == GridItemType.Root) {
				grid_textbox.Visible = false;
				InvalidateItem (entry);
				return;
			}

			if (this.SelectedGridItem == entry) {
				SuspendLayout ();
				grid_textbox.Visible = false;
				if (entry.IsResetable || !entry.HasDefaultValue)
					grid_textbox.Font = bold_font;
				else
					grid_textbox.Font = this.Font;

				if (entry.IsReadOnly) {
					grid_textbox.DropDownButtonVisible = false;
					grid_textbox.DialogButtonVisible = false;
					grid_textbox.ReadOnly = true;
					grid_textbox.ForeColor = SystemColors.GrayText;
				} else {
					grid_textbox.DropDownButtonVisible = entry.AcceptedValues != null || 
						entry.EditorStyle == UITypeEditorEditStyle.DropDown;
					grid_textbox.DialogButtonVisible = entry.EditorStyle == UITypeEditorEditStyle.Modal;
					grid_textbox.ForeColor = SystemColors.ControlText;
					grid_textbox.ReadOnly = !entry.IsEditable;
				}
				UpdateGridTextBoxBounds (entry);
				grid_textbox.PasswordChar = entry.IsPassword ? PASSWORD_TEXT_CHAR : '\0';
				grid_textbox.Text = entry.IsMerged && !entry.HasMergedValue ? String.Empty : entry.ValueText;
				grid_textbox.Visible = true;
				InvalidateItem (entry);
				ResumeLayout (false);
			} else {
				grid_textbox.Visible = false;
			}
		}

		private void UpdateGridTextBoxBounds (GridEntry entry)
		{
			if (entry == null || this.RootGridItem == null)
				return;

			int y = -vbar.Value*row_height;
			CalculateItemY (entry, this.RootGridItem.GridItems, ref y);
			int x = SplitterLocation + ENTRY_SPACING + (entry.PaintValueSupported ? VALUE_PAINT_INDENT : 0);
			grid_textbox.SetBounds (x + ENTRY_SPACING, y + ENTRY_SPACING,
						ClientRectangle.Width - ENTRY_SPACING - x - (vbar.Visible ? vbar.Width : 0),
						row_height - ENTRY_SPACING);
		}

		// Calculates the sum of the heights of all items before the one
		//
		private bool CalculateItemY (GridEntry entry, GridItemCollection items, ref int y)
		{
			foreach (GridItem item in items) {
				if (item == entry)
					return true;
				y += row_height;
				if (item.Expandable && item.Expanded)
					if (CalculateItemY (entry, item.GridItems, ref y))
						return true;
			}
			return false;
		}

		private void ScrollToItem (GridEntry item)
		{
			if (item == null || this.RootGridItem == null)
				return;

			int itemY = -vbar.Value*row_height;
			int value = vbar.Value;;
			CalculateItemY (item, this.RootGridItem.GridItems, ref itemY);
			if (itemY < 0) // the new item is above the viewable area
				value += itemY / row_height;
			else if (itemY + row_height > Height) // the new item is below the viewable area
				value += ((itemY + row_height) - Height) / row_height + 1;
			if (value >= vbar.Minimum && value <= vbar.Maximum)
				vbar.Value = value;
		}

		internal void SelectItem (GridEntry oldItem, GridEntry newItem) 
		{
			if (oldItem != null)
				InvalidateItemLabel (oldItem);
			if (newItem != null) {
				UpdateItem (newItem);
				ScrollToItem (newItem);
			} else {
				grid_textbox.Visible = false;
				vbar.Visible = false;
			}
		}

		internal void UpdateView ()
		{
			UpdateScrollBar ();
			Invalidate ();
			Update ();
			UpdateItem (this.SelectedGridItem);
		}

		internal void ExpandItem (GridEntry item)
		{
			UpdateItem (this.SelectedGridItem);
			Invalidate (new Rectangle (0, item.Top, Width, Height - item.Top));
		}

		internal void CollapseItem (GridEntry item)
		{
			UpdateItem (this.SelectedGridItem);
			Invalidate (new Rectangle (0, item.Top, Width, Height - item.Top));
		}

		private void ShowDropDownControl (Control control, bool resizeable) 
		{
			dropdown_form.Size = control.Size;
			control.Dock = DockStyle.Fill;

			if (resizeable) {
				dropdown_form.Padding = dropdown_form_padding;
				dropdown_form.Width += dropdown_form_padding.Right;
				dropdown_form.Height += dropdown_form_padding.Bottom;
				dropdown_form.FormBorderStyle = FormBorderStyle.Sizable;
				dropdown_form.SizeGripStyle = SizeGripStyle.Show;
			} else {
				dropdown_form.FormBorderStyle = FormBorderStyle.None;
				dropdown_form.SizeGripStyle = SizeGripStyle.Hide;
				dropdown_form.Padding = Padding.Empty;
			}

			dropdown_form.Controls.Add (control);
			dropdown_form.Width = Math.Max (ClientRectangle.Width - SplitterLocation - (vbar.Visible ? vbar.Width : 0), 
							control.Width);
			dropdown_form.Location = PointToScreen (new Point (grid_textbox.Right - dropdown_form.Width, grid_textbox.Location.Y + row_height));
			RepositionInScreenWorkingArea (dropdown_form);
			Point location = dropdown_form.Location;

			Form owner = FindForm ();
			owner.AddOwnedForm (dropdown_form);
			dropdown_form.Show ();
			if (dropdown_form.Location != location)
				dropdown_form.Location = location;

			System.Windows.Forms.MSG msg = new MSG ();
			object queue_id = XplatUI.StartLoop (Thread.CurrentThread);
			control.Focus ();
			while (dropdown_form.Visible && XplatUI.GetMessage (queue_id, ref msg, IntPtr.Zero, 0, 0)) {
				switch (msg.message) {
					case Msg.WM_NCLBUTTONDOWN:
				    case Msg.WM_NCMBUTTONDOWN:
				    case Msg.WM_NCRBUTTONDOWN:
				    case Msg.WM_LBUTTONDOWN:
				    case Msg.WM_MBUTTONDOWN:
				    case Msg.WM_RBUTTONDOWN:
				    	if (!HwndInControl (dropdown_form, msg.hwnd))
							CloseDropDown ();
					break;
					case Msg.WM_ACTIVATE:
					case Msg.WM_NCPAINT:
				 		if (owner.window.Handle == msg.hwnd)
							CloseDropDown ();
					break;						
				}
				XplatUI.TranslateMessage (ref msg);
				XplatUI.DispatchMessage (ref msg);
			}
			XplatUI.EndLoop (Thread.CurrentThread);			
		}

		private void RepositionInScreenWorkingArea (Form form)
		{
			Rectangle workingArea = Screen.FromControl (form).WorkingArea;
			if (!workingArea.Contains (form.Bounds)) {
				int x, y;
				x = form.Location.X;
				y = form.Location.Y;

				if (form.Location.X < workingArea.X)
					x = workingArea.X;

				if (form.Location.Y + form.Size.Height > workingArea.Height) {
					Point aboveTextBox = PointToScreen (new Point (grid_textbox.Right - form.Width, grid_textbox.Location.Y));
					y = aboveTextBox.Y - form.Size.Height;
				}

				form.Location = new Point (x, y);
			}
		}

		private bool HwndInControl (Control c, IntPtr hwnd)
		{
			if (hwnd == c.window.Handle)
				return true;
			foreach (Control cc in c.Controls.GetAllControls ()) {
				if (HwndInControl (cc, hwnd))
					return true;
			}
			return false;
		}
		#endregion

		#region IWindowsFormsEditorService Members

		public void CloseDropDown () 
		{
			dropdown_form.Hide ();
			dropdown_form.Controls.Clear ();
		}

		public void DropDownControl (Control control) 
		{
			bool resizeable = this.SelectedGridItem != null ? SelectedGridItem.EditorResizeable : false;
			ShowDropDownControl (control, resizeable);
		}

		public System.Windows.Forms.DialogResult ShowDialog (Form dialog) {
			return dialog.ShowDialog (this);
		}

		#endregion

		internal class PropertyGridDropDown : Form 
		{
			protected override CreateParams CreateParams {
				get {
					CreateParams cp = base.CreateParams;
					cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
					cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOPMOST);				
					return cp;
				}
			}

		}
	}
}
