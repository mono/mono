//
// System.Drawing.Design.ColorEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//  Jonathan Chambers (joncham@gmail.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// (C) 2006 Jonathan Chambers
// 

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

using System;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Drawing.Design
{
	public class ColorEditor : UITypeEditor
	{
		private IWindowsFormsEditorService editorService;
		private Color selected_color;
		private bool color_chosen;
		private Control editor_control = null;

		public ColorEditor()
		{
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			if (provider != null) {
				editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (editorService != null) {
					if (editor_control == null)
						editor_control = GetEditorControl (value);
					editorService.DropDownControl(editor_control);
					if (color_chosen)
						return selected_color;
					else
						return null;
				}
			}
			return base.EditValue(context, provider, value);
		}

		private Control GetEditorControl (object value)
		{
			TabControl tab_control = new TabControl();
			tab_control.Dock = DockStyle.Fill;
			TabPage custom_tab = new TabPage("Custom");
			TabPage web_tab = new TabPage("Web");
			TabPage system_tab = new TabPage("System");

			ColorListBox web_listbox = new ColorListBox();
			ColorListBox system_listbox = new ColorListBox();
			web_listbox.Dock = DockStyle.Fill;
			system_listbox.Dock = DockStyle.Fill;

			web_tab.Controls.Add(web_listbox);
			system_tab.Controls.Add(system_listbox);

			SystemColorCompare system_compare = new SystemColorCompare();
			System.Collections.ArrayList color_list = new System.Collections.ArrayList();
			foreach (System.Reflection.PropertyInfo property in typeof(SystemColors).GetProperties(System.Reflection.BindingFlags.Public |System.Reflection.BindingFlags.Static)) {
				Color clr = (Color)property.GetValue(null,null);
				color_list.Add(clr);
			}
			color_list.Sort(system_compare);
			system_listbox.Items.AddRange(color_list.ToArray());
			system_listbox.MouseUp+=new MouseEventHandler(HandleMouseUp);
			system_listbox.SelectedValueChanged+=new EventHandler(HandleChange);

			WebColorCompare web_compare = new WebColorCompare();
			color_list = new System.Collections.ArrayList();
			foreach (KnownColor known_color in Enum.GetValues(typeof(KnownColor))) 
			{
				Color color = Color.FromKnownColor(known_color);
				if (color.IsSystemColor)
					continue;
				color_list.Add(color);
			}
			color_list.Sort(web_compare);
			web_listbox.Items.AddRange(color_list.ToArray());
			web_listbox.MouseUp+=new MouseEventHandler(HandleMouseUp);
			web_listbox.SelectedValueChanged+=new EventHandler(HandleChange);

			CustomColorPicker custom_picker = new CustomColorPicker ();
			custom_picker.Dock = DockStyle.Fill;
			custom_picker.ColorChanged += new EventHandler (CustomColorPicked);
			custom_tab.Controls.Add (custom_picker);

			tab_control.TabPages.Add(custom_tab);
			tab_control.TabPages.Add(web_tab);
			tab_control.TabPages.Add(system_tab);

			if (value != null) {
				Color current_color = (Color)value;
				if (current_color.IsSystemColor) 
				{
					system_listbox.SelectedValue = current_color;
					tab_control.SelectedTab = system_tab;
				}
				else if (current_color.IsKnownColor)
				{
					web_listbox.SelectedValue = current_color;
					tab_control.SelectedTab = web_tab;
				}
				selected_color = current_color;
				color_chosen = true;
			}

			tab_control.Height = 216; // the height of the custom colors tab
			return tab_control;
		}

		private void HandleChange(object sender, EventArgs e) 
		{
			selected_color = (Color)((ColorListBox)sender).Items[((ColorListBox)sender).SelectedIndex];
			color_chosen = true;
		}

		private void CustomColorPicked (object sender, EventArgs e)
		{
			selected_color = (Color)sender;
			color_chosen = true;
			if (editorService != null)
				editorService.CloseDropDown ();
		}

		private void HandleMouseUp(object sender, MouseEventArgs e) 
		{
			if (editorService != null)
				editorService.CloseDropDown();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;

			if (e.Value != null)
			{
				Color C = (Color) e.Value;
				using (SolidBrush sb = new SolidBrush (C))
					G.FillRectangle (sb, e.Bounds);
			}
		}

		class ColorListBox : ListBox {
			public ColorListBox() {
				this.DrawMode = DrawMode.OwnerDrawFixed;
				this.Sorted = true;
				this.ItemHeight = 14;
				this.BorderStyle = BorderStyle.FixedSingle;
			}

			protected override void OnDrawItem(DrawItemEventArgs e) {
				e.DrawBackground();
				Color color = (Color)this.Items[e.Index];
				using (System.Drawing.SolidBrush brush = new SolidBrush(color))
					e.Graphics.FillRectangle(brush, 2,e.Bounds.Top+2,21,9);
				e.Graphics.DrawRectangle(SystemPens.WindowText, 2,e.Bounds.Top+2,21,9);
				e.Graphics.DrawString(color.Name, this.Font, SystemBrushes.WindowText, 26,e.Bounds.Top);
				if ((e.State & DrawItemState.Selected) != 0)
					e.DrawFocusRectangle();
				base.OnDrawItem (e);
			}
		}

		class SystemColorCompare : System.Collections.IComparer {
			#region IComparer Members

			public int Compare(object x, object y) {
				Color c1 = (Color)x;
				Color c2 = (Color)y;
				return String.Compare(c1.Name, c2.Name);
			}

			#endregion
		}
		class WebColorCompare : System.Collections.IComparer 
		{
			#region IComparer Members

			public int Compare(object x, object y) 
			{
				Color c1 = (Color)x;
				Color c2 = (Color)y;
				return String.Compare(c1.Name, c2.Name);
			}

			#endregion
		}


		class CustomColorPicker : UserControl
		{
			Color[,] colors;
			bool highlighting;
			int x, y;
			public CustomColorPicker ()
			{
				colors = new Color[8, 8];
				colors[0, 0] = Color.White;
				colors[1, 0] = Color.FromArgb (224, 224, 224);
				colors[2, 0] = Color.Silver;
				colors[3, 0] = Color.Gray;
				colors[4, 0] = Color.FromArgb (64, 64, 64);
				colors[5, 0] = Color.Black;
				colors[6, 0] = Color.White;
				colors[7, 0] = Color.White;

				colors[0, 1] = Color.FromArgb (255, 192, 192);
				colors[1, 1] = Color.FromArgb (255, 128, 128);
				colors[2, 1] = Color.Red;
				colors[3, 1] = Color.FromArgb (192, 0, 0);
				colors[4, 1] = Color.Maroon;
				colors[5, 1] = Color.FromArgb (64, 0, 0);
				colors[6, 1] = Color.White;
				colors[7, 1] = Color.White;

				colors[0, 2] = Color.FromArgb (255, 224, 192);
				colors[1, 2] = Color.FromArgb (255, 192, 128);
				colors[2, 2] = Color.FromArgb (255, 128, 0);
				colors[3, 2] = Color.FromArgb (192, 64, 0);
				colors[4, 2] = Color.FromArgb (128, 64, 0);
				colors[5, 2] = Color.FromArgb (128, 64, 64);
				colors[6, 2] = Color.White;
				colors[7, 2] = Color.White;

				colors[0, 3] = Color.FromArgb (255, 255, 192);
				colors[1, 3] = Color.FromArgb (255, 255, 128);
				colors[2, 3] = Color.Yellow;
				colors[3, 3] = Color.FromArgb (192, 192, 0);
				colors[4, 3] = Color.Olive;
				colors[5, 3] = Color.FromArgb (64, 64, 0);
				colors[6, 3] = Color.White;
				colors[7, 3] = Color.White;

				colors[0, 4] = Color.FromArgb (192, 255, 192);
				colors[1, 4] = Color.FromArgb (128, 255, 128);
				colors[2, 4] = Color.Lime;
				colors[3, 4] = Color.FromArgb (0, 192, 0);
				colors[4, 4] = Color.Green;
				colors[5, 4] = Color.FromArgb (0, 64, 0);
				colors[6, 4] = Color.White;
				colors[7, 4] = Color.White;

				colors[0, 5] = Color.FromArgb (192, 255, 255);
				colors[1, 5] = Color.FromArgb (128, 255, 255);
				colors[2, 5] = Color.Cyan;
				colors[3, 5] = Color.FromArgb (0, 192, 192);
				colors[4, 5] = Color.Teal;
				colors[5, 5] = Color.FromArgb (0, 64, 64);
				colors[6, 5] = Color.White;
				colors[7, 5] = Color.White;

				colors[0, 6] = Color.FromArgb (192, 192, 255);
				colors[1, 6] = Color.FromArgb (128, 128, 255);
				colors[2, 6] = Color.Blue;
				colors[3, 6] = Color.FromArgb (0, 0, 192);
				colors[4, 6] = Color.Navy;
				colors[5, 6] = Color.FromArgb (0, 0, 64);
				colors[6, 6] = Color.White;
				colors[7, 6] = Color.White;

				colors[0, 7] = Color.FromArgb (255, 192, 255);
				colors[1, 7] = Color.FromArgb (255, 128, 255);
				colors[2, 7] = Color.Fuchsia;
				colors[3, 7] = Color.FromArgb (192, 0, 192);
				colors[4, 7] = Color.Purple;
				colors[5, 7] = Color.FromArgb (64, 0, 64);
				colors[6, 7] = Color.White;
				colors[7, 7] = Color.White;
			}

			public event EventHandler ColorChanged;

			protected override void OnPaint (PaintEventArgs e)
			{
				for (int i = 0; i < 8; i++)
					for (int j = 0; j < 8; j++) {
						DrawRect (e.Graphics, colors[i, j], j * 24, i * 24);
					}

				if (highlighting) {
					int i = x / 24;
					int j = y / 24;
					ControlPaint.DrawFocusRectangle (e.Graphics, new Rectangle (i * 24 - 2, j * 24 - 2, 24, 24));
				}

				base.OnPaint (e);
			}

			void DrawRect (Graphics g, Color color, int x, int y)
			{
				using (SolidBrush brush = new SolidBrush (color))
					g.FillRectangle (brush, x, y, 20, 20);
				ControlPaint.DrawBorder3D (g, x, y, 20, 20);
			}

			protected override void OnMouseDown (MouseEventArgs e)
			{
				if (e.X % 24 < 20 && e.Y % 24 < 20) {
					x = e.X;
					y = e.Y;
					highlighting = true;
					Invalidate ();
				}
				base.OnMouseDown (e);
			}

			protected override void OnMouseUp (MouseEventArgs e)
			{
				if (highlighting && this.ClientRectangle.Contains (e.X, e.Y)) {
					if (ColorChanged != null)
						ColorChanged (colors[y / 24, x / 24], EventArgs.Empty);
					highlighting = false;
				}
				base.OnMouseUp (e);
			}

			protected override void OnMouseMove (MouseEventArgs e)
			{
				if (highlighting) {
					int old_x = x;
					int old_y = y;
					x = e.X;
					y = e.Y;
					if ((old_x / 24 != x / 24 || old_y / 24 != y / 24) &&
						x / 24 < 8 && y / 24 < 8) {
						Region r = new Region ();
						r.Union (new Rectangle (old_x - 2, old_y - 2, 24, 24));
						r.Union (new Rectangle (x - 2, y - 2, 24, 24));
						Invalidate (r);
					}
				}
				base.OnMouseMove (e);
			}
		}
	}
}
