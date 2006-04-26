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

namespace System.Windows.Forms.PropertyGridInternal {
	internal class PropertyGridView : System.Windows.Forms.ScrollableControl, IWindowsFormsEditorService {

		#region Private Members
		private double splitter_percent = .5;
		private const int V_INDENT = 16;
		private int row_height;
		private int font_height_padding = 3;
		private const int RESIZE_WIDTH = 3;
		private const int BUTTON_WIDTH = 25;
		private PropertyGridTextBox grid_textbox;
		private PropertyGrid property_grid;
		private bool resizing_grid;
		private int open_grid_item_count = -1;
		private int skipped_grid_items;
		private PropertyGridDropDown dropdown_form;
		private bool dropdown_form_showing;
		private Form dialog_form;
		private ImplicitVScrollBar vbar;
		private StringFormat string_format;
		private Font bold_font;
		#endregion

		#region Contructors
		public PropertyGridView (PropertyGrid propertyGrid) {
			property_grid = propertyGrid;

			property_grid.SelectedGridItemChanged+=new SelectedGridItemChangedEventHandler(HandleSelectedGridItemChanged);
			property_grid.PropertyValueChanged+=new PropertyValueChangedEventHandler(HandlePropertyValueChanged);

			string_format = new StringFormat();
			string_format.FormatFlags = StringFormatFlags.NoWrap;
			string_format.Trimming = StringTrimming.None;

			grid_textbox = new PropertyGridTextBox();
			grid_textbox.DropDownButtonClicked +=new EventHandler(DropDownButtonClicked);
			grid_textbox.DialogButtonClicked +=new EventHandler(DialogButtonClicked);

			
			dropdown_form = new PropertyGridDropDown();
			dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.ShowInTaskbar = false;
			
			dialog_form = new Form();
			dialog_form.FormBorderStyle = FormBorderStyle.None;
			dialog_form.ShowInTaskbar = false;

			skipped_grid_items = 0;
			row_height = Font.Height + font_height_padding;

			grid_textbox.Visible = false;
			grid_textbox.Font = this.Font;
			grid_textbox.BackColor = this.BackColor;
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
			SizeChanged+=new EventHandler(RedrawEvent);
			
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, false);
		}

		#endregion

		#region Protected Instance Methods

		protected override void OnFontChanged(EventArgs e) {
			bold_font = new Font(this.Font, FontStyle.Bold);

			
			row_height = Font.Height + font_height_padding;
			base.OnFontChanged (e);
		}

		protected override void OnDoubleClick(EventArgs e) {
			if (property_grid.SelectedGridItem.Expandable)
				property_grid.SelectedGridItem.Expanded = !property_grid.SelectedGridItem.Expanded;
			else 
				ToggleValue();
			Invalidate();
			base.OnDoubleClick (e);
		}

