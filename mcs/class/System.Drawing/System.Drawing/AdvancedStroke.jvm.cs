using System;
using System.Drawing.Drawing2D;
using java.lang;

using java.awt;
using java.awt.geom;
using sun.dc.path;
using sun.dc.pr;

namespace System.Drawing {

	internal enum PenFit {
		NotThin,
		Thin,
		ThinAntiAlias
	}

	internal class AdvancedStroke : Stroke {

		public const float PenUnits = 0.01f;
		public const int MinPenUnits = 100;
		public const int MinPenUnitsAA = 20;
		public const float MinPenSizeAA = PenUnits * MinPenUnitsAA;
		public const double MinPenSizeAASquared = (MinPenSizeAA * MinPenSizeAA);
		public const double MinPenSizeSquared = 1.000000001;
		public const double MinPenSizeNorm = 1.5;
		public const double MinPenSizeSquaredNorm = (MinPenSizeNorm * MinPenSizeNorm);

		/**
		 * Joins path segments by extending their outside edges until
		 * they meet.
		 */
		public const int JOIN_MITER = 0;

		/**
		 * Joins path segments by rounding off the corner at a radius
		 * of half the line width.
		 */
		public const int JOIN_ROUND = 1;

		/**
		 * Joins path segments by connecting the outer corners of their
		 * wide outlines with a straight segment.
		 */
		public const int JOIN_BEVEL = 2;

		/**
		 * Ends unclosed subpaths and dash segments with no added
		 * decoration.
		 */
		public const int CAP_BUTT = 0;

		/**
		 * Ends unclosed subpaths and dash segments with a round
		 * decoration that has a radius equal to half of the width
		 * of the pen.
		 */
		public const int CAP_ROUND = 1;

		/**
		 * Ends unclosed subpaths and dash segments with a square
		 * projection that extends beyond the end of the segment
		 * to a distance equal to half of the line width.
		 */
		public const int CAP_SQUARE = 2;

		float width;

		int join;
		int cap;
		float miterlimit;

		float[] dash;
		float dash_phase;

		AffineTransform _penTransform;
		AffineTransform _outputTransform;
		PenFit _penFit;

		/**
		 * Constructs a new <code>AdvancedStroke</code> with the specified
		 * attributes.
		 * @param width the width of this <code>AdvancedStroke</code>.  The
		 *         width must be greater than or equal to 0.0f.  If width is
		 *         set to 0.0f, the stroke is rendered as the thinnest
		 *         possible line for the target device and the antialias
		 *         hint setting.
		 * @param cap the decoration of the ends of a <code>AdvancedStroke</code>
		 * @param join the decoration applied where path segments meet
		 * @param miterlimit the limit to trim the miter join.  The miterlimit
		 *        must be greater than or equal to 1.0f.
		 * @param dash the array representing the dashing pattern
		 * @param dash_phase the offset to start the dashing pattern
		 * @throws IllegalArgumentException if <code>width</code> is negative
		 * @throws IllegalArgumentException if <code>cap</code> is not either
		 *         CAP_BUTT, CAP_ROUND or CAP_SQUARE
		 * @throws IllegalArgumentException if <code>miterlimit</code> is less
		 *         than 1 and <code>join</code> is JOIN_MITER
		 * @throws IllegalArgumentException if <code>join</code> is not
		 *         either JOIN_ROUND, JOIN_BEVEL, or JOIN_MITER
		 * @throws IllegalArgumentException if <code>dash_phase</code>
		 *         is negative and <code>dash</code> is not <code>null</code>
		 * @throws IllegalArgumentException if the length of
		 *         <code>dash</code> is zero
		 * @throws IllegalArgumentException if dash lengths are all zero.
		 */
		public AdvancedStroke(float width, int cap, int join, float miterlimit,
			float[] dash, float dash_phase, AffineTransform penTransform,
			AffineTransform outputTransform, PenFit penFit) {
			if (width < 0.0f) {
				throw new IllegalArgumentException("negative width");
			}
			if (cap != CAP_BUTT && cap != CAP_ROUND && cap != CAP_SQUARE) {
				throw new IllegalArgumentException("illegal end cap value");
			}
			if (join == JOIN_MITER) {
				if (miterlimit < 1.0f) {
					throw new IllegalArgumentException("miter limit < 1");
				}
			} else if (join != JOIN_ROUND && join != JOIN_BEVEL) {
				throw new IllegalArgumentException("illegal line join value");
			}
			if (dash != null) {
				if (dash_phase < 0.0f) {
					throw new IllegalArgumentException("negative dash phase");
				}
				bool allzero = true;
				for (int i = 0; i < dash.Length; i++) {
					float d = dash[i];
					if (d > 0.0) {
						allzero = false;
					} else if (d < 0.0) {
						throw new IllegalArgumentException("negative dash length");
					}
				}
				if (allzero) {
					throw new IllegalArgumentException("dash lengths all zero");
				}
			}
			this.width	= width;
			this.cap	= cap;
			this.join	= join;
			this.miterlimit	= miterlimit;
			if (dash != null) {
				this.dash = (float []) dash.Clone();
			}
			this.dash_phase	= dash_phase;
			this._penTransform = penTransform;
			this._outputTransform = outputTransform;
			this._penFit = penFit;
		}

