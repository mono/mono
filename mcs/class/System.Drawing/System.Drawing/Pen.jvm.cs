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
				return _dashPattern;
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
				if (_transform == null)
					_transform = new Matrix ();
				return _transform;
			}
					
			set 
			{
				EnsureModifiable();

				_transform = value;
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
			Transform.Multiply (matrix);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			Transform.Multiply (matrix, order);
		}

		public void ResetTransform ()
		{
			Transform.Reset ();
		}

		public void RotateTransform (float angle)
		{
			Transform.Rotate (angle);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			Transform.Rotate (angle, order);
		}

		public void ScaleTransform (float sx, float sy)
		{
			Transform.Scale (sx, sy);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			Transform.Scale (sx, sy, order);
		}

		public void TranslateTransform (float dx, float dy) {
			Transform.Translate (dx, dy);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			Transform.Translate (dx, dy, order);
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

		#region Stroke Members

		awt.Shape awt.Stroke.createStrokedShape(awt.Shape arg_0) {
			float[] dashPattern = null;
			//spivak.BUGBUG
			//You will see many magic numbers above this place
			//behaviours of dash patterns in .NET and JAVA are not similar
			//also it looks like JAVA have some design flaw there.
			//The issue is that in java only switched on (ODD) entries are
			//looks to be dependent on current line with. Switched off (EVEN)
			//entries allways remains exact width as you specify. So we should 
			//do some calculations to determine actual java pattern 
			//Also note that ODD entries does not grow proportionally with line width
			//so they should be sligntly ajusted also.
			//Well, i know that potential perfomance of this staf could be bad, but
			//that is solution for now. Note, that .NET have also numerous bugs in this
			//region, for example they mandatory could not tolerate patternalising
			//lines of 1 pixel width - look will be BAD.
			switch (DashStyle) {
				case DashStyle.Custom:
					if (DashPattern != null) {
						dashPattern = new float[DashPattern.Length];
						for(int i = 0; i < DashPattern.Length; i++) {
							if((i & 1) == 0) {
								if (DashPattern[i] > 1.0f)
									dashPattern[i] = DashPattern[i] + (DashPattern[i]-1.0f) * Width / 2f;
							}
							else
								dashPattern[i] = DashPattern[i] * Width * 2f;
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

			awt.Stroke stroke = StrokeFactory.CreateStroke(Width, cap, 
				join, MiterLimit, dashPattern, DashOffset,
				_transform != null ? _transform.NativeObject : null);
			
			return stroke.createStrokedShape(arg_0);
		}

		#endregion
	}
}
