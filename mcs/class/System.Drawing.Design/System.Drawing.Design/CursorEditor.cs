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
namespace System.Drawing.Design
{
	public class CursorEditor : UITypeEditor
	{

		public CursorEditor()
		{
		}

		[MonoTODO]
		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			// TODO IMPLEMENT
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle (
			ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
	}
}