		protected override void OnPaint(PaintEventArgs e) {
			// Decide if we need a scrollbar
			open_grid_item_count = 0;

			// Background
			e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
			
			// Left column
			e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor), 0,0,V_INDENT, ClientRectangle.Height);
			

			int yLoc = -vbar.Value*row_height;
			DrawGridItems(property_grid.grid_items, e, 1, ref yLoc);

			// Grid
			DrawGrid(e);

			// Border
			e.Graphics.DrawRectangle(SystemPens.ControlDark, 0,0,Width-1,Height-1 );
			
			UpdateScrollBar();
			
			base.OnPaint(e);
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

			if (resizing_grid) {
				int loc = Math.Max(e.X,2*V_INDENT);
				SplitterPercent = 1.0*loc/Width;
				Refresh();
			}
			if (e.X > SplitterLocation - RESIZE_WIDTH && e.X < SplitterLocation + RESIZE_WIDTH) 
				this.Cursor = Cursors.SizeWE;
			else
				this.Cursor = Cursors.Default;
			base.OnMouseMove (e);
		}

		protected override void OnMouseDown (MouseEventArgs e) {
			if (e.X > SplitterLocation - RESIZE_WIDTH && e.X < SplitterLocation + RESIZE_WIDTH) {
				resizing_grid = true;
			}
			else {
				int offset = -vbar.Value*row_height;
				GridItem foundItem = GetSelectedGridItem(property_grid.grid_items, e.Y, ref offset);
				
				if (foundItem != null) {
					if (foundItem.Expandable) {
						if (e.X >=3 && e.X <= 11 && (e.Y % row_height >= row_height/2-2 && e.Y % row_height <= row_height/2+4)) {
							foundItem.Expanded = !foundItem.Expanded;
							Invalidate();
						}
					}
					this.property_grid.SelectedGridItem = foundItem;
				}
				
				base.OnMouseDown (e);
			}
		}

		protected override void OnMouseUp (MouseEventArgs e) {
			resizing_grid = false;
			base.OnMouseUp (e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e) {
			if (property_grid.SelectedGridItem.Expandable)
				property_grid.SelectedGridItem.Expanded = !property_grid.SelectedGridItem.Expanded;
			base.OnKeyPress (e);
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
				splitter_percent = Math.Max(Math.Min(value, .9),.1);
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

		private void DrawGrid(PaintEventArgs pevent) {
			Pen pen = ThemeEngine.Current.ResPool.GetPen(property_grid.LineColor);
			// vertical divider line
			pevent.Graphics.DrawLine(pen, SplitterLocation, 0, SplitterLocation, (open_grid_item_count-skipped_grid_items)*row_height);
			
			int y = 0;
			while (y < ClientRectangle.Height && y/row_height + skipped_grid_items < open_grid_item_count) {
				// horizontal lines
				pevent.Graphics.DrawLine(pen, 0, y, ClientRectangle.Width, y);
				y += row_height;
			}
		}

		private void DrawGridItems(GridItemCollection grid_items, PaintEventArgs pevent, int depth, ref int yLoc) {
			foreach (GridItem grid_item in grid_items) {
				DrawGridItem (grid_item, pevent, depth, ref yLoc);
				if (grid_item.Expanded)
					DrawGridItems(grid_item.GridItems, pevent, (grid_item.GridItemType == GridItemType.Category) ? depth : depth+1, ref yLoc);
			}
		}

		private void DrawGridItemLabel(GridItem grid_item, PaintEventArgs pevent, Rectangle rect) {
			int x = rect.X+1;
			if (grid_item.Parent != null && grid_item.Parent.GridItemType != GridItemType.Category)
				x += V_INDENT;

			Font font = this.Font;
			Brush brush = SystemBrushes.WindowText;
			if (grid_item.GridItemType == GridItemType.Category) {
				font = bold_font;
				brush = SystemBrushes.ControlDark;
			}

			if (grid_item == property_grid.SelectedGridItem && grid_item.GridItemType != GridItemType.Category) {
				pevent.Graphics.FillRectangle (SystemBrushes.Highlight, rect);
				// Label
				brush = SystemBrushes.HighlightText;
			}

			
			if (grid_item.GridItemType == GridItemType.Category) {
				pevent.Graphics.DrawString(grid_item.Label,font,brush,x, rect.Y + 2);
				if (grid_item == property_grid.SelectedGridItem) {
					SizeF size = pevent.Graphics.MeasureString(grid_item.Label, bold_font);
					ControlPaint.DrawFocusRectangle(pevent.Graphics, new Rectangle(x,rect.Y+2, (int)size.Width, (int)size.Height));
				}
			}
			else
				pevent.Graphics.DrawString(grid_item.Label,font,brush,new Rectangle(x, rect.Y + 2,x-rect.X+rect.Width-2,rect.Height-2),string_format);
		}

		private void DrawGridItemValue(GridItem grid_item, PaintEventArgs pevent, Rectangle rect) {
			// Value
			if (grid_item.PropertyDescriptor != null) {

				bool paintsValue = false;
				UITypeEditor editor = null;
				object temp = grid_item.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				editor = (UITypeEditor)temp;//grid_item.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				if (editor != null) {
					paintsValue = editor.GetPaintValueSupported();
				}

				if (grid_item == property_grid.SelectedGridItem) {
					grid_textbox.ReadOnly = false;
					grid_textbox.DropDownButtonVisible = false;
					grid_textbox.DialogButtonVisible = false;
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
							if (grid_item.PropertyDescriptor.Converter != null) {
								if (grid_item.PropertyDescriptor.Converter.GetStandardValuesSupported()) {
							
									grid_textbox.DropDownButtonVisible = true;
									grid_textbox.ReadOnly = true;
								}
							}
							else {
								System.Console.WriteLine("Converter not available for type {0}",grid_item.PropertyDescriptor.PropertyType);
							}
						
						}
						catch (Exception) {
						}
					}
				}

				

				int xLoc = SplitterLocation+1;
				if (paintsValue) {
					pevent.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(Color.Black), SplitterLocation+2,rect.Y+2, 20, row_height-4);
					try {
						editor.PaintValue(grid_item.Value, pevent.Graphics, new Rectangle(SplitterLocation+3,rect.Y+3, 19, row_height-5));
					}
					catch (Exception ex) {
						System.Console.WriteLine(ex.Message);
						System.Console.WriteLine("Paint Value failed for type {0}",grid_item.PropertyDescriptor.PropertyType);
						// design time stuff is not playing nice
					}
					xLoc += 27;
				}

				Font font = this.Font;
				try {
					if (grid_item.PropertyDescriptor.Converter != null) {
						string value = grid_item.PropertyDescriptor.Converter.ConvertToString(grid_item.Value);
						if (grid_item.PropertyDescriptor.CanResetValue(property_grid.SelectedObject))
							font = bold_font;
				
						pevent.Graphics.DrawString(value,font,SystemBrushes.WindowText,new RectangleF(xLoc,rect.Y+2, ClientRectangle.Width-(xLoc), row_height),string_format);
					}
					else {
						System.Console.WriteLine("No converter for type {0}",grid_item.PropertyDescriptor.PropertyType);
					}

				}
				catch (Exception) {
				}
				if (grid_item == property_grid.SelectedGridItem && grid_item.GridItemType != GridItemType.Category) {
					grid_textbox.SetBounds(xLoc, rect.Top, ClientRectangle.Width-xLoc - (vbar.Visible ? vbar.Width: 0),row_height);
				}
			}
		}

		private void DrawGridItem (GridItem grid_item, PaintEventArgs pevent, int depth, ref int yLoc) {
			if (yLoc > -row_height && yLoc < ClientRectangle.Height) {
				if (grid_item.Expandable) {
					grid_item.PlusMinusBounds = DrawPlusMinus(pevent.Graphics, 3, yLoc+row_height/2-3, grid_item.Expanded, grid_item.GridItemType == GridItemType.Category);
				}
			
				if (grid_item.GridItemType == GridItemType.Category) {
					pevent.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (property_grid.LineColor), depth*V_INDENT,yLoc,ClientRectangle.Width-(depth*V_INDENT), row_height);
				}

				DrawGridItemLabel(grid_item, pevent, new Rectangle(depth*V_INDENT,yLoc, SplitterLocation-depth*V_INDENT, row_height));
				DrawGridItemValue(grid_item, pevent, new Rectangle(SplitterLocation+2,yLoc, ClientRectangle.Width-SplitterLocation-2, row_height));
			}
			grid_item.Top = yLoc;
			yLoc += row_height;
			open_grid_item_count++;
		}

		private Rectangle DrawPlusMinus (Graphics g, int x, int y, bool expanded, bool category) {
			Rectangle bounds = new Rectangle(x, y, 8, 8);
			if (!category) g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush(Color.White), bounds);
			g.DrawRectangle (SystemPens.ControlDark, bounds);
			g.DrawLine (SystemPens.ControlDark, x+2, y+4, x + 6, y+4);
			if (!expanded)
				g.DrawLine (SystemPens.ControlDark, x+4, y+2, x+4, y+6);

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
							System.Console.WriteLine("No converter for type {0}",desc.PropertyType);
						}
					}
					catch (Exception) {
						Console.WriteLine("Error converting string");
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

		private void dropdown_form_Deactivate (object sender, EventArgs e) {
			dropdown_form_showing = false;
			dropdown_form.Hide();
		}

		private void listBox_MouseUp(object sender, MouseEventArgs e) {
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					SetPropertyValue(((PropertyGridListBox)sender).SelectedItem);
				}
			}
			dropdown_form_showing = false;
			dropdown_form.Hide();
			Refresh();
		}

		private void SetPropertyValue(object newVal) {
			if (this.property_grid.SelectedGridItem != null) {
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) {
					desc.SetValue(property_grid.SelectedObject, newVal);
				}
			}
		}

		private void DropDownButtonClicked (object sender, EventArgs e) {
			if (property_grid.SelectedGridItem.PropertyDescriptor.GetEditor(typeof(UITypeEditor)) == null) {
				dropdown_form.Deactivate +=new EventHandler(dropdown_form_Deactivate);
				PropertyGridListBox listBox = new PropertyGridListBox();
				listBox.BorderStyle = BorderStyle.FixedSingle;
				listBox.Dock = DockStyle.Fill;
				int selected_index = 0;
				int i = 0;
				foreach (object obj in property_grid.SelectedGridItem.PropertyDescriptor.Converter.GetStandardValues()) {
					listBox.Items.Add(obj);
					if (property_grid.SelectedGridItem.Value.Equals(obj))
						selected_index = i;
					i++;
				}
				listBox.SelectedIndex = selected_index;
				listBox.MouseUp+=new MouseEventHandler(listBox_MouseUp);
				dropdown_form.Controls.Clear();
				dropdown_form.Controls.Add(listBox);
				dropdown_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+row_height));
				dropdown_form.Width = grid_textbox.Width;
				dropdown_form.Show();
			}
			else { // use editor
				UITypeEditor editor = (UITypeEditor)property_grid.SelectedGridItem.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				System.ComponentModel.Design.ServiceContainer service_container = new System.ComponentModel.Design.ServiceContainer();
				service_container.AddService(typeof(System.Windows.Forms.Design.IWindowsFormsEditorService), this);
				SetPropertyValue(editor.EditValue(new ITypeDescriptorContextImpl(this.property_grid), service_container,property_grid.SelectedGridItem.Value));
			}
		}
		
		private void DialogButtonClicked(object sender, EventArgs e) {
			dialog_form.Show();
		}

		private void HandleValueChanged(object sender, EventArgs e) {
			if (vbar.Value <= 0) {
				vbar.Value = 0;
			}
			if (vbar.Value > vbar.Maximum-ClientRectangle.Height/row_height) {
				vbar.Value = vbar.Maximum-ClientRectangle.Height/row_height+1;
			}
			
			int scroll_amount = (skipped_grid_items-vbar.Value)*row_height;
			XplatUI.ScrollWindow(Handle, 0, scroll_amount, false);
			skipped_grid_items = vbar.Value;
			if (scroll_amount > 0)
				Invalidate(new Rectangle(0,0,Width,scroll_amount+1));
			else
				Invalidate(new Rectangle(0,Height+scroll_amount-1,Width,1-scroll_amount));
		}

		private void grid_textbox_ToggleValue(object sender, EventArgs e) {
			ToggleValue();
			Invalidate();
		}

		private void HandleSelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e) {			
			// Region not working correctly
			//Region clip = new Region();
			//if (property_grid.SelectedGridItem != null)
			//	clip.Union(new Rectangle(0,property_grid.SelectedGridItem.Top, ClientRectangle.Width, row_height));
			//	clip.Union(new Rectangle(0,property_grid.SelectedGridItem.Top, ClientRectangle.Width, row_height));

			if (e.NewSelection.GridItemType == GridItemType.Property)
				grid_textbox.Visible = true;
			else
				grid_textbox.Visible = false;

			if (e.NewSelection.PropertyDescriptor != null) {
				grid_textbox.Text = e.NewSelection.PropertyDescriptor.Converter.ConvertToString(e.NewSelection.Value);
				if (e.NewSelection.PropertyDescriptor.CanResetValue(property_grid.SelectedObject))
					grid_textbox.Font = bold_font;
				else
					grid_textbox.Font = this.Font;
			}

			Invalidate(/*clip*/this.ClientRectangle);
			Update();
		}

		private void HandlePropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
			if (e.ChangedItem.PropertyDescriptor != null) {
				grid_textbox.Text = e.ChangedItem.PropertyDescriptor.Converter.ConvertToString(e.ChangedItem.Value);
				if (e.ChangedItem.PropertyDescriptor.CanResetValue(property_grid.SelectedObject))
					grid_textbox.Font = bold_font;
				else
					grid_textbox.Font = this.Font;
			}
		}
		

		
		#endregion

		#region IWindowsFormsEditorService Members

		public void CloseDropDown() {
			dropdown_form_showing = false;
			dropdown_form.Hide();
		}

		public void DropDownControl(Control control) {
			dropdown_form.Deactivate +=new EventHandler(dropdown_form_Deactivate);
			dropdown_form.Size = control.Size;
			control.Dock = DockStyle.Fill;
			dropdown_form.Controls.Clear();
			dropdown_form.Controls.Add(control);
			dropdown_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+row_height));
			dropdown_form.Width = grid_textbox.Width;

			dropdown_form_showing = true;
			dropdown_form.Show();
			System.Windows.Forms.MSG msg = new MSG();
			while (XplatUI.GetMessage(ref msg, IntPtr.Zero, 0, 0) && dropdown_form_showing) {
				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);
			}
		}

		public System.Windows.Forms.DialogResult ShowDialog(Form dialog) {
			return dialog.ShowDialog(this);
		}

		#endregion

		#region DropDownForm Class
		#endregion DropDownForm Class

		#region Internal Classes
		internal class ITypeDescriptorContextImpl : System.ComponentModel.ITypeDescriptorContext {
			private PropertyGrid property_grid;
			public ITypeDescriptorContextImpl(PropertyGrid propertyGrid) {
				property_grid = propertyGrid;
			}
			#region ITypeDescriptorContext Members

			public void OnComponentChanged() {
				// TODO:  Add SystemComp.OnComponentChanged implementation
			}

			public IContainer Container {
				get {
					return property_grid as IContainer;
				}
			}

			public bool OnComponentChanging() {
				// TODO:  Add SystemComp.OnComponentChanging implementation
				return false;
			}

			public object Instance {
				get {
					return property_grid.SelectedGridItem.Value;
				}
			}

			public PropertyDescriptor PropertyDescriptor {
				get {
					return property_grid.SelectedGridItem.PropertyDescriptor;
				}
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType) {
				// TODO:  Add SystemComp.GetService implementation
				return null;
			}

			#endregion

		}


		
		/*
			class ComboListBox
		*/
		internal class PropertyGridDropDown : Form {
			protected override CreateParams CreateParams {
				get {
					CreateParams cp = base.CreateParams;				
					cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
					cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);				
					return cp;
				}
			}

		}

		internal class PropertyGridListBox : ListBox {
			private bool mouse_down;
			public PropertyGridListBox() {
				SetStyle(ControlStyles.DoubleBuffer, true);
				mouse_down =false;
			}

			protected override void OnMouseDown(MouseEventArgs e) {
				mouse_down = true;
				base.OnMouseDown (e);
			}

			protected override void OnMouseUp(MouseEventArgs e) {
				mouse_down = false;
				base.OnMouseUp (e);
			}

			protected override void OnMouseMove(MouseEventArgs e) {
				if (!mouse_down)
					return;
				for (int i = 0; i < Items.Count; i++) {
					if (GetItemRectangle(i).Contains(e.X, e.Y)) {
						SelectedIndex = i;
						break;
					}
				}
				base.OnMouseMove (e);
			}





		}

		#endregion
	}
}
