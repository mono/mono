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
	public class SolidBrush	: Brush {
		
		Color color;
		internal ISolidBrushFactory factory_ = Factories.GetSolidBrushFactory();

		public SolidBrush( Color color ) {
			implementation_ = factory_.SolidBrush(color);
			this.Color = color;
		}

		public Color Color {
			get {
				return color;
			}
			set {
			    color = value;
				((ISolidBrush)implementation_).Color = value;
			}
		}
		
		public override object Clone()
		{
			return new SolidBrush( color );
		}
		
	}
}

