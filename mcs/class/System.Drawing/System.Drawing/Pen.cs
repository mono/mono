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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		private CustomLineCap startCap;
		private CustomLineCap endCap;
		private bool disposed;

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
			lock (this)
			{
				Status status = GDIPlus.GdipCreatePen2 (brush.nativeObject, width, Unit.UnitWorld, out nativeObject);
				GDIPlus.CheckStatus (status);
			
				this.brush = brush;
				if (brush is SolidBrush) {
					color = ((SolidBrush) brush).Color;
					status = GDIPlus.GdipSetPenColor (nativeObject, color.ToArgb ());
					GDIPlus.CheckStatus (status);
				}
			}
		}

		public Pen (Color color, float width)
		{
			lock (this)
			{
				Status status = GDIPlus.GdipCreatePen1 (color.ToArgb (), width, Unit.UnitWorld, out nativeObject);
				GDIPlus.CheckStatus (status);
	
				this.color = color;
				brush = new SolidBrush (color);
				status = GDIPlus.GdipSetPenBrushFill (nativeObject, brush.nativeObject);
				GDIPlus.CheckStatus (status);
			}
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
					throw new ArgumentException ("This Pen object can't be modified.");

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
					throw new ArgumentException ("This Pen object can't be modified.");
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
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}

		public float [] CompoundArray {
			get {
				int count;
				Status status = GDIPlus.GdipGetPenCompoundCount (nativeObject, out count);
				GDIPlus.CheckStatus (status);

				float [] compArray = new float [count];
				status = GDIPlus.GdipGetPenCompoundArray (nativeObject, compArray, count);
				GDIPlus.CheckStatus (status);

				return compArray;
			}

			set {
				if (isModifiable) {
                                        int length = value.Length;
                                        if (length < 2)
                                                throw new ArgumentException ("Invalid parameter.");
                                        foreach (float val in value)
                                                if (val < 0 || val > 1)
                                                        throw new ArgumentException ("Invalid parameter.");

					Status status = GDIPlus.GdipSetPenCompoundArray (nativeObject, value, value.Length);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}

		public CustomLineCap CustomEndCap {
			get {
				return endCap;
			}

			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenCustomEndCap (nativeObject, value.nativeObject);
					GDIPlus.CheckStatus (status);
					endCap = value;
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}

		public CustomLineCap CustomStartCap {
			get {
				return startCap;
			}

			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenCustomStartCap (nativeObject, value.nativeObject);
					GDIPlus.CheckStatus (status);
					startCap = value;
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}

                public DashCap DashCap {

                        get {
                                DashCap retval;
                                Status status = GDIPlus.GdipGetPenDashCap197819 (nativeObject, out retval);
				GDIPlus.CheckStatus (status);
                                return retval;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenDashCap197819 (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
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
					throw new ArgumentException ("This Pen object can't be modified.");
                        }
                }

                public float [] DashPattern {
                        get {
                                int count;
                                Status status = GDIPlus.GdipGetPenDashCount (nativeObject, out count);
				GDIPlus.CheckStatus (status);

				float [] pattern = new float [count];
                                status = GDIPlus.GdipGetPenDashArray (nativeObject, pattern, count);
				GDIPlus.CheckStatus (status);

                                return pattern;
                        }

                        set {
				if (isModifiable) {
					int length = value.Length;
					if (length == 0)
						throw new ArgumentException ("Invalid parameter.");
					foreach (float val in value)
						if (val <= 0)
							throw new ArgumentException ("Invalid parameter.");
                                	Status status = GDIPlus.GdipSetPenDashArray (nativeObject, value, value.Length);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
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
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}

		public LineCap StartCap {
			get {
				LineCap retval;
				Status status = GDIPlus.GdipGetPenStartCap (nativeObject, out retval);
				GDIPlus.CheckStatus (status);

				return retval;
			}

			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenStartCap (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}
 
		public LineCap EndCap {
			get {
				LineCap retval;
				Status status = GDIPlus.GdipGetPenEndCap (nativeObject, out retval);
				GDIPlus.CheckStatus (status);

				return retval;
			}

			set {
				if (isModifiable) {
					Status status = GDIPlus.GdipSetPenEndCap (nativeObject, value);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
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
					throw new ArgumentException ("This Pen object can't be modified.");
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
					throw new ArgumentException ("This Pen object can't be modified.");
                        }
                                
                }

                public PenType PenType {
                        get {
				PenType type;
				Status status = GDIPlus.GdipGetPenFillType (nativeObject, out type);
				GDIPlus.CheckStatus (status);

				return type;
			}
                }

                public Matrix Transform {

                        get {
				Matrix matrix = new Matrix ();
				Status status = GDIPlus.GdipGetPenTransform (nativeObject, matrix.nativeMatrix);
				GDIPlus.CheckStatus (status);

				return matrix;
                        }

                        set {
				if (isModifiable) {
                                	Status status = GDIPlus.GdipSetPenTransform (nativeObject, value.nativeMatrix);
					GDIPlus.CheckStatus (status);
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
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
					throw new ArgumentException ("This Pen object can't be modified.");
			}
		}

		public object Clone ()
		{
                        IntPtr ptr;
                        Status status = GDIPlus.GdipClonePen (nativeObject, out ptr);
			GDIPlus.CheckStatus (status);
                        Pen p = new Pen (ptr);
			p.brush = brush;
			p.color = color;
			p.startCap = startCap;
			p.endCap = endCap;

			return p;
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			lock (this){
				// Pen is disposed if and only if it is not disposed and
				// it is modifiable OR it is not disposed and it is being
				// collected by GC.
				if (! disposed) {
					if (isModifiable || disposing == false) {
						Status status = GDIPlus.GdipDeletePen (nativeObject);
						GDIPlus.CheckStatus (status);
						nativeObject = IntPtr.Zero;
						disposed = true;
					}
				}
				else
					throw new ArgumentException ("This Pen object can't be modified.");
			}
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
			if (isModifiable) {
				Status status = GDIPlus.GdipSetPenLineCap197819 (nativeObject, startCap, endCap, dashCap);
				GDIPlus.CheckStatus (status);
			}
			else
				throw new ArgumentException ("This Pen object can't be modified.");
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
