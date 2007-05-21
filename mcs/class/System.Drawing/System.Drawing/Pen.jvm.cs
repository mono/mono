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

using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing 
{

	public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable, awt.Stroke
	{
		#region Member Vars

		static readonly float [] DASH_ARRAY = {4.0f,1.0f};
		static readonly float [] DASHDOT_ARRAY = {4.0f,1.0f,1.0f,1.0f};
		static readonly float [] DASHDOTDOT_ARRAY = {4.0f,1.0f,1.0f,1.0f,1.0f,1.0f};
		static readonly float [] DOT_ARRAY = {1.0f,1.0f};

		internal bool isModifiable = true;

		Brush _brush;
		DashStyle _dashStyle;
		DashCap _dashCap;
		LineCap _startCap;
		LineCap _endCap;

		LineJoin _lineJoin;

		PenAlignment _alignment;
		Matrix _transform;
		float _width;
		float _dashOffset;
		float[] _dashPattern;
		//float[] _compoundArray;

		float _miterLimit;

		#endregion

		#region Ctors. and Dtor

		public Pen (Brush brush) : this (brush, 1.0F)
		{}

		public Pen (Color color) : this (color, 1.0F)
		{}

		public Pen (Color color, float width) : this(new SolidBrush(color), width)
		{}

		public Pen (Brush brush, float width)
		{
			_brush = (Brush)brush.Clone();;
			_width = width;
			_dashStyle = DashStyle.Solid;
			_startCap = LineCap.Flat;
			_dashCap = DashCap.Flat;
			_endCap = LineCap.Flat;
			_alignment = PenAlignment.Center;
			_lineJoin = LineJoin.Miter;
			_miterLimit = 10f;
			_transform = new Matrix();
		}
		#endregion
		//
		// Properties
		//
		#region Alignment [TODO]
		public PenAlignment Alignment 
		{
			get 
			{
				return _alignment;
			}

			set 
			{
				EnsureModifiable();
				_alignment = value;
			}
		}
		#endregion

		#region Brush
		public Brush Brush 
		{
			get 
			{
				return _brush;
			}

			set 
			{
				EnsureModifiable();
				if (value == null)
					throw new ArgumentNullException("brush");
				_brush = value;
			}
		}
		#endregion

		#region Color
		public Color Color 
		{
			get 
			{
				if(Brush is SolidBrush)
					return ((SolidBrush)Brush).Color;
				else if(Brush is HatchBrush)
					return ((HatchBrush)Brush).ForegroundColor;
				else
					return Color.Empty;
			}

			set 
			{
				EnsureModifiable();
				_brush = new SolidBrush (value);
			}
		}
		#endregion 

		#region CompoundArray [TODO]
		[MonoTODO]
		public float[] CompoundArray {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion
            
		#region CustomEndCap [TODO]
		[MonoTODO]
		public CustomLineCap CustomEndCap 
		{
			get 
			{
				throw new NotImplementedException ();
			}
			// do a check for isModifiable when implementing this property
			set 
			{
				throw new NotImplementedException ();                                
			}
		}
		#endregion 

		#region CustomStartCap [TODO]
		[MonoTODO]
		public CustomLineCap CustomStartCap 
		{
			get 
			{
				throw new NotImplementedException ();                                
			}

			// do a check for isModifiable when implementing this property
			set 
			{
				throw new NotImplementedException ();                                
			}
		}
		#endregion

		#region DashCap
		[MonoTODO]
		public DashCap DashCap {
			get {
				return _dashCap;
			}

			set {
				EnsureModifiable();
				_dashCap = value;
			}
		}
		#endregion

		#region DashOffset
		public float DashOffset 
		{

			get 
			{
				return _dashOffset;
			}

			set 
			{
				EnsureModifiable();
				_dashOffset = value;
			}
		}
		#endregion

		#region DashPattern

		public float [] DashPattern 
		{
			get 
			{
				return (float [])_dashPattern.Clone();
			}

			set 
			{
				EnsureModifiable();

				_dashPattern = value;
				DashStyle = (_dashPattern == null) ? DashStyle.Solid : DashStyle.Custom;
			}
		}
		#endregion

		#region DashStyle
		public DashStyle DashStyle 
		{
			get 
			{
				return _dashStyle;
			}

			set 
			{
				EnsureModifiable();
				_dashStyle = value;
			}
		}
		#endregion 

		#region StartCap [TODO - now allways endcap]
		[MonoTODO]
		public LineCap StartCap {
			get { 
				return _startCap;
			}

			set {
				EnsureModifiable();
				_startCap = value;
			}
		}
		#endregion

		#region EndCap 
		[MonoTODO]
		public LineCap EndCap 
		{
			get 
			{
				return _endCap;
			}

			set 
			{
				EnsureModifiable();

				_endCap = value;
			}
		}
		#endregion
 
		#region LineJoin [partial TODO - missed styles]
		[MonoTODO]
		public LineJoin LineJoin {
			get {
				return _lineJoin;
			}

			set {
				EnsureModifiable();
				_lineJoin = value;
			}
		}

		#endregion

		#region MiterLimit 
		public float MiterLimit 
		{

			get 
			{
				return _miterLimit;
			}

			set 
			{
				EnsureModifiable();

				_miterLimit = value;			
			}
	                    
		}
		#endregion

		#region PenType
		public PenType PenType 
		{
			get 
			{
				if (Brush is TextureBrush)
					return PenType.TextureFill;
				else if (Brush is HatchBrush)
					return PenType.HatchFill;
				else if (Brush is LinearGradientBrush)
					return PenType.LinearGradient;
				else if (Brush is PathGradientBrush)
					return PenType.PathGradient;
				else
					return PenType.SolidColor;
			}
		}
		#endregion

		#region Transform
		public Matrix Transform 
		{
			get 
			{
				return _transform.Clone();
			}
					
			set 
			{
				EnsureModifiable();

				value.CopyTo(_transform);
			}
		}
		#endregion

		#region Width
		public float Width 
		{
			get 
			{
				return _width;
			}
			set 
			{
				EnsureModifiable();
												
				_width = value;
			}
		}
		#endregion

		#region Clone
		public object Clone ()
		{
			Pen clone = (Pen)MemberwiseClone();
			if (clone._transform != null)
				clone._transform = clone._transform.Clone();
			if (clone._dashPattern != null)
				clone._dashPattern = (float[])clone._dashPattern.Clone();
			return clone;
		}
		#endregion

		#region Dispose 
		public void Dispose ()
		{
			Dispose (true);
		}
		void Dispose (bool disposing)
		{
			if (!isModifiable && disposing)
				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			// Restore the dtor if adding anything below
		}
		#endregion

		#region Transform Funcs
		public void MultiplyTransform (Matrix matrix)
		{
			_transform.Multiply (matrix);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			_transform.Multiply (matrix, order);
		}

		public void ResetTransform ()
		{
			_transform.Reset ();
		}

		public void RotateTransform (float angle)
		{
			_transform.Rotate (angle);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			_transform.Rotate (angle, order);
		}

		public void ScaleTransform (float sx, float sy)
		{
			_transform.Scale (sx, sy);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			_transform.Scale (sx, sy, order);
		}

		public void TranslateTransform (float dx, float dy) {
			_transform.Translate (dx, dy);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			_transform.Translate (dx, dy, order);
		}
		#endregion

		public void SetLineCap (LineCap startCap, LineCap endCap, DashCap dashCap)
		{
			StartCap = startCap;
			DashCap = dashCap;
			EndCap = endCap;
		}

		void EnsureModifiable() {
			if (!isModifiable)
				throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
		}

		internal double GetSquaredTransformedWidth(geom.AffineTransform coordsTransform) {
			geom.AffineTransform transform = _transform.NativeObject;
			double A = transform.getScaleX();	// m00
			double B = transform.getShearY();	// m10
			double C = transform.getShearX();	// m01
			double D = transform.getScaleY();	// m11

			double K = coordsTransform.getScaleX();	// m00
			double L = coordsTransform.getShearY();	// m10
			double M = coordsTransform.getShearX();	// m01
			double N = coordsTransform.getScaleY();	// m11

			double AD = A*D, BC = B*C, KN = K*N, LM = L*M;
			double KN_LM = KN-LM;
			return Math.Abs(Width*Width * (AD*KN_LM - BC*KN_LM));
		}

		
		internal bool CanCreateBasicStroke {
			get {
				if (!_transform.IsIdentity)
					return false;

				//FIXME: add more logic when more features will
				// be implemented.
				return true;
			}
		}

		internal awt.Stroke GetNativeObject(geom.AffineTransform outputTransform, PenFit penFit) {
			return GetNativeObject(null, outputTransform, penFit);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="outputTransform">transform which will be applied on the final shape</param>
		/// <param name="fitPen">ensure the shape will wide enough to be visible</param>
		/// <returns></returns>
		internal awt.Stroke GetNativeObject(geom.AffineTransform penTransform, geom.AffineTransform outputTransform, PenFit penFit) {
			float[] dashPattern = null;

			switch (DashStyle) {
				case DashStyle.Custom:
					if (_dashPattern != null) {
						dashPattern = new float[_dashPattern.Length];
						for(int i = 0; i < _dashPattern.Length; i++) {

							if (EndCap == LineCap.Flat)
								dashPattern[i] = _dashPattern[i] * Width;
							else {
								if ((i & 1) == 0) {
									// remove the size of caps from the opaque parts
									dashPattern[i] = (_dashPattern[i] * Width) - Width;
									if (_dashPattern[i] < 0)
										dashPattern[i] = 0;
								}
								else
									// add the size of caps to the transparent parts
									dashPattern[i] = (_dashPattern[i] * Width) + Width;
							}
						}
					}
					break;
				case DashStyle.Dash:
					dashPattern = DASH_ARRAY;
					break;
				case DashStyle.DashDot:
					dashPattern = DASHDOT_ARRAY;
					break;
				case DashStyle.DashDotDot:
					dashPattern = DASHDOTDOT_ARRAY;
					break;
				
					//				default:
					//				case DashStyle.Solid:
					//					break;
			}

			int join;
			switch (LineJoin) {
				case LineJoin.Bevel:
					join = java.awt.BasicStroke.JOIN_BEVEL;
					break;
				default:
				case LineJoin.Miter:
				case LineJoin.MiterClipped:
					join = java.awt.BasicStroke.JOIN_MITER;
					break;
				case LineJoin.Round:
					join = java.awt.BasicStroke.JOIN_ROUND;
					break;
			}

			// We go by End cap for now.
			int cap;
			switch (EndCap) {
				default:
				case LineCap.Square:
				case LineCap.SquareAnchor:
					cap = awt.BasicStroke.CAP_SQUARE;
					break;
				case LineCap.Round: 
				case LineCap.RoundAnchor:
					cap = awt.BasicStroke.CAP_ROUND;
					break;
				case LineCap.Flat:
					cap = awt.BasicStroke.CAP_BUTT;
					break;
			}

			geom.AffineTransform penT = _transform.NativeObject;
			if (penTransform != null && !penTransform.isIdentity()) {
				penT = (geom.AffineTransform) penT.clone();
				penT.concatenate(penTransform);
			}

			return StrokeFactory.CreateStroke(Width, cap, 
				join, MiterLimit, dashPattern, DashOffset,
				penT, outputTransform, penFit);
		}

		#region Stroke Members

		awt.Shape awt.Stroke.createStrokedShape(awt.Shape arg_0) {
			return GetNativeObject(null, PenFit.NotThin).createStrokedShape(arg_0);
		}

		#endregion
	}
}
