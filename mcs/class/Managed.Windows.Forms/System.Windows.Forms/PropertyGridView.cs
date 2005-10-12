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
	internal class PropertyGridView : System.Windows.Forms.ScrollableControl, IWindowsFormsEditorService
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
		private PropertyGridDropDown dropdown_form;
		private bool dropdown_form_showing;
		private Form dialog_form;
		private VScrollBar vbar;
		private StringFormat stringFormat;
		#endregion

		#region Contructors
		public PropertyGridView (PropertyGrid propertyGrid) 
		{
			property_grid = propertyGrid;

			property_grid.SelectedGridItemChanged+=new SelectedGridItemChangedEventHandler(HandleSelectedGridItemChanged);
			property_grid.PropertyValueChanged+=new PropertyValueChangedEventHandler(HandlePropertyValueChanged);

			stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			stringFormat.Trimming = StringTrimming.None;

			this.BackColor = Color.Beige;
			grid_textbox = new PropertyGridTextBox();
			grid_textbox.DropDownButtonClicked +=new EventHandler(DropDownButtonClicked);
			grid_textbox.DialogButtonClicked +=new EventHandler(DialogButtonClicked);

			
			dropdown_form = new PropertyGridDropDown();
			dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.ShowInTaskbar = false;
			
			dialog_form = new Form();
			//dialog_form.FormBorderStyle = FormBorderStyle.None;

			

			grid_textbox.Visible = false;
			grid_textbox.Font = this.Font;//new Font(this.Font,FontStyle.Bold);
			grid_textbox.BackColor = this.BackColor;
			// Not working at all, used to??
			grid_textbox.Validating += new CancelEventHandler(TextBoxValidating);
			this.Controls.Add(grid_textbox);

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
			if (property_grid.SelectedGridItem != null && property_grid.SelectedGridItem.GridItemType == GridItemType.Property) 
			{
				grid_textbox.Visible = true;
				if (!grid_textbox.Focused)
					grid_textbox.Focus();
			}
			else 
			{
				grid_textbox.Visible = false;
			}

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
			e.Graphics.DrawRectangle(SystemPens.ControlDark, 0,0,Width-1,Height-1 );
			
			


			UpdateScrollBar();
			
			base.OnPaint(e);
		}

		protected override void OnMouseMove (MouseEventArgs e) 
		{

			if (resizing_grid) 
			{
				splitter_location = Math.Max(e.X,2*V_INDENT);
				Refresh();
			}
			if (e.X > splitter_location - RESIZE_WIDTH && e.X < splitter_location + RESIZE_WIDTH) 
				this.Cursor = Cursors.SizeWE;
			else
				this.Cursor = Cursors.Default;
			base.OnMouseMove (e);
		}

		private GridItem GetSelectedGridItem (GridItemCollection grid_items, int y, ref int current) 
		{
			foreach (GridItem child_grid_item in grid_items) 
			{
				if (y > current && y < current + ROW_HEIGHT) 
				{
					return child_grid_item;
				}
				current += ROW_HEIGHT;
				if (child_grid_item.Expanded)
				{
					GridItem foundItem = GetSelectedGridItem(child_grid_item.GridItems, y, ref current);
					if (foundItem != null)
						return foundItem;
				}
			}
			return null;
		}

		protected override void OnMouseDown (MouseEventArgs e) 
		{
			if (e.X > splitter_location - RESIZE_WIDTH && e.X < splitter_location + RESIZE_WIDTH) 
			{
				resizing_grid = true;
			}
			else 
			{
				int offset = -vbar.Value*ROW_HEIGHT;
				GridItem foundItem = GetSelectedGridItem(property_grid.grid_items, e.Y, ref offset);
				
				if (foundItem != null)
				{
					if (foundItem.Expandable) 
					{
						if (e.X >=3 && e.X <= 11 && (e.Y % ROW_HEIGHT >= 3 && e.Y % ROW_HEIGHT <= 11))
						{
							foundItem.Expanded = !foundItem.Expanded;
							Invalidate();
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
					DrawGridItems(grid_item.GridItems, pevent, (grid_item.GridItemType == GridItemType.Category) ? depth : depth+1, ref yLoc);
			}
		}

		private void DrawGridItemLabel(GridItem grid_item, PaintEventArgs pevent, Rectangle rect)
		{
			int x = rect.X+1;
			if (grid_item.Parent != null && grid_item.Parent.GridItemType != GridItemType.Category)
				x += V_INDENT;

			Font font = this.Font;
			Brush brush = SystemBrushes.WindowText;
			if (grid_item.GridItemType == GridItemType.Category)
			{
				font = new Font(font, FontStyle.Bold);
				brush = SystemBrushes.ControlDark;
			}

			if (grid_item == property_grid.SelectedGridItem && grid_item.GridItemType != GridItemType.Category)
			{
				pevent.Graphics.FillRectangle (SystemBrushes.Highlight, rect);
				// Label
				brush = SystemBrushes.HighlightText;
			}

			
			pevent.Graphics.DrawString(grid_item.Label,font,brush,new Rectangle(x, rect.Y + 2,x-rect.X+rect.Width-2,rect.Height-2),stringFormat);
		}

		private void DrawGridItemValue(GridItem grid_item, PaintEventArgs pevent, Rectangle rect)
		{
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
					grid_textbox.ReadOnly = false;
					grid_textbox.DropDownButtonVisible = false;
					grid_textbox.DialogButtonVisible = false;
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
						try
						{
							if (grid_item.PropertyDescriptor.Converter != null)
							{
								if (grid_item.PropertyDescriptor.Converter.GetStandardValuesSupported()) 
								{
							
									grid_textbox.DropDownButtonVisible = true;
									grid_textbox.ReadOnly = true;
								}
							}
							else
							{
								System.Console.WriteLine("Converter not available for type {0}",grid_item.PropertyDescriptor.PropertyType);
							}
						
						}
						catch (Exception ex)
						{
						}
					}
				}

				

				int xLoc = splitter_location+1;
				if (paintsValue)
				{
					pevent.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(Color.Black), splitter_location+2,rect.Y+2, 20, ROW_HEIGHT-4);
					try
					{
						editor.PaintValue(grid_item.Value, pevent.Graphics, new Rectangle(splitter_location+3,rect.Y+3, 19, ROW_HEIGHT-5));
					}
					catch (Exception ex)
					{
						System.Console.WriteLine(ex.Message);
						System.Console.WriteLine("Paint Value failed for type {0}",grid_item.PropertyDescriptor.PropertyType);
						// design time stuff is not playing nice
					}
					xLoc += 27;
				}

				Font font = this.Font;
				try 
				{
					if (grid_item.PropertyDescriptor.Converter != null)
					{
						string value = grid_item.PropertyDescriptor.Converter.ConvertToString(grid_item.Value);
						if (grid_item.PropertyDescriptor.CanResetValue(property_grid.SelectedObject))
							font = new Font(font, FontStyle.Bold);
				
						pevent.Graphics.DrawString(value,font,SystemBrushes.WindowText,new RectangleF(xLoc,rect.Y+2, ClientRectangle.Width-(xLoc), ROW_HEIGHT),stringFormat);
					}
					else
					{
						System.Console.WriteLine("No converter for type {0}",grid_item.PropertyDescriptor.PropertyType);
					}

				}
				catch (Exception e)
				{
				}
				if (grid_item == property_grid.SelectedGridItem && grid_item.GridItemType != GridItemType.Category)
				{
					grid_textbox.SetBounds(xLoc, rect.Top, ClientRectangle.Width-xLoc - (vbar.Visible ? vbar.Width: 0),ROW_HEIGHT);
				}
			}
		}

		private void DrawGridItem (GridItem grid_item, PaintEventArgs pevent, int depth, ref int yLoc) 
		{
			if (yLoc > -ROW_HEIGHT && yLoc < ClientRectangle.Height)
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

				DrawGridItemLabel(grid_item, pevent, new Rectangle(depth*V_INDENT,yLoc, splitter_location-depth*V_INDENT, ROW_HEIGHT));
				DrawGridItemValue(grid_item, pevent, new Rectangle(splitter_location+2,yLoc, ClientRectangle.Width-splitter_location-2, ROW_HEIGHT));
			
				
				
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
			if (this.property_grid.SelectedGridItem != null) 
			{
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) 
				{
					try 
					{
						if (desc.Converter != null)
						{
							SetPropertyValue(desc.Converter.ConvertFromString(grid_textbox.Text));
						}
						else
						{
							System.Console.WriteLine("No converter for type {0}",desc.PropertyType);
						}
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
			dropdown_form_showing = false;
			dropdown_form.Hide();
			//dropdown_form = new Form();
		}

		private void listBox_SelectedIndexChanged (object sender, EventArgs e) 
		{
			if (this.property_grid.SelectedGridItem != null) 
			{
				PropertyDescriptor desc = property_grid.SelectedGridItem.PropertyDescriptor;
				if (desc != null) 
				{
					SetPropertyValue(((ListBox)sender).SelectedItem);
				}
			}
			dropdown_form.Hide();
			//dropdown_form = new Form();
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
			if (property_grid.SelectedGridItem.PropertyDescriptor.GetEditor(typeof(UITypeEditor)) == null)
			{
				//dropdown_form.FormBorderStyle = FormBorderStyle.None;
				dropdown_form.Deactivate +=new EventHandler(dropdown_form_Deactivate);
				ListBox listBox = new ListBox();
				listBox.Dock = DockStyle.Fill;
				listBox.SelectedIndexChanged +=new EventHandler(listBox_SelectedIndexChanged);
				foreach (object obj in property_grid.SelectedGridItem.PropertyDescriptor.Converter.GetStandardValues())
					listBox.Items.Add(obj);
				dropdown_form.Controls.Clear();
				dropdown_form.Controls.Add(listBox);
				dropdown_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+ROW_HEIGHT));
				dropdown_form.Width = grid_textbox.Width;
				dropdown_form.Show();
			}
			else // use editor
			{
				UITypeEditor editor = (UITypeEditor)property_grid.SelectedGridItem.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
				System.ComponentModel.Design.ServiceContainer service_container = new System.ComponentModel.Design.ServiceContainer();
				service_container.AddService(typeof(System.Windows.Forms.Design.IWindowsFormsEditorService), this);
				SetPropertyValue(editor.EditValue(new ITypeDescriptorContextImpl(this.property_grid), service_container,property_grid.SelectedGridItem.Value));
			}
		}
		
		private void DialogButtonClicked(object sender, EventArgs e) 
		{
			//dialog_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+ROW_HEIGHT));
			//dropdown_form.Width = grid_textbox.Width;
			dialog_form.Show();
		}

		private void HandleScroll(object sender, ScrollEventArgs e)
		{
			if (e.NewValue <= 0)
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
					grid_textbox.Top += ROW_HEIGHT;
					Invalidate(ClientRectangle);
					//Invalidate(new Rectangle(0,0,ClientRectangle.Width,ROW_HEIGHT));
					break;
				case ScrollEventType.SmallIncrement:
					XplatUI.ScrollWindow(Handle, 0, -ROW_HEIGHT, false);
					grid_textbox.Top -= ROW_HEIGHT;
					Invalidate(ClientRectangle);
					//Invalidate(new Rectangle(0,ClientRectangle.Bottom-ROW_HEIGHT,ClientRectangle.Width,ROW_HEIGHT));
					break;
				case ScrollEventType.LargeDecrement:
					XplatUI.ScrollWindow(Handle, 0, ROW_HEIGHT, false);
					Invalidate(ClientRectangle);
					break;
				case ScrollEventType.LargeIncrement:
					XplatUI.ScrollWindow(Handle, 0, -ROW_HEIGHT, false);
					Invalidate(ClientRectangle);
					break;
				case ScrollEventType.ThumbTrack:
					XplatUI.ScrollWindow(Handle, 0, -(vbar.Value-e.NewValue), false);
					Invalidate(ClientRectangle);
					break;
				case ScrollEventType.ThumbPosition:
					Invalidate(ClientRectangle);
					break;
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
			{
				grid_textbox.Text = e.NewSelection.PropertyDescriptor.Converter.ConvertToString(e.NewSelection.Value);
				if (e.NewSelection.PropertyDescriptor.CanResetValue(property_grid.SelectedObject))
					grid_textbox.Font = new Font(this.Font, FontStyle.Bold);
				else
					grid_textbox.Font = this.Font;
			}

			Invalidate(/*clip*/this.ClientRectangle);
			Update();
		}

		private void HandlePropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			if (e.ChangedItem.PropertyDescriptor != null)
			{
				grid_textbox.Text = e.ChangedItem.PropertyDescriptor.Converter.ConvertToString(e.ChangedItem.Value);
				if (e.ChangedItem.PropertyDescriptor.CanResetValue(property_grid.SelectedObject))
					grid_textbox.Font = new Font(this.Font, FontStyle.Bold);
				else
					grid_textbox.Font = this.Font;
			}
		}
		
		#region IWindowsFormsEditorService Members

		public void CloseDropDown()
		{
			dropdown_form_showing = false;
			dropdown_form.Hide();
		}

		public void DropDownControl(Control control)
		{
			//dropdown_form.FormBorderStyle = FormBorderStyle.None;
			dropdown_form.Deactivate +=new EventHandler(dropdown_form_Deactivate);
			dropdown_form.Size = control.Size;
			control.Dock = DockStyle.Fill;
			dropdown_form.Controls.Clear();
			dropdown_form.Controls.Add(control);
			dropdown_form.Location = PointToScreen(new Point(grid_textbox.Location.X,grid_textbox.Location.Y+ROW_HEIGHT));
			dropdown_form.Width = grid_textbox.Width;

			dropdown_form_showing = true;
			dropdown_form.Show();
			System.Windows.Forms.MSG msg = new MSG();
			while (XplatUI.GetMessage(ref msg, IntPtr.Zero, 0, 0) && dropdown_form_showing) 
			{
				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);
			}
		}

		public System.Windows.Forms.DialogResult ShowDialog(Form dialog)
		{
			return dialog.ShowDialog(this);
		}

		#endregion

		#region DropDownForm Class
		#endregion DropDownForm Class

		#region Internal Classes
		internal class ITypeDescriptorContextImpl : System.ComponentModel.ITypeDescriptorContext
		{
			private PropertyGrid property_grid;
			public ITypeDescriptorContextImpl(PropertyGrid propertyGrid)
			{
				property_grid = propertyGrid;
			}
			#region ITypeDescriptorContext Members

			public void OnComponentChanged()
			{
				// TODO:  Add SystemComp.OnComponentChanged implementation
			}

			public IContainer Container
			{
				get
				{
					return property_grid as IContainer;
				}
			}

			public bool OnComponentChanging()
			{
				// TODO:  Add SystemComp.OnComponentChanging implementation
				return false;
			}

			public object Instance
			{
				get
				{
					return property_grid.SelectedGridItem.Value;
				}
			}

			public PropertyDescriptor PropertyDescriptor
			{
				get
				{
					return property_grid.SelectedGridItem.PropertyDescriptor;
				}
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{
				// TODO:  Add SystemComp.GetService implementation
				return null;
			}

			#endregion

		}


		
		/*
			class ComboListBox
		*/
		internal class PropertyGridDropDown : Form 
		{
			protected override CreateParams CreateParams
			{
				get 
				{
					CreateParams cp = base.CreateParams;				
					cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
					cp.ExStyle |= (int)(WindowStyles.WS_EX_TOOLWINDOW | WindowStyles.WS_EX_TOPMOST);				
					return cp;
				}
			}

		}
		#endregion

	}
}
