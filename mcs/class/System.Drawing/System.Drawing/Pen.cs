//
// System.Drawing.Pen.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing {

	public sealed class Pen : MarshalByRefObject { //, ICloneable, IDisposable {
		Brush brush;
		Color color;
		float width;
        internal IPen implementation_;
		internal static IPenFactory factory_ = Factories.GetPenFactory();
		//PenAlignment alignment;
		
		public Pen (Brush brush)
		{
			implementation_ = factory_.Pen(brush, 1);
			this.brush = brush;
			width = 1;
		}

		public Pen (Color color)
		{
			implementation_ = factory_.Pen(color, 1);
			this.color = color;
			width = 1;
		}

		public Pen (Brush brush, float width)
		{
			implementation_ = factory_.Pen(brush, width);
			this.width = width;
			this.brush = brush;
		}

		public Pen (Color color, float width)
		{
			implementation_ = factory_.Pen(color, width);
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
			implementation_.Dispose();
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
		}

		~Pen ()
		{
			Dispose (false);
		}
	}
}
