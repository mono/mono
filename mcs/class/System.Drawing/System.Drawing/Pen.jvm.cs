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
using java.awt;

namespace System.Drawing 
{

	public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable 
	{
		#region Member Vars
		java.awt.BasicStroke nativeObject;
		internal bool isModifiable = true;
		Brush brush;
		DashStyle _ds = DashStyle.Solid; 
		PenAlignment _alignment;
		Matrix _transform;
		#endregion

		#region Internals
		internal Pen (java.awt.BasicStroke p)
		{
			nativeObject = p;
		}

		internal java.awt.BasicStroke NativeObject
		{
			get
			{
				return nativeObject;
			}
			set
			{
				nativeObject=value;
			}
		}
		#endregion

		#region Ctors. and Dtor
//		~Pen ()
//		{
//			Dispose (false);
//		}

		public Pen (Brush brush) : this (brush, 1.0F)
		{
		}

		public Pen (Color color) : this (color, 1.0F)
		{
		}

		public Pen (Color color, float width)			
		{
			brush = new SolidBrush(color);
			nativeObject = new java.awt.BasicStroke(width, BasicStroke.CAP_BUTT, BasicStroke.JOIN_MITER);
		}

		public Pen (Brush brush, float width)
		{
			brush = (Brush)brush.Clone();
			nativeObject = new java.awt.BasicStroke(width);
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
				if (!isModifiable)
					throw new ArgumentException ("Pen is not modifiable");
				_alignment = value;
			}
		}
		#endregion

