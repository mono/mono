//
// System.Drawing.Drawing2D.ColorBlend.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc. http://www.ximian.com
// (C) 2004 Novell, Inc. http://www.novell.com
//
using System;

namespace System.Drawing.Drawing2D
{
	public sealed class ColorBlend
	{
		private float [] positions;
		private Color [] colors;

		public ColorBlend ()
		{
			positions = new float [1];
			colors = new Color [1];
		}

		public ColorBlend (int count)
		{
			positions = new float [count];
			colors = new Color [count];
		}

		public Color [] Colors {
			get {
				return colors;
			}

			set {
				colors = value;
			}
		}

		public float [] Positions {
			get {
				return positions;
			}

			set {
				positions = value;				
			}
		}
	}
}
