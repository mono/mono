//
// System.Drawing.Pen.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//   Duncan Mak (duncan@ximian.com)
//   Ravindra (rkumar@novell.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Novell, Inc.  http://www.novell.com
//

using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing {

	public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable {
		internal IntPtr nativeObject;
		internal bool isModifiable = true;
		internal Brush brush;
		internal Color color;
		internal Matrix matrix;

                internal Pen (IntPtr p)
                {
                        nativeObject = p;
                }

		public Pen (Brush brush) : this (brush, 1.0F)
		{
		}

		public Pen (Color color) : this (color, 1.0F)
		{
		}

		public Pen (Brush brush, float width)
		{
			int pen;
			GDIPlus.GdipCreatePen2 (brush.nativeObject, width, Unit.UnitWorld, out pen);
			nativeObject = (IntPtr) pen;
			this.brush = brush;
			if (brush is SolidBrush) {
				color = ((SolidBrush) brush).Color;
				GDIPlus.GdipSetPenColor (nativeObject, color.ToArgb ());
			}
		}

		public Pen (Color color, float width)
		{
			int pen;
			GDIPlus.GdipCreatePen1 (color.ToArgb (), width, Unit.UnitWorld, out pen);
			nativeObject = (IntPtr)pen;
			this.color = color;
			brush = new SolidBrush (color);
			GDIPlus.GdipSetPenBrushFill (nativeObject, brush.nativeObject);
		}

		//
		// Properties
		//
		public PenAlignment Alignment {
			get {
				PenAlignment retval;
                                GDIPlus.GdipGetPenMode (nativeObject, out retval);

                                return retval;
                        }

			set {
				if (isModifiable)
					GDIPlus.GdipSetPenMode (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");

			}
		}

		public Brush Brush {
			get {
                                return brush;
                        }

			set {
				if (isModifiable) {
					brush = value;
					if (value is SolidBrush) {
						GDIPlus.GdipSetPenBrushFill (nativeObject, value.nativeObject);
						color = ((SolidBrush) brush).Color;
						GDIPlus.GdipSetPenColor (nativeObject, color.ToArgb ());
					}
					else {
						// other brushes should clear the color property
						GDIPlus.GdipSetPenBrushFill (nativeObject, value.nativeObject);
						GDIPlus.GdipSetPenColor (nativeObject, 0);
						color = Color.Empty;
					}
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

		public Color Color {
			get {
                                return color;
			}

			set {
				if (isModifiable) {
					color = value;
					GDIPlus.GdipSetPenColor (nativeObject, value.ToArgb ());
					brush = new SolidBrush (color);
					GDIPlus.GdipSetPenBrushFill (nativeObject, brush.nativeObject);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

                public float [] CompoundArray {
                        get {
                                throw new NotImplementedException ();
//                                 int count;
//                                 GDIPlus.GdipGetPenCompoundArrayCount (nativeObject, out count);

//                                 IntPtr tmp = Marshal.AllocHGlobal (8 * count);
//                                 GDIPlus.GdipGetPenCompoundArray (nativeObject, out tmp, out count);

//                                 float [] retval = new float [count];
//                                 Marshal.Copy (tmp, retval, 0, count);

//                                 Marshal.FreeHGlobal (tmp);

//                                 return retval;
                        }

                        set {
                                throw new NotImplementedException ();                                
//                              if (isModifiable) {
//                                 int length = value.Length;
//                                 IntPtr tmp = Marshal.AllocHGlobal (8 * length);
//                                 Marshal.Copy (value, 0, tmp, length);
//                                 GDIPlus.GdipSetPenCompoundArray (nativeObject, tmp, length);

//                                 Marshal.FreeHGlobal (tmp);
//                              }
//                              else
//				   throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

                [MonoTODO]
                public CustomLineCap CustomEndCap {
                        get {
                                throw new NotImplementedException ();
                        }

			// do a check for isModifiable when implementing this property
                        set {
                                throw new NotImplementedException ();                                
                        }
                }

                [MonoTODO]
                public CustomLineCap CustomStartCap {

                        get {
                                throw new NotImplementedException ();                                
                        }

			// do a check for isModifiable when implementing this property
                        set {
                                throw new NotImplementedException ();                                
                        }
                }

                public DashCap DashCap {

                        get {
                                DashCap retval;
                                GDIPlus.GdipGetPenDashCap (nativeObject, out retval);

                                return retval;
                        }

                        set {
				if (isModifiable)
                                	GDIPlus.GdipSetPenDashCap (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

                public float DashOffset {

                        get {
                                float retval;
                                GDIPlus.GdipGetPenDashOffset (nativeObject, out retval);

                                return retval;
                        }

                        set {
				if (isModifiable)
                                	GDIPlus.GdipSetPenDashOffset (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

                public float [] DashPattern {
                        get {
                                int count;
                                GDIPlus.GdipGetPenDashCount (nativeObject, out count);

                                IntPtr tmp = Marshal.AllocHGlobal (8 * count);
                                GDIPlus.GdipGetPenDashArray (nativeObject, out tmp, out count);

                                float [] retval = new float [count];
                                Marshal.Copy (tmp, retval, 0, count);

                                Marshal.FreeHGlobal (tmp);

                                return retval;
                        }

                        set {
				if (isModifiable) {
                                	int length = value.Length;
                                	IntPtr tmp = Marshal.AllocHGlobal (8 * length);
                                	Marshal.Copy (value, 0, tmp, length);
                                	GDIPlus.GdipSetPenDashArray (nativeObject, tmp, length);

                                	Marshal.FreeHGlobal (tmp);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

		public DashStyle DashStyle {
			get {
				DashStyle retval;
                                GDIPlus.GdipGetPenDashStyle (nativeObject, out retval);

                                return retval;
			}

			set {
				if (isModifiable)
					GDIPlus.GdipSetPenDashStyle (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

		public LineCap StartCap {
			get {
                                throw new NotImplementedException ();
// 				LineCap retval;
//                                 GDIPlus.GdipGetPenStartCap (nativeObject, out retval);

//                                 return retval;
			}

			set {
                                throw new NotImplementedException ();                                
//			if (isModifiable)
// 				GDIPlus.GdipSetPenStartCap (nativeObject, value);
//			else
//				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
 
		public LineCap EndCap {
			get {
                                throw new NotImplementedException ();                                
// 				LineCap retval;
//                                 GDIPlus.GdipGetPenEndCap (nativeObject, out retval);

//                                 return retval;
			}

			set {
                                throw new NotImplementedException ();                                
//			if (isModifiable)
// 				GDIPlus.GdipSetPenEndCap (nativeObject, value);
//			else
//				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
 
                public LineJoin LineJoin {

                        get {
                                LineJoin result;
                                GDIPlus.GdipGetPenLineJoin (nativeObject, out result);
                                return result;
                        }

                        set {
				if (isModifiable)
                                	GDIPlus.GdipSetPenLineJoin (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                                
                }

                public float MiterLimit {

                        get {
                                float result;
                                GDIPlus.GdipGetPenMiterLimit (nativeObject, out result);
                                return result;
                        }

                        set {
				if (isModifiable)
                                	GDIPlus.GdipSetPenMiterLimit (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                                
                }

                public Matrix Transform {

                        get {
				if (matrix == null) {
					IntPtr m;
					GDIPlus.GdipGetPenTransform (nativeObject, out m);
					matrix = new Matrix (m);
				}
				return matrix;
                        }

                        set {
				if (isModifiable) {
                                	GDIPlus.GdipSetPenTransform (nativeObject, value.nativeMatrix);
					matrix = value;
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

		public float Width {
			get {
				float f;
                                GDIPlus.GdipGetPenWidth (nativeObject, out f);
                                return f;
			}
			set {
				if (isModifiable)
					GDIPlus.GdipSetPenWidth (nativeObject, value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

		public object Clone ()
		{
                        IntPtr ptr;
                        GDIPlus.GdipClonePen (nativeObject, out ptr);

                        return new Pen (ptr);
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			if (isModifiable)
                        	GDIPlus.GdipDeletePen (nativeObject);
			else
				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
		}

		~Pen ()
		{
			Dispose (false);
		}

                public void MultiplyTransform (Matrix matrix)
                {
                        MultiplyTransform (matrix, MatrixOrder.Prepend);
                }

                public void MultiplyTransform (Matrix matrix, MatrixOrder order)
                {
                        GDIPlus.GdipMultiplyPenTransform (nativeObject, matrix.nativeMatrix, order);
                }

                public void ResetTransform ()
                {
                        GDIPlus.GdipResetPenTransform (nativeObject);
                }

                public void RotateTransform (float angle)
                {
                        RotateTransform (angle, MatrixOrder.Prepend);
                }

                public void RotateTransform (float angle, MatrixOrder order)
                {
                        GDIPlus.GdipRotatePenTransform (nativeObject, angle, order);
                }

                public void ScaleTransform (float sx, float sy)
                {
                        ScaleTransform (sx, sy, MatrixOrder.Prepend);
                }

                public void ScaleTransform (float sx, float sy, MatrixOrder order)
                {
                        GDIPlus.GdipScalePenTransform (nativeObject, sx, sy, order);
                }

                public void SetLineCap (LineCap startCap, LineCap endCap, DashCap dashCap)
                {
			// do a check for isModifiable when implementing this method
                        // GDIPlus.GdipSetLineCap197819 (nativeObject, startCap, endCap, dashCap);
                }

                public void TranslateTransform (float dx, float dy)
                {
                        TranslateTransform (dx, dy, MatrixOrder.Prepend);
                }

                public void TranslateTransform (float dx, float dy, MatrixOrder order)
                {
                        GDIPlus.GdipTranslatePenTransform (nativeObject, dx, dy, order);
                }
	}
}
