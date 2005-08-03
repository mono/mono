//
// System.Drawing.Drawing2D.GraphicsPathIterator.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Duncan Mak (duncan@ximian.com)
//   Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002/3 Ximian, Inc (http://www.ximian.com)
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

		public GraphicsPathIterator (GraphicsPath path)
		{
			throw new NotImplementedException();
		}

		// Public Properites

		public int Count {
			get {
				throw new NotImplementedException();
			}
		}

		public int SubpathCount {
			get {
				throw new NotImplementedException();
			}
		}

		internal void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}

		// Public Methods.

		public int CopyData (ref PointF [] points, ref byte [] types, int startIndex, int endIndex)
		{
			throw new NotImplementedException();
		}

		public void Dispose ()
		{
			throw new NotImplementedException();
		}

		~GraphicsPathIterator ()
		{
			Dispose (false);
		}

		public int Enumerate (ref PointF [] points, ref byte [] types)
		{
			throw new NotImplementedException();
		}

		public bool HasCurve ()
		{
			throw new NotImplementedException();
		}

		public int NextMarker (GraphicsPath path)
		{
			throw new NotImplementedException();
		}

		public int NextMarker (out int startIndex, out int endIndex)
		{
			throw new NotImplementedException();
		}

		public int NextPathType (out byte pathType, out int startIndex, out int endIndex)
		{
			throw new NotImplementedException();
		}

		public int NextSubpath (GraphicsPath path, out bool isClosed)
		{
			throw new NotImplementedException();
		}

		public int NextSubpath (out int startIndex, out int endIndex, out bool isClosed)
		{
			throw new NotImplementedException();
		}

		public void Rewind ()
		{
			throw new NotImplementedException();
		}
	}
}
