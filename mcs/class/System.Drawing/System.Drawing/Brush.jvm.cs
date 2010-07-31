using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;

using awt = java.awt;
using image = java.awt.image;
using geom = java.awt.geom;


namespace System.Drawing
{
	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable, awt.Paint {
		
		#region fields

		private Matrix _brushTransform = new Matrix();

		#endregion

		protected abstract java.awt.Paint NativeObject {
			get;
		}

		awt.PaintContext awt.Paint.createContext (image.ColorModel cm,
			awt.Rectangle deviceBounds, geom.Rectangle2D userBounds, geom.AffineTransform xform,
			awt.RenderingHints hints) {

			return createContextInternal(cm, deviceBounds, userBounds, xform, hints);
		}

		protected virtual awt.PaintContext createContextInternal (image.ColorModel cm,
			awt.Rectangle deviceBounds, geom.Rectangle2D userBounds, geom.AffineTransform xform,
			awt.RenderingHints hints) {

			Matrix.Multiply(xform, _brushTransform.NativeObject, MatrixOrder.Append);
			return NativeObject.createContext (cm, deviceBounds, userBounds, xform, hints);
		}

		int awt.Transparency.getTransparency () {
			return NativeObject.getTransparency ();
		}

		abstract public object Clone ();

		public void Dispose () {
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing) {
		}

		protected Brush InternalClone() {
			Brush brush = (Brush)this.MemberwiseClone();
			brush._brushTransform = this._brushTransform.Clone();
			return brush;
		}

		#region Brush transform

		internal Matrix BrushTransform {
			get { return _brushTransform.Clone(); }
			set { 
				if (value == null)
					throw new ArgumentNullException("matrix");

				value.CopyTo( _brushTransform ); 
			}
		}

		protected internal void BrushTranslateTransform (float dx, float dy) {
			BrushTranslateTransform(dx, dy, MatrixOrder.Prepend);
		}
		protected internal void BrushTranslateTransform (float dx, float dy, MatrixOrder order) {
			_brushTransform.Translate(dx,dy,order);
		}
		protected internal void BrushResetTransform () {
			_brushTransform.Reset();
		}
		protected internal void BrushRotateTransform (float angle) {
			BrushRotateTransform(angle, MatrixOrder.Prepend);
		}
		protected internal void BrushRotateTransform (float angle, MatrixOrder order) {
			_brushTransform.Rotate(angle, order);
		}
		protected internal void BrushScaleTransform (float sx, float sy) {
			BrushScaleTransform(sx, sy, MatrixOrder.Prepend);
		}
		protected internal void BrushScaleTransform (float sx, float sy, MatrixOrder order) {
			_brushTransform.Scale(sx, sy, order);
		}
		protected internal void BrushMultiplyTransform (Matrix matrix) {
			BrushMultiplyTransform(matrix, MatrixOrder.Prepend);
		}
		protected internal void BrushMultiplyTransform (Matrix matrix, MatrixOrder order) {
			if (matrix == null)
				throw new ArgumentNullException("matrix");
			_brushTransform.Multiply(matrix, order);			
		}

		#endregion
	}
}

