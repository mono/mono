//
// System.Drawing.Pen.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing.Drawing2D;

namespace System.Drawing {

	internal interface IPenFactory {
		IPen Pen (Brush brush, float width);
		IPen Pen (Color color, float width);
	}

	internal interface IPen  : IDisposable, ICloneable {
		
		Brush Brush {
			get;
			set;
		}

		Color Color {
			get;
			set;
		}

		float Width {
			get;
			set;
		}

		PenAlignment Alignment {
			get;
			set;
		}
	}
}
