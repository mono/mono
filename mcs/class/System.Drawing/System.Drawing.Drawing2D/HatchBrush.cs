//
// System.Drawing.Drawing2D.HatchBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for HatchBrush.
	/// </summary>
	public sealed class HatchBrush : Brush {
		public HatchBrush(HatchStyle hatchstyle, Color foreColor, Color backColor) {
		}

		public HatchBrush(HatchStyle hatchstyle, Color foreColor) {
		}

		public Color BackgroundColor {
			get {
				throw new NotImplementedException ();
			}
		}

		public Color ForegroundColor {
			get {
				throw new NotImplementedException ();
			}
		}

		public HatchStyle HatchStyle {
			get {
				throw new NotImplementedException ();
			}
		}

		public override object Clone(){
			throw new NotImplementedException ();
		}


	}
}
