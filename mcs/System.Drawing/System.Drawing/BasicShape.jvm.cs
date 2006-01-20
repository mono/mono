using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for BasicShape.
	/// </summary>
	public abstract class BasicShape : MarshalByRefObject, awt.Shape, IDisposable
	{
		awt.Shape _shape;

		protected BasicShape(awt.Shape shape)
		{
			_shape = shape;
		}

		protected awt.Shape Shape {
			get {
				return _shape;
			}
			set {
				_shape = value;
			}
		}

		#region IDisposable
		public void Dispose () {
			Dispose (true);
		}

		void Dispose (bool disposing) {
		}
		#endregion

		#region Shape Members

		awt.Rectangle awt.Shape.getBounds() {
			return Shape.getBounds();
		}

		bool awt.Shape.contains(double arg_0, double arg_1) {
			return Shape.contains(arg_0, arg_1);
		}

		bool awt.Shape.contains(geom.Point2D arg_0) {
			return Shape.contains(arg_0);
		}

		bool awt.Shape.contains(double arg_0, double arg_1, double arg_2, double arg_3) {
			return Shape.contains(arg_0, arg_1, arg_2, arg_3);
		}

		bool awt.Shape.contains(geom.Rectangle2D arg_0) {
			return Shape.contains(arg_0);
		}

		geom.PathIterator awt.Shape.getPathIterator(geom.AffineTransform arg_0) {
			return Shape.getPathIterator(arg_0);
		}

		geom.PathIterator awt.Shape.getPathIterator(geom.AffineTransform arg_0, double arg_1) {
			return Shape.getPathIterator(arg_0, arg_1);
		}

		geom.Rectangle2D awt.Shape.getBounds2D() {
			return Shape.getBounds2D();
		}

		bool awt.Shape.intersects(double arg_0, double arg_1, double arg_2, double arg_3) {
			return Shape.intersects(arg_0, arg_1, arg_2, arg_3);
		}

		bool awt.Shape.intersects(geom.Rectangle2D arg_0) {
			return Shape.intersects(arg_0);
		}

		#endregion
	}
}
