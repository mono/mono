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
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;

namespace System.Windows.Forms.PropertyGridInternal 
{
	public class PropertyGridView : System.Windows.Forms.ScrollableControl 
	{

		#region Private Members
		internal const int V_INDENT = 16;
		internal const int ROW_HEIGHT = 16;
		internal const int RESIZE_WIDTH = 3;
		internal const int BUTTON_WIDTH = 25;
		private PropertyGridTextBox grid_textbox;
		internal PropertyGrid property_grid;
		internal bool redraw;
		internal bool resizing_grid;
		internal int label_column_width;
		private bool add_hscroll;
		private int open_grid_item_count = -1;
		private VScrollBar vbar;
		private bool vbar_added;
		private int skipped_grid_items;
		private Form dropdown_form;
		private ListBox listBox;
		#endregion

		#region Contructors
		public PropertyGridView (PropertyGrid propertyGrid) 
		{
			property_grid = propertyGrid;
			this.BackColor = Color.Beige;
			grid_textbox = new PropertyGridTextBox();
			grid_textbox.DropDownButtonClicked +=new EventHandler(grid_textbox_DropDownButtonClicked);

			dropdown_form = new Form();
			dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.Deactivate +=new EventHandler(dropdown_form_Deactivate);

			listBox = new ListBox();
			listBox.Dock = DockStyle.Fill;
			listBox.SelectedIndexChanged +=new EventHandler(listBox_SelectedIndexChanged);
			dropdown_form.Controls.Add(listBox);

			grid_textbox.Visible = true;
			grid_textbox.Font = new Font(this.Font,FontStyle.Bold);
			grid_textbox.BackColor = this.BackColor;
			// Not working at all, used to??
			//grid_textbox.TextBox.Validated += new EventHandler(TextBoxValidated);

			label_column_width = 65;
			resizing_grid = false;

			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
			SizeChanged+=new EventHandler(RedrawEvent);
			
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}

		#endregion

