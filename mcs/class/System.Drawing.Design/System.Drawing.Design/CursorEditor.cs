//
// System.Drawing.Design.CursorEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 

using System;
using System.Drawing;
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
	}
}
