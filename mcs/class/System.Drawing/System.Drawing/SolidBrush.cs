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
		private Color color;

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
			lock (this)
			{
				this.color = color;
				Status status = GDIPlus.GdipCreateSolidFill (color.ToArgb (), out nativeObject);
				GDIPlus.CheckStatus (status);
			}
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
					throw new ArgumentException ("This SolidBrush object can't be modified.");
			}
		}
		
		public override object Clone()
		{
			lock (this)
			{
				IntPtr clonePtr;
				Status status = GDIPlus.GdipCloneBrush (nativeObject, out clonePtr);
				GDIPlus.CheckStatus (status);
	
				SolidBrush clone = new SolidBrush (clonePtr);
				return clone;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			// SolidBrush is disposed if and only if it is not disposed
			// and it is modifiable OR it is not disposed and it is being
			// collected by GC.
			if (! disposed) {
				if (isModifiable || disposing == false) {
					lock (this) 
					{
						Status status = GDIPlus.GdipDeleteBrush (nativeObject);
						GDIPlus.CheckStatus (status);
						disposed = true;
					}
				}
				else
					throw new ArgumentException ("This SolidBrush object can't be modified.");
			}
		}
	}
}

