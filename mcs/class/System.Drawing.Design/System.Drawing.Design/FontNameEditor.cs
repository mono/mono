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
		private const String PreviewString = "Ab";

		public  FontNameEditor()
		{
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;

			// Draw the background 
			G.FillRectangle (SystemBrushes.ActiveCaption, e.Bounds);

			// Draw the sample string
			if (e.Value != null)
			{
				Font F = (Font) e.Value;
				G.DrawString (PreviewString, F, SystemBrushes.ActiveCaptionText, e.Bounds);
			}

			// Draw the border again to ensure it is not overlapped by the text
			G.DrawRectangle (Pens.Black, e.Bounds);
		}
	}
}
