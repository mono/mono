//
// System.Drawing.Drawing2D.GeneralPathIterator.cs
//
// Author:
// Bors Kirzner <boris@mainsoft.com>	
//
// Copyright (C) 2005 Mainsoft Corporation, (http://www.mainsoft.com)
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

using java.awt.geom;

namespace System.Drawing.Drawing2D
{
	internal class GeneralPathIterator : PathIterator
	{
		#region Fields

		int typeIdx = 0;
		int pointIdx   = 0;
		ExtendedGeneralPath _path;
		AffineTransform _affine;

		private static readonly int [] curvesize = {2, 2, 4, 6, 0};

		#endregion // Fileds

		#region Constructors

		public GeneralPathIterator(ExtendedGeneralPath _path) : this (_path, null)
		{
		}

		public GeneralPathIterator(ExtendedGeneralPath _path, AffineTransform at) 
		{
			this._path = _path;
			this._affine = at;
		}

		#endregion // Constructors

		#region Methods

		public int getWindingRule() 
		{
			return _path.getWindingRule ();
		}

		public bool isDone() 
		{
			return (typeIdx >= _path.TypesCount);
		}

		public void next() 
		{
			int type = _path.Types [typeIdx++] & ExtendedGeneralPath.SEG_MASK;
			pointIdx += curvesize [type];
		}

		public int currentSegment(float [] coords) {
			int type = _path.Types [typeIdx] & ExtendedGeneralPath.SEG_MASK;
			int numCoords = curvesize [type];
			if (numCoords > 0 && _affine != null)
				_affine.transform (_path.Coords, pointIdx, coords, 0, numCoords/2);
			else
				Array.Copy (_path.Coords, pointIdx, coords, 0, numCoords);
			return type;
		}

		public int currentSegment(double [] coords) 
		{
			int type = _path.Types [typeIdx] & ExtendedGeneralPath.SEG_MASK;
			int numCoords = curvesize [type];
			if (numCoords > 0 && _affine != null)
				_affine.transform (_path.Coords, pointIdx, coords, 0, numCoords/2);
			else 
				for (int i=0; i < numCoords; i++)
					coords [i] = _path.Coords [pointIdx + i];
		
			return type;
		}

		#endregion // Methods
	}  
}
