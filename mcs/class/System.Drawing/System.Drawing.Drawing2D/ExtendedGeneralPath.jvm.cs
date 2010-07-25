//
// System.Drawing.Drawing2D.ExtendedGeneralPath.cs
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

using java.awt;
using java.awt.geom;
using java.lang;

namespace System.Drawing.Drawing2D
{
	internal class ExtendedGeneralPath : Shape, ICloneable
	{
		#region Fields

		public const int WIND_EVEN_ODD = 0; //PathIterator__Finals.WIND_EVEN_ODD;
		public const int WIND_NON_ZERO = 1; //PathIterator__Finals.WIND_NON_ZERO;
		
		public const sbyte SEG_MOVETO  = 0; //(byte) PathIterator__Finals.SEG_MOVETO;
		public const sbyte SEG_LINETO  = 1; //(byte) PathIterator__Finals.SEG_LINETO;
		public const sbyte SEG_QUADTO  = 2; //(byte) PathIterator__Finals.SEG_QUADTO;
		public const sbyte SEG_CUBICTO = 3; //(byte) PathIterator__Finals.SEG_CUBICTO;
		public const sbyte SEG_CLOSE   = 4; //(byte) PathIterator__Finals.SEG_CLOSE;
		
		public const sbyte SEG_START	  = 16; // segment start

		public const sbyte SEG_MASK	  = SEG_MOVETO | SEG_LINETO | SEG_QUADTO | SEG_CUBICTO | SEG_CLOSE; // mask to eliminate SEG_CLOSE and SEG_MARKER

		private const sbyte SEG_MARKER = 32; // path marker
		

		private sbyte [] _types;
		private float [] _coords;
		private int _typesCount;
		private int _coordsCount;
		private int _windingRule;

		private PathData _pathData;
		private GeneralPath _generalPath;

		const int INIT_SIZE = 20;
		const int EXPAND_MAX = 500;

		#endregion // Fileds

		#region Constructors

	    public ExtendedGeneralPath() : this (WIND_NON_ZERO, INIT_SIZE, INIT_SIZE)
		{
		}

		public ExtendedGeneralPath(int rule) : this (rule, INIT_SIZE, INIT_SIZE)
		{
		}

		public ExtendedGeneralPath(int rule, int initialCapacity) : this (rule, initialCapacity, initialCapacity)
		{
		}

		public ExtendedGeneralPath(Shape s) : this(WIND_NON_ZERO, INIT_SIZE, INIT_SIZE)
		{
			PathIterator pi = s.getPathIterator (null);
			setWindingRule (pi.getWindingRule ());
			append (pi, false);
		}

		private ExtendedGeneralPath(int rule, int initialTypes, int initialCoords) 
		{
			setWindingRule(rule);
			Reset (initialTypes, initialCoords);
		}

		#endregion // Constructors

		#region Properties

		private GeneralPath GeneralPath
		{
			get {
				if (_generalPath == null) {				
					_generalPath = GetGeneralPath ();
				}
				return _generalPath;
			}
		}

		public sbyte [] Types
		{
			get { return _types; }
		}

		public float [] Coords
		{
			get { return _coords; }
		}

		public int TypesCount
		{
			get { return _typesCount; }
		}

		public int CoordsCount
		{
			get { return _coordsCount; }
		}

		public bool LastFigureClosed
		{
			get { 
				return ((TypesCount == 0) || 
					((Types [TypesCount - 1] & ExtendedGeneralPath.SEG_CLOSE) != 0) ||
					((Types [TypesCount - 1] & ExtendedGeneralPath.SEG_START) != 0));
			}
		}

		public int PointCount
		{
			get {
				return CoordsCount / 2;
			}
		}

		public PathData PathData 
		{
			get 
			{
				if (_pathData == null)
					_pathData = GetPathData ();
				
				return _pathData;
			}
		}

		#endregion // Properties

		#region Methods

		#region CachedData

		private void ClearCache ()
		{
			_pathData = null;
			_generalPath = null;
		}

		private GeneralPath GetGeneralPath ()
		{
			PathIterator iter = getPathIterator (null);
			GeneralPath path = new GeneralPath ();
			path.append (iter, false);
			return path;
		}

