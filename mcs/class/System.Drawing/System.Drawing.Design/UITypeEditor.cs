// System.Drawing.Design.UITypeEditor.cs
// 
// Author:
//     Alan Tam Siu Lung <Tam@SiuLung.com>
// 
// (C) 2003 Alan Tam Siu Lung <Tam@SiuLung.com>
// 

using System;
using System.ComponentModel;

namespace System.Drawing.Design
{
	/// <summary>
	/// Summary description for UITypeEditor.
	/// </summary>
	public class UITypeEditor
	{
		[MonoTODO]
		public UITypeEditor()
		{
		}

		[MonoTODO]
		public virtual object EditValue(ITypeDescriptorContext context,
						IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		public object EditValue(IServiceProvider provider, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UITypeEditorEditStyle GetEditStyle()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PaintValue(object value, Graphics canvas, Rectangle rectangle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void PaintValue(PaintValueEventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}
