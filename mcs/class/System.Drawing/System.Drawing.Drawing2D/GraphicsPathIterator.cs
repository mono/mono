//
// System.Drawing.Drawing2D.GraphicsPathIterator.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2002/3 Ximian, Inc
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

namespace System.Drawing.Drawing2D
{
	public sealed class GraphicsPathIterator : MarshalByRefObject, IDisposable
	{
		private PointF [] _points;
		private byte [] _types;
		private int _count;
		private int _current;

		// Constructors
		public GraphicsPathIterator (GraphicsPath path)
		{
			this._points = path.PathPoints;
			this._types = path.PathTypes;
			this._count = path.PointCount;
			this._current = 0;
		}

		// Public Properites
		public int Count {
			get {
				return _count;
			}
		}

		public int SubpathCount {
			get {
				int count = 0;

				foreach (byte b in _types)
					if (b == (byte) PathPointType.Start)
						count++;
				
				return count;
			}
		}

		// Public Methods.
		public int CopyData (ref PointF [] points, ref byte [] types, int startIndex, int endIndex)
		{
			for (int i = 0, j = startIndex; j < endIndex; i++, j++) {
				points [i] = _points [j];
				types [i] = _types [j];
			}

			return endIndex - startIndex;
		}

		public void Dispose ()
		{
		}

		~GraphicsPathIterator ()
		{
		}

		public int Enumerate (ref PointF [] points, ref byte [] types)
		{
			points = _points;
			types = _types;

			return _count;
		}

		public bool HasCurve ()
		{
			foreach (byte b in _types)
				if (b == (byte) PathPointType.Bezier)
					return true;

			return false;
		}

		[MonoTODO]
		public int NextMarker (GraphicsPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int NextMarker (out int startIndex, out int endIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int NextPathType (out byte pathType, out int startIndex, out int endIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int NextSubpath (GraphicsPath path, out bool isClosed)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int NextSubpath (out int startIndex, out int endIndex, out bool isClosed)
		{
			throw new NotImplementedException ();
		}

		public void Rewind ()
		{
			_current = 0;
		}
	}
}
