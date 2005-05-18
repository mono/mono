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
using System.Windows.Forms.Design;

namespace System.Windows.Forms.PropertyGridInternal 
{
	public class PropertyGridView : System.Windows.Forms.ScrollableControl, IWindowsFormsEditorService
	{

		#region Private Members
		private const int V_INDENT = 16;
		private const int ROW_HEIGHT = 16;
		private const int RESIZE_WIDTH = 3;
		private const int BUTTON_WIDTH = 25;
		private PropertyGridTextBox grid_textbox;
		private PropertyGrid property_grid;
		private bool resizing_grid;
		private int splitter_location;
		private int open_grid_item_count = -1;
		private int skipped_grid_items;
		private Form dropdown_form;
		private Form dialog_form;
		private ListBox listBox;
		private VScrollBar vbar;
		#endregion

		#region Contructors
		public PropertyGridView (PropertyGrid propertyGrid) 
		{
			property_grid = propertyGrid;

			property_grid.SelectedGridItemChanged+=new SelectedGridItemChangedEventHandler(HandleSelectedGridItemChanged);
			property_grid.PropertyValueChanged+=new PropertyValueChangedEventHandler(HandlePropertyValueChanged);

			this.BackColor = Color.Beige;
			grid_textbox = new PropertyGridTextBox();
			grid_textbox.DropDownButtonClicked +=new EventHandler(DropDownButtonClicked);
			grid_textbox.DialogButtonClicked +=new EventHandler(DialogButtonClicked);

			dropdown_form = new Form();
			dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.Deactivate +=new EventHandler(dropdown_form_Deactivate);
			
			dialog_form = new Form();
			//dialog_form.FormBorderStyle = FormBorderStyle.None;

			listBox = new ListBox();
			listBox.Dock = DockStyle.Fill;
			listBox.SelectedIndexChanged +=new EventHandler(listBox_SelectedIndexChanged);
			dropdown_form.Controls.Add(listBox);

			grid_textbox.Visible = true;
			grid_textbox.Font = this.Font;//new Font(this.Font,FontStyle.Bold);
			grid_textbox.BackColor = this.BackColor;
			// Not working at all, used to??
			grid_textbox.Validating += new CancelEventHandler(TextBoxValidating);

			vbar = new VScrollBar();
			vbar.Visible = false;
			vbar.Scroll+=new ScrollEventHandler(HandleScroll);
			vbar.Dock = DockStyle.Right;
			this.Controls.Add(vbar);

			splitter_location = 65;
			resizing_grid = false;

			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
			SizeChanged+=new EventHandler(RedrawEvent);
			
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, false);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}

		#endregion

		#region Protected Instance Methods
		
		protected override void OnPaint(PaintEventArgs e)
		{
			// Decide if we need a scrollbar
			open_grid_item_count = 0;

			// draw grid outline
			//DrawBackground(e);
			
			e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);

			// draw grid items
			// can we use the transform
			//pevent.Graphics.TranslateTransform(0, -vbar.Value*ROW_HEIGHT);
			int yLoc = -vbar.Value*ROW_HEIGHT;
			DrawGridItems(property_grid.grid_items, e, 1, ref yLoc);

			DrawGrid(e, yLoc);
			
			


			if (property_grid.SelectedGridItem != null && property_grid.SelectedGridItem.GridItemType == GridItemType.Property) 
			{
				this.Controls.Add(grid_textbox);
				if (!grid_textbox.Focused)
					grid_textbox.Focus();
			}
			else 
			{
				this.Controls.Remove(grid_textbox);
			}
			UpdateScrollBar();
			