		private PathData GetPathData ()
		{
			PathData pathData = new PathData();
			int nPts = PointCount;
			for (int i = 0; i < TypesCount; i++)
				if ((Types [i] & SEG_MASK) == SEG_QUADTO)
					nPts++;

			pathData.Types = new byte [nPts];
			pathData.Points = new PointF [nPts];
			int tpos = 0;
			int ppos = 0;
			int cpos = 0;
			byte marker;
			bool start;
			for (int i = 0; i < TypesCount; i++) {
				sbyte segmentType = (sbyte)(Types [i] & SEG_MASK);

				// set the masks and the markers
				marker = ((Types [i] & SEG_MARKER) != 0) ? (byte)PathPointType.PathMarker : (byte)0;
				start = ((Types [i] & SEG_START) != 0);
				
				switch (segmentType) {
					case SEG_CLOSE:
						pathData.Types [tpos - 1] = (byte) (pathData.Types [tpos - 1] | (byte) PathPointType.CloseSubpath | marker);
						break;
					case SEG_MOVETO:
						pathData.Types [tpos++] = (byte)((byte) PathPointType.Start | marker);
						pathData.Points [ppos++] = new PointF (Coords [cpos++], Coords [cpos++]);
						break;
					case SEG_LINETO:
						pathData.Types [tpos++] = (byte) ((byte) PathPointType.Line | marker);
						pathData.Points [ppos++] = new PointF (Coords [cpos++], Coords [cpos++]);
						break;
					case SEG_QUADTO:
						/*
							.net does not support Quadratic curves, so convert to Cubic according to http://pfaedit.sourceforge.net/bezier.html
							  
							The end points of the cubic will be the same as the quadratic's.
							CP0 = QP0
							CP3 = QP2 

							The two control points for the cubic are:

							CP1 = QP0 + 2/3 *(QP1-QP0)
							CP2 = CP1 + 1/3 *(QP2-QP0) 
						*/

						float x0 = Coords[cpos-2]; //QP0
						float y0 = Coords[cpos-1]; //QP0
			
						float x1 = x0 + (2/3 * (Coords [cpos++]-x0));
						float y1 = y0 + (2/3 * (Coords [cpos++]-y0));

						float x3 = Coords [cpos++]; //QP2
						float y3 = Coords [cpos++]; //QP2
						
						float x2 = x1 + (1/3 * (x3-x0));
						float y2 = y1 + (1/3 * (y3-y0));

						pathData.Types [tpos++] = (byte)(byte) PathPointType.Bezier;
						pathData.Points [ppos++] = new PointF (x1, y1);
						pathData.Types [tpos++] = (byte)(byte) PathPointType.Bezier;
						pathData.Points [ppos++] = new PointF (x2, y2);
						pathData.Types [tpos++] = (byte) ((byte)PathPointType.Bezier | marker);
						pathData.Points [ppos++] = new PointF (x3, y3);
						break;
					case SEG_CUBICTO:
						pathData.Types [tpos++] = (byte)(byte) PathPointType.Bezier3;
						pathData.Points [ppos++] = new PointF (Coords [cpos++], Coords [cpos++]);
						pathData.Types [tpos++] = (byte) PathPointType.Bezier3;
						pathData.Points [ppos++] = new PointF (Coords [cpos++], Coords [cpos++]);
						pathData.Types [tpos++] = (byte) ((byte)PathPointType.Bezier3 | marker);
						pathData.Points [ppos++] = new PointF (Coords [cpos++], Coords [cpos++]);
						break;
				}
			}
			return pathData;
		}

		#endregion // CachedData

		public void append(Shape s)
		{
			append (s, !LastFigureClosed);
		}

		#region GeneralPath

