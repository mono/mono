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
	public sealed class SolidBrush : Brush {
		
		internal bool isModifiable = true;
		Color color;

                internal SolidBrush (IntPtr ptr)
                        : base (ptr)
                {
			int val;
			GDIPlus.GdipGetSolidFillColor (ptr, out val);
			color = Color.FromArgb (val);
                }

		public SolidBrush (Color color)
                {
			this.Color = color;
			int brush;
			GDIPlus.GdipCreateSolidFill (color.ToArgb (), out brush);
			nativeObject = (IntPtr) brush;
		}

		public Color Color {
			get {
				return color;
			}
			set {
				if (isModifiable)
					color = value;
				else
					throw new ArgumentException ("You may not change this Brush because it does not belong to you.");
			}
		}
		
		public override object Clone()
		{
			return new SolidBrush( color );
		}
		
		protected override void Dispose (bool disposing)
		{
			if (isModifiable)
				GDIPlus.GdipDeleteBrush (nativeObject);
			else
				throw new ArgumentException ("You may not change this Brush because it does not belong to you.");
		}
	}
}

