//
// System.Drawing.Drawing2D.GraphicsPathIterator.cs
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
using System.Drawing;

namespace System.Drawing.Drawing2D
{
	public sealed class GraphicsPathIterator : MarshalByRefObject, IDisposable
	{
		#region Fields

		private readonly GraphicsPath _path;
		private int _marker = -1;
		private int _subpath = -1;

		#endregion // Fields

		#region Constructors

		public GraphicsPathIterator (GraphicsPath path)
		{
			_path = path;
		}

		#endregion // Constructors

		#region Properites

		public int Count 
		{
			get { return _path.NativeObject.PointCount; }
		}

		public int SubpathCount {
			get {
				int count = 0;
				int start, end;
				bool isClosed;
				while (NextSubpath (out start, out end, out isClosed) != 0)
					count++;
				return count;
			}
		}

		#endregion // Properties

		#region Methods

		public int CopyData (ref PointF [] points, ref byte [] types, int startIndex, int endIndex)
		{
			int j = 0;
			for (int i = startIndex; i <= endIndex && i < _path.PointCount; i++) {
				points [j] = _path.PathPoints [i];
				types [j++] = _path.PathTypes [i];
			}
			return j;
		}

		public void Dispose ()
		{
		}

		public int Enumerate (ref PointF [] points, ref byte [] types)
		{
			return CopyData (ref points, ref types, 0, _path.PointCount);
		}

		public bool HasCurve ()
		{
			byte [] types = _path.PathTypes;
			for (int i=0; i < types.Length; i++)
				if ((types [i] & (byte)PathPointType.PathTypeMask) == (byte)PathPointType.Bezier3)
					return true;
			return false;
		}

		public int NextMarker (GraphicsPath path)
		{
			if (path == null)
				return 0;

			int startIndex;
			int endIndex;
			int count = NextMarker (out startIndex, out endIndex);

			if (count != 0)
				SetPath (_path, startIndex, count, path);

			return count;
		}

		public int NextMarker (out int startIndex, out int endIndex)
		{
			if (_marker >= _path.PointCount) {
				startIndex = 0;
				endIndex = 0;
				return 0;
			}

			startIndex = ++_marker;
			while ((_marker < _path.PointCount) && ((_path.PathTypes [_marker] & (byte)PathPointType.PathMarker) == 0))
				_marker++;

			endIndex = (_marker < _path.PointCount) ? _marker : _path.PointCount - 1;
			return endIndex - startIndex + 1;
		}

		public int NextPathType (out byte pathType, out int startIndex, out int endIndex)
		{
			if ((_subpath >= _path.PointCount - 1) | (_subpath < 0)) {
				startIndex = 0;
				endIndex = 0;
				pathType = (_subpath < 0) ? (byte)PathPointType.Start : _path.PathTypes [_path.PointCount - 1];
				return 0;
			}

			// .net acts different, but it seems to be a bug
			if ((_path.PathTypes [_subpath + 1] & (byte)PathPointType.PathMarker) != 0) {
				startIndex = 0;
				endIndex = 0;
				pathType = _path.PathTypes [_subpath];
				return 0;
			}

			startIndex = _subpath++;
			endIndex = startIndex;
			pathType = (byte)(_path.PathTypes [startIndex + 1] & (byte)PathPointType.PathTypeMask);

			while (((_subpath) < _path.PointCount) && ((_path.PathTypes [_subpath] & (byte)PathPointType.PathTypeMask) == pathType))
				_subpath++;
			
			endIndex = (_subpath < _path.PointCount) ? --_subpath : _path.PointCount - 1;
			return endIndex - startIndex + 1;
		}

		public int NextSubpath (GraphicsPath path, out bool isClosed)
		{
			int startIndex;
			int endIndex;
			int count = NextSubpath (out startIndex, out endIndex, out isClosed);

			if ((count != 0) && (path != null))
				SetPath (_path, startIndex, count, path);

			return count;
		}

		private void SetPath (GraphicsPath source, int start, int count, GraphicsPath target)
		{
			PointF [] points = new PointF [count];
			byte [] types = new byte [count];
			PointF [] pathPoints = _path.PathPoints;
			byte [] pathTypes = _path.PathTypes;

			for (int i = 0; i < count; i++) {
				points [i] = pathPoints [start + i];
				types [i] = pathTypes [start + i];
			}
			
			target.SetPath (points, types);
		}

		public int NextSubpath (out int startIndex, out int endIndex, out bool isClosed)
		{
			_subpath++;
			while (((_subpath) < _path.PointCount) && (_path.PathTypes [_subpath] != (byte)PathPointType.Start))
				_subpath++;

				
			if (_subpath >= _path.PointCount - 1) {
				startIndex = 0;
				endIndex = 0;
				isClosed = true;
				return 0;
			}			

			startIndex = _subpath;
			int offset = 1;
			while (((_subpath + offset) < _path.PointCount) && (_path.PathTypes [_subpath + offset] != (byte)PathPointType.Start))
				offset++;

			endIndex = ((_subpath + offset) < _path.PointCount) ? _subpath + offset - 1 : _path.PointCount - 1;
			isClosed = (_path.PathTypes [endIndex] & (byte)PathPointType.CloseSubpath) != 0;
			return endIndex - startIndex + 1;
		}

		public void Rewind ()
		{
			_marker = -1;
			_subpath = -1;
		}

		#endregion // Methods
	}
}
