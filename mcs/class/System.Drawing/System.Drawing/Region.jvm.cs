
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing
{
	[ComVisible (false)]
	public sealed class Region : BasicShape
	{
		#region Member Vars
		internal static readonly Region InfiniteRegion = new Region(new Rectangle(-0x400000, -0x400000, 0x800000, 0x800000));
		#endregion
        
		#region Internals
		internal geom.Area NativeObject
		{
			get
			{
				return (geom.Area)Shape;
			}
		}
		#endregion
		
		#region Ctors. and Dtor


		public Region ()
			: this ((geom.Area) InfiniteRegion.NativeObject.clone ())
		{                  
		}

        internal Region(geom.Area native) : base(native)
		{
        }
                
		
		public Region (GraphicsPath path) : this(new geom.Area(path.NativeObject))
		{	
		}

		public Region (Rectangle rect) : this(new geom.Area(rect.NativeObject))
		{
		}

		public Region (RectangleF rect) : this(new geom.Area(rect.NativeObject))
		{
		}

		[MonoTODO]
		public Region (RegionData region_data) : this((geom.Area)null)
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Union
		public void Union (GraphicsPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			NativeObject.add(new geom.Area(path.NativeObject));
		}


		public void Union (Rectangle rect)
		{                                    
			NativeObject.add(new geom.Area(rect.NativeObject));
		}

		public void Union (RectangleF rect)
		{
			NativeObject.add(new geom.Area(rect.NativeObject));
		}

		public void Union (Region region)
		{
			if (region == null)
				throw new ArgumentNullException("region");
			NativeObject.add(region.NativeObject);
		}
		#endregion                                                                                 

		#region Intersect
		//
		public void Intersect (GraphicsPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			NativeObject.intersect(new geom.Area(path.NativeObject));
		}

		public void Intersect (Rectangle rect)
		{
			NativeObject.intersect(new geom.Area(rect.NativeObject));
		}

		public void Intersect (RectangleF rect)
		{
			NativeObject.intersect(new geom.Area(rect.NativeObject));
		}

		public void Intersect (Region region)
		{
			if (region == null)
				throw new ArgumentNullException("region");
			NativeObject.intersect(region.NativeObject);
		}
		#endregion

		#region  Complement
		//
		public void Complement (GraphicsPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			geom.Area a = new geom.Area(path.NativeObject);
			a.subtract(NativeObject);
			Shape = a;
		}

		public void Complement (Rectangle rect)
		{
			geom.Area a = new geom.Area(rect.NativeObject);
			a.subtract(NativeObject);
			Shape = a;
		}

		public void Complement (RectangleF rect)
		{
			geom.Area a = new geom.Area(rect.NativeObject);
			a.subtract(NativeObject);
			Shape = a;
		}

		public void Complement (Region region)
		{
			if (region == null)
				throw new ArgumentNullException("region");
			geom.Area a = (geom.Area) region.NativeObject.clone ();
			a.subtract(NativeObject);
			Shape = a;
		}
		#endregion

		#region Exclude
		//
		public void Exclude (GraphicsPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			NativeObject.subtract(new geom.Area(path.NativeObject));
		}

		public void Exclude (Rectangle rect)
		{
			NativeObject.subtract(new geom.Area(rect.NativeObject));
		}

		public void Exclude (RectangleF rect)
		{
			NativeObject.subtract(new geom.Area(rect.NativeObject));
		}

		public void Exclude (Region region)
		{
			if (region == null)
				throw new ArgumentNullException("region");
			NativeObject.subtract(region.NativeObject);
		}
		#endregion

		#region  Xor
		//
		public void Xor (GraphicsPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			NativeObject.exclusiveOr(new geom.Area(path.NativeObject));
		}

		public void Xor (Rectangle rect)
		{
			NativeObject.exclusiveOr(new geom.Area(rect.NativeObject));
		}

		public void Xor (RectangleF rect)
		{
			NativeObject.exclusiveOr(new geom.Area(rect.NativeObject));
		}

		public void Xor (Region region)
		{
			if (region == null)
				throw new ArgumentNullException("region");
			NativeObject.exclusiveOr(region.NativeObject);
		}
		#endregion

		#region GetBounds
		//
		public RectangleF GetBounds (Graphics graphics)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");
			return new RectangleF(NativeObject.getBounds2D());
		}
		#endregion

		#region  Translate
		//
		public void Translate (int dx, int dy)
		{
			Translate((float)dx, (float)dy);
		}

		public void Translate (float dx, float dy)
		{
			if (NativeObject.equals(InfiniteRegion.NativeObject))
				return;
			NativeObject.transform(geom.AffineTransform.getTranslateInstance(
				dx,
				dy));
		}
		#endregion

		#region  IsVisible [TODO]
		//
		public bool IsVisible (int x, int y, Graphics g)
		{
			return IsVisible((float)x, (float)y, g);
		}

		public bool IsVisible (int x, int y, int width, int height)
		{
			return IsVisible((float)x, (float)y, (float)width, (float)height);
		}

		public bool IsVisible (int x, int y, int width, int height, Graphics g)
		{
			return IsVisible((float)x, (float)y, (float)width, (float)height, g);
		}

		public bool IsVisible (Point point)
		{
			return IsVisible(point.X, point.Y);
		}

		public bool IsVisible (PointF point)
		{
			return IsVisible(point.X, point.Y);
		}

		public bool IsVisible (Point point, Graphics g)
		{
			return IsVisible(point.X, point.Y, g);
		}

		public bool IsVisible (PointF point, Graphics g)
		{
			return IsVisible(point.X, point.Y, g);
		}

		public bool IsVisible (Rectangle rect)
		{
			return IsVisible(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public bool IsVisible (RectangleF rect)
		{
			return IsVisible(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public bool IsVisible (Rectangle rect, Graphics g)
		{
			return IsVisible(rect.X, rect.Y, rect.Width, rect.Height, g);
		}

		public bool IsVisible (RectangleF rect, Graphics g)
		{
			return IsVisible(rect.X, rect.Y, rect.Width, rect.Height, g);
		}

		public bool IsVisible (float x, float y)
		{
			return NativeObject.contains(x,y);
		}

		public bool IsVisible (float x, float y, Graphics g)
		{
			if (g == null)
				throw new ArgumentNullException("graphics");
			return NativeObject.contains(x,y);
		}

		public bool IsVisible (float x, float y, float width, float height)
		{
			return NativeObject.intersects(x,y,width,height);
		}

		public bool IsVisible (float x, float y, float width, float height, Graphics g) 
		{
			if (g == null)
				throw new ArgumentNullException("graphics");
			return NativeObject.intersects(x,y,width,height);
		}
		#endregion

		#region IsEmpty
		public bool IsEmpty(Graphics g)
		{
			if (g == null)
				throw new ArgumentNullException("graphics");
			return NativeObject.isEmpty();
		}
		#endregion

		#region IsInfinite
		public bool IsInfinite(Graphics g)
		{
			if (g == null)
				throw new ArgumentNullException("graphics");
			//probably too naive.
			return NativeObject.equals(InfiniteRegion.NativeObject);
		}
		#endregion

		#region MakeEmpty
		public void MakeEmpty()
		{
			NativeObject.reset();
		}
		#endregion

		#region MakeInfinite
		public void MakeInfinite()
		{
			Shape = (geom.Area) InfiniteRegion.NativeObject.clone ();
		}
		#endregion 

		#region Equals
		public bool Equals(Region region, Graphics g)
		{
			if (g == null)
				throw new ArgumentNullException("graphics");
			return NativeObject.equals(region.NativeObject);
		}
		#endregion
				
		[MonoTODO]
		public RegionData GetRegionData()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public IntPtr GetHrgn(Graphics g) {
			throw new NotImplementedException();
		}
		
		
		public RectangleF[] GetRegionScans(Matrix matrix)
		{
			throw new NotImplementedException();
		}
		
		#region Transform 
		public void Transform(Matrix matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException("matrix");
			if (NativeObject.equals(InfiniteRegion.NativeObject))
				return;
			NativeObject.transform(matrix.NativeObject);
		}		
		#endregion

		#region Clone
		public Region Clone()
		{
			return new Region ((geom.Area) NativeObject.clone ());
		}
		#endregion
	}
}