		public void append(PathIterator pi, bool connect) 
		{
			ClearCache ();
			float [] coords = new float [6];
			while (!pi.isDone ()) {
				switch (pi.currentSegment (coords)) {
					case SEG_MOVETO:
						if (!connect || _typesCount < 1 || _coordsCount < 2) {
							moveTo (coords [0], coords [1]);
							break;
						}
						if (_types [_typesCount - 1] != SEG_CLOSE &&
							_coords [_coordsCount - 2] == coords [0] &&
							_coords [_coordsCount - 1] == coords [1])
							break;	
						goto case SEG_LINETO;
					case SEG_LINETO:
						lineTo (coords [0], coords [1]);
						break;
					case SEG_QUADTO:
						quadTo (coords [0], coords [1], coords [2], coords [3]);
						break;
					case SEG_CUBICTO:
						curveTo (coords [0], coords [1], coords [2], coords [3], coords [4], coords [5]);
						break;
					case SEG_CLOSE:
						closePath ();
					break;
				}
				pi.next	();
				connect = false;
			}
		}

		public void append(Shape s, bool connect) 
		{
			PathIterator pi = s.getPathIterator (null);
			append (pi,connect);
		}

		public object Clone() 
		{
			ExtendedGeneralPath copy = (ExtendedGeneralPath)MemberwiseClone ();
			copy._types = (sbyte []) _types.Clone ();
			copy._coords = (float []) _coords.Clone ();
			return copy;
		}

		public void closePath() 
		{
			ClearCache ();
			if (_typesCount == 0 || _types[_typesCount - 1] != SEG_CLOSE) {
				needRoom (1, 0, true);
				_types [_typesCount++] = SEG_CLOSE;
			}
		}

		public bool contains(double x, double y) 
		{			
			return GeneralPath.contains (x, y);
		}

		public bool contains(double x, double y, double w, double h) 
		{
			return GeneralPath.contains (x, y, w, h);
		}

		public bool contains(Point2D p) 
		{
			return contains (p.getX (), p.getY ());
		}

		public bool contains(Rectangle2D r) 
		{
			return contains (r.getX (), r.getY (), r.getWidth (), r.getHeight ());
		}

		public Shape createTransformedShape(AffineTransform at) 
		{
			ExtendedGeneralPath gp = (ExtendedGeneralPath) Clone ();
			if (at != null) {
				gp.transform (at);
			}
			return gp;
		}

		public void curveTo(float x1, float y1, float x2, float y2, float x3, float y3) 
		{
			ClearCache ();
			needRoom (1, 6, true);
			_types [_typesCount++] = SEG_CUBICTO;
			_coords [_coordsCount++] = x1;
			_coords [_coordsCount++] = y1;
			_coords [_coordsCount++] = x2;
			_coords [_coordsCount++] = y2;
			_coords [_coordsCount++] = x3;
			_coords [_coordsCount++] = y3;
		}

		public java.awt.Rectangle getBounds() 
		{
			return getBounds2D ().getBounds ();
		}

		public Rectangle2D getBounds2D() 
		{
			float x1, y1, x2, y2;
			int i = _coordsCount;
			if (i > 0) {
				y1 = y2 = _coords [--i];
				x1 = x2 = _coords [--i];
				while (i > 0) {
					float y = _coords [--i];
					float x = _coords [--i];
					if (x < x1) x1 = x;
					if (y < y1) y1 = y;
					if (x > x2) x2 = x;
					if (y > y2) y2 = y;
				}
			} 
			else {
				x1 = y1 = x2 = y2 = 0f;
			}
			return new Rectangle2D.Float (x1, y1, x2 - x1, y2 - y1);
		}

		public Point2D getCurrentPoint() 
		{
			if (_typesCount < 1 || _coordsCount < 2)
				return null;
			
			int index = _coordsCount;
			if (_types [_typesCount - 1] == SEG_CLOSE)
				for (int i = _typesCount - 2; i > 0; i--) {
					switch (_types [i]) {
						case SEG_MOVETO:
							//break loop;
							goto loopend;
						case SEG_LINETO:
							index -= 2;
							break;
						case SEG_QUADTO:
							index -= 4;
							break;
						case SEG_CUBICTO:
							index -= 6;
							break;
						case SEG_CLOSE:
							break;
					}
				}
			loopend:

			return new Point2D.Float (_coords [index - 2], _coords [index - 1]);
		}

		public PathIterator getPathIterator(AffineTransform at) {
			return new GeneralPathIterator (this, at);
		}

		public PathIterator getPathIterator(AffineTransform at, double flatness) {
			return new FlatteningPathIterator (getPathIterator (at), flatness);
		}

