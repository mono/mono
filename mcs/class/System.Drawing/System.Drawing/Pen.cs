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
using System.Drawing.Drawing2D;

namespace System.Drawing {

	public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable {
		Brush brush;
		Color color;
		float width;
		internal IntPtr nativeObject;

		PenAlignment alignment;
		
		public Pen (Brush brush) : this (brush, 1.0F)
		{
		}

		public Pen (Color color) : this (color, 1.0F)
		{
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
			int pen;
			GDIPlus.GdipCreatePen1 (color.ToArgb (), width, Unit.UnitWorld, out pen);
			nativeObject = (IntPtr)pen;
		}

		//
		// Properties
		//
		public PenAlignment Alignment {
			get {
				return alignment;
			}

			set {
				alignment = value;
			}
		}

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

                public LineCap LineCap {

                        get {
                                LineCap result;
                                GDIPlus.GdipGetPenLineCap (nativeObject, out result);
                                return result;
                        }

                        set {
                                GDIPlus.GdipSetPenLineCap (nativeObject, value);
                        }
                                
                }

                public LineJoin LineJoin {

                        get {
                                LineJoin result;
                                GDIPlus.GdipGetPenLineJoin (nativeObject, out result);
                                return result;
                        }

                        set {
                                GDIPlus.GdipSetPenLineJoin (nativeObject, value);
                        }
                                
                }

                public float MiterLimit {

                        get {
                                float result;
                                GDIPlus.GdipGetPenMiterLimit (nativeObject, out result);
                                return result;
                        }

                        set {
                                GDIPlus.GdipSetPenMiterLimit (nativeObject, value);
                        }
                                
                }

                public Matrix Transform {

                        get {
                                Matrix result;
                                GDIPlus.GdipGetPenTransform (nativeObject, out result);
                                return result;
                        }

                        set {
                                GDIPlus.GdipSetPenTransform (nativeObject, value);
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

		public object Clone ()
		{
			Pen p = new Pen (brush, width);
			
			p.color = color;
			p.alignment = alignment;

			return p;
		}

		public void Dispose ()
		{
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
