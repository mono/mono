//
// System.Drawing.Region.cs
//
// Author:
//	Miguel de Icaza (miguel@ximian.com)
//      Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2003 Ximian, Inc
//
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible (false)]
	public sealed class Region : MarshalByRefObject, IDisposable
	{
                internal IntPtr nativeRegion = IntPtr.Zero;
                
		public Region()
		{                        
                        Status status = GDIPlus.GdipCreateRegion (out nativeRegion);
                        GDIPlus.CheckStatus (status);
		}

                internal Region(IntPtr native)
		{
                        nativeRegion = native; 
                }
                
		
		public Region (GraphicsPath path)
		{			
                        Status status = GDIPlus.GdipCreateRegionPath (path.NativeObject, out nativeRegion);
                        GDIPlus.CheckStatus (status);
		}

		public Region (Rectangle rect)                
		{
                        Status status = GDIPlus.GdipCreateRegionRectI (rect, out nativeRegion);
                        GDIPlus.CheckStatus (status);
		}

		public Region (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCreateRegionRect (rect, out nativeRegion);
                        GDIPlus.CheckStatus (status);
		}

                [MonoTODO]
		public Region (RegionData region_data)
		{
		}
		
		//                                                                                                     a
		// Union
		//

		public void Union (GraphicsPath path)
		{
                        Status status = GDIPlus.GdipCombineRegionPath (nativeRegion, path.NativeObject, CombineMode.Union);
                        GDIPlus.CheckStatus (status);                        
		}


		public void Union (Rectangle rect)
		{                                    
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, rect, CombineMode.Union);
                        GDIPlus.CheckStatus (status);
		}

		public void Union (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, rect, CombineMode.Union);
                        GDIPlus.CheckStatus (status);
		}

		public void Union (Region region)
		{
                        Status status = GDIPlus.GdipCombineRegionRegion (nativeRegion, region.NativeObject, CombineMode.Union);
                        GDIPlus.CheckStatus (status);
		}                                                                                         

		
		//
		// Intersect
		//
		public void Intersect (GraphicsPath path)
                {
                        Status status = GDIPlus.GdipCombineRegionPath (nativeRegion, path.NativeObject, CombineMode.Intersect);
                        GDIPlus.CheckStatus (status);  
		}

		public void Intersect (Rectangle rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, rect, CombineMode.Intersect);
                        GDIPlus.CheckStatus (status);
		}

		public void Intersect (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, rect, CombineMode.Intersect);
                        GDIPlus.CheckStatus (status);
		}

                public void Intersect (Region region)
		{
                        Status status = GDIPlus.GdipCombineRegionRegion (nativeRegion, region.NativeObject, CombineMode.Intersect);
                        GDIPlus.CheckStatus (status);
		}

		//
		// Complement
		//
		public void Complement (GraphicsPath path)
		{
                        Status status = GDIPlus.GdipCombineRegionPath (nativeRegion, path.NativeObject, CombineMode.Complement);
                        GDIPlus.CheckStatus (status);  
		}

		public void Complement (Rectangle rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, rect, CombineMode.Complement);
                        GDIPlus.CheckStatus (status);
		}

		public void Complement (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, rect, CombineMode.Complement);
                        GDIPlus.CheckStatus (status);
		}

                public void Complement (Region region)
		{
                        Status status = GDIPlus.GdipCombineRegionRegion (nativeRegion, region.NativeObject, CombineMode.Complement);
                        GDIPlus.CheckStatus (status);
		}

		//
		// Exclude
		//
		public void Exclude (GraphicsPath path)
		{
                        Status status = GDIPlus.GdipCombineRegionPath (nativeRegion, path.NativeObject, CombineMode.Exclude);
                        GDIPlus.CheckStatus (status);                                                   
		}

		public void Exclude (Rectangle rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, rect, CombineMode.Exclude);
                        GDIPlus.CheckStatus (status);
		}

		public void Exclude (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, rect, CombineMode.Exclude);
                        GDIPlus.CheckStatus (status);
		}

                public void Exclude (Region region)
		{
                        Status status = GDIPlus.GdipCombineRegionRegion (nativeRegion, region.NativeObject, CombineMode.Exclude);
                        GDIPlus.CheckStatus (status);
		}

		//
		// Xor
		//
		public void Xor (GraphicsPath path)
		{
                        Status status = GDIPlus.GdipCombineRegionPath (nativeRegion, path.NativeObject, CombineMode.Xor);
                        GDIPlus.CheckStatus (status);  
		}

		public void Xor (Rectangle rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, rect, CombineMode.Xor);
                        GDIPlus.CheckStatus (status);
		}

		public void Xor (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, rect, CombineMode.Xor);
                        GDIPlus.CheckStatus (status);
		}

                public void Xor (Region region)
		{
                        Status status = GDIPlus.GdipCombineRegionRegion (nativeRegion, region.NativeObject, CombineMode.Xor);
                        GDIPlus.CheckStatus (status); 
		}

		//
		// GetBounds
		//
		public RectangleF GetBounds (Graphics graphics)
		{
                        RectangleF rect = new Rectangle();
                        
                        Status status = GDIPlus.GdipGetRegionBounds (nativeRegion, graphics.NativeObject, ref rect);
                        GDIPlus.CheckStatus (status);

                        return rect;
                }

		//
		// Translate
		//
		public void Translate (int dx, int dy)
		{
                        Status status = GDIPlus.GdipTranslateRegionI (nativeRegion, dx, dy);
                        GDIPlus.CheckStatus (status);   
		}

		public void Translate (float dx, float dy)
		{
                        Status status = GDIPlus.GdipTranslateRegion (nativeRegion, dx, dy);
                        GDIPlus.CheckStatus (status);
		}

		//
		// IsVisible
		//
		public bool IsVisible (int x, int y, Graphics g)
		{
                        bool result;
                        
		    	Status status = GDIPlus.GdipIsVisibleRegionPointI (nativeRegion, x, y, g.NativeObject, out result);
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (int x, int y, int width, int height)
		{
		        bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRectI (nativeRegion, x, y,
                                width, height, IntPtr.Zero, out result);

                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (int x, int y, int width, int height, Graphics g)
		{
		        bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRectI (nativeRegion, x, y,
                                width, height, g.NativeObject, out result);

                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (Point point)
		{
		        bool result;

		    	Status status = GDIPlus.GdipIsVisibleRegionPointI (nativeRegion, point.X, point.Y,
                                IntPtr.Zero, out result);
                                
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (PointF point)
		{
		       bool result;

		    	Status status = GDIPlus.GdipIsVisibleRegionPoint (nativeRegion, point.X, point.Y,
                                IntPtr.Zero, out result);

                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (Point point, Graphics g)
		{
                        bool result;

		    	Status status = GDIPlus.GdipIsVisibleRegionPointI (nativeRegion, point.X, point.Y,
                                g.NativeObject, out result);

                        GDIPlus.CheckStatus (status);

                        return result;                                                      
		}

		public bool IsVisible (PointF point, Graphics g)
		{
		        bool result;

		    	Status status = GDIPlus.GdipIsVisibleRegionPoint (nativeRegion, point.X, point.Y,
                                g.NativeObject, out result);

                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (Rectangle rect)
		{
		        bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRectI (nativeRegion, rect.X, rect.Y,
                                rect.Width, rect.Height, IntPtr.Zero, out result);

                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (RectangleF rect)
		{
                        bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRect (nativeRegion, rect.X, rect.Y,
                                rect.Width, rect.Height, IntPtr.Zero, out result);

                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (Rectangle rect, Graphics g)
		{
		        bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRectI (nativeRegion, rect.X, rect.Y,
                                rect.Width, rect.Height, g.NativeObject, out result);
                        
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (RectangleF rect, Graphics g)
		{
		      bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRect (nativeRegion, rect.X, rect.Y,
                                rect.Width, rect.Height, g.NativeObject, out result);
                                
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (float x, float y)
		{
                        bool result;

		    	Status status = GDIPlus.GdipIsVisibleRegionPoint (nativeRegion, x, y, IntPtr.Zero, out result);
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (float x, float y, Graphics g)
		{
		        bool result;

		    	Status status = GDIPlus.GdipIsVisibleRegionPoint (nativeRegion, x, y, g.NativeObject, out result);
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (float x, float y, float width, float height)
		{
		        bool result;
                        
                        Status status = GDIPlus.GdipIsVisibleRegionRect (nativeRegion, x, y, width, height, IntPtr.Zero, out result);
                        GDIPlus.CheckStatus (status);

                        return result;
		}

		public bool IsVisible (float x, float y, float width, float height, Graphics g) 
		{
                        bool result;

                        Status status = GDIPlus.GdipIsVisibleRegionRect (nativeRegion, x, y, width, height, g.NativeObject, out result);
                        GDIPlus.CheckStatus (status);

                        return result;
		}


		//
		// Miscellaneous
		//

		public bool IsEmpty(Graphics g)
		{
                        bool result;               

                        Status status = GDIPlus.GdipIsEmptyRegion (nativeRegion, g.NativeObject, out result);
                        GDIPlus.CheckStatus (status);

                        return result;                        
		}

		public bool IsInfinite(Graphics g)
		{
                        bool result;

                        Status status = GDIPlus.GdipIsInfiniteRegion (nativeRegion, g.NativeObject, out result);
                        GDIPlus.CheckStatus (status);

                        return result;  
		}

		public void MakeEmpty()
		{
                        Status status = GDIPlus.GdipSetEmpty (nativeRegion);
                        GDIPlus.CheckStatus (status);               
		}

		public void MakeInfinite()
		{
                        Status status = GDIPlus.GdipSetInfinite (nativeRegion);
                        GDIPlus.CheckStatus (status);                      
		}
		
		
		[ComVisible (false)]
		public Region Clone()
		{
                        IntPtr cloned;
                        
                        Status status = GDIPlus.GdipCloneRegion (nativeRegion, out cloned);
                        GDIPlus.CheckStatus (status);

                        return new Region (cloned); 
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
                        if (disposing)
                                GDIPlus.GdipDeleteRegion (nativeRegion);
		}

		~Region ()
		{
			Dispose (false);
		}

                internal IntPtr NativeObject
                {
			get{
				return nativeRegion;
			}
			set	{
				nativeRegion = value;
			}
		}

        
	}
}
