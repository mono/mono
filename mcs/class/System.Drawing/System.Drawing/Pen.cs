//
// System.Drawing.Pen.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing {

	public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable {
		internal IntPtr nativeObject;
		internal bool isModifiable = true;

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
		}

		public Pen (Color color, float width)
		{
			int pen;
			GDIPlus.GdipCreatePen1 (color.ToArgb (), width, Unit.UnitWorld, out pen);
			nativeObject = (IntPtr)pen;
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");

			}
		}

		public Brush Brush {
			get {
                                IntPtr retval;
                                GDIPlus.GdipGetPenBrushFill (nativeObject, out retval);
                                BrushType type;
                                GDIPlus.GdipGetBrushType (retval, out type);
                                
                                return Brush.CreateBrush (retval, type);
                        }

			set {
				if (isModifiable)
                                	GDIPlus.GdipSetPenBrushFill (nativeObject, value.nativeObject);
				else
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
			}
		}

		public Color Color {
			get {
				int argb;
                                GDIPlus.GdipGetPenColor (nativeObject, out argb);

                                return Color.FromArgb (argb);
			}

			set {
				if (isModifiable)
					GDIPlus.GdipSetPenColor (nativeObject, value.ToArgb ());
				else
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
//				   throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
//				throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
//				throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
                        }
                                
                }

                public Matrix Transform {

                        get {
                                Matrix result;
                                GDIPlus.GdipGetPenTransform (nativeObject, out result);
                                return result;
                        }

                        set {
				if (isModifiable)
                                	GDIPlus.GdipSetPenTransform (nativeObject, value);
				else
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
					throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
				throw new ArgumentException("You may not change this Pen because it does not belong to you.");
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
