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
using System.Drawing;

namespace System.Windows.Forms.PropertyGridInternal
{
	public class PropertyGridView : System.Windows.Forms.ScrollableControl
	{
		internal const int LEFT_COLUMN_WIDTH = 16;
		internal const int ROW_HEIGHT = 16;
		internal const int RESIZE_WIDTH = 3;
		private TextBox grid_textbox;
		internal PropertyGrid property_grid;
		internal bool redraw;
		internal bool resizing_grid;
		internal int label_column_width;

		public PropertyGridView(PropertyGrid property_grid)
		{
			this.property_grid = property_grid;
			this.BackColor = Color.Beige;
			grid_textbox = new TextBox();

			grid_textbox.Visible = false;
			grid_textbox.Font = new Font(this.Font,FontStyle.Bold);
			grid_textbox.BorderStyle = BorderStyle.None;
			grid_textbox.BackColor = this.BackColor;
			grid_textbox.Validated += new EventHandler(grid_textbox_Validated);

			label_column_width = 65;
			resizing_grid = false;

			this.Controls.Add(grid_textbox);

			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
			SizeChanged+=new EventHandler(RedrawEvent);
			MouseMove+=new MouseEventHandler(PropertyGridView_MouseMove);
			
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			this.grid_textbox.Visible = false;
			Draw(pevent);
			pevent.Graphics.DrawImage(this.ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
			if (property_grid.SelectedGridItem != null)
				grid_textbox.Visible = true;
			base.OnPaint (pevent);
		}

		// Derived classes should override Draw method and we dont want
		// to break the control signature, hence this approach.
		internal virtual void Draw (PaintEventArgs e) 
		{
			if (redraw) 
			{
				Rectangle grid_rect = new Rectangle(0,0,this.Width-1,this.Height-1);

				Rectangle grid_left_rect = new Rectangle(grid_rect.Left+1,grid_rect.Top+1,LEFT_COLUMN_WIDTH,ROW_HEIGHT);
				Rectangle grid_label_rect = new Rectangle(grid_left_rect.Right,grid_rect.Top+1,label_column_width,ROW_HEIGHT);
				Rectangle grid_value_rect = new Rectangle(grid_label_rect.Right,grid_rect.Top+1,grid_rect.Right-grid_label_rect.Right,ROW_HEIGHT);


				Brush label_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText);
				Brush label_backcolor_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindow);
				Brush back_color_brush = ThemeEngine.Current.ResPool.GetSolidBrush(this.BackColor);
				Brush control_brush = ThemeEngine.Current.ResPool.GetSolidBrush(property_grid.LineColor);
				Pen control_pen = ThemeEngine.Current.ResPool.GetPen(property_grid.LineColor);
				
				// draw grid outline
				e.Graphics.FillRectangle(back_color_brush,grid_rect);
				e.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(SystemColors.ControlDark),grid_rect);

				// draw items
				GridItemCollection grid_items = this.property_grid.grid_items;
				for (int i = 0; i < grid_items.Count; i++) {
					GridItem grid_item = grid_items[i];
					
					label_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText);
					label_backcolor_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindow);
					if (grid_item == this.property_grid.SelectedGridItem) {
						label_backcolor_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHilight);
						label_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHilightText);
						grid_textbox.Size = new Size(grid_value_rect.Size.Width-6,grid_value_rect.Size.Height);
						grid_textbox.Location = new Point(grid_value_rect.Location.X+4,grid_value_rect.Location.Y+1);
						
						// PDB - added check to prevent crash with test app
						if (grid_item.Value != null)  {
							grid_textbox.Text = grid_item.Value.ToString();
						} else {
							grid_textbox.Text = string.Empty;
						}
					}

					e.Graphics.FillRectangle(control_brush,grid_left_rect);

					e.Graphics.FillRectangle(label_backcolor_brush,grid_label_rect);
					e.Graphics.DrawRectangle(control_pen,grid_label_rect);
					e.Graphics.DrawString(grid_item.Label,this.Font,label_brush,grid_label_rect.Left + 5,grid_label_rect.Top+1);

					e.Graphics.FillRectangle(back_color_brush,grid_value_rect);
					e.Graphics.DrawRectangle(control_pen,grid_value_rect);
					// PDB - added check to prevent crash with test app
					if (grid_item.Value != null) {
						e.Graphics.DrawString(grid_item.Value.ToString(),new Font(this.Font,FontStyle.Bold),ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText),grid_value_rect.Left + 2,grid_value_rect.Top+1);
					}

				   
					// shift down for next item
					grid_left_rect.Y = grid_label_rect.Y = grid_value_rect.Y = grid_left_rect.Y + ROW_HEIGHT;
				}
				redraw = false;
			}
		}

			
		private void grid_textbox_Validated(object sender, EventArgs e)
		{
			if (this.property_grid.SelectedGridItem != null)
			this.property_grid.SelectedGridItem.PropertyDescriptor.SetValue(this.property_grid.SelectedObject,this.property_grid.SelectedGridItem.PropertyDescriptor.Converter.ConvertTo(((TextBox)sender).Text,this.property_grid.SelectedGridItem.PropertyDescriptor.PropertyType));
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (resizing_grid) {
				label_column_width = Math.Max(e.X - LEFT_COLUMN_WIDTH,LEFT_COLUMN_WIDTH);
				Redraw();
			} else if (e.X > label_column_width+LEFT_COLUMN_WIDTH - RESIZE_WIDTH && e.X < label_column_width+LEFT_COLUMN_WIDTH + RESIZE_WIDTH) {
				this.Cursor = Cursors.VSplit;
			}
			base.OnMouseMove (e);
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.X > label_column_width+LEFT_COLUMN_WIDTH + 2 - RESIZE_WIDTH && e.X < label_column_width+LEFT_COLUMN_WIDTH + 2 + RESIZE_WIDTH) {
				resizing_grid = true;
			} else {
				int index = e.Y / ROW_HEIGHT;
				if (index < property_grid.grid_items.Count)
					this.property_grid.SelectedGridItem = this.property_grid.grid_items[index];
				base.OnMouseDown (e);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			resizing_grid = false;
			base.OnMouseUp (e);
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (this.property_grid.SelectedGridItem != null) {
				grid_textbox.Focus();
			}
			base.OnKeyDown (e);
		}


		internal void Redraw() 
		{
			redraw = true;
			Refresh ();
		}

		private void RedrawEvent(object sender, System.EventArgs e) 
		{
			Redraw();
		}

		private void PropertyGridView_MouseMove(object sender, MouseEventArgs e)
		{

			if (e.X > label_column_width+LEFT_COLUMN_WIDTH - RESIZE_WIDTH && e.X < label_column_width+LEFT_COLUMN_WIDTH + RESIZE_WIDTH
				|| resizing_grid) {
				this.Cursor = Cursors.VSplit;
			} else {
				this.Cursor = Cursors.Default;
			}
		}
	}
}