			base.OnPaint(e);
		}

		protected override void OnMouseMove (MouseEventArgs e) 
		{
			if (resizing_grid) {
				splitter_location = Math.Max(e.X,V_INDENT);
				Refresh();
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
			if (e.X > splitter_location - RESIZE_WIDTH && e.X < splitter_location + RESIZE_WIDTH) {
				resizing_grid = true;
			}
			else {
				GridItem foundItem = GetSelectedGridItem(property_grid.grid_items, e.Y);
				
				if (foundItem != null)
				{
					if (foundItem.Expandable) 
					{
						if (foundItem.PlusMinusBounds.Contains(e.X,e.Y))
						{
							foundItem.Expanded = !foundItem.Expanded;
						}
					}
					this.property_grid.SelectedGridItem = foundItem;
				}
				
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

		private void UpdateScrollBar()
		{
			int visible_rows = this.ClientRectangle.Height/ROW_HEIGHT;
			if (open_grid_item_count > visible_rows)
			{
				vbar.Visible = true;
				vbar.SmallChange = 1;
				vbar.Minimum = 0;
				vbar.Maximum = open_grid_item_count-1;
				vbar.LargeChange = visible_rows;
			}
			else
			{
				vbar.Visible = false;
			}

		}

		private GridItem GetGridItemAt (int y) 
		{
			return null;
		}

		#region Drawing Code

		private void DrawGrid(PaintEventArgs pevent, int yLoc)
		{
			Pen pen = ThemeEngine.Current.ResPool.GetPen(property_grid.LineColor);
			// vertical divider line
			pevent.Graphics.DrawLine(pen, splitter_location, 0, splitter_location, yLoc);
			
			while (yLoc >= 0)
			{
				// horizontal lines
				pevent.Graphics.DrawLine(pen, 0, yLoc, ClientRectangle.Width, yLoc);
				yLoc -= ROW_HEIGHT;
			}
		}

		private void DrawGridItems(GridItemCollection grid_items, PaintEventArgs pevent, int depth, ref int yLoc)
		{
			foreach (GridItem grid_item in grid_items) 
			{
				DrawGridItem (grid_item, pevent, depth, ref yLoc);
				if (grid_item.Expanded)
					DrawGridItems(grid_item.GridItems, pevent, depth, ref yLoc);
			}
		}

		private void DrawGridItem (GridItem grid_item, PaintEventArgs pevent, int depth, ref int yLoc) 
		{
			// left column
			pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor), 0,yLoc,V_INDENT, ROW_HEIGHT);

			if (grid_item.Expandable) 
			{
				grid_item.PlusMinusBounds = DrawPlusMinus(pevent, 3, yLoc+3, grid_item.Expanded, grid_item.GridItemType == GridItemType.Category);
			}
			
			if (grid_item.GridItemType == GridItemType.Category)
			{
				pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor), depth*V_INDENT,yLoc,ClientRectangle.Width-(depth*V_INDENT), ROW_HEIGHT);
			}
			
			if (grid_item == property_grid.SelectedGridItem && grid_item.GridItemType != GridItemType.Category)
			{
				pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (Color.Blue), new Rectangle(V_INDENT,yLoc, splitter_location-V_INDENT, ROW_HEIGHT));
				// Label
				pevent.Graphics.DrawString(grid_item.Label,this.Font,SystemBrushes.Window,new RectangleF(1+depth*V_INDENT,yLoc+2, splitter_location-(5+depth*V_INDENT), ROW_HEIGHT-4));
				
				grid_textbox.Location = new Point(splitter_location+1, yLoc);
				grid_textbox.Size = new Size(ClientRectangle.Width-splitter_location,ROW_HEIGHT);
			}
			else
			{
				Font font = this.Font;
				Brush brush = SystemBrushes.WindowText;
				if (grid_item.GridItemType == GridItemType.Category)
				{
					font = new Font(font, FontStyle.Bold);
					brush = SystemBrushes.ControlDark;
				}
				// Label
				pevent.Graphics.DrawString(grid_item.Label,font,brush,new RectangleF(1+depth*V_INDENT,yLoc+2, splitter_location-(5+depth*V_INDENT), ROW_HEIGHT-4));
			}
			// Value
			if (grid_item.PropertyDescriptor != null)
			{

				bool paintsValue = false;
				UITypeEditor editor = null;
				object temp = grid_item.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				editor = (UITypeEditor)temp;//grid_item.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				if (editor != null) 
				{
					paintsValue = editor.GetPaintValueSupported();
				}

				if (grid_item == property_grid.SelectedGridItem)
				{
					grid_textbox.DropDownButtonVisible = false;
					grid_textbox.DialogButtonVisible = false;
					listBox.Items.Clear();
					if (editor != null) 
					{
						UITypeEditorEditStyle style = editor.GetEditStyle();
					
						switch (style)
						{
							case UITypeEditorEditStyle.DropDown:
								grid_textbox.DropDownButtonVisible = true;
								break;
							case UITypeEditorEditStyle.Modal:
								grid_textbox.DialogButtonVisible = true;
								break;
						}
					}
					else 
					{
						if (grid_item.PropertyDescriptor.Converter.GetStandardValuesSupported()) 
						{
							foreach (object obj in grid_item.PropertyDescriptor.Converter.GetStandardValues())
								listBox.Items.Add(obj);
							grid_textbox.DropDownButtonVisible = true;
							grid_textbox.ReadOnly = true;
						}
					}
				}

				

				int xLoc = splitter_location+1;
				if (paintsValue)
				{
					pevent.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(Color.Black), splitter_location+2,yLoc+2, 20, ROW_HEIGHT-4);
					try
					{
						editor.PaintValue(grid_item.Value, pevent.Graphics, new Rectangle(splitter_location+3,yLoc+3, 19, ROW_HEIGHT-5));
					}
					catch (Exception ex)
					{
						// design time stuff is not playing nice
					}
					xLoc += 27;
				}

				string value = grid_item.PropertyDescriptor.Converter.ConvertToString(grid_item.Value);
				pevent.Graphics.DrawString(value,this.Font,SystemBrushes.WindowText,new RectangleF(xLoc,yLoc+2, ClientRectangle.Width-(xLoc), ROW_HEIGHT-4));
			}
			grid_item.Top = yLoc;
			yLoc += ROW_HEIGHT;
			open_grid_item_count++;
		}

		private Rectangle DrawPlusMinus (PaintEventArgs pevent, int x, int y, bool expanded, bool category)
		{
			Rectangle bounds = new Rectangle(x, y, 8, 8);
			if (!category) pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush(Color.White), bounds);
			pevent.Graphics.DrawRectangle (SystemPens.ControlDark, bounds);
			pevent.Graphics.DrawLine (SystemPens.ControlDark, x+2, y+4, x + 6, y+4);
			if (!expanded)
				pevent.Graphics.DrawLine (SystemPens.ControlDark, x+4, y+2, x+4, y+6);

			return bounds;
		}

		#endregion

		#region Event Handling
		private void RedrawEvent (object sender, System.EventArgs e) 
		{
			Refresh();
		}

		private void TextBoxValidating (object sender, CancelEventArgs e) 
		{
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					try 
					{
						
						SetPropertyValue(desc.Converter.ConvertFromString(grid_textbox.Text));
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error converting string");
					}
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
					SetPropertyValue(listBox.SelectedItem);
				}
			}
			dropdown_form.Hide();
			Refresh();
		}

		private void SetPropertyValue(object newVal)
		{
			if (this.property_grid.SelectedGridItem != null) 
			{
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) 
				{
					desc.SetValue(property_grid.SelectedObject, newVal);
				}
			}
		}

		private void DropDownButtonClicked (object sender, EventArgs e) 
		{
			if (listBox.Items.Count > 0)
			{
				dropdown_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+ROW_HEIGHT));
				dropdown_form.Width = grid_textbox.Width;
				dropdown_form.Show();
			}
			else // use editor
			{
				UITypeEditor editor = (UITypeEditor)property_grid.SelectedGridItem.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				System.ComponentModel.Design.ServiceContainer service_container = new System.ComponentModel.Design.ServiceContainer();
				service_container.AddService(typeof(System.Windows.Forms.Design.IWindowsFormsEditorService), this);
				editor.EditValue(null, service_container,property_grid.SelectedGridItem.Value);
			}
		}
		
		private void DialogButtonClicked(object sender, EventArgs e) 
		{
			//dialog_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+ROW_HEIGHT));
			//dropdown_form.Width = grid_textbox.Width;
			dialog_form.ShowDialog(this);
		}

		private void HandleScroll(object sender, ScrollEventArgs e)
		{
			if (e.NewValue < 0)
			{
				e.NewValue = 0;
				if (e.NewValue == vbar.Value)return;
			}
			if (e.NewValue > vbar.Maximum-ClientRectangle.Height/ROW_HEIGHT)
			{
				e.NewValue = vbar.Maximum-ClientRectangle.Height/ROW_HEIGHT+1;
				if (e.NewValue == vbar.Value)return;
			}

			switch (e.Type)
			{
				case ScrollEventType.SmallDecrement:
					XplatUI.ScrollWindow(Handle, 0, ROW_HEIGHT, false);
					Invalidate(new Rectangle(0,0,ClientRectangle.Width,ROW_HEIGHT));
					break;
				case ScrollEventType.SmallIncrement:
					XplatUI.ScrollWindow(Handle, 0, -ROW_HEIGHT, false);
					Invalidate(new Rectangle(0,ClientRectangle.Bottom-ROW_HEIGHT,ClientRectangle.Width,ROW_HEIGHT));
					break;
				case ScrollEventType.LargeDecrement:
					XplatUI.ScrollWindow(Handle, 0, ROW_HEIGHT, false);
					Invalidate(ClientRectangle);
					break;
				case ScrollEventType.LargeIncrement:
					XplatUI.ScrollWindow(Handle, 0, -ROW_HEIGHT, false);
					Invalidate(ClientRectangle);
					break;
				/*case ScrollEventType.ThumbTrack:
					XplatUI.ScrollWindow(Handle, 0, -(vbar.Value-e.NewValue), false);
					Invalidate(ClientRectangle);
					break;
				case ScrollEventType.ThumbPosition:
					Invalidate(ClientRectangle);
					break;*/
			}
		}


		private void HandleSelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
		{			
			// Region not working correctly
			//Region clip = new Region();
			//if (property_grid.SelectedGridItem != null)
			//	clip.Union(new Rectangle(0,property_grid.SelectedGridItem.Top, ClientRectangle.Width, ROW_HEIGHT));
				//	clip.Union(new Rectangle(0,property_grid.SelectedGridItem.Top, ClientRectangle.Width, ROW_HEIGHT));

			if (e.NewSelection.PropertyDescriptor != null)
				grid_textbox.Text = e.NewSelection.PropertyDescriptor.Converter.ConvertToString(e.NewSelection.Value);

			Invalidate(/*clip*/this.ClientRectangle);
			Update();
		}

		private void HandlePropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			if (e.ChangedItem.PropertyDescriptor != null)
				grid_textbox.Text = e.ChangedItem.PropertyDescriptor.Converter.ConvertToString(e.ChangedItem.Value);
		}
		
		#region IWindowsFormsEditorService Members

		public void CloseDropDown()
		{
			// TODO:  Add PropertyGrid.CloseDropDown implementation
		}

		public void DropDownControl(Control control)
		{
			// TODO:  Add PropertyGrid.DropDownControl implementation
		}

		public System.Windows.Forms.DialogResult ShowDialog(Form dialog)
		{
			// TODO:  Add PropertyGrid.ShowDialog implementation
			return DialogResult.OK;
		}

		#endregion

		#region DropDownForm Class
		#endregion DropDownForm Class


	}
}
