//
// System.Drawing.Design.UITypeEditor.cs
// 
// Authors:
//  Alan Tam Siu Lung <Tam@SiuLung.com>
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Alan Tam Siu Lung <Tam@SiuLung.com>
// (C) 2003 Andreas Nahr
// 
using System;
using System.ComponentModel;
namespace System.Drawing.Design
{
	public class UITypeEditor
	{

		public UITypeEditor()
		{
		}

		public virtual object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			// We already stated that we can't edit ;)
			return value;
		}
		public object EditValue(IServiceProvider provider, object value)
		{
			return EditValue (null, provider, value);
		}
		public virtual UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.None;
		}
		public UITypeEditorEditStyle GetEditStyle ()
		{
			return GetEditStyle (null);
		}
		public bool GetPaintValueSupported ()
		{
			return GetPaintValueSupported (null);
		}
		public virtual bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return false;
		}
		public void PaintValue (object value, Graphics canvas, Rectangle rectangle)
		{
			PaintValue (new PaintValueEventArgs (null, value, canvas, rectangle));
		}
		public virtual void PaintValue (PaintValueEventArgs e)
		{
			// LAMESPEC: Did not find info in the docs if this should do something here.
			// Usually you would expect, that this class gets inherited and this overridden, 
			// but on the other hand the class is not abstract. Could never observe it did paint anything
			return;
		}
	}
}