		#region Brush
		public Brush Brush 
		{
			get 
			{
				return brush;
			}

			set 
			{
				if (isModifiable) 
				{
					brush = value;
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion

		#region Color
		public Color Color 
		{
			get 
			{
				if(brush is SolidBrush)
					return ((SolidBrush)brush).Color;
				else if(brush is HatchBrush)
					return ((HatchBrush)brush).ForegroundColor;
				else
					return Color.Empty;
			}

			set 
			{
				if (isModifiable) 
				{
					brush = new SolidBrush (value);
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
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

		#region CustoStartCap [TODO]
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

		#region DashCap [TODO, now - allways flat]
		public DashCap DashCap 
		{
			get 
			{
				//TODO
				return DashCap.Flat;
			}

			set 
			{
				
			}
		}
		#endregion

		#region DashOffset
		public float DashOffset 
		{

			get 
			{
				return nativeObject.getDashPhase();
			}

			set 
			{
				if (isModifiable)
					nativeObject = new java.awt.BasicStroke(
						nativeObject.getLineWidth(),
						nativeObject.getEndCap(),
						nativeObject.getLineJoin(),
						nativeObject.getMiterLimit(),
						nativeObject.getDashArray(),
						value);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion

		#region DashPattern

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

		internal void SetDashPattern(float [] patt,DashStyle s)
		{
			if(patt == null)
			{
				nativeObject = new java.awt.BasicStroke(
					nativeObject.getLineWidth(),
					nativeObject.getEndCap(),
					nativeObject.getLineJoin(),
					nativeObject.getMiterLimit(),
					null,
					nativeObject.getDashPhase());
					_ds = DashStyle.Solid;
			}
			else
			{
				float [] temp = new float[patt.Length];
				patt.CopyTo(temp,0);
				float w = nativeObject.getLineWidth();
				int i;
				for(i = 0;i<temp.Length;i+=2)
					if(temp[i] > 1.0f)
						temp[i] = temp[i] + (temp[i]-1.0f) * w / (float)2;

				for(i = 1;i<temp.Length;i+=2)
					temp[i] *= nativeObject.getLineWidth() * (float)2;

				nativeObject = new java.awt.BasicStroke(
					nativeObject.getLineWidth(),
					nativeObject.getEndCap(),
					nativeObject.getLineJoin(),
					nativeObject.getMiterLimit(),
					temp,
					nativeObject.getDashPhase());
				_ds = DashStyle.Custom;
			}

		}

		public float [] DashPattern 
		{
			get 
			{
				float w = nativeObject.getLineWidth();
				float [] temp = nativeObject.getDashArray();
				for(int i = 0;i<temp.Length;i+=2)
					if(temp[i] > 1.0f)
						temp[i] -= (temp[i] - 1.0f) / w * (float)2;

				for(int i = 1;i<temp.Length;i+=2)
					temp[i] /= w * 2;

				return temp; 
			}

			set 
			{
				
				if (isModifiable) 
					SetDashPattern(value,DashStyle.Custom);
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion

		#region DashStyle
		public DashStyle DashStyle 
		{
			get 
			{
				return _ds;
			}

			set 
			{
				if (isModifiable)
				{
					if (value == DashStyle.Solid)
						SetDashPattern(null,value);
					else if (value == DashStyle.Dash)
						SetDashPattern(System.Drawing.Drawing2D.DashAttribs.DASH_ARRAY,value);
					else if (value == DashStyle.DashDot)
						SetDashPattern(System.Drawing.Drawing2D.DashAttribs.DASHDOT_ARRAY,value);
					else if (value == DashStyle.DashDotDot)
						SetDashPattern(System.Drawing.Drawing2D.DashAttribs.DASHDOTDOT_ARRAY,value);
					else if (value == DashStyle.Dot)
						SetDashPattern(System.Drawing.Drawing2D.DashAttribs.DOT_ARRAY,value);
					else
						throw new ArgumentOutOfRangeException();
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion 

		#region StartCap [TODO - now allways endcap]

		public LineCap StartCap 
		{
			get 
			{
				//FALLBACK: StartCap, EndCap and DashCap are the same
                return EndCap;           
			}

			set 
			{
				EndCap = value;
			}
		}
		#endregion

		#region EndCap 
		public LineCap EndCap 
		{
			get 
			{
				int cup = nativeObject.getEndCap();
				if(cup == BasicStroke.CAP_ROUND)
					return LineCap.Round;
				else if(cup == BasicStroke.CAP_BUTT)
					return LineCap.Flat;
				else if(cup == BasicStroke.CAP_SQUARE)
					return LineCap.Square;
				else 
					return LineCap.Custom;
			}

			set 
			{
				if (isModifiable)
				{
					int cap;
					if((value == LineCap.Square) ||
						(value == LineCap.SquareAnchor))
						cap = BasicStroke.CAP_SQUARE;
					else if ((value == LineCap.Round) || 
						(value == LineCap.RoundAnchor))
						cap = BasicStroke.CAP_ROUND;
					else if ((value == LineCap.Flat))
						cap = BasicStroke.CAP_BUTT;
					else
						//TODO:default
						cap = BasicStroke.CAP_SQUARE;

					nativeObject = new java.awt.BasicStroke(
						nativeObject.getLineWidth(),
						cap,
						nativeObject.getLineJoin(),
						nativeObject.getMiterLimit(),
						nativeObject.getDashArray(),
						nativeObject.getDashPhase());
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion
 
		#region LineJoin [partial TODO - missed styles]
		public LineJoin LineJoin 
		{
			//TODO:missed styles

			get 
			{

				int join = nativeObject.getLineJoin();
				if(join == java.awt.BasicStroke.JOIN_BEVEL)
					return LineJoin.Bevel;
				else if(join == java.awt.BasicStroke.JOIN_MITER)
					return LineJoin.Miter;
				else if(join == java.awt.BasicStroke.JOIN_ROUND)
					return LineJoin.Round;
				else
					throw new ArgumentOutOfRangeException();
			}

			set 
			{
				if (isModifiable)
				{
					int join = 0;
					if (value ==  LineJoin.Bevel)
						join = java.awt.BasicStroke.JOIN_BEVEL;
					if ((value ==  LineJoin.Miter) || (value==LineJoin.MiterClipped))
						join = java.awt.BasicStroke.JOIN_MITER;
					if (value ==  LineJoin.Round)
						join = java.awt.BasicStroke.JOIN_ROUND;

					nativeObject = new java.awt.BasicStroke(
						nativeObject.getLineWidth(),
						nativeObject.getEndCap(),
						join,
						nativeObject.getMiterLimit(),
						nativeObject.getDashArray(),
						nativeObject.getDashPhase());
				}
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion

		#region MiterLimit 
		public float MiterLimit 
		{

			get 
			{
				return nativeObject.getMiterLimit();
			}

			set 
			{
				if (isModifiable)
					nativeObject = new java.awt.BasicStroke(
						nativeObject.getLineWidth(),
						nativeObject.getEndCap(),
						nativeObject.getLineJoin(),
						value,
						nativeObject.getDashArray(),
						nativeObject.getDashPhase());									
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
	                    
		}
		#endregion

		#region PenType
		public PenType PenType 
		{

			get 
			{
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
		#endregion

		#region Transform [TODO]
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
				if (!isModifiable)
                    throw new ArgumentException ("Pen is not modifiable");
				_transform = value;
			}
		}
		#endregion

		#region Width
		public float Width 
		{
			get 
			{
				return nativeObject.getLineWidth();
			}
			set 
			{
				if (isModifiable)
					nativeObject = new java.awt.BasicStroke(
						value,
						nativeObject.getEndCap(),
						nativeObject.getLineJoin(),
						nativeObject.getMiterLimit(),
						nativeObject.getDashArray(),
						nativeObject.getDashPhase());														
				else
					throw new ArgumentException ("You may not change this Pen because it does not belong to you.");
			}
		}
		#endregion

		#region Clone
		public object Clone ()
		{
			Pen p = new Pen (nativeObject);
			p.isModifiable = isModifiable;
			p.brush = brush;
			p._ds = _ds;
			p._alignment = _alignment;
			p._transform = _transform;
			return p;
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

		#region Transform Funcs [TODO]
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
			//FALLBACK: StartCap, EndCap and DashCap are the same
			EndCap = endCap;
		}
	}
}
