//
// System.Drawing.Region.cs
//
// Author:
//	Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc
//
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible(false)]
	public sealed class Region : MarshalByRefObject, IDisposable
	{
		public Region()
		{
			// Initialize region with infinite interior.
		}
		
		public Region (GraphicsPath path)
		{
			// Initializes the Region from the GraphicsPath 
		}

		public Region (Rectangle rect)
		{
		}

		public Region (RectangleF rect)
		{
		}

		public Region (RegionData region_data)
		{
		}
		
		//
		// Union
		//
		
		public void Union (GraphicsPath path)
		{
		}

		public void Union (Rectangle rect)
		{
		}

		public void Union (RectangleF rect)
		{
		}

		public void Union (Region region)
		{
		}

		
		//
		// Intersect
		//
		public void Intersect (GraphicsPath path)
		{
		}

		public void Intersect (Rectangle rect)
		{
		}

		public void Intersect (RectangleF rect)
		{
		}

		public void Intersect (Region region)
		{
		}

		//
		// Complement
		//
		public void Complement (GraphicsPath path)
		{
		}

		public void Complement (Rectangle rect)
		{
		}

		public void Complement (RectangleF rect)
		{
		}

		public void Complement (Region region)
		{
		}

		//
		// Exclude
		//
		public void Exclude (GraphicsPath path)
		{
		}

		public void Exclude (Rectangle rect)
		{
		}

		public void Exclude (RectangleF rect)
		{
		}

		public void Exclude (Region region)
		{
		}

		//
		// Xor
		//
		public void Xor (GraphicsPath path)
		{
		}

		public void Xor (Rectangle rect)
		{
		}

		public void Xor (RectangleF rect)
		{
		}

		public void Xor (Region region)
		{
		}

		//
		// GetBounds
		//
		public RectangleF GetBounds (Graphics graphics)
		{
			return new RectangleF ();
		}

		//
		// Translate
		//
		public void Translate (int dx, int dy)
		{
		}

		public void Translate (float dx, float dy)
		{
		}

		
		
		[ComVisible(false)]
		public Region Clone()
		{
			return this;
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
		}

		~Region ()
		{
			Dispose (false);
		}
	}
}
