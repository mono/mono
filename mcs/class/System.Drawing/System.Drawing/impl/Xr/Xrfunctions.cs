//
// System.Drawing.XrImpl.Xrfunctions.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Author: Alexandre Pigolkine <pigolkine@gmx.de>
//
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.XrImpl {
	internal class Xr {
	
		internal enum XrFormat : int {
	    	XrFormatARGB32 = 0,
		    XrFormatRGB24 = 1,
		    XrFormatA8 = 2,
	    	XrFormatA1 = 4
		}
	
		#region Xr
		/// <summary>
		// Xr interface
		/// </summary>
		
		const string Xrimp = "Xr";
		
		[DllImport(Xrimp)]
		internal static extern IntPtr XrCreate ();
		
		[DllImport(Xrimp)]
		internal static extern void XrSetTargetImage (IntPtr xrs, IntPtr bytes, XrFormat format, int width, int height, int stride);
		
		[DllImport(Xrimp)]
		internal static extern IntPtr XrDestroy (IntPtr xrs);
		
		[DllImport(Xrimp)]
		internal static extern void XrSetRGBColor (IntPtr xrs, double red, double green, double blue);
		
		[DllImport(Xrimp)]
		internal static extern void XrSetLineWidth (IntPtr xrs, double width);
		
		[DllImport(Xrimp)]
		internal static extern void XrMoveTo (IntPtr xrs, double x, double y);
		
		[DllImport(Xrimp)]
		internal static extern void XrLineTo (IntPtr xrs, double x, double y);
		
		[DllImport(Xrimp)]
		internal static extern void XrStroke (IntPtr xrs);
		
		
		#endregion
	}
}