		/**
		 * Constructs a solid <code>AdvancedStroke</code> with the specified 
		 * attributes.
		 * @param width the width of the <code>AdvancedStroke</code>
		 * @param cap the decoration of the ends of a <code>AdvancedStroke</code>
		 * @param join the decoration applied where path segments meet
		 * @param miterlimit the limit to trim the miter join
		 * @throws IllegalArgumentException if <code>width</code> is negative
		 * @throws IllegalArgumentException if <code>cap</code> is not either
		 *         CAP_BUTT, CAP_ROUND or CAP_SQUARE
		 * @throws IllegalArgumentException if <code>miterlimit</code> is less
		 *         than 1 and <code>join</code> is JOIN_MITER
		 * @throws IllegalArgumentException if <code>join</code> is not
		 *         either JOIN_ROUND, JOIN_BEVEL, or JOIN_MITER
		 */
		public AdvancedStroke(float width, int cap, int join, float miterlimit) :
			this(width, cap, join, miterlimit, null, 0.0f, null, null, PenFit.NotThin) {
		}

		/**
		 * Constructs a solid <code>AdvancedStroke</code> with the specified 
		 * attributes.  The <code>miterlimit</code> parameter is 
		 * unnecessary in cases where the default is allowable or the 
		 * line joins are not specified as JOIN_MITER.
		 * @param width the width of the <code>AdvancedStroke</code>
		 * @param cap the decoration of the ends of a <code>AdvancedStroke</code>
		 * @param join the decoration applied where path segments meet
		 * @throws IllegalArgumentException if <code>width</code> is negative
		 * @throws IllegalArgumentException if <code>cap</code> is not either
		 *         CAP_BUTT, CAP_ROUND or CAP_SQUARE
		 * @throws IllegalArgumentException if <code>join</code> is not
		 *         either JOIN_ROUND, JOIN_BEVEL, or JOIN_MITER
		 */
		public AdvancedStroke(float width, int cap, int join) :
			this(width, cap, join, 10.0f, null, 0.0f, null, null, PenFit.NotThin) {
		}

		/**
		 * Constructs a solid <code>AdvancedStroke</code> with the specified 
		 * line width and with default values for the cap and join 
		 * styles.
		 * @param width the width of the <code>AdvancedStroke</code>
		 * @throws IllegalArgumentException if <code>width</code> is negative
		 */
		public AdvancedStroke(float width) :
			this(width, CAP_SQUARE, JOIN_MITER, 10.0f, null, 0.0f, null, null, PenFit.NotThin) {
		}

		/**
		 * Constructs a new <code>AdvancedStroke</code> with defaults for all 
		 * attributes.
		 * The default attributes are a solid line of width 1.0, CAP_SQUARE,
		 * JOIN_MITER, a miter limit of 10.0.
		 */
		public AdvancedStroke() :
			this(1.0f, CAP_SQUARE, JOIN_MITER, 10.0f, null, 0.0f, null, null, PenFit.NotThin) {
		}


