//
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine(pigolkine@gmx.de)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
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
			Status status = GDIPlus.GdipGetSolidFillColor (ptr, out val);
			GDIPlus.CheckStatus (status);
			color = Color.FromArgb (val);
                }

		public SolidBrush (Color color)
                {
			this.color = color;
			int brush;
			Status status = GDIPlus.GdipCreateSolidFill (color.ToArgb (), out brush);
			GDIPlus.CheckStatus (status);
			nativeObject = (IntPtr) brush;
		}

		public Color Color {
			get {
				return color;
			}
			set {
				if (isModifiable) {
					color = value;
					Status status = GDIPlus.GdipSetSolidFillColor (nativeObject, value.ToArgb ());
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Brush because it does not belong to you.");
			}
		}
		
		public override object Clone()
		{
			return new SolidBrush (color);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (isModifiable) {
				Status status = GDIPlus.GdipDeleteBrush (nativeObject);
				GDIPlus.CheckStatus (status);
			}
			else
				throw new ArgumentException ("You may not change this Brush because it does not belong to you.");
		}
	}
}

