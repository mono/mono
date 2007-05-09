//
// System.Drawing.Drawing2D.CustomLineCap.cs
//
// Authors:
//	Dennis Hayes (dennish@Raytek.com)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002/3 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
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

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for CustomLineCap.
	/// </summary>
	[MonoNotSupported ("")]
	public class CustomLineCap : MarshalByRefObject, ICloneable
	{
		private bool disposed;

		// Constructors

		internal CustomLineCap () { }

		[MonoNotSupported ("")]
		public CustomLineCap (GraphicsPath fillPath, GraphicsPath strokePath) : this (fillPath, strokePath, LineCap.Flat, 0)
		{
		}

		[MonoNotSupported ("")]
		public CustomLineCap (GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap) : this (fillPath, strokePath, baseCap, 0)
		{
		}

		[MonoNotSupported ("")]
		public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap, float baseInset)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public LineCap BaseCap {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public LineJoin StrokeJoin {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public float BaseInset {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public float WidthScale {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		// Public Methods
		[MonoTODO]
		public virtual object Clone ()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void GetStrokeCaps (out LineCap startCap, out LineCap endCap)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetStrokeCaps(LineCap startCap, LineCap endCap)
		{
			throw new NotImplementedException();
		}
	}
}
