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

namespace System.Drawing {
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdiplusStartupInput
	{
    	uint 		GdiplusVersion;
    	IntPtr 		DebugEventCallback;
    	int			SuppressBackgroundThread;
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
	internal struct BitmapData_RAW
	{
		internal int 		width;
    	internal int 		height;
    	internal int 		stride;
    	internal int 		pixelFormat;
    	internal IntPtr 	scan0;
    	internal int 		reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Rect
	{
		internal int left;
		internal int top;
		internal int right;
		internal int bottom;
	}

}

