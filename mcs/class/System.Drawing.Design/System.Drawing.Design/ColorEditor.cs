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
using System;
using System.Drawing;
using System.ComponentModel;
namespace System.Drawing.Design
{
	public class ColorEditor : UITypeEditor
	{

		public ColorEditor()
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

		public override bool GetPaintValueSupported (
			ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;
			if (e.Value != null)
			{
				Color C = (Color) e.Value;
				G.FillRectangle(new SolidBrush (C), e.Bounds);
			}
			G.DrawRectangle (Pens.Black, e.Bounds);
		}
	}
}
