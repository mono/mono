//
// System.Drawing.Design.ColorEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

		public ColorEditor()
		{
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			if (context != null && provider != null) {
				editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (editorService != null) {
					// Create the UI editor control
					
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


					tab_control.TabPages.Add(custom_tab);
					tab_control.TabPages.Add(web_tab);
					tab_control.TabPages.Add(system_tab);

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


					editorService.DropDownControl(tab_control);
					return selected_color;
				}
			}
			return base.EditValue(context, provider, value);
		}

		private void HandleChange(object sender, EventArgs e) 
		{
			selected_color = (Color)((ColorListBox)sender).Items[((ColorListBox)sender).SelectedIndex];
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
	}
}
