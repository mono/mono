//
// System.ComponentModel.Design.MultilineStringEditor.cs
// 
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2007 Andreas Nahr
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

#if NET_2_0
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.ComponentModel.Design
{
	public sealed class MultilineStringEditor : UITypeEditor
	{
		private class EditorControl : TextBox
		{
			public EditorControl ()
			{
				Multiline = true;
				AcceptsReturn = true;
				Height = 250;
				ScrollBars = ScrollBars.Both;
				WordWrap = false;
				BorderStyle = BorderStyle.None;
			}
		}

		private IWindowsFormsEditorService editorService;
		private EditorControl control = new EditorControl ();
		private string editContent;

		public MultilineStringEditor ()
		{
			control.LostFocus += new EventHandler (control_LostFocus);
		}

		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && provider != null)
			{
				editorService = (IWindowsFormsEditorService)provider.GetService (typeof (IWindowsFormsEditorService));
				if (editorService != null)
				{
					if (!(value is string))
						return value;

					editContent = value as string;
					if (editContent == null)
						editContent = String.Empty;
					control.Text = editContent;

					editorService.DropDownControl (control);
					return editContent;
				}
			}
			return base.EditValue (context, provider, value);
		}

		void control_LostFocus (object sender, EventArgs e)
		{
			editContent = control.Text;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return false;
		}
	}
}
#endif