//
// System.Drawing.Drawing2D.GraphicsPathIterator.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;
using System.Drawing;

namespace System.Drawing.Drawing2D
{
        public sealed class GraphicsPathIterator : MarshalByRefObject, IDisposable
        {
                PointF [] _points;
                byte [] _types;
                int _count;
                int _current;
                
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
