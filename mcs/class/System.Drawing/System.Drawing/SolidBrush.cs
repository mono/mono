//
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	public class SolidBrush	: Brush {
		
		Color color;
		
		public SolidBrush( Color color ) {
			this.color = color;
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