		#region Protected Instance Methods
		protected override void WndProc (ref Message m) 
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_PAINT: {				
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle);
				DoPaint (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}
			}
			base.WndProc (ref m);
		}

		protected override void OnMouseMove (MouseEventArgs e) 
		{
			if (resizing_grid) {
				label_column_width = Math.Max(e.X - V_INDENT,V_INDENT);
				Redraw();
			}
			else if (e.X > label_column_width+V_INDENT - RESIZE_WIDTH && e.X < label_column_width+V_INDENT + RESIZE_WIDTH) {
				this.Cursor = Cursors.VSplit;
			}
			base.OnMouseMove (e);
		}

		private GridItem GetSelectedGridItem (GridItemCollection grid_items, int y) 
		{
			foreach (GridItem child_grid_item in grid_items) {
				if (y > child_grid_item.Top && y < child_grid_item.Top + ROW_HEIGHT) {
					return child_grid_item;
				}
				GridItem foundItem = GetSelectedGridItem(child_grid_item.GridItems, y);
				if (foundItem != null)
					return foundItem;
			}
			return null;
		}

		protected override void OnMouseDown (MouseEventArgs e) 
		{
			if (e.X > label_column_width+V_INDENT + 2 - RESIZE_WIDTH && e.X < label_column_width+V_INDENT + 2 + RESIZE_WIDTH) {
				resizing_grid = true;
			}
			else {
				GridItem foundItem = GetSelectedGridItem(property_grid.grid_items, e.Y);
				if (this.property_grid.SelectedGridItem != null) {
					PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
					if (desc != null) {
						desc.SetValue(property_grid.SelectedObject, desc.Converter.ConvertFromString(grid_textbox.TextBoxText));
					}
				}
				if (foundItem != null)
					property_grid.SelectedGridItem = foundItem;
				if (property_grid.SelectedGridItem.Expandable) {
					if (((CategoryGridEntry)property_grid.SelectedGridItem).PlusMinusBounds.Contains(e.X,e.Y)){
						property_grid.SelectedGridItem.Expanded = !property_grid.SelectedGridItem.Expanded;
					}
				}
				Redraw();
				base.OnMouseDown (e);
			}
		}

		protected override void OnMouseUp (MouseEventArgs e) 
		{
			resizing_grid = false;
			base.OnMouseUp (e);
		}

		protected override void OnKeyDown (KeyEventArgs e) 
		{
			base.OnKeyDown (e);
		}

		#endregion

		#region Private Helper Methods
		private void AddVerticalScrollBar (int total_grid_items, bool count_changed) 
		{
			if (vbar == null) {
				vbar = new VScrollBar ();
				count_changed = true;
			}

			vbar.Bounds = new Rectangle (ClientRectangle.Width - vbar.Width,
				0, vbar.Width, Height);

			if (count_changed) {
				vbar.Maximum = total_grid_items;
				int height = ClientRectangle.Height;
				vbar.LargeChange = height / ROW_HEIGHT;
			}

			if (!vbar_added) {
				Controls.Add (vbar);
				vbar.ValueChanged += new EventHandler (VScrollBarValueChanged);
				vbar_added = true;
			}

			vbar.Visible = true;
		}

		internal void Redraw () 
		{
			redraw = true;
			Refresh ();
		}

		private GridItem GetGridItemAt (int y) 
		{
			return null;
		}

		#region Drawing Code
		private void DoPaint (PaintEventArgs pevent) 
		{
			Draw (pevent.ClipRectangle, pevent.Graphics);
		}

		[MonoTODO("Do a better job of clipping")]
		private void Draw (Rectangle clip, Graphics dc) 
		{
			if (redraw) {
				
				// Decide if we need a scrollbar
				bool add_vscroll = false;
				int old_open_grid_item_count = open_grid_item_count;

				// Use same brushes for all grid items
				Brush line_brush = ThemeEngine.Current.ResPool.GetSolidBrush(property_grid.LineColor);
				Pen line_pen = ThemeEngine.Current.ResPool.GetPen(property_grid.LineColor);
				Brush text_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText);


				Rectangle fill = ClientRectangle;
				// draw grid outline
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), fill);
				dc.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(SystemColors.ControlDark),fill);

				int depth = 0;
				int item_height = ROW_HEIGHT;
				Font font = Font;
				int height = ClientRectangle.Height;

				GridItemCollection grid_items = this.property_grid.grid_items;

				open_grid_item_count = 0;
				foreach (GridItem grid_item in grid_items) {
					DrawGridItem (grid_item, dc, clip, ref depth, item_height, font, height, line_brush, text_brush, line_pen);
					depth = 0;
				}

				add_vscroll = (open_grid_item_count * ROW_HEIGHT) > ClientRectangle.Height;

				if (add_vscroll) {
					AddVerticalScrollBar (open_grid_item_count, old_open_grid_item_count != open_grid_item_count);
				} else if (vbar != null) {
					vbar.Visible = false;
					skipped_grid_items = 0;
				}

				if (property_grid.SelectedGridItem != null && property_grid.SelectedGridItem.GridItemType == GridItemType.Property) {
					//if (grid_textbox.Visible == false) {
					this.Controls.Add(grid_textbox);
					//}
					//grid_textbox.Visible = true;
					if (!grid_textbox.Focused)
						grid_textbox.Focus();
				}
				else {
					this.Controls.Remove(grid_textbox);
					//grid_textbox.Visible = false;
				}

				redraw = false;
			}
		}

		private void DrawGridItem (GridItem grid_item, Graphics dc, Rectangle clip, ref int depth, int item_height,
			Font font, int max_height, Brush line_brush, Brush text_brush, Pen line_pen) 
		{

			int y = ClientRectangle.Top+1+(ROW_HEIGHT*(open_grid_item_count-skipped_grid_items));
			grid_item.Top = y;

			Rectangle indentRectangle = new Rectangle(ClientRectangle.Left+1,y,V_INDENT,ROW_HEIGHT);
			Rectangle labelRectangle = new Rectangle(indentRectangle.Right,y,label_column_width,ROW_HEIGHT);
			Rectangle valueRectangle = new Rectangle(labelRectangle.Right,y,ClientRectangle.Right-labelRectangle.Right,ROW_HEIGHT);

			DrawGridItemIndent(grid_item, dc, line_brush, indentRectangle, grid_item.Expandable, grid_item.Expanded);
			DrawGridItemLabel(grid_item, dc, labelRectangle, line_pen, text_brush, Font, depth, line_brush);
			DrawGridItemValue(grid_item, dc, valueRectangle, line_pen, text_brush, new Font(Font, FontStyle.Bold), line_brush);

			if (grid_item == property_grid.SelectedGridItem) {
				if (grid_item.Value != null) {
					grid_textbox.TextBoxText = grid_item.Value.ToString();
				} 
				else {
					grid_textbox.TextBoxText = string.Empty;
				}
				grid_textbox.Size = new Size(valueRectangle.Size.Width- (vbar == null || !vbar.Visible ? 0 : ThemeEngine.Current.VerticalScrollBarWidth),valueRectangle.Size.Height);
				grid_textbox.Location = new Point(valueRectangle.Location.X+4,valueRectangle.Location.Y+1);
			}
			
			open_grid_item_count++;

			
			depth++;
			if (grid_item.Expanded) {
				foreach (GridItem child_item in grid_item.GridItems) {
					int tdepth = depth;
					DrawGridItem(child_item, dc, clip, ref tdepth, item_height, font, max_height, line_brush, text_brush, line_pen);
				}
			}
		}

		private void DrawGridItemIndent (GridItem grid_item, Graphics dc, Brush brush, Rectangle rect, bool showPlusMinus, bool expanded) 
		{

			dc.FillRectangle(brush,rect);

			if (showPlusMinus) {
				Rectangle plus_minus_rect = new Rectangle(rect.X+3,rect.Y+3,8,8);
				grid_item.PlusMinusBounds = plus_minus_rect;

				dc.DrawRectangle (SystemPens.ControlDark, plus_minus_rect);

				int middle = (plus_minus_rect.Bottom-plus_minus_rect.Top)/2 + plus_minus_rect.Top;
				int x = plus_minus_rect.X;
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 

				if (!expanded) {
					dc.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
				}
			}
		}

		private void DrawGridItemLabel (GridItem grid_item, Graphics dc, Rectangle rect, Pen pen, Brush brush, Font font, int depth, Brush line_brush) 
		{
			Brush backBrush = ThemeEngine.Current.ResPool.GetSolidBrush(BackColor);
			Brush foreBrush = brush;
			//bool selectedItem = false, expandable = false;

			if (grid_item == property_grid.SelectedGridItem){
				backBrush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHilight);
				foreBrush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHilightText);
			}
			if (grid_item.Expandable){
				backBrush = line_brush;
				foreBrush = brush;
			}

			dc.FillRectangle(backBrush,rect);
			dc.DrawRectangle(pen,rect);
			dc.DrawString(grid_item.Label,font,foreBrush,rect.Left + 5 + depth*V_INDENT,rect.Top+1);
		}

		[MonoTODO("GetEditor once the TypeDescriptor class is complete")]
		private void DrawGridItemValue (GridItem grid_item, Graphics dc, Rectangle rect, Pen pen, Brush brush, Font font, Brush line_brush) 
		{
			dc.FillRectangle((grid_item.GridItemType == GridItemType.Property) ? ThemeEngine.Current.ResPool.GetSolidBrush(BackColor) : line_brush,rect);
			dc.DrawRectangle(pen,rect);

			// PDB - added check to prevent crash with test app
			if (grid_item.Value != null) {
				dc.DrawString(grid_item.Value.ToString(),font,brush,rect.Left + 2,rect.Top+1);
			}

			if (grid_item == property_grid.SelectedGridItem) {
				grid_textbox.ReadOnly = false;
				if (grid_item.PropertyDescriptor != null) {
					UITypeEditor editor = null;//(UITypeEditor)grid_item.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
					if (editor != null) {
						UITypeEditorEditStyle style =  editor.GetEditStyle();
						if (style == UITypeEditorEditStyle.DropDown) {
							grid_textbox.DropDownButtonVisible = true;
							grid_textbox.DialogButtonVisible = false;
							
						}
						else if (style == UITypeEditorEditStyle.Modal) {
							grid_textbox.DropDownButtonVisible = false;
							grid_textbox.DialogButtonVisible = true;
						}
						
					}
					else if (grid_item.PropertyDescriptor.Converter.GetStandardValuesSupported()) {
						listBox.Items.Clear();
						foreach (object obj in grid_item.PropertyDescriptor.Converter.GetStandardValues())
							listBox.Items.Add(obj);
						grid_textbox.DropDownButtonVisible = true;
						grid_textbox.DialogButtonVisible = false;
						grid_textbox.ReadOnly = true;
					}
					else {
						grid_textbox.DropDownButtonVisible = false;
						grid_textbox.DialogButtonVisible = false;
					}
				}
				else {
					grid_textbox.DropDownButtonVisible = false;
					grid_textbox.DialogButtonVisible = false;
				}
			}
		}

		#endregion

		#region Event Handling
		private void RedrawEvent (object sender, System.EventArgs e) 
		{
			Redraw();
		}

		private void VScrollBarValueChanged (object sender, EventArgs e) 
		{
			int old_skip = skipped_grid_items;
			skipped_grid_items = vbar.Value;

			int y_move = (old_skip - skipped_grid_items) * ROW_HEIGHT;
			// need this to refresh drawing
			redraw = true;
			XplatUI.ScrollWindow (Handle, ClientRectangle, 0, y_move, true);
		}

		private void TextBoxValidated (object sender, EventArgs e) 
		{
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					desc.SetValue(property_grid.SelectedObject, desc.Converter.ConvertFromString(grid_textbox.TextBoxText));
				}
			}
		}

		#endregion

		
		#endregion

		private void dropdown_form_Deactivate (object sender, EventArgs e) 
		{
			dropdown_form.Hide();
		}


		private void listBox_SelectedIndexChanged (object sender, EventArgs e) 
		{
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					desc.SetValue(property_grid.SelectedObject, listBox.SelectedItem);
				}
			}
			dropdown_form.Hide();
			Redraw();
		}

		private void grid_textbox_DropDownButtonClicked (object sender, EventArgs e) 
		{
			dropdown_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+ROW_HEIGHT));
			dropdown_form.Width = grid_textbox.Width;
			dropdown_form.Show();
		}
	}
}
