//
// System.Drawing.Pen.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing {

	public sealed class Pen : MarshalByRefObject { //, ICloneable, IDisposable {
		Brush brush;
		Color color;
		float width;
		//PenAlignment alignment;
		
		public Pen (Brush brush)
		{
			this.brush = brush;
			width = 1;
		}

		public Pen (Color color)
		{
			this.color = color;
			width = 1;
		}

		public Pen (Brush brush, float width)
		{
			this.width = width;
			this.brush = brush;
		}

		public Pen (Color color, float width)
		{
			this.width = width;
			this.color = color;
		}

		//
		// Properties
		//
//		public PenAlignment Alignment {
//			get {
//				return alignment;
//			}
//
//			set {
//				alignment = value;
//			}
//		}

		public Brush Brush {
			get {
				return brush;
			}

			set {
				brush = value;
			}
		}

		public Color Color {
			get {
				return color;
			}

			set {
				color = value;
			}
		}

		public float Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}

//		public object Clone ()
//		{
//			Pen p = new Pen (brush, width);
//			
//			p.color = color;
//			p.alignment = alignment;
//
//			return p;
//		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			// Nothing for now.
		}

		~Pen ()
		{
			Dispose (false);
		}
	}
}
