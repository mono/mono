//
// System.Drawing.Region.cs
//
// Author:
//	Miguel de Icaza (miguel@ximian.com)
//      Jordi Mas i Hernandez (jordi@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
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
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible (false)]
	public sealed class Region : MarshalByRefObject, IDisposable
	{
                private IntPtr nativeRegion = IntPtr.Zero;
                
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
			Status status = GDIPlus.GdipCreateRegionRectI (ref rect, out nativeRegion);
			GDIPlus.CheckStatus (status);
		}

		public Region (RectangleF rect)
		{
			Status status = GDIPlus.GdipCreateRegionRect (ref rect, out nativeRegion);
			GDIPlus.CheckStatus (status);
		}

                [MonoTODO]
		public Region (RegionData region_data)
		{
			throw new NotImplementedException ();
		}
		
		//                                                                                                     
		// Union
		//

		public void Union (GraphicsPath path)
		{
                        Status status = GDIPlus.GdipCombineRegionPath (nativeRegion, path.NativeObject, CombineMode.Union);
                        GDIPlus.CheckStatus (status);                        
		}


		public void Union (Rectangle rect)
		{                                    
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, ref rect, CombineMode.Union);
                        GDIPlus.CheckStatus (status);
		}

		public void Union (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, ref rect, CombineMode.Union);
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
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, ref rect, CombineMode.Intersect);
                        GDIPlus.CheckStatus (status);
		}

		public void Intersect (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, ref rect, CombineMode.Intersect);
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
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, ref rect, CombineMode.Complement);
                        GDIPlus.CheckStatus (status);
		}

		public void Complement (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, ref rect, CombineMode.Complement);
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
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, ref rect, CombineMode.Exclude);
                        GDIPlus.CheckStatus (status);
		}

		public void Exclude (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, ref rect, CombineMode.Exclude);
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
                        Status status = GDIPlus.GdipCombineRegionRectI (nativeRegion, ref rect, CombineMode.Xor);
                        GDIPlus.CheckStatus (status);
		}

		public void Xor (RectangleF rect)
		{
                        Status status = GDIPlus.GdipCombineRegionRect (nativeRegion, ref rect, CombineMode.Xor);
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
		
		public bool Equals(Region region, Graphics g)
		{
			bool result;
			
			Status status = GDIPlus.GdipIsEqualRegion (nativeRegion, region.NativeObject,
                           g.NativeObject, out result);                                   
                           
                        GDIPlus.CheckStatus (status);                      
                        
			return result;			
		}
		
		
		public static Region FromHrgn(IntPtr hrgn)
		{
			return new Region (hrgn);
		}
		
		
		public IntPtr GetHrgn(Graphics g)
		{
			return nativeRegion;
		}
		
		
		public RegionData GetRegionData()
		{
			int size, filled;			
			
			Status status = GDIPlus.GdipGetRegionDataSize (nativeRegion, out size);                  
                        GDIPlus.CheckStatus (status);                      
                        
                        byte[] buff = new byte [size];			
                        
			status = GDIPlus.GdipGetRegionData (nativeRegion, buff, size, out filled);
			GDIPlus.CheckStatus (status);                      
			
			RegionData rgndata = new RegionData();
			rgndata.Data = buff;
			
			return rgndata;
		}
		
		
		public RectangleF[] GetRegionScans(Matrix matrix)
		{
			int cnt;			
			
			Status status = GDIPlus.GdipGetRegionScansCount (nativeRegion, out cnt, matrix.NativeObject);                  
                        GDIPlus.CheckStatus (status);                                 
                        
                        if (cnt == 0)
                        	return new RectangleF[0];
                                                
                        RectangleF[] rects = new RectangleF [cnt];					
                        int size = Marshal.SizeOf (rects[0]);                  
                        
                        IntPtr dest = Marshal.AllocHGlobal (size * cnt);			
                        
			status = GDIPlus.GdipGetRegionScans (nativeRegion, dest, out cnt, matrix.NativeObject);
			GDIPlus.CheckStatus (status);                	
			
			GDIPlus.FromUnManagedMemoryToRectangles (dest, rects);			
			return rects;			
		}		
		
		public void Transform(Matrix matrix)
		{
			Status status = GDIPlus.GdipTransformRegion (nativeRegion, matrix.NativeObject);
			GDIPlus.CheckStatus (status);                      				
		}		
		
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
