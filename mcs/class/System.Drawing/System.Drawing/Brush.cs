//
// System.Drawing.Brush.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Ravindra (rkumar@novell.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Novell, Inc.  Http://www.novell.com
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Drawing
{
	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable
	{
		internal IntPtr nativeObject;
		abstract public object Clone ();

                internal Brush ()
                { }

		internal Brush (IntPtr ptr)
		{
                        nativeObject = ptr;
		}
		
		internal IntPtr NativeObject {
			get {
				return nativeObject;
			}
			set {
				nativeObject = value;
			}
		}

                internal Brush CreateBrush (IntPtr brush, System.Drawing.BrushType type)
                {
                        switch (type) {

                        case BrushType.BrushTypeSolidColor:
                                return new SolidBrush (brush);

                        case BrushType.BrushTypeHatchFill:
                                return new HatchBrush (brush);

                        case BrushType.BrushTypeTextureFill:
                                return new TextureBrush (brush);

                        default:
                                throw new NotImplementedException ();
                        }
                }

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing for now.
		}

		~Brush ()
		{
			Dispose (false);
		}
	}
}

