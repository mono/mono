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
	namespace Win32Impl {

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
			System.Drawing.Brush brush;
			Color color;
			float width;
			internal IntPtr hpen_ = IntPtr.Zero;
			//PenAlignment alignment;
		
			public Pen (System.Drawing.Brush brush)
			{
				this.brush = brush;
				width = 1;
				// FIXME: get color from brush
				hpen_ = Win32.CreatePen(PenStyle.PS_SOLID,(int)width, Win32.RGB(Color.Black));
			}

			public Pen (Color color)
			{
				this.color = color;
				width = 1;
				hpen_ = Win32.CreatePen(PenStyle.PS_SOLID, (int)width, Win32.RGB(color));
			}

			public Pen (System.Drawing.Brush brush, float width)
			{
				this.width = width;
				this.brush = brush;
				// FIXME: get color from brush
				hpen_ = Win32.CreatePen(PenStyle.PS_SOLID,(int)width, Win32.RGB(Color.Black));
			}

			public Pen (Color color, float width)
			{
				this.width = width;
				this.color = color;
				hpen_ = Win32.CreatePen(PenStyle.PS_SOLID,(int)width, Win32.RGB(color));
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
					Win32.DeleteObject(hpen_);
				}
			}

			~Pen ()
			{
				Dispose (false);
			}
		}
	}

}
