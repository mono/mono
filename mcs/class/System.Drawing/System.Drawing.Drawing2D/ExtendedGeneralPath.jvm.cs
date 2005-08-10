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
		private const sbyte SEG_UNMARK_MASK = 95; // path unmarker
		

		private sbyte [] _types;
		private float [] _coords;
		private int _typesCount;
		private int _coordsCount;
		private int _windingRule;

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
			_types = new sbyte [initialTypes];
			_coords = new float [initialCoords * 2];
		}

		#endregion // Constructors

		#region Properties

		private GeneralPath GeneralPath
		{
			get {
				PathIterator iter = getPathIterator (null);
				GeneralPath path = new GeneralPath ();
				path.append (iter, false);
				return path;
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



		#endregion // Properties

		#region Methods

		public void append(Shape s)
		{
			append (s, !LastFigureClosed);
		}

		#region GeneralPath

		public void append(PathIterator pi, bool connect) 
		{
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
			ExtendedGeneralPath copy = new ExtendedGeneralPath ();
			copy._types = (sbyte []) _types.Clone ();
			copy._coords = (float []) _coords.Clone ();
			return copy;
		}

		public void closePath() 
		{
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
			needRoom (1, 2, true);
			_types [_typesCount++] = SEG_LINETO;
			_coords [_coordsCount++] = x;
			_coords [_coordsCount++] = y;
		}

		public void moveTo(float x, float y) 
		{
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
			needRoom (1, 4, true);
			_types [_typesCount++] = SEG_QUADTO;
			_coords [_coordsCount++] = x1;
			_coords [_coordsCount++] = y1;
			_coords [_coordsCount++] = x2;
			_coords [_coordsCount++] = y2;
		}

		public void reset() 
		{
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
			at.transform (_coords, 0, _coords, 0, _coordsCount/2);
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
			Types [ TypesCount - 1] |= SEG_MARKER;
		}

		public void ClearMarkers()
		{
			for (int i = 0; i < TypesCount; i++)
				Types [i] &= SEG_UNMARK_MASK;
		}

		#endregion //Methods

 		
	}
}