		/**
		 * Returns a <code>Shape</code> whose interior defines the 
		 * stroked outline of a specified <code>Shape</code>.
		 * @param s the <code>Shape</code> boundary be stroked
		 * @return the <code>Shape</code> of the stroked outline.
		 */
		public Shape createStrokedShape(Shape s) {
			FillAdapter filler = new FillAdapter();
			PathStroker stroker = new PathStroker(filler);
			PathConsumer consumer;

			stroker.setPenDiameter(width);
			switch (_penFit) {
				case PenFit.Thin:
					stroker.setPenFitting(PenUnits, MinPenUnits);
					break;
				case PenFit.ThinAntiAlias:
					stroker.setPenFitting(PenUnits, MinPenUnitsAA);
					break;
			}

			float[] t4 = null;
			if (PenTransform != null && !PenTransform.isIdentity() && (PenTransform.getDeterminant() > 1e-25)) {
				t4 = new float[]{
					(float)PenTransform.getScaleX(), (float)PenTransform.getShearY(), 
					(float)PenTransform.getShearX(), (float)PenTransform.getScaleY()
				};
			}

			float[] t6 = null;
			if (OutputTransform != null && !OutputTransform.isIdentity()) {
				t6 = new float[] {
					(float)OutputTransform.getScaleX(), (float)OutputTransform.getShearY(), 
					(float)OutputTransform.getShearX(), (float)OutputTransform.getScaleY(),
					(float)OutputTransform.getTranslateX(), (float)OutputTransform.getTranslateY()
				};
			}

			stroker.setPenT4(t4);
			stroker.setOutputT6(t6);
			stroker.setCaps(RasterizerCaps[cap]);
			stroker.setCorners(RasterizerCorners[join], miterlimit);
			if (dash != null) {
				PathDasher dasher = new PathDasher(stroker);
				dasher.setDash(dash, dash_phase);
				dasher.setDashT4(t4);
				consumer = dasher;
			} else {
				consumer = stroker;
			}

			PathIterator pi = s.getPathIterator(null);

			try {
				consumer.beginPath();
				bool pathClosed = false;
				float mx = 0.0f;
				float my = 0.0f;
				float[] point  = new float[6];

				while (!pi.isDone()) {
					int type = pi.currentSegment(point);
					if (pathClosed == true) {
						pathClosed = false;
						if (type !=  PathIterator__Finals.SEG_MOVETO) {
							// Force current point back to last moveto point
							consumer.beginSubpath(mx, my);
						}
					}
					switch ((GraphicsPath.JPI)type) {
						case GraphicsPath.JPI.SEG_MOVETO:
							mx = point[0];
							my = point[1];
							consumer.beginSubpath(point[0], point[1]);
							break;
						case GraphicsPath.JPI.SEG_LINETO:
							consumer.appendLine(point[0], point[1]);
							break;
						case GraphicsPath.JPI.SEG_QUADTO:
							// Quadratic curves take two points
							consumer.appendQuadratic(point[0], point[1],
								point[2], point[3]);
							break;
						case GraphicsPath.JPI.SEG_CUBICTO:
							// Cubic curves take three points
							consumer.appendCubic(point[0], point[1],
								point[2], point[3],
								point[4], point[5]);
							break;
						case GraphicsPath.JPI.SEG_CLOSE:
							consumer.closedSubpath();
							pathClosed = true;
							break;
					}
					pi.next();
				}

				consumer.endPath();
			} catch (PathException e) {
				throw new InternalError("Unable to Stroke shape ("+
					e.Message+")");
			}

			return filler.getShape();
		}

		/**
		 * Returns the line width.  Line width is represented in user space, 
		 * which is the default-coordinate space used by Java 2D.  See the
		 * <code>Graphics2D</code> class comments for more information on
		 * the user space coordinate system.
		 * @return the line width of this <code>AdvancedStroke</code>.
		 * @see Graphics2D
		 */
		public float getLineWidth() {
			return width;
		}

		/**
		 * Returns the end cap style.
		 * @return the end cap style of this <code>AdvancedStroke</code> as one
		 * of the static <code>int</code> values that define possible end cap
		 * styles.
		 */
		public int getEndCap() {
			return cap;
		}

		/**
		 * Returns the line join style.
		 * @return the line join style of the <code>AdvancedStroke</code> as one
		 * of the static <code>int</code> values that define possible line
		 * join styles.
		 */
		public int getLineJoin() {
			return join;
		}

		/**
		 * Returns the limit of miter joins.
		 * @return the limit of miter joins of the <code>AdvancedStroke</code>.
		 */
		public float getMiterLimit() {
			return miterlimit;
		}

		/**
		 * Returns the array representing the lengths of the dash segments.
		 * Alternate entries in the array represent the user space lengths
		 * of the opaque and transparent segments of the dashes.
		 * As the pen moves along the outline of the <code>Shape</code>
		 * to be stroked, the user space
		 * distance that the pen travels is accumulated.  The distance
		 * value is used to index into the dash array.
		 * The pen is opaque when its current cumulative distance maps
		 * to an even element of the dash array and transparent otherwise.
		 * @return the dash array.
		 */
		public float[] getDashArray() {
			if (dash == null) {
				return null;
			}

			return (float[]) dash.Clone();
		}

