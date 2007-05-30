//
// System.Drawing.Design.CursorEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Drawing.Design
{
	public class CursorEditor : UITypeEditor
	{

		public CursorEditor()
		{
		}

		// TODO: Enhance (MS.Net has a version with graphical cursor preview)
		private class CursorUI : ListBox
		{
			private object value;
			private IWindowsFormsEditorService service;

			public CursorUI (UITypeEditor host, IWindowsFormsEditorService service, object value)
			{
				this.service = service;
				this.value = value;
				TypeConverter conv = TypeDescriptor.GetConverter (typeof (Cursor));
				foreach (object o in conv.GetStandardValues())
					base.Items.Add (o);
			}

			protected override void OnClick (EventArgs e)
			{
				base.OnClick (e);
				value = base.SelectedItem;
				service.CloseDropDown();
			}

			public object Value {
				get { return value; }
			}
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			if (provider == null)
				return value;

			IWindowsFormsEditorService service = 
				(IWindowsFormsEditorService)provider.GetService (typeof (IWindowsFormsEditorService));

			if (service == null)
				return value;

			CursorUI ui = new CursorUI (this, service, value);
			service.DropDownControl (ui);

			return ui.Value;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
#if NET_2_0
		public override bool IsDropDownResizable {
			get { return true; }
		}
#endif
	}
}
