//
// System.Drawing.Design.ContentAlignmentEditor.cs
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
	public class ContentAlignmentEditor : UITypeEditor
	{

		public ContentAlignmentEditor()
		{
		}

		// TODO: Enhance (MS.Net has a nice graphical version)
		private class AlignmentUI : ListBox
		{
			private object value;
			private IWindowsFormsEditorService service;

			public AlignmentUI (UITypeEditor host, IWindowsFormsEditorService service, object value)
			{
				this.service = service;
				this.value = value;
				base.Items.Add (ContentAlignment.TopLeft);
				base.Items.Add (ContentAlignment.TopCenter);
				base.Items.Add (ContentAlignment.TopRight);
				base.Items.Add (ContentAlignment.MiddleLeft);
				base.Items.Add (ContentAlignment.MiddleCenter);
				base.Items.Add (ContentAlignment.MiddleRight);
				base.Items.Add (ContentAlignment.BottomLeft);
				base.Items.Add (ContentAlignment.BottomCenter);
				base.Items.Add (ContentAlignment.BottomRight);
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

			AlignmentUI ui = new AlignmentUI (this, service, value);
			service.DropDownControl (ui);

			return ui.Value;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
	}
}
