//
// System.Drawing.gdipStructs.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
// Jordi Mas (jordi@ximian.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace System.Drawing 
{
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
	internal struct GdiColorPalette
	{
   		internal int Flags;             // Palette flags
    		internal int Count;             // Number of color entries    				
    	}
    	
    	[StructLayout(LayoutKind.Sequential)]
    	internal struct  GdiColorMap 
    	{
    		internal int from;
    		internal int to;
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal  struct LOGFONTA  
	{
		internal int    lfHeight;
		internal uint   lfWidth;
		internal uint   lfEscapement;
		internal uint   lfOrientation;
		internal uint   lfWeight;
		internal byte   lfItalic;
		internal byte   lfUnderline;
		internal byte   lfStrikeOut;
		internal byte   lfCharSet;
		internal byte   lfOutPrecision;
		internal byte   lfClipPrecision;
		internal byte   lfQuality;
		internal byte   lfPitchAndFamily;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
		internal string lfFaceName;
	}  
	
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	internal struct GdipImageCodecInfo	/*Size 76 bytes*/
	{
    		internal Guid Clsid;    		
    		internal Guid  FormatID;    		
    		internal IntPtr CodecName;
    		internal IntPtr DllName;
    		internal IntPtr FormatDescription;
    		internal IntPtr FilenameExtension;
    		internal IntPtr MimeType;
		internal ImageCodecFlags Flags;
		internal int Version;
		internal int SigCount;
		internal int SigSize;
    		IntPtr SigPattern;
    		IntPtr SigMask;
    		
    		internal static void MarshalTo (GdipImageCodecInfo gdipcodec, ImageCodecInfo codec)
    		{
    			// FIXME: See run-time bug 57706. When fixed, we should be able to remove the if(s)    			
    			if (gdipcodec.CodecName != IntPtr.Zero)
    				codec.CodecName = Marshal.PtrToStringUni (gdipcodec.CodecName);  			    			
    			
    			if (gdipcodec.DllName != IntPtr.Zero)
    				codec.DllName = Marshal.PtrToStringUni (gdipcodec.DllName);
    			
    			if (gdipcodec.FormatDescription != IntPtr.Zero)
    				codec.FormatDescription = Marshal.PtrToStringUni (gdipcodec.FormatDescription);
    			
    			if (gdipcodec.FilenameExtension != IntPtr.Zero)    			
    				codec.FilenameExtension = Marshal.PtrToStringUni (gdipcodec.FilenameExtension);
    			
    			if (gdipcodec.MimeType != IntPtr.Zero)    			
    				codec.MimeType = Marshal.PtrToStringUni (gdipcodec.MimeType);
    			
    			codec.Clsid = gdipcodec.Clsid;
    			codec.FormatID = gdipcodec.FormatID;			
    			codec.Flags = gdipcodec.Flags;
			codec.Version  = gdipcodec.Version;		
			
    		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct GdipEncoderParameter {
		internal Guid guid;
		internal uint numberOfValues;
		internal EncoderParameterValueType type;
		internal IntPtr value;
	}
}