		public int getWindingRule() 
		{
			return _windingRule;
		}

		public bool intersects(double x, double y, double w, double h) 
		{
			return GeneralPath.intersects (x, y, w, h);
		}

		public bool intersects(Rectangle2D r) 
		{
			return intersects (r.getX (), r.getY (), r.getWidth (), r.getHeight ());
		}

		public void lineTo(float x, float y) 
		{
			ClearCache ();
			needRoom (1, 2, true);
			_types [_typesCount++] = SEG_LINETO;
			_coords [_coordsCount++] = x;
			_coords [_coordsCount++] = y;
		}

		public void moveTo(float x, float y) 
		{
			ClearCache ();
			if (_typesCount > 0 && _types [_typesCount - 1] == SEG_MOVETO) {
				_coords [_coordsCount - 2] = x;
				_coords [_coordsCount - 1] = y;
			} 
			else {
				needRoom (1, 2, false);
				_types [_typesCount++] = SEG_MOVETO;
				_coords [_coordsCount++] = x;
				_coords [_coordsCount++] = y;
			}
		}

		public void quadTo(float x1, float y1, float x2, float y2) 
		{
			// restore quadTo as cubic affects quality
			ClearCache ();
			needRoom (1, 4, true);
			_types [_typesCount++] = SEG_QUADTO;
			_coords [_coordsCount++] = x1;
			_coords [_coordsCount++] = y1;
			_coords [_coordsCount++] = x2;
			_coords [_coordsCount++] = y2;
		}

		public void reset() 
		{
			ClearCache ();
			_typesCount = 0;
			_coordsCount = 0;
		}

		public void setWindingRule(int rule) 
		{
			if (rule != WIND_EVEN_ODD && rule != WIND_NON_ZERO) {
				throw new IllegalArgumentException ("winding rule must be WIND_EVEN_ODD or WIND_NON_ZERO");
			}
			_windingRule = rule;
		}

		public void transform(AffineTransform at) 
		{
			transform(at, 0, CoordsCount);
		}

		public void transform(AffineTransform at, int startCoord, int numCoords) {
			ClearCache ();
			at.transform (_coords, startCoord, _coords, startCoord, numCoords/2);
		}

		private void needRoom(int newTypes, int newCoords, bool needMove) 
		{
			if (needMove && _typesCount == 0)
				throw new IllegalPathStateException ("missing initial moveto in path definition");
			
			int size = _coords.Length;
			if (_coordsCount + newCoords > size) {
				int grow = size;
				if (grow > EXPAND_MAX * 2)
					grow = EXPAND_MAX * 2;
				
				if (grow < newCoords)
					grow = newCoords;
				
				float [] arr = new float [size + grow];
				Array.Copy (_coords, 0, arr, 0, _coordsCount);
				_coords = arr;
			}
			size = _types.Length;
			if (_typesCount + newTypes > size) {
				int grow = size;
				if (grow > EXPAND_MAX)
					grow = EXPAND_MAX;
				
				if (grow < newTypes)
					grow = newTypes;
				
				sbyte [] arr = new sbyte [size + grow];
				Array.Copy (_types, 0, arr, 0, _typesCount);
				_types = arr;
			}
		}

		#endregion // GeneralPath

		public void SetMarkers()
		{
			ClearCache ();
			if (TypesCount > 0)
				Types [ TypesCount - 1] |= SEG_MARKER;
		}

		public void ClearMarkers()
		{
			ClearCache ();
			for (int i = 0; i < TypesCount; i++)
				Types [i] &= ~SEG_MARKER;
		}

		public void StartFigure ()
		{
			ClearCache ();
			if (TypesCount > 0)
				Types [TypesCount - 1] |= ExtendedGeneralPath.SEG_START;
		}

		private void Reset (int initialTypes, int initialCoords)
		{
			ClearCache ();
			_types = new sbyte [initialTypes];
			_coords = new float [initialCoords * 2];
			_typesCount = 0;
			_coordsCount = 0;
		}

		internal void Clear ()
		{
			Reset (INIT_SIZE, INIT_SIZE);
		}

