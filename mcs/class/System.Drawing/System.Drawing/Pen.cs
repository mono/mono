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
			Status status = GDIPlus.GdipCreatePen2 (brush.nativeObject, width, Unit.UnitWorld, out pen);
			GDIPlus.CheckStatus (status);

			nativeObject = (IntPtr) pen;
			this.brush = brush;
			if (brush is SolidBrush) {
				color = ((SolidBrush) brush).Color;
				status = GDIPlus.GdipSetPenColor (nativeObject, color.ToArgb ());
				GDIPlus.CheckStatus (status);
			}
		}

		public Pen (Color color, float width)
		{
			int pen;
			Status status = GDIPlus.GdipCreatePen1 (color.ToArgb (), width, Unit.UnitWorld, out pen);
			GDIPlus.CheckStatus (status);
			nativeObject = (IntPtr)pen;
			this.color = color;
			brush = new SolidBrush (color);
			status = GDIPlus.GdipSetPenBrushFill (nativeObject, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}

		//
		// Properties
		//
		public PenAlignment Alignment {
			get {
				PenAlignment retval;
                                Status status = GDIPlus.GdipGetPenMode (nativeObject, out retval);
				GDIPlus.CheckStatus (status);
                                return retval;
                        }

			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenMode (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
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
						Status status = GDIPlus.GdipSetPenBrushFill (nativeObject, value.nativeObject);
						GDIPlus.CheckStatus (status);
						color = ((SolidBrush) brush).Color;
						status = GDIPlus.GdipSetPenColor (nativeObject, color.ToArgb ());
						GDIPlus.CheckStatus (status);
					}
					else {
						// other brushes should clear the color property
						Status status = GDIPlus.GdipSetPenBrushFill (nativeObject, value.nativeObject);
						GDIPlus.CheckStatus (status);
						status = GDIPlus.GdipSetPenColor (nativeObject, 0);
						GDIPlus.CheckStatus (status);
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
					Status status = GDIPlus.GdipSetPenColor (nativeObject, value.ToArgb ());
					GDIPlus.CheckStatus (status);
					brush = new SolidBrush (color);
					status = GDIPlus.GdipSetPenBrushFill (nativeObject, brush.nativeObject);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

                public float [] CompoundArray {
                        get {
                                throw new NotImplementedException ();
//                                 int count;
//                                 Status status = GDIPlus.GdipGetPenCompoundArrayCount (nativeObject, out count);

//                                 IntPtr tmp = Marshal.AllocHGlobal (8 * count);
//                                 status = GDIPlus.GdipGetPenCompoundArray (nativeObject, out tmp, out count);

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
//                                 Status status = GDIPlus.GdipSetPenCompoundArray (nativeObject, tmp, length);

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
                                Status status = GDIPlus.GdipGetPenDashCap (nativeObject, out retval);
				GDIPlus.CheckStatus (status);
                                return retval;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenDashCap (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

                public float DashOffset {

                        get {
                                float retval;
                                Status status = GDIPlus.GdipGetPenDashOffset (nativeObject, out retval);
				GDIPlus.CheckStatus (status);
                                return retval;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenDashOffset (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

                public float [] DashPattern {
                        get {
                                int count;
                                Status status = GDIPlus.GdipGetPenDashCount (nativeObject, out count);
				GDIPlus.CheckStatus (status);
                                IntPtr tmp = Marshal.AllocHGlobal (8 * count);
                                status = GDIPlus.GdipGetPenDashArray (nativeObject, out tmp, out count);
                                float [] retval = new float [count];
                                Marshal.Copy (tmp, retval, 0, count);

                                Marshal.FreeHGlobal (tmp);
				GDIPlus.CheckStatus (status);
                                return retval;
                        }

                        set {
				if (isModifiable) {
                                	int length = value.Length;
                                	IntPtr tmp = Marshal.AllocHGlobal (8 * length);
                                	Marshal.Copy (value, 0, tmp, length);
                                	Status status = GDIPlus.GdipSetPenDashArray (nativeObject, tmp, length);
                                	Marshal.FreeHGlobal (tmp);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

		public DashStyle DashStyle {
			get {
				DashStyle retval;
                                Status status = GDIPlus.GdipGetPenDashStyle (nativeObject, out retval);
				GDIPlus.CheckStatus (status);
                                return retval;
			}

			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenDashStyle (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

		public LineCap StartCap {
			get {
                                throw new NotImplementedException ();
// 				LineCap retval;
//                                 Status status = GDIPlus.GdipGetPenStartCap (nativeObject, out retval);

//                                 return retval;
			}

			set {
                                throw new NotImplementedException ();                                
//			if (isModifiable)
// 				Status status = GDIPlus.GdipSetPenStartCap (nativeObject, value);
//			else
//				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
 
		public LineCap EndCap {
			get {
                                throw new NotImplementedException ();                                
// 				LineCap retval;
//                                 Status status = GDIPlus.GdipGetPenEndCap (nativeObject, out retval);

//                                 return retval;
			}

			set {
                                throw new NotImplementedException ();                                
//			if (isModifiable)
// 				Status status = GDIPlus.GdipSetPenEndCap (nativeObject, value);
//			else
//				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
 
                public LineJoin LineJoin {

                        get {
                                LineJoin result;
                                Status status = GDIPlus.GdipGetPenLineJoin (nativeObject, out result);
				GDIPlus.CheckStatus (status);
                                return result;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenLineJoin (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                                
                }

                public float MiterLimit {

                        get {
                                float result;
                                Status status = GDIPlus.GdipGetPenMiterLimit (nativeObject, out result);
				GDIPlus.CheckStatus (status);
                                return result;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenMiterLimit (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                                
                }

                public PenType PenType {

                        get {
                                if (brush is TextureBrush)
                                        return PenType.TextureFill;
                                else if (brush is HatchBrush)
                                        return PenType.HatchFill;
                                else if (brush is LinearGradientBrush)
                                        return PenType.LinearGradient;
                                else if (brush is PathGradientBrush)
                                        return PenType.PathGradient;
                                else
                                        return PenType.SolidColor;
                        }
                }

                public Matrix Transform {

                        get {
				if (matrix == null) {
					IntPtr m;
					Status status = GDIPlus.GdipGetPenTransform (nativeObject, out m);
					GDIPlus.CheckStatus (status);
					matrix = new Matrix (m);
				}
				return matrix;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenTransform (nativeObject, value.nativeMatrix);
					GDIPlus.CheckStatus (status);
					matrix = value;
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
                        }
                }

		public float Width {
			get {
				float f;
                                Status status = GDIPlus.GdipGetPenWidth (nativeObject, out f);
				GDIPlus.CheckStatus (status);
                                return f;
			}
			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenWidth (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}

		public object Clone ()
		{
                        IntPtr ptr;
                        Status status = GDIPlus.GdipClonePen (nativeObject, out ptr);
			GDIPlus.CheckStatus (status);
                        return new Pen (ptr);
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			// Let the GC collect it
			if (isModifiable || disposing == false) {
                        	Status status = GDIPlus.GdipDeletePen (nativeObject);
				GDIPlus.CheckStatus (status);
			}
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
                        Status status = GDIPlus.GdipMultiplyPenTransform (nativeObject, matrix.nativeMatrix, order);
			GDIPlus.CheckStatus (status);
                }

                public void ResetTransform ()
                {
                        Status status = GDIPlus.GdipResetPenTransform (nativeObject);
			GDIPlus.CheckStatus (status);
                }

                public void RotateTransform (float angle)
                {
                        RotateTransform (angle, MatrixOrder.Prepend);
                }

                public void RotateTransform (float angle, MatrixOrder order)
                {
                        Status status = GDIPlus.GdipRotatePenTransform (nativeObject, angle, order);
			GDIPlus.CheckStatus (status);
                }

                public void ScaleTransform (float sx, float sy)
                {
                        ScaleTransform (sx, sy, MatrixOrder.Prepend);
                }

                public void ScaleTransform (float sx, float sy, MatrixOrder order)
                {
                        Status status = GDIPlus.GdipScalePenTransform (nativeObject, sx, sy, order);
			GDIPlus.CheckStatus (status);
                }

                public void SetLineCap (LineCap startCap, LineCap endCap, DashCap dashCap)
                {
			// do a check for isModifiable when implementing this method
                        // Status status = GDIPlus.GdipSetLineCap197819 (nativeObject, startCap, endCap, dashCap);
                }

                public void TranslateTransform (float dx, float dy)
                {
                        TranslateTransform (dx, dy, MatrixOrder.Prepend);
                }

                public void TranslateTransform (float dx, float dy, MatrixOrder order)
                {
                        Status status = GDIPlus.GdipTranslatePenTransform (nativeObject, dx, dy, order);
			GDIPlus.CheckStatus (status);
                }
	}
}
