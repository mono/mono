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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;

namespace System.Drawing
{
	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable
	{
		internal IntPtr nativeObject;
		internal bool disposed = false;
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
			lock (this){
				if (disposed == false) {
					GDIPlus.GdipDeleteBrush (nativeObject);
					disposed = true;
					nativeObject = IntPtr.Zero;
				}
			}
		}

		~Brush ()
		{
			Dispose (false);
		}
	}
}