		internal void Reverse ()
		{
			ClearCache ();
			// revert coordinates
			for (int i=0, max = CoordsCount / 2; i < max;) {
				int ix = i++;
				int iy = i++;
				int rix = CoordsCount - i;
				int riy = rix + 1;
				float tmpx = Coords [ix];
				float tmpy = Coords [iy];
				Coords [ix] = Coords [rix];
				Coords [iy] = Coords [riy];
				Coords [rix] = tmpx;
				Coords [riy] = tmpy;
			}

			// revert types
			sbyte [] newTypes = new sbyte [TypesCount];
			int oldIdx = 0;
			int newIdx = TypesCount - 1;
			int copyStart;
			int copyEnd;
			sbyte mask1 = 0;
			sbyte mask2 = 0;
			sbyte closeMask = 0;
			bool closedFigure = false;
			
			while (oldIdx < TypesCount) {
				// start copying after moveto
				copyStart = ++oldIdx;
				// continue to the next figure start
				while ((Types [oldIdx] != SEG_MOVETO) && (oldIdx < TypesCount))
					oldIdx++;

				copyEnd = oldIdx - 1;
				// check whenever current figure is closed
				if ((Types [oldIdx - 1] & SEG_CLOSE) != 0) {
					closedFigure = true;
					// close figure
					newTypes [newIdx--] = (sbyte)(SEG_CLOSE | mask1);
					mask1 = 0;
					mask2 = 0;
					// end copy one cell earlier
					copyEnd--;
					closeMask = (sbyte)(Types [oldIdx - 1] & (sbyte)SEG_MARKER);
				}
				else {
					mask2 = mask1;
					mask1 = 0;
				}

				// copy reverted "inner" types
				for(int i = copyStart; i <= copyEnd; i++) {
					newTypes [newIdx--] = (sbyte)((Types [i] & SEG_MASK) | mask2);
					mask2 = mask1;
					mask1 = (sbyte)(Types [i] & (sbyte)SEG_MARKER);
				}

				// copy moveto
				newTypes [newIdx--] = SEG_MOVETO;

				// pass close mask to the nex figure
				if (closedFigure) {
					mask1 = closeMask;
					closedFigure = false;
				}
			}

			_types = newTypes;
		}

		public PointF GetLastPoint ()
		{
			if (CoordsCount == 0)
				throw new System.ArgumentException ("Invalid parameter used.");

			return new PointF (Coords [CoordsCount - 2], Coords [CoordsCount - 1]);
		}

		#endregion //Methods

		#region Private helpers

#if DEBUG
		private void Print()
		{
			Console.WriteLine ("\n\n");
			float [] fpoints = _coords;
			int cpos = 0;
			for (int i=0; i < _typesCount; i++) {
				sbyte type = _types [i];
				string marker = String.Empty;
				if ((type & SEG_MARKER) != 0)
					marker = " | MARKER";

				switch (type & SEG_MASK) {
					case SEG_CLOSE:
						Console.WriteLine ("CLOSE {0}",marker);
						break;
					case SEG_MOVETO:
						Console.WriteLine("{0}{3} ({1},{2})","MOVETO", fpoints[cpos++], fpoints[cpos++], marker);
						break;
					case SEG_LINETO:
						Console.WriteLine("{0}{3} ({1},{2})","LINETO", fpoints[cpos++], fpoints[cpos++], marker);
						break;
					case SEG_QUADTO:
						Console.WriteLine("{0}{3} ({1},{2})","QUADTO", fpoints[cpos++], fpoints[cpos++], marker);
						Console.WriteLine("       ({1},{2})","QUADTO", fpoints[cpos++], fpoints[cpos++]);
						break;
					case SEG_CUBICTO:
						Console.WriteLine("{0}{3} ({1},{2})","CUBICTO", fpoints[cpos++], fpoints[cpos++], marker);
						Console.WriteLine("        ({1},{2})","CUBICTO", fpoints[cpos++], fpoints[cpos++]);
						Console.WriteLine("        ({1},{2})","CUBICTO", fpoints[cpos++], fpoints[cpos++]);
						break;
				}
			}
		}
#endif
		#endregion // Private helpers
 		
	}
}
