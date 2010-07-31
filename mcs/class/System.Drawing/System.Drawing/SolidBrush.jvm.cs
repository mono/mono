//
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine(pigolkine@gmx.de)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
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
namespace System.Drawing
{
	public sealed class SolidBrush : Brush
	{
		internal bool isModifiable = true;
		Color _color;

		protected override java.awt.Paint NativeObject {
			get {
				return _color.NativeObject;
			}
		}
        
		public SolidBrush (Color color)
		{
			_color = color;
		}

		public Color Color {
			get {
				return _color;
			}
			set {
				if (isModifiable) 					
					_color = value;
				else
					throw new ArgumentException ("This SolidBrush object can't be modified.");
			}
		}
		
		public override object Clone()
		{
			return new SolidBrush(_color);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (!isModifiable && disposing)
				throw new ArgumentException ("This SolidBrush object can't be modified.");
		}
	}
}
