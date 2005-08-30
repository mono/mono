using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;

using awt = java.awt;
using image = java.awt.image;
using geom = java.awt.geom;


namespace System.Drawing
{
	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable, awt.Paint
	{
		protected abstract java.awt.Paint NativeObject {
			get;
		}

		awt.PaintContext awt.Paint.createContext (image.ColorModel cm,
			awt.Rectangle deviceBounds, geom.Rectangle2D userBounds, geom.AffineTransform xform,
			awt.RenderingHints hints) {
			Matrix.Multiply(xform, _brushTransform.NativeObject, MatrixOrder.Append);
			return NativeObject.createContext (cm, deviceBounds, userBounds, xform, hints);
		}

		int awt.Transparency.getTransparency () {
			return NativeObject.getTransparency ();
		}

		abstract public object Clone ();

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		protected internal Matrix _brushTransform = new Matrix();

//		~Brush ()
//		{
//			Dispose (false);
//		}
	}
}