		/**
		 * Returns the current dash phase.
		 * The dash phase is a distance specified in user coordinates that 
		 * represents an offset into the dashing pattern. In other words, the dash 
		 * phase defines the point in the dashing pattern that will correspond to 
		 * the beginning of the stroke.
		 * @return the dash phase as a <code>float</code> value.
		 */
		public float getDashPhase() {
			return dash_phase;
		}

		/**
		 * Returns the hashcode for this stroke.
		 * @return      a hash code for this stroke.
		 */
		public override int GetHashCode() {
			int hash = Float.floatToIntBits(width);
			hash = hash * 31 + join;
			hash = hash * 31 + cap;
			hash = hash * 31 + Float.floatToIntBits(miterlimit);
			if (dash != null) {
				hash = hash * 31 + Float.floatToIntBits(dash_phase);
				for (int i = 0; i < dash.Length; i++) {
					hash = hash * 31 + Float.floatToIntBits(dash[i]);
				}
			}
			return hash;
		}

		/**
		 * Returns true if this AdvancedStroke represents the same
		 * stroking operation as the given argument.
		 */
		/**
		 * Tests if a specified object is equal to this <code>AdvancedStroke</code>
		 * by first testing if it is a <code>AdvancedStroke</code> and then comparing 
		 * its width, join, cap, miter limit, dash, and dash phase attributes with 
		 * those of this <code>AdvancedStroke</code>.
		 * @param  obj the specified object to compare to this 
		 *              <code>AdvancedStroke</code>
		 * @return <code>true</code> if the width, join, cap, miter limit, dash, and
		 *            dash phase are the same for both objects;
		 *            <code>false</code> otherwise.
		 */
		public override bool Equals(object obj) {
			if (!(obj is AdvancedStroke)) {
				return false;
			}

			AdvancedStroke bs = (AdvancedStroke) obj;
			if (width != bs.width) {
				return false;
			}

			if (join != bs.join) {
				return false;
			}

			if (cap != bs.cap) {
				return false;
			}

			if (miterlimit != bs.miterlimit) {
				return false;
			}

			if (dash != null) {
				if (dash_phase != bs.dash_phase) {
					return false;
				}

				if (!java.util.Arrays.equals(dash, bs.dash)) {
					return false;
				}
			}
			else if (bs.dash != null) {
				return false;
			}

			return true;
		}

		public AffineTransform PenTransform { 
			get{
				return _penTransform;
			}
			set{
				_penTransform = value;
			}
		}

		public AffineTransform OutputTransform {
			get {
				return _outputTransform;
			}
			set {
				_outputTransform = value;
			}
		}

		private static readonly int[] RasterizerCaps = {
														   Rasterizer.BUTT, Rasterizer.ROUND, Rasterizer.SQUARE
													   };

		private static readonly int[] RasterizerCorners = {
															  Rasterizer.MITER, Rasterizer.ROUND, Rasterizer.BEVEL
														  };

		#region FillAdapter

		private class FillAdapter : PathConsumer {
			bool closed;
			GeneralPath path;

			public FillAdapter() {
				path = new GeneralPath(GeneralPath.WIND_NON_ZERO);
			}

			public Shape getShape() {
				return path;
			}

			public void beginPath() {}

			public void beginSubpath(float x0, float y0) {
				if (closed) {
					path.closePath();
					closed = false;
				}
				path.moveTo(x0, y0);
			}

			public void appendLine(float x1, float y1) {
				path.lineTo(x1, y1);
			}

			public void appendQuadratic(float xm, float ym, float x1, float y1) {
				path.quadTo(xm, ym, x1, y1);
			}

			public void appendCubic(float xm, float ym,
				float xn, float yn,
				float x1, float y1) {
				path.curveTo(xm, ym, xn, yn, x1, y1);
			}

			public void closedSubpath() {
				closed = true;
			}

			public void endPath() {
				if (closed) {
					path.closePath();
					closed = false;
				}
			}

			public virtual PathConsumer getConsumer () {
				return this;
			}

			public void useProxy(FastPathProducer proxy) {
				proxy.sendTo(this);
			}

			public long getCPathConsumer() {
				return 0;
			}

			public void dispose() {
			}
		}

		#endregion
	}
}
