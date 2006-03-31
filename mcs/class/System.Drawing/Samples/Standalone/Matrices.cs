//
// Matrices.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace Samples.Common {

	public class Matrices {

		static public object[] GetList ()
		{
			return new object[] {
				"Null",
				"Empty",
				"Rotate 15\'",
				"Rotate 45\' at 100,100",
				"Scale 1.5 x 0.5",
				"Shear 2.0 x 2.0",
				"Translate 50, 50"
			};
		}

		static public Matrix GetMatrix (int index)
		{
			// not defined (-1) or first (0)
			if (index <= 0)
				return null;

			Matrix m = new Matrix ();
			switch (index) {
			case 1:
				// empty
				break;
			case 2:
				m.Rotate (15);
				break;
			case 3:
				m.RotateAt (45, new PointF (100, 100));
				break;
			case 4:
				m.Scale (1.5f, 0.5f);
				break;
			case 5:
				m.Shear (2.0f, 2.0f);
				break;
			case 6:
				m.Translate (50, 50);
				break;
			}
			return m;
		}
	}
}
