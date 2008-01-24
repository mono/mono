//
// System.Windows.Forms.Design.DockEditor.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004-2008 Novell
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

using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	public sealed class DockEditor : UITypeEditor
	{
		public DockEditor ()
		{
		}

		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && provider != null)
			{
				IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (editorService != null)
				{
					// Create the UI editor control
					DockEditorControl dockEditorControl = new DockEditorControl(editorService); 
					dockEditorControl.DockStyle = (DockStyle) value;
					editorService.DropDownControl(dockEditorControl);

					return dockEditorControl.DockStyle;
				}
			}
			return base.EditValue(context, provider, value);
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
		private class DockEditorControl : System.Windows.Forms.UserControl
		{
			private System.Windows.Forms.CheckBox buttonNone;
			private System.Windows.Forms.Panel panel1;
			private System.Windows.Forms.CheckBox buttonBottom;
			private System.Windows.Forms.CheckBox buttonTop;
			private System.Windows.Forms.Panel panel2;
			private System.Windows.Forms.CheckBox buttonLeft;
			private System.Windows.Forms.CheckBox buttonRight;
			private System.Windows.Forms.CheckBox buttonFill;
			private IWindowsFormsEditorService editorService;
			private DockStyle dockStyle;

			public DockEditorControl(IWindowsFormsEditorService editorService)
			{
				buttonNone = new System.Windows.Forms.CheckBox();
				panel1 = new System.Windows.Forms.Panel();
				buttonBottom = new System.Windows.Forms.CheckBox();
				buttonTop = new System.Windows.Forms.CheckBox();
				panel2 = new System.Windows.Forms.Panel();
				buttonLeft = new System.Windows.Forms.CheckBox();
				buttonRight = new System.Windows.Forms.CheckBox();
				buttonFill = new System.Windows.Forms.CheckBox();
				panel1.SuspendLayout();
				panel2.SuspendLayout();
				SuspendLayout();

				buttonNone.Appearance = Appearance.Button;
				buttonNone.Dock = System.Windows.Forms.DockStyle.Bottom;
				buttonNone.Location = new System.Drawing.Point(0, 92);
				buttonNone.Size = new System.Drawing.Size(150, 23);
				buttonNone.TabIndex = 5;
				buttonNone.Text = "None";
				buttonNone.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				buttonNone.Click += new System.EventHandler(buttonClick);

				panel1.Controls.Add(panel2);
				panel1.Controls.Add(buttonTop);
				panel1.Controls.Add(buttonBottom);
				panel1.Dock = System.Windows.Forms.DockStyle.Fill;
				panel1.Location = new System.Drawing.Point(0, 0);
				panel1.Name = "panel1";
				panel1.Size = new System.Drawing.Size(150, 92);
				panel1.TabStop = false;

				buttonBottom.Appearance = Appearance.Button;
				buttonBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
				buttonBottom.Location = new System.Drawing.Point(0, 69);
				buttonBottom.Name = "buttonBottom";
				buttonBottom.Size = new System.Drawing.Size(150, 23);
				buttonBottom.TabIndex = 5;
				buttonBottom.Click += new System.EventHandler(buttonClick);

				buttonTop.Appearance = Appearance.Button;
				buttonTop.Dock = System.Windows.Forms.DockStyle.Top;
				buttonTop.Location = new System.Drawing.Point(0, 0);
				buttonTop.Name = "buttonTop";
				buttonTop.Size = new System.Drawing.Size(150, 23);
				buttonTop.TabIndex = 1;
				buttonTop.Click += new System.EventHandler(buttonClick);

				panel2.Controls.Add(buttonFill);
				panel2.Controls.Add(buttonRight);
				panel2.Controls.Add(buttonLeft);
				panel2.Dock = System.Windows.Forms.DockStyle.Fill;
				panel2.Location = new System.Drawing.Point(0, 23);
				panel2.Size = new System.Drawing.Size(150, 46);
				panel2.TabIndex = 2;
				panel2.TabStop = false;

				buttonLeft.Appearance = Appearance.Button;
				buttonLeft.Dock = System.Windows.Forms.DockStyle.Left;
				buttonLeft.Location = new System.Drawing.Point(0, 0);
				buttonLeft.Size = new System.Drawing.Size(24, 46);
				buttonLeft.TabIndex = 2;
				buttonLeft.Click += new System.EventHandler(buttonClick);

				buttonRight.Appearance = Appearance.Button;
				buttonRight.Dock = System.Windows.Forms.DockStyle.Right;
				buttonRight.Location = new System.Drawing.Point(126, 0);
				buttonRight.Size = new System.Drawing.Size(24, 46);
				buttonRight.TabIndex = 4;
				buttonRight.Click += new System.EventHandler(buttonClick);

				buttonFill.Appearance = Appearance.Button;
				buttonFill.Dock = System.Windows.Forms.DockStyle.Fill;
				buttonFill.Location = new System.Drawing.Point(24, 0);
				buttonFill.Size = new System.Drawing.Size(102, 46);
				buttonFill.TabIndex = 3;
				buttonFill.Click += new System.EventHandler(buttonClick);

				Controls.Add(panel1);
				Controls.Add(buttonNone);
				Size = new System.Drawing.Size(150, 115);
				panel1.ResumeLayout(false);
				panel2.ResumeLayout(false);
				ResumeLayout(false);


				this.editorService = editorService;
				dockStyle = DockStyle.None;

			}

			private void buttonClick(object sender, System.EventArgs e)
			{
				if (sender == buttonNone)
					dockStyle = DockStyle.None;
				else if (sender == buttonFill)
					dockStyle = DockStyle.Fill;
				else if (sender == buttonLeft)
					dockStyle = DockStyle.Left;
				else if (sender == buttonRight)
					dockStyle = DockStyle.Right;
				else if (sender == buttonTop)
					dockStyle = DockStyle.Top;
				else if (sender == buttonBottom)
					dockStyle = DockStyle.Bottom;
				editorService.CloseDropDown();
			}


			public DockStyle DockStyle
			{
				get
				{
					return dockStyle;
				}
				set
				{
					dockStyle = value;
					buttonNone.Checked = false;
					buttonBottom.Checked = false;
					buttonTop.Checked = false;
					buttonLeft.Checked = false;
					buttonRight.Checked = false;
					buttonFill.Checked = false;
					switch (DockStyle)
					{
						case DockStyle.Fill:
							buttonFill.CheckState = CheckState.Checked;
							break;
						case DockStyle.None:
							buttonNone.CheckState = CheckState.Checked;
							break;
						case DockStyle.Left:
							buttonLeft.CheckState = CheckState.Checked;
							break;
						case DockStyle.Right:
							buttonRight.CheckState = CheckState.Checked;
							break;
						case DockStyle.Top:
							buttonTop.CheckState = CheckState.Checked;
							break;
						case DockStyle.Bottom:
							buttonBottom.CheckState = CheckState.Checked;
							break;
					}
				}
			}
		}
	}
}
