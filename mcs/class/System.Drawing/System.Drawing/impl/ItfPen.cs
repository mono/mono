//
// System.Drawing.Pen.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing {

	internal interface IPenFactory {
		IPen Pen(Brush brush, float width);
		IPen Pen(Color color, float width);
	}

	internal interface IPen  : IDisposable {
		
		//
		// Properties
		//
//		PenAlignment Alignment {
//			get {
//				return alignment;
//			}
//
//			set {
//				alignment = value;
//			}
//		}

		Brush Brush {
			get ;

			set ;
		}

		Color Color {
			get ;

			set ;
		}

		float Width {
			get ;
			set ;
		}

//		object Clone ()
//		{
//			Pen p = new Pen (brush, width);
//			
//			p.color = color;
//			p.alignment = alignment;
//
//			return p;
//		}

	}
}
