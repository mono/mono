//
// System.Drawing.gdipStructs.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace System.Drawing {
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdiplusStartupInput
	{
    	uint 		GdiplusVersion;
    	IntPtr 		DebugEventCallback;
    	int             SuppressBackgroundThread;
    	int 		SuppressExternalCodecs;
    
    	internal static GdiplusStartupInput MakeGdiplusStartupInput ()
    	{
    		GdiplusStartupInput result = new GdiplusStartupInput ();
        	result.GdiplusVersion = 1;
        	result.DebugEventCallback = IntPtr.Zero;
        	result.SuppressBackgroundThread = 0;
        	result.SuppressExternalCodecs = 0;
        	return result;
    	}
    	
    }
    
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdiplusStartupOutput
	{
	    internal IntPtr 	NotificationHook;
    	internal IntPtr		NotificationUnhook;
    	
    	internal static GdiplusStartupOutput MakeGdiplusStartupOutput ()
    	{
    		GdiplusStartupOutput result = new GdiplusStartupOutput ();
    		result.NotificationHook = result.NotificationUnhook = IntPtr.Zero;
        	return result;
    	}
	}
	
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdipRect
	{
		internal int left;
		internal int top;
		internal int right;
		internal int bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct GdipRectF
	{
		internal float left;
		internal float top;
		internal float right;
		internal float bottom;

		public GdipRectF (RectangleF r)
		{
			left = r.Left;
			top = r.Top;
			right = r.Right;
			bottom = r.Bottom;
		}
	}

}

