//
// System.Drawing.Drawing2D.ColorBlend.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D
{
	public sealed class ColorBlend {
		private int count;
		private float [] positions;
		private Color [] colors;

		public ColorBlend(int count) {
			//FIXME: 
			if(count < 2){
				throw new ArgumentOutOfRangeException("Count", count, "Must be at least 2");
			}
			if(count == 2){
				//FIXME: call ColorBlend!
				count = 2;
				positions = new float [1];
				colors = new Color [1];
				positions[0] = 0.0F;
				positions[1] = 1.0F;
				colors[0] = Color.FromArgb(0,0,0);
				colors[1] = Color.FromArgb(255,255,255);
			}
			this.count = count;
			int i;
			for(i = 0; i < count; i++){
				positions[i] = (1.0F/count) * i;
				//FIXME: Do real default color blend
				//FIXME: I used 254 to prevent overflow, should use 255, if anyone cares?
				colors[i] = Color.FromArgb((1/count) * i * 254,(1/count) * i * 254,(1/count) * i * 254);
			}
			//fix any rounding errors that would generate an invald list.
			positions[0] = 0.0F;
			positions[1] = 1.0F;
			colors[0] = Color.FromArgb(0,0,0);
			colors[1] = Color.FromArgb(255,255,255);

		}

		public ColorBlend() {
			count = 2;
			positions = new float [1];
			colors = new Color [1];
			positions[0] = 0.0F;
			positions[1] = 1.0F;
			colors[0] = Color.FromArgb(0,0,0);
			colors[1] = Color.FromArgb(255,255,255);
		}

		public Color [] Colors{
			get {
				return colors;
			}
			set{
				count = value.GetUpperBound(0) + 1;
				colors = value;
			}
		}

		public float [] Positions{
			get {
				return Positions;
			}
			set{
				count = value.GetUpperBound(0) + 1;
				positions = value;				
			}
		}
	}

}
