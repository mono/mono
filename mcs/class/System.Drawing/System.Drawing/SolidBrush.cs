//
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine(pigolkine@gmx.de)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	public sealed class SolidBrush	: Brush {
		
		Color color;

		public SolidBrush( Color color ) {
			this.Color = color;
			int brush;
			GDIPlus.GdipCreateSolidFill (color.ToArgb (), out brush);
			nativeObject = (IntPtr)brush;
		}

		public Color Color {
			get {
				return color;
			}
			set {
			    color = value;
			}
		}
		
		public override object Clone()
		{
			return new SolidBrush( color );
		}
		
	}
}

