//
// System.Drawing.Design.FontNameEditor.cs
// 
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 
using System;
using System.Drawing;
using System.ComponentModel;
namespace System.Drawing.Design
{
	public class FontNameEditor : UITypeEditor
	{

		public  FontNameEditor()
		{
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		[MonoTODO]
		public override void PaintValue (PaintValueEventArgs e)
		{
			// TODO may not be correct
			Graphics G = e.Graphics;
			G.FillRectangle (SystemBrushes.ActiveCaption, e.Bounds);
			if (e.Value != null)
			{
				Font F = (Font) e.Value;
				G.DrawString("Ab", F, SystemBrushes.ActiveCaptionText, e.Bounds);
			}
			G.DrawRectangle (Pens.Black, e.Bounds);
		}
	}
}
