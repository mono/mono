//
// System.Drawing.Pen.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine(pigolkine@gmx.de)
// 
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing {
	namespace XrImpl {

		internal class PenFactory : IPenFactory
		{
			public IPen Pen(System.Drawing.Brush brush, float width)
			{
				return new Pen(brush, width);
			}

			public IPen Pen(System.Drawing.Color color, float width)
			{
				return new Pen(color, width);
			}
		}

		public sealed class Pen : MarshalByRefObject, IPen { //, ICloneable, IDisposable {
			System.Drawing.Brush 	brush = null;
			Color color = Color.Black;
			float width = 1.0F;
			//internal IntPtr nativeObject_;
			//PenAlignment alignment;

			void CommonInit()
			{
				//nativeObject_ = Xr.MonoGI_CreatePen((double)color.R, (double)color.G, (double)color.B,(double)width);
			}
					
			public Pen (System.Drawing.Brush brush)
			{
				this.brush = brush;
				// FIXME: get color from brush
				CommonInit();
			}

			public Pen (Color color)
			{
				this.color = color;
				CommonInit();
			}

			public Pen (System.Drawing.Brush brush, float width)
			{
				this.width = width;
				this.brush = brush;
				// FIXME: get color from brush
				CommonInit();
			}

			public Pen (Color color, float width)
			{
				this.width = width;
				this.color = color;
				CommonInit();
			}

			internal void SetXrValues(IntPtr xrs)
			{
				Xr.XrSetRGBColor (xrs, (double)color.R, (double)color.G, (double)color.B);
				Xr.XrSetLineWidth (xrs, (double)width);
			}
			
			//
			// Properties
			//
			//		public PenAlignment Alignment {
			//			get {
			//				return alignment;
			//			}
			//
			//			set {
			//				alignment = value;
			//			}
			//		}

			System.Drawing.Brush IPen.Brush 
			{
				get 
				{
					return brush;
				}

				set 
				{
					brush = value;
				}
			}

			Color IPen.Color 
			{
				get 
				{
					return color;
				}

				set 
				{
					color = value;
				}
			}

			float IPen.Width 
			{
				get 
				{
					return width;
				}
				set 
				{
					width = value;
				}
			}

			//		public object Clone ()
			//		{
			//			Pen p = new Pen (brush, width);
			//			
			//			p.color = color;
			//			p.alignment = alignment;
			//
			//			return p;
			//		}

			void IDisposable.Dispose ()
			{
				Dispose (true);
				System.GC.SuppressFinalize (this);
			}

			void Dispose (bool disposing)
			{
				if( disposing) 
				{
					//Xr.MonoGI_DestroyPen(nativeObject_);
				}
			}

			~Pen ()
			{
				Dispose (false);
			}
		}
	}

}
